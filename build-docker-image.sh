#!/bin/bash
# Description: Build the backend docker image.
# Give the current user id and group id as build arguments to set the user in the container.
docker build --network=host --build-arg USER_ID=$(id -u) --build-arg GROUP_ID=$(id -g) -t backend-app .
