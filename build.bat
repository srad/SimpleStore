@echo off

docker build -t sedrad/simplestore:latest .\SimpleStore
docker push sedrad/simplestore

docker build -t sedrad/simplestore-admin:latest .\SimpleStore.Admin
docker push sedrad/simplestore-admin