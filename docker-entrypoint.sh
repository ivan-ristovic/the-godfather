#!/bin/sh
set -e;

chown -R gf:gf /app/data
exec sudo -u gf "$@"
