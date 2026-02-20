const fs = require("fs");
const path = require("path");
const validator = require("gltf-validator");

const args = process.argv.slice(2);
const failOnWarning = args.includes("--fail-on-warning");
const files = args.filter((arg) => !arg.startsWith("--"));

if (files.length === 0) {
  console.error("Usage: node ./scripts/validate-glb.js <file.glb> [more.glb] [--fail-on-warning]");
  process.exit(1);
}

const severityNames = {
  0: "ERROR",
  1: "WARNING",
  2: "INFO",
  3: "HINT"
};

async function validateFile(filePath) {
  const absolutePath = path.resolve(filePath);
  const data = fs.readFileSync(absolutePath);

  const result = await validator.validateBytes(new Uint8Array(data), {
    uri: absolutePath,
    maxIssues: 0,
    writeTimestamp: false
  });

  const issues = result.issues;
  console.log(`\n[glTF-Validator] ${absolutePath}`);
  console.log(
    `errors=${issues.numErrors}, warnings=${issues.numWarnings}, infos=${issues.numInfos}, hints=${issues.numHints}`
  );

  for (const message of issues.messages) {
    const severity = severityNames[message.severity] ?? String(message.severity);
    const pointer = message.pointer ?? "<no-pointer>";
    console.log(`  ${severity} ${message.code} ${pointer} :: ${message.message}`);
  }

  return issues.numErrors > 0 || (failOnWarning && issues.numWarnings > 0);
}

async function main() {
  let hasFailure = false;

  for (const file of files) {
    try {
      const failed = await validateFile(file);
      hasFailure = hasFailure || failed;
    } catch (error) {
      hasFailure = true;
      console.error(`\n[glTF-Validator] Failed to validate ${file}`);
      console.error(error && error.stack ? error.stack : String(error));
    }
  }

  process.exit(hasFailure ? 2 : 0);
}

main();
