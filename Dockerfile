# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY RoundsApp.sln ./
COPY RoundsApp/RoundsApp.csproj RoundsApp/
COPY RoundsApp.Tests/RoundsApp.Tests.csproj RoundsApp.Tests/
COPY .config/ .config/
COPY .editorconfig ./
COPY .globalconfig ./
COPY stylecop.json ./

# Restore dependencies
RUN dotnet restore RoundsApp/RoundsApp.csproj

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /src/RoundsApp
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create a non-root user
RUN addgroup --system --gid 1001 appuser \
    && adduser --system --uid 1001 --gid 1001 appuser

# Copy published files
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "RoundsApp.dll"]
