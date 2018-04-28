cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.AspNetCore\CatFactory.AspNetCore
set testPath=%cd%\CatFactory.AspNetCore\CatFactory.AspNetCore.Tests
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
