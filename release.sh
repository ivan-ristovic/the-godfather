#!/bin/bash

image=ivanristovic/godfather
docker build . --no-cache -t ${image}:latest $@
docker push ${image}:latest
