#!/bin/bash

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

echo "Inner docker daemon is ready" >> /proc/1/fd/1;

sleep 30;

cd /app/SaucyBot/ && docker-compose -f compose.prod.yml up

# # Held for manual debugging: the inner docker daemon is up but the stack is NOT
# # started automatically. Open a shell in this container and run:
# #
# #     cd /app/SaucyBot/ && docker-compose -f compose.prod.yml up
# #
# echo "HOLD: not starting the stack automatically." >> /proc/1/fd/1;
# echo "Open a shell in this container and run:" >> /proc/1/fd/1;
# echo "    cd /app/SaucyBot/ && docker-compose -f compose.prod.yml up" >> /proc/1/fd/1;
# tail -f /dev/null
