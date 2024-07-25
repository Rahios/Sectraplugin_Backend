#!/bin/bash
# Description: Build the backend docker image.
# Give the current user id and group id as build arguments to set the user in the container.

# Change to the directory where the Dockerfile is located
cd "/home/elias/tbSectra/backend/sectraplugin_backend"

# Build the image
docker build --network=host --build-arg USER_ID=$(id -u) --build-arg GROUP_ID=$(id -g) --build-arg DOCKER_GID=1003 -f "API REST/Dockerfile" -t api-rest .
