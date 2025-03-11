#/bin/bash

cp ../SaucyBot/.dockerignore .;
docker-compose build "$@";
rm .dockerignore;
