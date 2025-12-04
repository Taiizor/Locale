/**
 * Post-install script for Locale CLI
 * Downloads the platform-specific binary from GitHub Releases
 */

import { createWriteStream, existsSync, mkdirSync, chmodSync, rmSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { createRequire } from "node:module";
import { pipeline } from "node:stream/promises";
import { createGunzip } from "node:zlib";
import { spawn } from "node:child_process";
import { getBinaryDir, getBinaryPath, getDownloadUrl, getPlatformInfo } from "./platform.js";

const __dirname = dirname(fileURLToPath(import.meta.url));
const rootDir = resolve(__dirname, "..");

const require = createRequire(import.meta.url);
const packageJson = require("../package.json");

/**
 * Download a file from URL
 * @param {string} url - URL to download
 * @param {string} destPath - Destination file path
 */
async function downloadFile(url, destPath) {
  const response = await fetch(url, {
    redirect: "follow",
    headers: {
      "User-Agent": "locale-cli-npm-installer",
    },
  });

  if (!response.ok) {
    throw new Error(`Failed to download: ${response.status} ${response.statusText}`);
  }

  const destDir = dirname(destPath);
  if (!existsSync(destDir)) {
    mkdirSync(destDir, { recursive: true });
  }

  const fileStream = createWriteStream(destPath);
  await pipeline(response.body, fileStream);
}

/**
 * Extract a zip file
 * @param {string} zipPath - Path to zip file
 * @param {string} destDir - Destination directory
 */
async function extractZip(zipPath, destDir) {
  if (!existsSync(destDir)) {
    mkdirSync(destDir, { recursive: true });
  }

  // Use the built-in unzip command on Unix or PowerShell on Windows
  const isWindows = process.platform === "win32";

  return new Promise((resolve, reject) => {
    let child;

    if (isWindows) {
      child = spawn("powershell", [
        "-NoProfile",
        "-Command",
        `Expand-Archive -Path "${zipPath}" -DestinationPath "${destDir}" -Force`,
      ]);
    } else {
      child = spawn("unzip", ["-o", zipPath, "-d", destDir]);
    }

    child.on("error", (err) => {
      reject(new Error(`Failed to extract zip: ${err.message}`));
    });

    child.on("close", (code) => {
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`Extraction failed with code ${code}`));
      }
    });
  });
}

/**
 * Make a file executable (Unix only)
 * @param {string} filePath - Path to file
 */
function makeExecutable(filePath) {
  if (process.platform !== "win32") {
    chmodSync(filePath, 0o755);
  }
}

/**
 * Main installation function
 */
async function install() {
  const version = packageJson.version;
  const { rid, platform, arch } = getPlatformInfo();

  console.log(`Installing Locale CLI v${version} for ${rid}...`);

  const binaryPath = getBinaryPath(rootDir);
  const binaryDir = getBinaryDir(rootDir);

  // Check if already installed
  if (existsSync(binaryPath)) {
    console.log("Locale CLI binary already exists, skipping download.");
    return;
  }

  const downloadUrl = getDownloadUrl(version);
  const zipPath = join(rootDir, `locale-${rid}.zip`);

  try {
    console.log(`Downloading from ${downloadUrl}...`);
    await downloadFile(downloadUrl, zipPath);

    console.log("Extracting...");
    await extractZip(zipPath, binaryDir);

    // Make binary executable
    makeExecutable(binaryPath);

    console.log(`Locale CLI v${version} installed successfully!`);
  } catch (error) {
    console.error(`Failed to install Locale CLI: ${error.message}`);
    console.error("");
    console.error("Alternative installation methods:");
    console.error("  1. Install .NET SDK and use: dotnet tool install -g Locale.CLI");
    console.error("  2. Download manually from: https://github.com/Taiizor/Locale/releases");
    process.exit(1);
  } finally {
    // Clean up zip file
    if (existsSync(zipPath)) {
      rmSync(zipPath, { force: true });
    }
  }
}

// Run installation
install().catch((error) => {
  console.error("Installation failed:", error);
  process.exit(1);
});