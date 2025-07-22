#!/bin/bash

# Set non-interactive frontend for apt-get
export DEBIAN_FRONTEND=noninteractive

if [ ! -f $SECRETS_PATH ]; then
    echo "$SECRETS_PATH does not exists"
    exit 1
fi

cp $SECRETS_PATH $SECRETS_PATH.tmp
sed -i 's/\r$//' $SECRETS_PATH.tmp
. $SECRETS_PATH.tmp

# rm $SECRETS_PATH.tmp -f

export MSSQL_SA_PASSWORD=$MSSQL_SA_PASSWORD
export DB_PASSWORD=$DB_PASSWORD
export DB_USER=$DB_USER
export DB_NAME=$DB_NAME

echo "
########################################################
          Installing tools and dependencies
########################################################
"

apt-get update && apt-get install -y \
        nano \
        git \
        wget \
        curl \
        apt-transport-https \
        gnupg2 \
        jq \
        netcat \
        iproute2 \
        net-tools \
        procps

/opt/mssql/bin/sqlservr &
sleep 5

curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list

apt-get update && ACCEPT_EULA=Y apt-get install -y \
        msodbcsql17 \
        mssql-tools

# Check if SQL Server process is running
if ! pgrep -x "sqlservr" > /dev/null
then
    echo "SQL Server process is not running. Exiting."
    pgrep -x "sqlservr"
    exit 1
fi

# Check if SQL Server is ready
for i in {1..30}; do
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" &> /dev/null
    if [ $? -eq 0 ]; then
        echo "SQL Server is up and running."
        break
    fi
    if [ $i -eq 30 ]; then
        echo "Unable to login to sql. Exiting."
        exit 1
    fi
    echo "Waiting for SQL Server to be ready... ($i/30)"
    sleep 1
done

# Run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d master -i /docker_files/setup.sql -v DB_PASSWORD=$DB_PASSWORD -v DB_NAME=$DB_NAME -v DB_USER=$DB_USER

if [ $? -gt 0 ]; then
    echo "Error: SQL command failed"
    exit 1
fi

find /docker_files/migrations -name "*.sql" -exec /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d "$DB_NAME" -i {} \;

if [ $? -gt 0 ]; then
    echo "Error: SQL command failed"
    exit 1
fi

echo "SQL command executed successfully"

echo "
########################################################
                   Installing pscore
########################################################
"

apt-get update
apt-get install -y wget apt-transport-https software-properties-common
source /etc/os-release
wget -q https://packages.microsoft.com/config/ubuntu/$VERSION_ID/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update
apt-get install -y powershell
pwsh -c "Install-Module -Name SqlServer -AllowClobber -Force -Scope AllUsers"

pwsh -c "(Invoke-Sqlcmd -Query \"SELECT 'CONNECTION FOR ''$DB_USER'' WAS SUCCESSFUL' AS [Result]\" -ServerInstance localhost -Database $DB_NAME -Username $DB_USER -Password $DB_PASSWORD -TrustServerCertificate).Result"