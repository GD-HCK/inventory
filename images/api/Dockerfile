# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG ASPNETCORE_BUILD_CONTAINER_IMAGE_TAG=mcr.microsoft.com/dotnet/sdk:8.0
ARG ASPNETCORE_RUN_CONTAINER_IMAGE_TAG=mcr.microsoft.com/dotnet/aspnet:8.0

# This stage is used to build the service project
FROM ${ASPNETCORE_BUILD_CONTAINER_IMAGE_TAG} AS build

ARG VERSION_NUMBER
ENV VERSION_NUMBER=${VERSION_NUMBER}.0

USER root

WORKDIR /app_files

COPY /app_files .

RUN mv ./app/appsettings.template.json ./app/appsettings.json
RUN rm -f ./app/appsettings.Development.json

# Set version dynamically and build the project
RUN dotnet publish "./app/Inventory.csproj" -c Release -o ./app/publish /p:InformationalVersion=$VERSION_NUMBER /p:FileVersion=$VERSION_NUMBER /p:AssemblyVersion=$VERSION_NUMBER

COPY --from=pyroscope/pyroscope-dotnet:latest-glibc /Pyroscope.Profiler.Native.so ./app/publish/Pyroscope.Profiler.Native.so
COPY --from=pyroscope/pyroscope-dotnet:latest-glibc /Pyroscope.Linux.ApiWrapper.x64.so ./app/publish/Pyroscope.Linux.ApiWrapper.x64.so

# Run unit tests
RUN dotnet test "./InventoryUnitTests/InventoryUnitTests.csproj"

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)

FROM ${ASPNETCORE_RUN_CONTAINER_IMAGE_TAG} AS final

ARG MSSQL_SERVER_SERVICE_NAME
ARG ASPNETCORE_ENVIRONMENT
ARG SECRETS_PATH
ARG SECRETS_ID
ARG WEB_USER
ARG WEB_CONTAINER_SERVICE_PORT_HTTPS
ARG WEB_SCHEME_HTTPS
ARG VERSION_NUMBER
ARG WEB_DNS
ARG JWT_LIFETIME_MINUTES
ARG LOG_LEVEL
ARG OTLP_ENDPOINT_PORT
ARG OTLP_ENDPOINT_HOST
ARG OTLP_PREFIX
ARG AUTHOR
ARG PYROSCOPE_INGESTION_ENDPOINT

# Set environment variables
ENV MSSQL_SERVER_SERVICE_NAME=${MSSQL_SERVER_SERVICE_NAME}
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
ENV SECRETS_PATH=${SECRETS_PATH}
ENV VERSION_NUMBER=${VERSION_NUMBER}
ENV WEB_DNS=${WEB_DNS}
ENV WEB_CONTAINER_SERVICE_PORT_HTTPS=${WEB_CONTAINER_SERVICE_PORT_HTTPS}
ENV WEB_SCHEME_HTTPS=${WEB_SCHEME_HTTPS}
ENV JWT_LIFETIME_MINUTES=${JWT_LIFETIME_MINUTES}
ENV LOG_LEVEL=${LOG_LEVEL}
ENV OTLP_ENDPOINT_PORT=${OTLP_ENDPOINT_PORT}
ENV OTLP_ENDPOINT_HOST=${OTLP_ENDPOINT_HOST}
ENV OTLP_PREFIX=${OTLP_PREFIX}
ENV AUTHOR=${AUTHOR}

LABEL maintainer="${AUTHOR}"
LABEL version="${VERSION_NUMBER}"
LABEL description="Dockerfile for th Inventory api"

# Use root user
USER root

WORKDIR /docker_files

# Make the script executable
RUN chown root:root /docker_files -R
RUN chmod 771 /docker_files -R
COPY provisioning.sh .

COPY --from=build /app_files/app/publish /app

RUN rm -rf /app_files

# Make the script executable
RUN chown -R root:root /app
RUN chmod -R 770 /app

# Convert Windows-style line endings to Unix-style line endings
RUN sed -i 's/\r$//' /docker_files/provisioning.sh

# these environment variables are injected by dot sourcing the file from $SECRETS_PATH
# ENV DB_PASSWORD=${DB_PASSWORD}
# ENV DB_USER=${DB_USER}
# ENV DB_NAME=${DB_NAME}

# Run provisioning script
RUN --mount=type=secret,id=${SECRETS_ID},target=${SECRETS_PATH} \
    /docker_files/provisioning.sh

# Clean up
RUN rm -rf /docker_files

# Create a new user
RUN useradd -m ${WEB_USER} && echo "${WEB_USER}:${WEB_USER}" | chpasswd

# Set ownership and permissions for the /app directory
RUN chown -R ${WEB_USER}:${WEB_USER} /app
RUN chmod -R 770 /app

# Ensure the DataProtection-Keys directory exists
RUN mkdir -p /app/DataProtection-Keys
RUN chown -R ${WEB_USER}:${WEB_USER} /app/DataProtection-Keys
RUN chmod -R 770 /app/DataProtection-Keys

RUN mkdir -p /app/DataProtection-Cert
RUN chown -R ${WEB_USER}:${WEB_USER} /app/DataProtection-Cert
RUN chmod -R 777 /app/DataProtection-Cert

# Ensure pyroscope directory exists and set permissions
RUN mkdir -p /var/log/pyroscope/dotnet
RUN chown -R ${WEB_USER}:${WEB_USER} /var/log/pyroscope
RUN chmod -R 755 /var/log/pyroscope

USER ${WEB_USER}

WORKDIR /app

# pyroscope environment variables
ENV PYROSCOPE_APPLICATION_NAME=inventory-api
ENV PYROSCOPE_SERVER_ADDRESS=${PYROSCOPE_INGESTION_ENDPOINT}
ENV PYROSCOPE_LABELS=environment:${ASPNETCORE_ENVIRONMENT}
ENV PYROSCOPE_PROFILING_ENABLED=1
ENV PYROSCOPE_PROFILING_EXCEPTION_ENABLED=1
ENV PYROSCOPE_PROFILING_ALLOCATION_ENABLED=1
ENV PYROSCOPE_PROFILING_HEAP_ENABLED=1
ENV PYROSCOPE_PROFILING_LOCK_ENABLED=1
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={BD1A650D-AC5D-4896-B64F-D6FA25D6B26A}
ENV CORECLR_PROFILER_PATH=/app/Pyroscope.Profiler.Native.so
ENV LD_PRELOAD=/app/Pyroscope.Linux.ApiWrapper.x64.so

# Expose the port the app runs on
EXPOSE ${WEB_CONTAINER_SERVICE_PORT_HTTPS}

ENTRYPOINT ["dotnet", "Inventory.dll"]