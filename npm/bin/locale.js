#!/usr/bin/env node

/**
 * Locale CLI - Cross-platform localization management tool
 * This wrapper script executes the .NET-based Locale CLI tool.
 */

import { spawn } from "node:child_process";
import { existsSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { getBinaryPath, getPlatformInfo } from "../lib/platform.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const rootDir = resolve(__dirname, "..");

/**
 * Main entry point
 */
async function main() {
  const binaryPath = getBinaryPath(rootDir);

  if (!existsSync(binaryPath)) {
    const { platform, arch } = getPlatformInfo();
    console.error(
      `Error: Locale CLI binary not found at ${binaryPath}`
    );
    console.error(
      `Platform: ${platform}, Architecture: ${arch}`
    );
    console.error(
      "Please try reinstalling the package: npm install @taiizor/locale-cli"
    );
    process.exit(1);
  }

  // Pass all arguments to the binary
  const args = process.argv.slice(2);

  const child = spawn(binaryPath, args, {
    stdio: "inherit",
    cwd: process.cwd(),
  });

  child.on("error", (err) => {
    console.error(`Failed to start Locale CLI: ${err.message}`);
    process.exit(1);
  });

  child.on("close", (code) => {
    process.exit(code ?? 0);
  });
}

main();