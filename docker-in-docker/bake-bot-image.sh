#!/bin/sh
# Builds the bot image ahead of time and saves it as a tarball that the
# docker-in-docker image bakes in, so the container doesn't build it on startup.
#
# Usage: ./bake-bot-image.sh [platform]   (default: linux/amd64)

set -eu

platform="${1:-linux/amd64}"

cd "$(dirname "$0")"
mkdir -p images

docker buildx build \
    --platform "$platform" \
    --build-arg CONFIGURATION=Release \
    -t saucybot-bot:baked \
    -o type=docker,dest=images/bot.tar \
    -f ../SaucyBot/Containerfile \
    ../SaucyBot
