#!/bin/sh

# Set non-interactive frontend for apt-get
export DEBIAN_FRONTEND=noninteractive

if [ ! -f $SECRETS_PATH ]; then
    echo "$SECRETS_PATH does not exists"
    exit 1
fi

cp $SECRETS_PATH $SECRETS_PATH.tmp
sed -i 's/\r$//' $SECRETS_PATH.tmp
. $SECRETS_PATH.tmp

rm $SECRETS_PATH.tmp -f

echo "
########################################################
          Installing tools and dependencies
########################################################
"

apt-get update && apt-get install -y \
        apt-transport-https \
        jq \
        wget \
        curl \
        nano


cd /app

echo "Setting connection string in appsettings.json"

connection_string="Data Source=$MSSQL_SERVER_SERVICE_NAME;Initial Catalog=$DB_NAME;User Id=$DB_USER;Password=$DB_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;Encrypt=False;"

# Modify the JSON file with the connection string
jq --arg conn_str "$connection_string" '.ConnectionStrings.MSSQL = $conn_str' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json

# jwt encryption
jq --arg SecretKey "$JWT_SECRET_KEY" '.Authentication.Jwt.SecretKey = $SecretKey' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
jq --arg Issuer "$WEB_SCHEME_HTTPS://$WEB_DNS" '.Authentication.Jwt.Issuer = $Issuer' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
jq --arg Audience "$WEB_SCHEME_HTTPS://$WEB_DNS" '.Authentication.Jwt.Audience = $Audience' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
jq --arg TokenLifetimeMinutes "$JWT_LIFETIME_MINUTES" '.Authentication.Jwt.TokenLifetimeMinutes = $TokenLifetimeMinutes' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json

# api version
jq --arg version "$VERSION_NUMBER" '.Version = $version' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json

# data encryption
jq --arg CertName "$CERTIFICATE_NAME" '.DataEncryption.CertificateName = $CertName' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
jq --arg CertPass "$CERTIFICATE_PASSWORD" '.DataEncryption.CertificatePassword = $CertPass' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json

# logging
jq --arg LogLevel "$LOG_LEVEL" '.Logging.LogLevel.Default = $LogLevel' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json

# OpenTelemetry configuration
jq --arg EndpointHost "$OTLP_ENDPOINT_HOST" '.Otlp.EndpointHost = $EndpointHost' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
jq --arg EndpointPort "$OTLP_ENDPOINT_PORT" '.Otlp.EndpointPort = $EndpointPort' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
jq --arg Prefix "$OTLP_PREFIX" '.Otlp.Prefix = $Prefix' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json

# # kestrel configuration https
# jq --arg Url "$WEB_SCHEME_HTTPS://$WEB_DNS" '.Kestrel.Endpoints.localhostHttps.Url = $Url' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
# jq --arg PortHttps "$WEB_CONTAINER_SERVICE_PORT_HTTPS" '.Kestrel.Endpoints.localhostHttps.Port = $PortHttps' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
# jq --arg CertPath "/app/DataProtection-Cert/$CERTIFICATE_NAME" '.Kestrel.Endpoints.localhostHttps.Certificate.Path = $CertPath' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
# jq --arg CertPass "$CERTIFICATE_PASSWORD" '.Kestrel.Endpoints.localhostHttps.Certificate.Password = $CertPass' /app/appsettings.json > /app/docker_files.$$.json && mv /app/docker_files.$$.json /app/appsettings.json
