cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.AspNetCore
set testPath=%cd%\CatFactory.AspNetCore.Tests
set outputBasePath=C:\Temp\CatFactory.AspNetCore\
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %outputBasePath%\OnLineStore.WebApi.UnitTests
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
