# Rounds API

A social beer drinking application API built with ASP.NET Core.

## About

Rounds is a social application designed to enhance the beer drinking experience by helping groups of friends manage rounds, track drinks, and socialize together.

## Project Status

This project is in active development. Core infrastructure including authentication, database schema, and repository layer are implemented.

## Tech Stack

- ASP.NET Core 9.0 Web API
- C# / .NET 9.0
- PostgreSQL 18
- Entity Framework Core 9.0
- JWT Authentication
- ASP.NET Core Identity
- Docker & Docker Compose
- Scalar API Documentation
- Serilog (Structured Logging)
- xUnit (Testing Framework)

## Features

### Authentication & User Management
- JWT-based authentication
- User management with ASP.NET Core Identity
- Token-based authorization

### Database & Data Layer
- PostgreSQL database with Entity Framework Core
- Complete domain model with 17+ entities
- Repository pattern implementation with interfaces
- Audit fields tracking (CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
- Composite keys and complex relationships

### Infrastructure
- OpenAPI/Swagger documentation with Scalar UI
- Dockerized application and database
- Environment-based configuration
- Health checks for monitoring
- Structured logging with Serilog
- Comprehensive test suite with xUnit
- GitHub Actions CI/CD pipeline
- Code formatting standards with .editorconfig

### Domain Models
- **Sessions**: Drinking sessions with locations, participants, invites, comments, images, and tags
- **Drinks**: Drink types, drinks with images, user drink tracking, and favorites
- **Achievements**: User and session achievements with criteria
- **Social**: Friendships with bidirectional relationships and notifications
- **Users**: Extended ApplicationUser with audit tracking

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
- Scalar UI: `http://localhost:5001/scalar/v1`
- OpenAPI JSON: `http://localhost:5001/openapi/v1.json`
- Health Check: `http://localhost:5001/health`

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

## Implemented Features

### Data Models & Repository Layer
- ✅ Session management (drinking sessions, locations, participants, invites)
- ✅ Comment and image support for sessions
- ✅ Tag system for categorizing sessions
- ✅ Drink catalog with types and images
- ✅ User drink tracking per session
- ✅ Favorite drinks for users
- ✅ Achievement system (user and session achievements)
- ✅ Friendship system with directional relationships
- ✅ Notification system
- ✅ Complete CRUD operations via repositories

## Planned Features

### API Endpoints (In Progress)
- Session CRUD endpoints
- Participant and invite management
- Comment and image upload endpoints
- Drink catalog endpoints
- User statistics and achievements
- Friendship management
- Notification delivery

### Additional Features
- Payment tracking and splitting
- Real-time notifications via SignalR
- Image storage integration (S3/Azure Blob)
- Search and filtering
- Analytics and reporting

## Development

### Running Tests

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add <MigrationName> --project RoundsApp
```

Apply migrations:
```bash
dotnet ef database update --project RoundsApp
```

### Logging

Logs are written to:
- Console output (all environments)
- `logs/rounds-api-{Date}.txt` files (rolling daily)

### Code Style

This project uses `.editorconfig` for consistent code formatting. The CI pipeline enforces code style checks.

To format code:
```bash
dotnet format
```

### CI/CD

The project uses GitHub Actions for continuous integration:
- **Build and Test**: Runs on every push and pull request
- **Code Quality**: Checks code formatting and builds with warnings as errors
- **Docker Build**: Validates Docker image builds

### Project Structure

- `RoundsApp/` - Main API project
  - `Data/` - Database context and configurations
  - `DTOs/` - Data transfer objects
  - `Endpoints/` - Minimal API endpoints
  - `Migrations/` - EF Core migrations
  - `Models/` - Domain models (17+ entities)
  - `Repositories/` - Repository implementations
    - `IRepositories/` - Repository interfaces
  - `Services/` - Business logic and services
- `RoundsApp.Tests/` - Test project
  - Unit and integration tests
- `.github/` - GitHub Actions workflows and templates
  - `workflows/` - CI/CD pipelines
  - `ISSUE_TEMPLATE/` - Issue templates
  - `pull_request_template.md` - PR template
- `data_model.mermaid` - Complete database schema diagram

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- All tests pass
- Code follows the project's style guidelines
- Documentation is updated as needed

## License

See [LICENSE](LICENSE) file for details.

## Contact

@jerealeksanteri 
janiemi@hotmail.fi
