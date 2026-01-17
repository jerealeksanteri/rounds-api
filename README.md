# Rounds API

A social beer drinking application API built with ASP.NET Core.

## About

Rounds is a social application designed to enhance the beer drinking experience by helping groups of friends manage rounds, track drinks, and socialize together.

## Project Status

This project is in active development. Authentication, database integration, and core API infrastructure are implemented.

## Tech Stack

- ASP.NET Core 9.0 Web API
- C# / .NET 9.0
- PostgreSQL 18
- Entity Framework Core 9.0
- JWT Authentication
- ASP.NET Core Identity
- Docker & Docker Compose
- Scalar API Documentation

## Features

- JWT-based authentication
- User management with ASP.NET Core Identity
- PostgreSQL database with Entity Framework Core
- OpenAPI/Swagger documentation with Scalar UI
- Dockerized application and database
- Environment-based configuration

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker and Docker Compose (for containerized setup)
- Your preferred IDE (Visual Studio, VS Code, or JetBrains Rider)

### Installation

#### Option 1: Docker (Recommended)

1. Clone the repository
```bash
git clone <repository-url>
cd rounds-api
```

2. Copy the example environment file
```bash
cp .env.example .env
```

3. Update the `.env` file with your configuration (especially change JWT_SECRET_KEY for production)

4. Start the application with Docker Compose
```bash
docker-compose up -d
```

The API will be available at `http://localhost:5000` (or the port specified in your `.env` file).

#### Option 2: Local Development

1. Clone the repository
```bash
git clone <repository-url>
cd rounds-api
```

2. Set up PostgreSQL database (or use Docker for just the database)
```bash
docker-compose up postgres -d
```

3. Configure your environment variables in `appsettings.Development.json` or user secrets

4. Restore dependencies
```bash
dotnet restore
```

5. Run database migrations
```bash
dotnet ef database update --project RoundsApp
```

6. Run the application
```bash
dotnet run --project RoundsApp
```

## API Documentation

Once the application is running, access the API documentation at:
- Scalar UI: `http://localhost:5000/scalar/v1`
- OpenAPI JSON: `http://localhost:5000/openapi/v1.json`

## Configuration

Key configuration options in `.env`:

- `API_PORT`: API server port (default: 5000)
- `POSTGRES_USER`: PostgreSQL username
- `POSTGRES_PASSWORD`: PostgreSQL password
- `POSTGRES_DB`: PostgreSQL database name
- `POSTGRES_PORT`: PostgreSQL port (default: 5432)
- `JWT_SECRET_KEY`: Secret key for JWT signing (minimum 32 characters)
- `JWT_ISSUER`: JWT token issuer
- `JWT_AUDIENCE`: JWT token audience
- `JWT_EXPIRY_MINUTES`: JWT token expiration time in minutes

## Planned Features

- Round creation and management
- Drink tracking
- Social features for groups
- Payment tracking and splitting
- Real-time notifications

## Development

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add <MigrationName> --project RoundsApp
```

Apply migrations:
```bash
dotnet ef database update --project RoundsApp
```

### Project Structure

- `RoundsApp/` - Main API project
  - Controllers, models, services, and database context

## License

See [LICENSE](LICENSE) file for details.

## Contact

TBD
