@echo off

docker buildx build --push --platform linux/amd64,linux/arm64 --tag sedrad/simplestore:latest .

exit