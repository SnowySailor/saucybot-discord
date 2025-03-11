#/bin/bash

cp .dockerignore ..;
cd ..;
docker buildx build --platform linux/amd64 --push -t snowsailor/saucy:newest -f ./docker-in-docker/Dockerfile .;
rm .dockerignore;
cd docker-in-docker;
