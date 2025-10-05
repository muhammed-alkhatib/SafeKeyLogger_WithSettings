SafeKeyLogger (Educational)
-----------------------------
Project: SafeKeyLogger (with Settings UI)

How to build (using dotnet CLI, requires .NET 6 SDK):
1) Open terminal in project directory (where SafeKeyLogger.csproj is).
2) Run: dotnet build
3) Run: dotnet run

How to publish single EXE (self-contained) for Windows x64:
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o publish

The resulting EXE will be in the 'publish' folder.
If you prefer Visual Studio: Open the .csproj, set TargetFramework to net6.0-windows, then Publish -> Folder -> configure single file + self-contained.
