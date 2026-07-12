const fs = require('fs');
const path = require('path');

const stateFile = path.resolve(__dirname, 'playwright-backend.json');

module.exports = async () => {
  if (!fs.existsSync(stateFile)) {
    return;
  }

  try {
    const state = JSON.parse(fs.readFileSync(stateFile, 'utf8'));
    if (state?.pid) {
      try {
        process.kill(state.pid, 'SIGTERM');
      } catch (err) {
        // process may already be gone
      }
    }
  } catch {
    // ignore invalid state file
  }
};
