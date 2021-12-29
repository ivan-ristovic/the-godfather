#!/bin/bash

# docker pull ivan-ristovic/the-godfather:latest
docker run -v $PWD/data:/app/data the-godfather:latest

