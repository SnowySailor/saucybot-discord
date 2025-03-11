#/bin/bash

cp ../SaucyBot/.dockerignore .;
cd ..;
docker buildx build --platform linux/amd64 --push -t snowsailor/saucy:newest -f ./docker-in-docker/Dockerfile .;
cd docker-in-docker;
rm .dockerignore;
