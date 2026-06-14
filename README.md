# ObjectChess

ObjectChess is a web application for recording and managing chess matches. A user can register an account, log in, and keep track of the games they have played by adding, editing and deleting match records. Each match stores the two player names, the result, the date and if the user wants, the moves in standard chess notation.

Live site: https://objectchess.runasp.net

## Tech stack

- C# and ASP.NET Core MVC (.NET 10)
- MySQL database, accessed with raw ADO.NET (MySql.Data)
- xUnit for unit testing
- GitHub Actions for CI/CD

## Architecture

The solution uses a three-layer architecture so each part has one clear responsibility:

- **ObjectChess.Web** (Presentation): Controllers, Razor Views, View Models and the app startup.
- **ObjectChess.Business** (Business Logic): Services, Domain Models, Interfaces and the Security Helpers (Password hashing and the password policy).
- **ObjectChess.Data** (Data Access): Repository classes that talk to MySQL.
- **ObjectChess.Tests**: xUnit tests with hand-written fake repositories.

The interfaces live in the Business layer, so the dependency between Business and Data is inverted: Data depends on Business, not the other way around. This keeps the business logic independent of the database and makes it easy to test without one.

## Features

- Register and log in, using cookie-based authentication with claims
- Passwords stored with PBKDF2 hashing and a per-user salt, never in plain text
- Create, view, edit and delete your own matches
- Record moves in standard chess notation, validated before they are saved
- Match history with pagination, newest first
- Each user can only see and manage their own matches

## Running it locally

Prerequisites:

- .NET 10 SDK
- A local MySQL server

Steps:

1. Create a database and the Users, Games and Moves tables.
2. Put your local connection string in `ObjectChess.Web/appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=chess_db;Uid=root;Pwd=YOUR_PASSWORD;"
     }
   }
   ```

3. Run the app:

   ```bash
   dotnet run --project ObjectChess.Web
   ```

4. Open the URL shown in the console.

## Tests

Run the unit tests:

```bash
dotnet test
```

Run them with code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

The tests cover the business logic (authentication, the match rules and the move parser) and use fake repositories, so they run without a database.

## CI/CD

The repository uses GitHub Actions (`.github/workflows/deploy.yml`). On every push to `main` the pipeline:

1. builds the solution,
2. runs the unit tests and collects code coverage,
3. publishes the app and deploys it to MonsterASP over FTPS.

The production connection string and the deploy credentials are stored as GitHub repository secrets, never in the code.

## Configuration

- Local development reads the connection string from `appsettings.Development.json`.
- Production reads it from configuration set on the host, so no secret lives in the repository.
