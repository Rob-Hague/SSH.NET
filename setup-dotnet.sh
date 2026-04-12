#!/bin/bash
set -euo pipefail

# Install .NET SDK 10 on Ubuntu 24.04 (Noble) x86_64
# Uses the official Microsoft package feed

echo "==> Adding Microsoft package signing key and repo..."
apt-get update -qq
apt-get install -y --no-install-recommends wget ca-certificates

wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
dpkg -i /tmp/packages-microsoft-prod.deb
rm /tmp/packages-microsoft-prod.deb

echo "==> Installing .NET SDK 10..."
apt-get update -qq
apt-get install -y --no-install-recommends dotnet-sdk-10.0

echo "==> Verifying installation..."
dotnet --version

echo "==> Done."
