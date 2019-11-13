# SerializerBattle
Battle of .net serializers

This project is not using the dotnet Benchmark tool on purpose. We want to be able to build it with ReadyToRun and self-contained to test the max performance of each serializer.

Publish with:
`dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=true /p:PublishSingleFile=true /p:PublishTrimmed=true`

Change win-x64 to the appropriate RID for your OS.
