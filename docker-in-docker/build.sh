#/bin/bash

cp .dockerignore ..;
docker-compose build "$@";
rm ../.dockerignore;
