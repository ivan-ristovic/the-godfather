#!/bin/bash

set -xe;

if [[ ! -f .env ]]; then
    echo 'Cannot find .env file, re-creating ...'
    cp .env_example .env
    echo 'Attempting to edit generated env file with $EDITOR ...'
    $EDITOR .env
fi

mkdir -p data
mkdir -p data/backup
mkdir -p data/logs
mkdir -p data/lavalink

if [[ ! -f data/lavalink/application.yml ]]; then
    echo 'Warning: lavalink config not found'
fi

echo 'Starting the service stack ...'
sleep 1
docker compose up -d --remove-orphans
docker compose logs -f

