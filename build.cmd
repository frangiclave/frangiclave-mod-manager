docker run --name frangiclave-mod-manager -d lyrositor/frangiclave-mod-manager
docker cp ./ frangiclave-mod-manager:/workspace/
docker exec frangiclave-mod-manager nuget restore -NonInteractive -Verbosity quiet
docker exec frangiclave-mod-manager msbuild /p:Configuration=Release /clp:ErrorsOnly /p:OutputPath=/workspace/artifacts/
rmdir /s /q artifacts
docker cp frangiclave-mod-manager:/workspace/artifacts/ artifacts/
docker kill frangiclave-mod-manager
docker rm frangiclave-mod-manager
