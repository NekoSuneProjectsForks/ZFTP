#!/usr/bin/env bash
# ============================================================================
#  Builds ZFTP-<version>-<rid>.tar.gz: a self-contained publish of both
#  ZFTP.App (the GUI) and zftpd (the headless CLI/daemon), in one
#  extract-and-run folder - the same approach as the Linux tarball.
#
#  NOTE: this is an unsigned/unnotarized build. macOS Gatekeeper will refuse
#  to open it with a plain double-click ("unidentified developer") - users
#  need to right-click > Open the first time, or run
#  `xattr -d com.apple.quarantine ZFTP` after extracting. Proper code signing
#  and notarization need an Apple Developer Program membership and aren't set
#  up yet; this gets a working build shipping without blocking on that.
#
#  Usage: installer/mac/build-tarball.sh [version] [rid]
#  rid defaults to osx-arm64 (Apple Silicon) - the only Mac target ci.yml
#  currently builds/tests.
# ============================================================================
set -euo pipefail

VERSION="${1:-0.0.0}"
RID="${2:-osx-arm64}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
BUILD_DIR="$(mktemp -d)"
OUT_DIR="$BUILD_DIR/ZFTP"
trap 'rm -rf "$BUILD_DIR"' EXIT

echo "==> Publishing ZFTP.App + zftpd ($RID, self-contained)"
dotnet publish "$ROOT_DIR/src/ZFTP.App/ZFTP.App.csproj" \
    -c Release -f net8.0 -r "$RID" --self-contained true -o "$OUT_DIR"
dotnet publish "$ROOT_DIR/src/ZFTP.Daemon/ZFTP.Daemon.csproj" \
    -c Release -f net8.0 -r "$RID" --self-contained true -o "$OUT_DIR"
chmod +x "$OUT_DIR/ZFTP" "$OUT_DIR/zftpd"

echo "==> Bundling rclone"
RCLONE_ARCH="arm64"; [ "$RID" = "osx-x64" ] && RCLONE_ARCH="amd64"
"$ROOT_DIR/installer/download-rclone.sh" osx "$RCLONE_ARCH" "$OUT_DIR"

mkdir -p "$ROOT_DIR/dist"
OUT="$ROOT_DIR/dist/ZFTP-$VERSION-$RID.tar.gz"
echo "==> Archiving"
# macOS ships bsdtar, not GNU tar - skip GNU-only --owner/--group/--mode
# normalization (the Linux script uses those); not worth the extra risk of an
# incompatible flag breaking the build for a cosmetic ownership detail.
tar -C "$BUILD_DIR" -czf "$OUT" ZFTP

echo "==> Done: $OUT"
