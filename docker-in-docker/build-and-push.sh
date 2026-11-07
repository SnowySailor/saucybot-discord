#!/bin/sh

set -eu

cd "$(dirname "$0")"

./bake-bot-image.sh linux/amd64

cp .dockerignore ..
trap 'rm -f ../.dockerignore' EXIT

cd ..
docker buildx build --pull --platform linux/amd64 --push -t snowsailor/saucy:newest -f ./docker-in-docker/Dockerfile .
