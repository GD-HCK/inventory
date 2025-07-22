# First stage: Build and provision the database
ARG MSSQL_CONTAINER_IMAGE_TAG=mcr.microsoft.com/mssql/server:2022-latest

FROM ${MSSQL_CONTAINER_IMAGE_TAG}

ARG MSSQL_CONTAINER_SERVICE_PORT=1433

EXPOSE ${MSSQL_CONTAINER_SERVICE_PORT}

USER root

WORKDIR /docker_files

# Copy initialization script to the container
COPY . .

# Make the scripts executable
RUN chown root:root /docker_files -R
RUN chmod 770 /docker_files -R
RUN sed -i 's/\r$//' /docker_files/provisioning.sh

# https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-configure-environment-variables?view=sql-server-ver16
ARG ACCEPT_EULA
ARG MSSQL_PID
ARG MSSQL_MEMORY_LIMIT_MB
ARG SECRETS_PATH
ARG SECRETS_ID

# Set environment variables using build arguments
ENV MSSQL_MEMORY_LIMIT_MB=${MSSQL_MEMORY_LIMIT_MB}
ENV ACCEPT_EULA=${ACCEPT_EULA}
ENV MSSQL_PID=${MSSQL_PID}
ENV SECRETS_PATH=${SECRETS_PATH}

# the following environment variables are injected by dot sourcing the file from $SECRETS_PATH
# ENV DB_NAME=${DB_NAME}
# ENV DB_USER=${DB_USER}
# ENV DB_PASSWORD=${DB_PASSWORD}
# ENV MSSQL_SA_PASSWORD=${MSSQL_SA_PASSWORD}

# load secrets to variables,
# set MSSQL_SA_PASSWORD environment variable for the sql server process to be able to read it
# Start SQL Server and run the provisioning script

RUN --mount=type=secret,id=${SECRETS_ID},target=${SECRETS_PATH} \
    /docker_files/provisioning.sh

RUN rm -rf /docker_files

# Run SQL Server
ENTRYPOINT ["/opt/mssql/bin/sqlservr"]