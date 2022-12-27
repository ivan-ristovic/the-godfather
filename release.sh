#!/bin/bash

image=ivanristovic/the-godfather
docker build . --no-cache -t ${image}:latest $@
docker push ${image}:latest
