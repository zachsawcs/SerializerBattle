
# SerializerBattle
### Battle of .net serializers

This project is not using the dotnet Benchmark tool on purpose. We want to be able to build it with ReadyToRun and self-contained to test the max performance of each serializer.

Publish with:
`dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=true /p:PublishSingleFile=true /p:PublishTrimmed=true`

Change win-x64 to the appropriate RID for your OS.

### Results

**Test system:** Intel(R) Core(TM) i7-8550U CPU @ 1.80GHz, 4 Core(s), 8 Logical Processor(s) - CPU consistently at 3.83GHz when the test runs.

```
Binaron                 106 ms/op
NewtonsoftJson          241 ms/op
NetCore3Json            294 ms/op
```

Surprisingly the new .net core 3.0 JSON serializer ends up being the slowest.
