# Base image for ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /appBackend
EXPOSE 8080
EXPOSE 8081

# Install OpenSSL for certificate generation
RUN apt-get update && apt-get install -y openssl

# Add a non-root user and change ownership
RUN useradd -m appuser && chown -R appuser /appBackend

# Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API REST/API REST.csproj", "API REST/"]
COPY ["DTO/DTO.csproj", "DTO/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["DAL/DAL.csproj", "DAL/"]
RUN dotnet restore "./API REST/API REST.csproj"
COPY . .
WORKDIR "/src/API REST"
RUN dotnet build "./API REST.csproj" -c $BUILD_CONFIGURATION -o /appBackend/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API REST.csproj" -c $BUILD_CONFIGURATION -o /appBackend/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /appBackend
COPY --from=publish /appBackend/publish .

# Create directory for certificates
RUN mkdir /https

# Change ownership of the /https directory
RUN chown -R appuser /https

# Switch to the non-root user
USER appuser

# Generate the key and certificate
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /https/aspnetapp.key -out /https/aspnetapp.crt -subj "/CN=localhost"

# Export to PFX format
RUN openssl pkcs12 -export -out /https/aspnetapp.pfx -inkey /https/aspnetapp.key -in /https/aspnetapp.crt -passout pass:password

# Verify that the files exist
RUN ls -l /https

# Print certificate information for debugging
RUN openssl pkcs12 -in /https/aspnetapp.pfx -passin pass:password -info -nodes

# Environment variables for ASP.NET Core to use the certificate
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=password

ENTRYPOINT ["dotnet", "API REST.dll"]
