const { spawn } = require('child_process');
const fs = require('fs');
const net = require('net');
const path = require('path');

const backendProject = path.resolve(__dirname, '..', '..', 'backend', 'src', 'BootyByBeighley.Api', 'BootyByBeighley.Api.csproj');
const backendUrl = '127.0.0.1';
const backendPort = 5118;
const stateFile = path.resolve(__dirname, 'playwright-backend.json');

function waitForPort(host, port, timeout) {
  const deadline = Date.now() + timeout;
  return new Promise((resolve, reject) => {
    const tryConnect = () => {
      const socket = net.connect(port, host);
      socket.on('connect', () => {
        socket.destroy();
        resolve();
      });
      socket.on('error', () => {
        socket.destroy();
        if (Date.now() > deadline) {
          reject(new Error(`Timed out waiting for ${host}:${port}`));
        } else {
          setTimeout(tryConnect, 500);
        }
      });
    };
    tryConnect();
  });
}

module.exports = async () => {
  const backendProcess = spawn('dotnet', ['run', '--project', backendProject, '--urls', `http://${backendUrl}:${backendPort}`], {
    cwd: path.dirname(backendProject),
    shell: true,
    stdio: ['ignore', 'pipe', 'pipe'],
  });

  backendProcess.stdout.on('data', chunk => process.stdout.write(`[backend] ${chunk}`));
  backendProcess.stderr.on('data', chunk => process.stderr.write(`[backend] ${chunk}`));

  backendProcess.on('exit', code => {
    if (code !== 0) {
      console.error(`Backend process exited with code ${code}`);
    }
  });

  fs.writeFileSync(stateFile, JSON.stringify({ pid: backendProcess.pid, port: backendPort, host: backendUrl }));

  try {
    await waitForPort(backendUrl, backendPort, 120_000);
  } catch (error) {
    backendProcess.kill();
    throw error;
  }
};
