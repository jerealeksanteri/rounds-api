# Rounds API

A social beer drinking application API built with ASP.NET Core.

## About

Rounds is a social application designed to enhance the beer drinking experience by helping groups of friends manage rounds, track drinks, and socialize together.

## Project Status

This project is in active development. Core infrastructure is complete with:
- ✅ Full database schema (17+ entities)
- ✅ Repository layer with complete CRUD operations
- ✅ JWT authentication and authorization
- ✅ Comprehensive REST API endpoints
- ✅ 168 integration tests (all passing)
- ✅ CI/CD pipeline with GitHub Actions

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
- **Social**: Friendships with bidirectional relationships, friend groups, and notifications
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
- ✅ Complete database schema with 17+ entities
- ✅ Repository pattern with interfaces for all entities
- ✅ Session management (drinking sessions, locations, participants, invites)
- ✅ Comment and image support for sessions
- ✅ Tag system for categorizing sessions
- ✅ Drink catalog with types and images
- ✅ User drink tracking per session
- ✅ Favorite drinks for users
- ✅ Achievement system (user and session achievements)
- ✅ Friendship system with directional relationships
- ✅ Friend groups for organizing friends
- ✅ Notification system
- ✅ Audit tracking (CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)

## API Endpoints

### Implemented Endpoints

#### Authentication
- ✅ User registration with email validation
- ✅ User login with JWT token generation
- ✅ Password validation and security requirements

#### Sessions
- ✅ Session CRUD operations (GET, POST, PUT, DELETE)
- ✅ Get sessions by user ID
- ✅ Get upcoming sessions
- ✅ Session participant management (add, remove, update status)
- ✅ Session comment management (create, update, delete)
- ✅ Session invite management (create, accept, reject)
- ✅ Session tag management (create, delete)
- ✅ Session location management (create, update, delete)
- ✅ Session image management (metadata only)

#### Drinks & Drink Types
- ✅ Drink CRUD operations (GET, POST, PUT, DELETE)
- ✅ Search drinks by name
- ✅ Filter drinks by type
- ✅ Drink Type CRUD operations (GET, POST, PUT, DELETE)
- ✅ Get drink types by creator
- ✅ Name validation for drink types

#### Social Features
- ✅ Friendship management (create, update, delete)
- ✅ Get all friendships for a user
- ✅ Get received friend requests
- ✅ Get sent friend requests
- ✅ Accept/reject friend requests
- ✅ Authorization checks for friendship operations
- ✅ Friend group CRUD operations (create, update, delete)
- ✅ Add/remove members to friend groups
- ✅ Bulk invite friend group to session
- ✅ Validation that only friends can be added to groups
- ✅ Notification CRUD operations
- ✅ Get unread notifications
- ✅ Mark notifications as read

#### Achievements
- ✅ Achievement CRUD operations (GET, POST, PUT, DELETE)
- ✅ Filter achievements by type
- ✅ JSON criteria storage for achievement conditions

### Testing
- ✅ Comprehensive integration tests for all endpoints (168 tests)
- ✅ Test coverage for authentication flows
- ✅ Test coverage for authorization and access control
- ✅ Validation testing for required fields
- ✅ Database state verification in tests

## Planned Features

### Additional Features
- Image upload and storage integration (S3/Azure Blob)
- User drink tracking per session endpoints
- User favorite drinks endpoints
- User and session achievements assignment
- Payment tracking and splitting
- Real-time notifications via SignalR
- Advanced search and filtering capabilities
- Analytics and reporting dashboards
- Profile management endpoints
- Statistics and leaderboards

## Development

### Running Tests

The project has 168 integration tests covering all API endpoints.

Run all tests:
```bash
dotnet test
```

Run tests with detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

Run tests with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

Test coverage includes:
- Authentication and authorization
- All CRUD operations
- Input validation
- Access control and permissions
- Database state verification

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

The project uses GitHub Actions for continuous integration and automated releases:

#### Build and Test Workflow
- Runs on every push and pull request
- Code quality checks (formatting, warnings as errors)
- Validates Docker image builds

#### Release Workflow
The release workflow automatically creates new versions when code is merged to the `main` branch.

**Semantic Versioning:**
The workflow automatically determines version bumps based on PR labels:

| Label | Version Bump | Example |
|-------|--------------|---------|
| `breaking-change` | MAJOR (x.0.0) | 1.2.3 → 2.0.0 |
| `feature` or `enhancement` | MINOR (0.x.0) | 1.2.3 → 1.3.0 |
| `bug`, `fix`, or no label | PATCH (0.0.x) | 1.2.3 → 1.2.4 |

**How to Create a Release:**

1. Create a PR from `dev` → `stage` → `main`
2. Add appropriate labels to your PR:
   - `breaking-change` - for breaking changes that require major version bump
   - `feature` or `enhancement` - for new features that require minor version bump
   - `bug` or `fix` - for bug fixes (or leave unlabeled for patch bump)
3. Merge to `main` - the workflow will:
   - Run all tests
   - Build Docker images for multiple architectures (linux/amd64, linux/arm64)
   - Build platform-specific binaries (Linux x64/ARM64, Windows x64)
   - Automatically determine the version number based on PR labels
   - Create a GitHub release with auto-generated release notes
   - Publish Docker images to GitHub Container Registry
   - Attach binaries to the release

**Fallback: Conventional Commits**
If PR labels are not used, the workflow falls back to detecting version bumps from commit messages:
- `feat:` prefix or `feat!:` → MINOR or MAJOR bump
- `BREAKING CHANGE:` in commit body → MAJOR bump
- Other commits → PATCH bump

**Release Artifacts:**
Each release includes:
- Docker images: `ghcr.io/jerealeksanteri/rounds-api:vX.Y.Z` and `:latest`
- Binaries: `rounds-api-linux-x64.tar.gz`, `rounds-api-linux-arm64.tar.gz`, `rounds-api-win-x64.zip`
- Auto-generated release notes with all merged PRs
- Docker Compose usage examples

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
