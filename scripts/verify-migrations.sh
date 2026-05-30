#!/usr/bin/env bash
set -euo pipefail

dotnet tool restore >/dev/null 2>&1 || true

mkdir -p artifacts/migrations
dotnet ef migrations script --idempotent \
  --project src/JobPlatform.DAL/JobPlatform.DAL.csproj \
  --startup-project src/JobPlatform.API/JobPlatform.API.csproj \
  --configuration Release \
  --output artifacts/migrations/migrations.sql

echo "EF migration script generated: artifacts/migrations/migrations.sql"
