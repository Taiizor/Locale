/**
 * Platform detection utilities for Locale CLI
 */

import { join } from "node:path";
import { platform as osPlatform, arch as osArch } from "node:os";

/**
 * Platform identifiers used by .NET runtime identifiers
 */
const PLATFORM_MAP = {
  darwin: "osx",
  linux: "linux",
  win32: "win",
};

/**
 * Architecture identifiers used by .NET runtime identifiers
 */
const ARCH_MAP = {
  x64: "x64",
  arm64: "arm64",
  arm: "arm",
  ia32: "x86",
};

/**
 * Get the current platform and architecture
 * @returns {{ platform: string, arch: string, rid: string }}
 */
export function getPlatformInfo() {
  const platform = osPlatform();
  const arch = osArch();

  const mappedPlatform = PLATFORM_MAP[platform];
  const mappedArch = ARCH_MAP[arch];

  if (!mappedPlatform) {
    throw new Error(`Unsupported platform: ${platform}`);
  }

  if (!mappedArch) {
    throw new Error(`Unsupported architecture: ${arch}`);
  }

  // .NET Runtime Identifier (RID)
  const rid = `${mappedPlatform}-${mappedArch}`;

  return {
    platform: mappedPlatform,
    arch: mappedArch,
    rid,
  };
}

/**
 * Get the binary filename for the current platform
 * @returns {string}
 */
export function getBinaryFilename() {
  const platform = osPlatform();
  return platform === "win32" ? "locale.exe" : "locale";
}

/**
 * Get the full path to the binary
 * @param {string} rootDir - Root directory of the package
 * @returns {string}
 */
export function getBinaryPath(rootDir) {
  const { rid } = getPlatformInfo();
  const filename = getBinaryFilename();
  return join(rootDir, "bin", rid, filename);
}

/**
 * Get the download URL for the binary
 * @param {string} version - Package version
 * @returns {string}
 */
export function getDownloadUrl(version) {
  const { rid } = getPlatformInfo();
  const filename = getBinaryFilename();

  // GitHub Releases URL pattern
  return `https://github.com/Taiizor/Locale/releases/download/v${version}/locale-${rid}.zip`;
}

/**
 * Get the expected binary directory for the current platform
 * @param {string} rootDir - Root directory of the package
 * @returns {string}
 */
export function getBinaryDir(rootDir) {
  const { rid } = getPlatformInfo();
  return join(rootDir, "bin", rid);
}