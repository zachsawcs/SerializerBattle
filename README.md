# SerializerBattle  
### Battle of .net serializers  

[Run it on .NET Fiddler](https://dotnetfiddle.net/gVrYma)

#### Contestants  
 - Binaron.Serializer - https://github.com/zachsaw/Binaron.Serializer
 - Newtonsoft.Json - https://github.com/JamesNK/Newtonsoft.Json
 - System.Text.Json.JsonSerializer - https://github.com/dotnet/corefx
  
This project is not using the dotnet Benchmark tool on purpose (this is already covered in the Binaron.Serializer repo).

### Results  
  
As ran on .NET Fiddler.  
  
```  
NetCore3JsonTest
================
Warm-up
Running
Result: 23.320 ms/op

NewtonsoftJsonTest
==================
Warm-up
Running
Result: 22.260 ms/op

BinaronTest
===========
Warm-up
Running
Result: 5.940 ms/op
```  

Surprisingly the new .net 5.0 JSON serializer ends up being the *slowest*! But, at least it is no longer 15x slower as it used to be in .net core 3.0!  
  
It also failed to deserialize `Dictionary<string, object>` where the value was serialized as a string. As such, I would strongly recommend people to avoid  `System.Text.Json.JsonSerializer` until it is mature enough.