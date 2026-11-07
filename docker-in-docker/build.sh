#!/bin/sh
# Local build: bakes the bot image for this machine's platform, then builds the dind image.

set -eu

cd "$(dirname "$0")"

./bake-bot-image.sh "$(docker version -f '{{.Server.Os}}/{{.Server.Arch}}')"

cp .dockerignore ..
trap 'rm -f ../.dockerignore' EXIT

docker-compose build --pull "$@"
