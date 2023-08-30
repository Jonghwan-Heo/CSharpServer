#!/bin/sh

get_current_directory() {
    current_file="${0}"
    echo "${current_file%/*}"
}

CWD=$(get_current_directory)

echo "$CWD/Tool/protobuf-net/protoc"
echo "$CWD/Tool/protobuf-net/network/network"
echo "$CWD/Tool/protobuf-net/network/network/bin/Release/netcoreapp3.1/network.dll"
echo "$CWD/../Client/Assets/Plugins/Network"

$CWD/Tool/protobuf-net/protoc $CWD/network.proto --csharp_out=$CWD/Tool/protobuf-net/network/network --proto_path=$CWD

dotnet build $CWD/Tool/protobuf-net/network/network.sln -c:Release

cp $CWD/Tool/protobuf-net/network/network/bin/Release/netcoreapp3.1/network.dll $CWD/../Client/Assets/Plugins/Network
cp $CWD/Tool/protobuf-net/network/network/network.cs $CWD/Server
