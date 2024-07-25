#!/bin/bash

cd /home/elias/tbSectra/
docker compose --env-file /home/elias/tbSectra/ai/histo_lung/.env -f docker-compose-backend-histolung.yml up -d

