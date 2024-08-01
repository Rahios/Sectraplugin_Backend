# Base image for ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /appBackend
EXPOSE 8080
EXPOSE 8081

# Arguments for user and group IDs
ARG USER_ID
ARG GROUP_ID
ARG DOCKER_GID

# Install necessary packages including Docker CLI
RUN apt-get update && apt-get install -y \
    openssl \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg \
    lsb-release \
    software-properties-common \
    net-tools && \
    curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg && \
    echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    apt-get update && \
    apt-get install -y docker-ce-cli

# Add a non-root user and group with specified UID and GID
RUN addgroup --gid ${GROUP_ID} appgroup && \
    addgroup --gid ${DOCKER_GID} docker && \
    adduser --uid ${USER_ID} --gid ${GROUP_ID} --disabled-password --gecos "Default user" appuser && \
    usermod -aG docker appuser && \
    chown -R appuser:appgroup /appBackend

# Create directories for data and output
RUN mkdir -p /home/user/appBackend/data/tcga && \
    mkdir -p /home/user/appBackend/data/outputs && \
    chown -R appuser:appgroup /home/user/appBackend/data

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
RUN chown -R appuser:appgroup /https

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
