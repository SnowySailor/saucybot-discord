#!/bin/sh

set -eu

cd "$(dirname "$0")"

./bake-bot-image.sh linux/amd64

root="$(cd .. && pwd)"
cp .dockerignore "$root/"
trap 'rm -f "$root/.dockerignore"' EXIT

cd ..
docker buildx build --pull --platform linux/amd64 --push -t snowsailor/saucy:newest -f ./docker-in-docker/Dockerfile .
