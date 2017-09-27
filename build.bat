cls
set initialPath=%cd%
set srcPath=%cd%\src\CatFactory.AspNetCore
set testPath=%cd%\test\CatFactory.AspNetCore.Tests
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
