# SerializerBattle  
### Battle of .net serializers  
  
#### Contestants  
 - Binaron.Serializer - https://github.com/zachsaw/Binaron.Serializer
 - Newtonsoft.Json - https://github.com/JamesNK/Newtonsoft.Json
 - System.Text.Json.JsonSerializer - https://github.com/dotnet/corefx
  
This project is not using the dotnet Benchmark tool on purpose. We want to be able to build it with ReadyToRun and self-contained to test the max performance of each serializer as well as to verify that they work properly when built with the new `dotnet publish` switches.

Publish with:  
`dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=true /p:PublishSingleFile=true /p:PublishTrimmed=true`  
  
Change osx-x64 to the appropriate RID for your OS.  
  
### Results  
  
**Test system:** Intel(R) Core(TM) i5-3470 CPU @ 3.20GHz on macOS High Sierra v10.13.6  
  
```  
BinaronTest           83 ms/op  
NewtonsoftJsonTest    244 ms/op  
NetCore3JsonTest      1458 ms/op  
```  
  
Surprisingly the new .net core 3.0 JSON serializer ends up being the *slowest by miles*!  
  
Not sure what is wrong with the new .net core JSON serializer. No matter how you run it (even with just `dotnet run`), it's always excruciatingly slow.

As such, I would strongly recommend people to avoid  `System.Text.Json.JsonSerializer` until it is mature enough.