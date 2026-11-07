FROM docker:dind

COPY ./ /app/

RUN test -f /app/docker-in-docker/images/bot.tar \
    || (echo "images/bot.tar is missing: run docker-in-docker/bake-bot-image.sh first" >&2 && exit 1)

ENTRYPOINT ["sh", "/app/docker-in-docker/entrypoint.sh"]
