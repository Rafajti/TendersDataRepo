# TendersData

TendersData is a .NET application that aggregates and provides access to tender data with filtering and pagination capabilities. The application fetches data from an external API and caches it for efficient access.

## Project Purpose

This application serves as a data aggregation service for Polish tenders. It provides a RESTful API that allows clients to:
- Retrieve all tenders with optional filtering (by price range, date range)
- Get specific tenders by ID
- Use pagination for large result sets
- Sort results by various criteria

The application uses a background service to periodically fetch and cache tender data, ensuring fast response times for API consumers.

## Data Source

Data is fetched from **TendersGuru** - a public API for Polish tenders:
- **API Base URL:** [https://tenders.guru/api/pl/](https://tenders.guru/api/pl/)
- The application uses a background service to periodically load and cache data from this API
- Cached data is refreshed automatically at configured intervals

## Requirements

- **.NET 9.0** SDK
- **Docker** (optional, for containerized deployment)

## Local Startup

To run the application locally:

```bash
dotnet run --project src/TendersData.Api
```

The API will be available at:
- HTTP: `http://localhost:5216`
- HTTPS: `https://localhost:7181` (if configured)

### Configuration

Application settings can be configured in:
- `src/TendersData.Api/appsettings.json` - default configuration
- `src/TendersData.Api/appsettings.Development.json` - development overrides
- User Secrets (for sensitive data)

Key configuration options:
- `TendersGuru:BaseUrl` - Base URL for the TendersGuru API
- `TendersGuru:PagesCount` - Number of pages to fetch from the API

## Docker

The application is containerized and can be run using Docker Compose.

### Building and Running with Docker

From the repository root, run:

```bash
docker compose up --build
```

This will:
- Build the Docker image from the `Dockerfile` in `src/TendersData.Api/`
- Start the container with the API service
- Expose ports 8080 (HTTP) and 8081 (HTTPS)

### Docker Compose Configuration

The `docker-compose.yml` file includes:
- Service: `tendersdata-api`
- Port mappings: `8080:8080` and `8081:8081`
- Environment variables for TendersGuru configuration

You can override environment variables in `docker-compose.yml` or by using environment-specific compose files.

### Dockerfile

The `Dockerfile` is located at `src/TendersData.Api/Dockerfile` and uses a multi-stage build:
- Base stage: ASP.NET runtime image
- Build stage: SDK for building the application
- Publish stage: Publishes the application
- Final stage: Production-ready image

## Testing

To run all tests:

```bash
dotnet test
```

This will execute tests from:
- `TendersData.Api.Tests` - API controller and middleware tests
- `TendersData.Application.Tests` - Application layer tests
- `TendersData.Infrastructure.Tests` - Infrastructure layer tests

## API Documentation

### OpenAPI

In Development mode, OpenAPI documentation is available at:
- `/openapi/v1.json`

The API uses `Microsoft.AspNetCore.OpenApi` for OpenAPI generation. XML documentation comments from the controllers are included in the OpenAPI specification.