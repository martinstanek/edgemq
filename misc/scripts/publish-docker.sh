#!/bin/bash

dotnet build ../../*.sln
dotnet publish ../../src/EdgeMq.Api/EdgeMq.Api.csproj \
  --configuration Release \
  --runtime osx-x64 \
  -p:Version=0.0.1 \
  -p:AssemblyVersion=0.0.1 \
  -p:InformationalVersion=0.0.1

mkdir -p workspace
mv ../../src/EdgeMq.Api/bin/Release/net9.0/osx-x64/native/EdgeMq.Api ./workspace/EdgeMq.Api
cp ../Dockerfile ./workspace
cd ./workspace

docker build -t edgemq:0.0.1 .
