#!/usr/bin/env bash
# Local equivalent of .github/workflows/seeded-stack.yml (job: build-and-run-seeded).
#
#   1. Starts PostgreSQL 15 via docker compose (mirrors the postgres:15-alpine
#      service container + docker-compose.yml creds).
#   2. Restores/builds the .NET 10 backend (warnings as errors NU1903, ASPDEPR002).
#   3. npm ci + Angular CLI 21 production build for frontend/app.
#   4. Starts backend (dotnet run) + frontend (ng serve) in the background,
#      logging to .logs/, waits for readiness, verifies seeded Postgres rows,
#      and runs the same smoke test as CI.
#
# Usage:
#   ./run.sh                    # (re)start full stack: kills old PIDs + frees
#                               # API_PORT/WEB_PORT, then starts fresh (restart-safe)
#   ./run.sh --stop             # stop backend + frontend (pidfiles + ports) and exit
#   ./run.sh --skip-build       # skip restore/build + npm ci/build (just restart servers)
#   ./run.sh --backend-only      # only postgres + backend (also stops stale frontend)
#   ./run.sh --frontend-only     # only postgres + backend + frontend serve
#                                # (backend still starts: frontend proxies /api to it)
#   ./run.sh --no-verify        # skip seeded-data check + smoke test
#   ./run.sh --detach            # start servers and exit (no log tail, no cleanup trap)
#   ./run.sh --help              # usage
set -euo pipefail

# --- repo root (script lives in root) ---
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

# --- env (same defaults as workflow `env:`) ---
export DOTNET_NOLOGO="${DOTNET_NOLOGO:-true}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE="${DOTNET_SKIP_FIRST_TIME_EXPERIENCE:-true}"
export NODE_OPTIONS="${NODE_OPTIONS:- --disable-warning=DEP0040 --disable-warning=DEP0169}"
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
export API_PORT="${API_PORT:-5118}"
export WEB_PORT="${WEB_PORT:-4200}"
export PG_PORT="${PG_PORT:-5432}"
export PG_DB="${PG_DB:-booty_by_beighley_dev}"
export PG_USER="${PG_USER:-postgres}"
export PG_PASSWORD="${PG_PASSWORD:-Testpass123!}"
export ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection:-Host=localhost;Port=5432;Database=booty_by_beighley_dev;Username=postgres;Password=Testpass123!}"

SKIP_BUILD=0
BACKEND_ONLY=0
FRONTEND_ONLY=0   # kept for parity with --backend-only; backend still starts (proxy target)
NO_VERIFY=0
DETACH=0
STOP_ONLY=0

for arg in "$@"; do
  case "$arg" in
    --skip-build) SKIP_BUILD=1 ;;
    --backend-only) BACKEND_ONLY=1 ;;
    --frontend-only) FRONTEND_ONLY=1 ;;
    --no-verify) NO_VERIFY=1 ;;
    --detach) DETACH=1 ;;
    --stop) STOP_ONLY=1 ;;
    --help|-h)
      sed -n '2,22p' "$0"
      exit 0
      ;;
    *) echo "Unknown argument: $arg (try --help)" >&2; exit 1 ;;
  esac
done

log() { echo "[run.sh] $*"; }

# --- restart-safe shutdown helpers (pidfiles + ports + patterns) ---
kill_pid() {
  local pid="${1:-}"
  if [ -z "$pid" ] || ! kill -0 "$pid" 2>/dev/null; then return 0; fi
  kill "$pid" 2>/dev/null || true
  for _ in $(seq 1 5); do
    kill -0 "$pid" 2>/dev/null || return 0
    sleep 1
  done
  kill -9 "$pid" 2>/dev/null || true
  return 0
}

stop_from_pidfile() {
  local pidfile="$1"
  if [ -f "$pidfile" ]; then
    local pid
    pid="$(cat "$pidfile" 2>/dev/null || true)"
    if [ -n "${pid:-}" ]; then
      log "Stopping stale PID $pid from $pidfile..."
      kill_pid "$pid"
    fi
    rm -f "$pidfile"
  fi
}

# Kill whatever is listening on a TCP port (restart-safe even when the
# pidfile is stale/missing, e.g. previous --detach run or manual ng serve).
free_port() {
  local port="$1"
  local pids=""
  if command -v lsof >/dev/null 2>&1; then
    pids="$pids $(lsof -ti :"$port" 2>/dev/null || true)"
  fi
  if command -v fuser >/dev/null 2>&1; then
    # fuser reports PIDs on stderr (e.g. "5118/tcp: 1234 5678").
    pids="$pids $(fuser "${port}"/tcp 2>&1 || true)"
  fi
  # shellcheck disable=SC2009
  if command -v ps >/dev/null 2>&1; then
    # Fallback: match common listeners when lsof/fuser are unavailable.
    if [ "$port" = "$API_PORT" ]; then
      pids="$pids $(ps -eo pid,args 2>/dev/null | grep -E 'BootyByBeighley\.Api|dotnet run.*BootyByBeighley' | grep -v grep | awk '{print $1}' || true)"
    elif [ "$port" = "$WEB_PORT" ]; then
      pids="$pids $(ps -eo pid,args 2>/dev/null | grep -E 'ng serve|@angular/cli.*serve' | grep -v grep | awk '{print $1}' || true)"
    fi
  fi
  # Dedupe (same PID is often reported by lsof + fuser + ps).
  # shellcheck disable=SC2086
  pids="$(echo $pids | tr ' ' '\n' | grep -E '^[0-9]+$' | sort -u | tr '\n' ' ' || true)"
  for pid in $pids; do
    if [ "$pid" -eq "$$" ] 2>/dev/null; then continue; fi
    log "Killing process $pid on port $port..."
    kill_pid "$pid"
  done
  # Confirm the port is actually free before the caller rebinds it.
  for _ in $(seq 1 10); do
    if command -v lsof >/dev/null 2>&1; then
      lsof -ti :"$port" >/dev/null 2>&1 || return 0
    elif (echo > /dev/tcp/127.0.0.1/"$port") >/dev/null 2>&1; then
      sleep 1
    else
      return 0
    fi
    sleep 1
  done
  echo "Port $port is still occupied after kill attempts." >&2
  return 1
}

stop_service() {
  # $1=name $2=pidfile $3=port
  log "Stopping $1 (pidfile + port $3)..."
  stop_from_pidfile "$2"
  free_port "$3"
}

stop_all() {
  # Always stop both: backend-only mode must not leave a stale frontend behind.
  stop_service "backend" .logs/backend.pid "$API_PORT"
  stop_service "frontend" .logs/frontend.pid "$WEB_PORT"
}

cleanup() {
  # Only auto-stop servers in foreground mode; --detach leaves them running.
  # Belt-and-suspenders: pidfiles first, then anything still holding the ports
  # (covers dotnet/ng child processes that outlive their parent shell).
  if [ "$DETACH" -eq 1 ]; then return; fi
  log "Stopping servers..."
  stop_from_pidfile .logs/backend.pid
  stop_from_pidfile .logs/frontend.pid
  free_port "$API_PORT" || true
  if [ "$BACKEND_ONLY" -eq 0 ]; then
    free_port "$WEB_PORT" || true
  fi
}
if [ "$DETACH" -eq 0 ]; then
  trap cleanup EXIT INT TERM
fi

if [ "$STOP_ONLY" -eq 1 ]; then
  trap - EXIT INT TERM
  mkdir -p .logs
  stop_all
  log "Stopped."
  exit 0
fi

need() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing required tool: $1" >&2
    return 1
  fi
}

log "Checking prerequisites (mirrors setup-dotnet/setup-node)..."
need git; need dotnet; need node; need npm; need curl
if ! command -v docker >/dev/null 2>&1; then
  echo "Missing required tool: docker (needed for postgres). Start postgres manually if needed." >&2
  exit 1
fi

log "Versions:"
git --version
dotnet --version
dotnet --list-sdks
node --version
npm --version
if command -v pg_isready >/dev/null 2>&1; then
  pg_isready --version
else
  log "WARNING: pg_isready not found; using curl/tcp fallback for postgres wait."
  log "Install postgresql-client for full parity with CI (psql is required for seed verification)."
fi
if [ "$NO_VERIFY" -eq 0 ]; then need psql; fi

# --- 1. PostgreSQL (service container in CI -> docker compose locally) ---
log "Starting postgres via docker compose (mirrors postgres:15-alpine service)..."
if docker compose version >/dev/null 2>&1; then
  docker compose up -d postgres
else
  docker-compose up -d postgres
fi

log "Waiting for postgres at 127.0.0.1:${PG_PORT} (up to 90s)..."
for i in $(seq 1 90); do
  if command -v pg_isready >/dev/null 2>&1; then
    if pg_isready -h 127.0.0.1 -p "$PG_PORT" -U "$PG_USER" >/dev/null 2>&1; then
      log "PostgreSQL is ready."
      break
    fi
  else
    if (echo > /dev/tcp/127.0.0.1/"$PG_PORT") >/dev/null 2>&1; then
      log "PostgreSQL TCP port is open."
      break
    fi
  fi
  if [ "$i" -eq 90 ]; then
    echo "PostgreSQL never became ready." >&2
    exit 1
  fi
  sleep 1
done

# --- 2. Backend restore + build ---
if [ "$SKIP_BUILD" -eq 0 ]; then
  log 'Backend restore + build (warnings as errors NU1903, ASPDEPR002)...'
  dotnet restore backend/BootyByBeighley.sln
  dotnet build backend/BootyByBeighley.sln --no-restore -c Debug -p:WarningsAsErrors="NU1903%3BASPDEPR002"
else
  log "Skipping build (--skip-build)."
fi

# --- 3. Frontend install + production build ---
if [ "$SKIP_BUILD" -eq 0 ]; then
  log "Frontend install deps (npm ci, fallback to npm install)..."
  pushd frontend/app >/dev/null
  if [ -f package-lock.json ]; then
    npm ci || npm install
  else
    npm install
  fi
  if [ "$BACKEND_ONLY" -eq 0 ]; then
    log "Frontend production build (verifies Angular CLI 21 compile)..."
    npx --yes @angular/cli@21 build
  fi
  popd >/dev/null
else
  log "Skipping frontend install/build (--skip-build)."
fi

mkdir -p .logs

# --- 4. Start backend (background, plain HTTP like --urls in CI) ---
# Restart-safe: kill stale pidfile owners AND anything holding API_PORT
# (handles stale pidfiles, prior --detach runs, or manually started servers).
log "Restarting backend on http://localhost:${API_PORT} (log: .logs/backend.log)..."
stop_service "backend" .logs/backend.pid "$API_PORT"
nohup dotnet run --project backend/src/BootyByBeighley.Api/BootyByBeighley.Api.csproj \
  --no-build -c Debug --urls "http://localhost:${API_PORT}" \
  >.logs/backend.log 2>&1 &
echo $! > .logs/backend.pid
log "Backend PID: $(cat .logs/backend.pid)"

log "Waiting for http://localhost:${API_PORT}/openapi/v1.json (up to 150s)..."
for i in $(seq 1 150); do
  if curl -fsS "http://localhost:${API_PORT}/openapi/v1.json" -o /dev/null 2>&1; then
    log "Backend is up."
    break
  fi
  if ! kill -0 "$(cat .logs/backend.pid)" 2>/dev/null; then
    echo "Backend process exited. Last 60 log lines:" >&2
    tail -n 60 .logs/backend.log || true
    exit 1
  fi
  if [ "$i" -eq 150 ]; then
    echo "Backend did not expose /openapi/v1.json in time." >&2
    tail -n 60 .logs/backend.log || true
    exit 1
  fi
  sleep 1
done

# --- 5. Verify seeded Postgres data ---
if [ "$NO_VERIFY" -eq 0 ]; then
  log "Verifying seeded Postgres data (1 coach + 12 students, 3 plans, 8 movements)..."
  export PGPASSWORD="$PG_PASSWORD"
  echo "--- table row counts ---"
  psql -h 127.0.0.1 -p "$PG_PORT" -U "$PG_USER" -d "$PG_DB" -c \
    "SELECT 'Users' AS tbl, COUNT(*) FROM \"Users\" UNION ALL SELECT 'WorkoutPlans', COUNT(*) FROM \"WorkoutPlans\" UNION ALL SELECT 'Movements', COUNT(*) FROM \"Movements\";"
  USERS=$(psql -h 127.0.0.1 -p "$PG_PORT" -U "$PG_USER" -d "$PG_DB" -tAc 'SELECT COUNT(*) FROM "Users";')
  echo "Users=$USERS"
  if [ "${USERS:-0}" -lt 13 ]; then
    echo "Expected >= 13 seeded users (1 coach + 12 students), got $USERS" >&2
    exit 1
  fi
  log "Seed check passed."
else
  log "Skipping seed verification (--no-verify)."
fi

# --- 6. Start frontend (background, ng serve) ---
if [ "$BACKEND_ONLY" -eq 0 ]; then
  log "Restarting frontend on http://127.0.0.1:${WEB_PORT}/ (log: .logs/frontend.log)..."
  stop_service "frontend" .logs/frontend.pid "$WEB_PORT"
  pushd frontend/app >/dev/null
  mkdir -p ../../.logs
  nohup npx --yes @angular/cli@21 serve --host 127.0.0.1 --port "$WEB_PORT" \
    >../../.logs/frontend.log 2>&1 &
  echo $! > ../../.logs/frontend.pid
  log "Frontend PID: $(cat ../../.logs/frontend.pid)"
  popd >/dev/null

  log "Waiting for http://127.0.0.1:${WEB_PORT}/ (up to 180s)..."
  for i in $(seq 1 180); do
    if curl -fsS "http://127.0.0.1:${WEB_PORT}/" -o /dev/null 2>&1; then
      log "Frontend is up."
      break
    fi
    if ! kill -0 "$(cat .logs/frontend.pid)" 2>/dev/null; then
      echo "Frontend process exited. Last 60 log lines:" >&2
      tail -n 60 .logs/frontend.log || true
      exit 1
    fi
    if [ "$i" -eq 180 ]; then
      echo "Frontend did not respond in time." >&2
      tail -n 60 .logs/frontend.log || true
      exit 1
    fi
    sleep 1
  done
fi

# --- 7. Smoke test (frontend + backend + OpenAPI reachable together) ---
if [ "$NO_VERIFY" -eq 0 ]; then
  log "Smoke test..."
  curl -fsS "http://localhost:${API_PORT}/openapi/v1.json" -o /dev/null
  if [ "$BACKEND_ONLY" -eq 0 ]; then
    curl -fsS "http://127.0.0.1:${WEB_PORT}/" -o /dev/null
  fi
  echo "Smoke test OK:"
  if [ "$BACKEND_ONLY" -eq 0 ]; then
    echo "  Frontend: http://localhost:${WEB_PORT}/"
  fi
  echo "  Backend:  http://localhost:${API_PORT}/"
  echo "  OpenAPI:  http://localhost:${API_PORT}/openapi/v1.json"
  echo "  Postgres: localhost:${PG_PORT} db=${PG_DB} user=${PG_USER}"
else
  log "Skipping smoke test (--no-verify)."
  echo "  Backend:  http://localhost:${API_PORT}/"
  if [ "$BACKEND_ONLY" -eq 0 ]; then
    echo "  Frontend: http://localhost:${WEB_PORT}/"
  fi
fi

log "Logs: .logs/backend.log/.logs/frontend.log (same paths CI uploads as server-logs artifact)."

if [ "$DETACH" -eq 1 ]; then
  log "Detached mode: servers keep running. Stop with: kill \$(cat .logs/backend.pid) \$(cat .logs/frontend.pid)"
  # Remove EXIT trap so cleanup does not kill detached servers.
  trap - EXIT INT TERM
  exit 0
fi

log "Running in foreground. Press Ctrl+C to stop both servers."
tail -F .logs/backend.log .logs/frontend.log 2>/dev/null || tail -F .logs/backend.log
