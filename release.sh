#!/bin/bash

set -xe;

image=ivanristovic/godfather
docker build . --no-cache -t ${image}:latest $@
docker push ${image}:latest
