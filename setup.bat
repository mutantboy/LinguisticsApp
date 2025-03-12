md LinguisticsApp
cd LinguisticsApp
md src
cd src
md LinguisticsApp.DomainModel
md LinguisticsApp.Application
md LinguisticsApp.Infrastructure
md LinguisticsApp.WebApi
cd LinguisticsApp.DomainModel
dotnet new classlib
cd ../LinguisticsApp.Application
dotnet new classlib
dotnet add reference ../LinguisticsApp.DomainModel
cd ../LinguisticsApp.Infrastructure
dotnet new classlib
dotnet add reference ../LinguisticsApp.DomainModel
dotnet add reference ../LinguisticsApp.Application
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Paseto.NET
dotnet add package Microsoft.AspNetCore.Cryptography.KeyDerivation
cd ../LinguisticsApp.WebApi
dotnet new webapi
dotnet add reference ../LinguisticsApp.DomainModel
dotnet add reference ../LinguisticsApp.Application
dotnet add reference ../LinguisticsApp.Infrastructure
cd ../..
md test
cd test
md LinguisticsApp.Tests
cd LinguisticsApp.Tests
dotnet new xunit
dotnet add reference ../../src/LinguisticsApp.DomainModel
dotnet add reference ../../src/LinguisticsApp.Application
dotnet add reference ../../src/LinguisticsApp.Infrastructure
dotnet add reference ../../src/LinguisticsApp.WebApi
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Bogus
cd ../..
dotnet new sln
dotnet sln add src/LinguisticsApp.DomainModel
dotnet sln add src/LinguisticsApp.Application
dotnet sln add src/LinguisticsApp.Infrastructure
dotnet sln add src/LinguisticsApp.WebApi
dotnet sln add test/LinguisticsApp.Tests
start LinguisticsApp.sln