#!/usr/bin/env bash
set -e

# Wait for Postgres to be ready before starting the app.
# Read host/port from env or defaults.
DB_HOST=${POSTGRES_HOST:-postgres}
DB_PORT=${POSTGRES_PORT:-5432}

echo "Waiting for Postgres at $DB_HOST:$DB_PORT..."
until </dev/tcp/${DB_HOST}/${DB_PORT}; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

echo "Postgres is up - starting application"

exec dotnet BlazorChat.Server.dll
