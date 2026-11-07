#!/bin/sh

retry_limit=12
retry_count=0

start_docker() {
    dockerd-entrypoint.sh &
}

check_docker() {
    /usr/local/bin/docker info >/dev/null
}

start_docker

while ! check_docker; do
    echo "docker not running yet. Waiting..." >> /proc/1/fd/1;
    sleep 5;
    retry_count=$((retry_count+1))
    if [ $retry_count -ge $retry_limit ]; then
        echo "Exceeded retry limit. Restarting docker..." >> /proc/1/fd/1;
        retry_count=0
        start_docker
    fi
done

echo "Loading prebuilt bot image" >> /proc/1/fd/1;
docker load -i /app/docker-in-docker/images/bot.tar >> /proc/1/fd/1 2>&1

# Once compose has swapped containers over to the freshly loaded image, clean up
# the previous (now dangling) bot image so the persisted storage doesn't grow forever.
(sleep 300; docker image prune -f >/dev/null 2>&1) &

echo "Starting docker" >> /proc/1/fd/1;

cd /app/SaucyBot/ && docker-compose -f compose.prod.yml -f ../docker-in-docker/docker-compose.dind.yml up
