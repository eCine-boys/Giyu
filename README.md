# Giyu
Discord Bot using Discord.Net v3 with LavaLink server, based on .NET Core.

## Built With

* [DotNet Core (Version - 3.1)](https://dotnet.microsoft.com/download/dotnet-core/2.2) - Dotnet version.
* [Discord.Net (Version - 3.4.0)](https://github.com/RogueException/Discord.Net) - The Discord Library used
* [Dependency Injection](https://github.com/aspnet/DependencyInjection) - [Design pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1) 
* [Victoria (Version - 5.2.3)](https://github.com/Yucked/Victoria) - LavaLink Library. by [Yucked](https://github.com/Yucked)

## Steps

```terminal
ex@root ~$ dotnet restore
```
```terminal
ex@root ~$ dotnet build
```
- After the first starts, go to Giyu\Giyu\bin\Debug\netcoreapp3.1\Resources;
- config.json example:
```json
{
  "token": bot token
  "prefix": bot prefix
  "authorization": lavalink passwd
  "hostname": lavalink host
  "clientsecret": bot client secret
  "clientid": bot clientid
}
```

## Author
* [**Kore**](https://github.com/korex71/)
