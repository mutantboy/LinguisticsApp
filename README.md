# Final project

Erstellen Sie eine Web Anwendung nach dem Clean Architecture Muster. 
Achten Sie dabei wenn möglich auf die Verwendung moderner Programmierparadigmen

IMPORTANT: Every week at least one Commit/Push.

## Domain modelling
Modellierung und dem Mapping eines OO-Models in eine relationale Datenbank.

**Start with Plant UML. UK Notation!!!**

## What should be included:
- Entities, at least 5.
- Inheritance.
- Different relationships (One to many, Inheritance, One to one?, ...)
- Enums 
- Value Objects
- Rich Types
- DB Constraints (not null, …)
- Generate test data with Bogus.
- Configuring a property as a concurrency token.
- ...
 
## Persistence Layer

- Work with services.
- Work with Repositories.

## Web (RESTful API)

- Use validation.
- Use simple Authentication + Authorization
- Implement a meaningful exception handling.
- Add pagination logic to your endpoints.
- Add query and path parameters.
- Use DTOs (with GUIDs -> no IDs in DTOs) and Commands.
- Use the correct HTTP Status Codes.
- Add pagination logic to your endpoints.
- Add query and path parameters.

### Beispiel einer Servicedokumentation, die Resourcen eines OrderManagers:

| Ressource   |      URI      |  URI	Methode(n) |
|----------|:--------------|:------|
| Alle Bestellungen |  /orders | GET, POST |
| Einzelne Bestellung |	/orders/{id} |	GET, PUT, POST
| Stornierte Bestellungen |	/orders?state=cancelled |	GET
| Ausgelieferte Bestellungen |	/orders?state=shipped |	GET
|…		| |

Die Stornierungen wird hier als eigene Ressource abgebildet.

| Ressource   |      URI      |  URI	Methode(n) |
|----------|:--------------|:------|
| Stornierungen |	/cancellations/	| GET, POST
| Einzelne Stornierung |	/cancellations/{id}	| GET

Für die Benutzeroberfläche ist es sinnvoll einzelne Bestellpositionen zu bearbeiten, daher gibt es noch:

| Ressource   |      URI      |  URI	Methode(n) |
|----------|:--------------|:------|
| Bestellpositionen einer Bestellung |	/orders/{id}/items	| GET, POST
| Einzelne Bestellposition |	/orders/{id}/items/{itemId}	| GET, PUT, DELETE

Bauen Sie auch PATCH-Methoden in ihren REST API ein.

### PATCH request

The PUT and PATCH methods are used to update an existing resource. The difference between them is that PUT replaces the entire resource, while PATCH specifies only the changes, see: [MS Documentation](https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch).


## Testen

- XUnit Test (Domain, ...)
- Verwenden Sie bei einigen Test auch Mocking.
- Persistencetests, funktionieren die Queries?
- Siehe dazu auch: [Datenbanktests](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy), speziell auch Tests, die, die [DB ändern](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database#tests-which-modify-data).
- Tests der Web API (Endpoint Tests mit WebApplicationFactory). 
- Integrationstests.
  
   

## Create a new Solution (folder structure like a maven project)

You can also choose your own structure, however it should follow (for example) the Clean-Architecture guidelines.

**Rename XxxApp!**

```cmd
md XxxApp
cd XxxApp
md src
cd src
md XxxApp.Application
md XxxApp.WebApi
cd XxxApp.Application
dotnet new classlib
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
cd ..\XxxApp.WebApi
dotnet new webapi
dotnet add reference ..\XxxApp.Application
cd ..
cd ..
md test
cd test
md XxxApp.Test
cd XxxApp.Test
dotnet new xunit
dotnet add reference ..\..\src\XxxApp.Application
cd ../..
dotnet new sln
dotnet sln add src\XxxApp.WebApi
dotnet sln add src\XxxApp.Application
dotnet sln add test\XxxApp.Test
start XxxApp.sln
```
