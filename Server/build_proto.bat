Tool\protobuf-net\protoc.exe network.proto --csharp_out=:Tool\protobuf-net\network\network

dotnet build Tool\protobuf-net\network\network.sln -c:Release

copy Tool\protobuf-net\network\network\bin\Release\netcoreapp3.1\network.dll ..\Client\Assets\Plugins\Network
copy Tool\protobuf-net\network\network\network.cs Server\

pause