#!/bin/sh
dotnet test --filter DisplayName=OrangeCabinet.Tests.TestServer.TestForever

# exclude
# dotnet test --filter "FullyQualifiedName!=OrangeCabinet.Tests.TestServer.TestForever" --collect:"XPlat Code Coverage"
