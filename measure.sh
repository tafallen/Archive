#!/bin/bash
# Start the service
cd Archiver.Services
dotnet build
dotnet run --no-build > service.log 2>&1 &
PID=$!
sleep 5 # wait for it to start

node measure_compression.js

kill $PID
