const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const stateFile = path.resolve(__dirname, 'playwright-backend.json');

module.exports = async () => {
  if (!fs.existsSync(stateFile)) {
    return;
  }

  try {
    const state = JSON.parse(fs.readFileSync(stateFile, 'utf8'));
    if (state?.pid) {
      try {
        // Use Windows-compatible kill command (SIGKILL, not SIGTERM)
        if (process.platform === 'win32') {
          try {
            execSync(`taskkill /PID ${state.pid} /F`, { stdio: 'ignore' });
          } catch {
            // Process may already be gone
          }
        } else {
          process.kill(state.pid, 'SIGKILL');
        }
      } catch (err) {
        // process may already be gone
      }
    }
  } catch {
    // ignore invalid state file
  }

  // Clean up state file
  try {
    fs.unlinkSync(stateFile);
  } catch {
    // ignore
  }
};
