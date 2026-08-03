#!/usr/bin/env bash
# ============================================================================
#  Downloads the latest rclone release for one platform/arch and drops the
#  executable into <out-dir>/tools/rclone (the same "tools" convention
#  ToolResolver.cs looks for next to the app - see ZFTP.Core). Shared by the
#  Linux tarball, Linux AppImage, and macOS tarball build scripts so bundled
#  rclone always matches whatever rclone most recently released - update this
#  one script and every package that calls it ships the update, the same way
#  release.yml keeps Windows's bundled rclone.exe current.
#
#  Usage: installer/download-rclone.sh <rclone-os> <rclone-arch> <out-dir>
#  rclone-os/arch use rclone's own release-asset naming (not .NET RIDs):
#    linux amd64   |  linux arm64   |  osx amd64   |  osx arm64
#
#  Set GITHUB_TOKEN in the environment to avoid GitHub API's low unauthenticated
#  rate limit (CI already has this via secrets.GITHUB_TOKEN).
# ============================================================================
set -euo pipefail

RCLONE_OS="$1"
RCLONE_ARCH="$2"
OUT_DIR="$3"
BUILD_DIR="$(mktemp -d)"
trap 'rm -rf "$BUILD_DIR"' EXIT

command -v jq >/dev/null || { echo "jq is required (present by default on GitHub-hosted runners)." >&2; exit 1; }

AUTH_HEADER=()
if [ -n "${GITHUB_TOKEN:-}" ]; then AUTH_HEADER=(-H "Authorization: Bearer $GITHUB_TOKEN"); fi

echo "==> Fetching latest rclone release info"
curl -fsSL -H "User-Agent: ZFTP-CI" "${AUTH_HEADER[@]}" \
    "https://api.github.com/repos/rclone/rclone/releases/latest" -o "$BUILD_DIR/release.json"

ASSET_URL="$(jq -r --arg os "$RCLONE_OS" --arg arch "$RCLONE_ARCH" \
    '[.assets[] | select(.name | test("^rclone-v.*-" + $os + "-" + $arch + "\\.zip$"))][0].browser_download_url // empty' \
    "$BUILD_DIR/release.json")"
if [ -z "$ASSET_URL" ]; then
    echo "Could not find a rclone $RCLONE_OS-$RCLONE_ARCH .zip asset in the latest release." >&2
    exit 1
fi

echo "==> Downloading $ASSET_URL"
curl -fsSL "$ASSET_URL" -o "$BUILD_DIR/rclone.zip"
unzip -q "$BUILD_DIR/rclone.zip" -d "$BUILD_DIR/extract"

RCLONE_BIN="$(find "$BUILD_DIR/extract" -type f -name rclone | head -1)"
if [ -z "$RCLONE_BIN" ]; then
    echo "rclone binary not found inside the downloaded archive." >&2
    exit 1
fi

mkdir -p "$OUT_DIR/tools"
cp "$RCLONE_BIN" "$OUT_DIR/tools/rclone"
chmod +x "$OUT_DIR/tools/rclone"
echo "==> Bundled rclone into $OUT_DIR/tools/rclone"
