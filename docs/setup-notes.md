Just some notes I've taken whilst getting things setup - to be properly organised into something readable later!

## Linux .NET Install

https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install

Had to set `PATH` and `DOTNET_ROOT` env vars by hand.

```
apt-get install libicu-dev
```

From the RGB `lib` dir
```
cp librgbmatrix.so.1 /root/.dotnet/shared/Microsoft.NETCore.App/8.0.13
```