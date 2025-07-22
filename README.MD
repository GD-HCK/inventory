# Inventory Api

## How to setup

1. Under the compose folder, remove the `.template` from both the `.env.template` and `.secrets.template` file names
1. Update the values for both files and in particular for
    - WEB_DNS
    - OTLP_ENDPOINT_HOST
    - OTLP_PREFIX
    - PYROSCOPE_INGESTION_ENDPOINT
    - MSSQL_SA_PASSWORD
    - DB_PASSWORD
    - DB_USER
    - DB_NAME
    - CERTIFICATE_NAME
    - CERTIFICATE_PASSWORD
    - JWT_SECRET_KEY
1. Generate a pfx certificate and place it under the same directory as `DATA_PROTECTION_ENCRYPTION_CERT_PATH`
1. Generate a strong Base64 key for `JWT_SECRET_KEY`, used to encrypt the JWT token payload
1. build the images by running docker compose build --no-cache from the compose directory
1. if running via compose, navigate to the compose directory and run `docker-compose up -d`
1. if running via kubernetes, navigate to the kubernetes directory and run `kubectl apply -f inventory-linux.yaml`

## Useful links and scripts

1. [Grafana install - OSS server](https://grafana.com/docs/grafana/latest/setup-grafana/installation/debian/)
    Example install:

    ```sh
    sudo mkdir -p /etc/apt/keyrings/
    wget -q -O - https://apt.grafana.com/gpg.key | gpg --dearmor | sudo tee /etc/apt/keyrings/grafana.gpg > /dev/null
    echo "deb [signed-by=/etc/apt/keyrings/grafana.gpg] https://apt.grafana.com stable main" | sudo tee -a /etc/apt/sources.list.d/grafana.list
    sudo apt-get update
    sudo apt-get install grafana
    sudo systemctl daemon-reload
    sudo systemctl start grafana-server
    sudo systemctl enable grafana-server.service
    sudo systemctl restart grafana-server
    ```

1. [Prometheus install - metrics ingestion](https://grafana.com/docs/grafana/latest/getting-started/get-started-grafana-prometheus/)
    Example install:

    ```sh
    # install prometheus
    wget https://github.com/prometheus/prometheus/releases/download/v3.5.0-rc.0/prometheus-3.5.0-rc.0.linux-amd64.tar.gz
    tar xvf prometheus-3.5.0-rc.0.linux-amd64.tar.gz
    cd prometheus-3.5.0-rc.0.linux-amd64

    # Create Prometheus User
    sudo useradd --no-create-home --shell /bin/false prometheus

    # Move Prometheus Files to a Proper Location
    sudo mv prometheus-3.5.0-rc.0.linux-amd64 /opt/prometheus
    sudo ln -s /opt/prometheus/prometheus /usr/local/bin/prometheus
    sudo ln -s /opt/prometheus/promtool /usr/local/bin/promtool

    # Create Prometheus Configuration Directory
    sudo mkdir -p /etc/prometheus /var/lib/prometheus
    sudo cp /opt/prometheus/prometheus.yml /etc/prometheus/
    sudo chown -R prometheus:prometheus /etc/prometheus /var/lib/prometheus

    sudo tee /etc/systemd/system/prometheus.service > /dev/null <<EOF
    [Unit]
    Description=Prometheus Monitoring System
    Documentation=https://prometheus.io/docs/introduction/overview/
    Wants=network-online.target
    After=network-online.target

    [Service]
    User=prometheus
    Group=prometheus
    Type=simple
    ExecStart=/opt/prometheus/prometheus \
    --config.file=/etc/prometheus/prometheus.yml \
    --storage.tsdb.path=/var/lib/prometheus \
    --web.console.templates=/opt/prometheus/consoles \
    --web.console.libraries=/opt/prometheus/console_libraries \
    --web.enable-lifecycle

    [Install]
    WantedBy=multi-user.target
    EOF

    sudo systemctl daemon-reload
    sudo systemctl start prometheus
    sudo systemctl enable prometheus
    sudo systemctl status -l --no-pager prometheus
    ```

1. [AlertManager install - metrics alerts](https://github.com/prometheus/alertmanager)
    Example Install:

    ```sh
    # Install alert manager
    sudo useradd --no-create-home --shell /bin/false alertmanager

    wget https://github.com/prometheus/alertmanager/releases/download/v0.28.1/alertmanager-0.28.1.linux-amd64.tar.gz
    tar -xvzf alertmanager-0.28.1.linux-amd64.tar.gz
    cd alertmanager-0.28.1.linux-amd64

    sudo ln -s alertmanager /usr/local/bin/alertmanager
    sudo ln -s amtool /usr/local/bin/amtool

    sudo mkdir -p /etc/alertmanager

    sudo tee /etc/alertmanager/alert_manager.yml <<EOF
    # my global config
    global:
    smtp_smarthost: '<your-smtp-ip>:<your-smtp-port>'
    smtp_from: 'prometheus@example.com'

    route:
    receiver: 'email-alert'

    receivers:
    - name: 'email-alert'
        email_configs:
        - to: '<your-email-address>'
    EOF

    sudo tee /etc/systemd/system/alertmanager.service > /dev/null <<EOF
    [Unit]
    Description=Prometheus Alertmanager
    Documentation=https://prometheus.io/docs/alerting/latest/alertmanager/
    After=network.target

    [Service]
    User=alertmanager
    Group=alertmanager
    Type=simple
    ExecStart=/usr/local/bin/alertmanager \
    --config.file=/etc/alertmanager/alert_manager.yml \
    --storage.path=/var/lib/alertmanager
    Restart=always

    [Install]
    WantedBy=multi-user.target
    EOF

    sudo mkdir -p /var/lib/alertmanager
    sudo chown alertmanager:alertmanager /var/lib/alertmanager
    sudo chown -R alertmanager:alertmanager /etc/alertmanager

    sudo systemctl daemon-reload
    sudo systemctl start alertmanager
    sudo systemctl enable alertmanager
    sudo systemctl status -l --no-pager alertmanager

    # Reload Prometheus configuration
    curl -X POST http://<your-prometheus-instance-ip>:9090/-/reload
    ```

1. [Loki install - logs ingestion](https://grafana.com/docs/loki/latest/)
    Example script:

    ```sh
    loki_version="3.4.4"
    sudo useradd --no-create-home --shell /bin/false loki
    sudo groupadd loki
    sudo usermod -aG loki loki
    sudo mkdir -p /var/lib/loki
    sudo chown loki:loki /var/lib/loki
    sudo chmod 770 /var/lib/loki
    wget https://github.com/grafana/loki/releases/download/v${loki_version}/loki_${loki_version}_amd64.deb
    sudo dpkg -i loki_${loki_version}_amd64.deb
    sudo rm loki_${loki_version}_amd64.deb

    sudo systemctl stop loki

    sudo yq --yaml-output --argjson allow_structured_metadata 'true' '.limits_config.allow_structured_metadata = $allow_structured_metadata' \
    /etc/loki/config.yml > /tmp/config.$$.yml && sudo mv /tmp/config.$$.yml /etc/loki/config.yml

    sudo yq --yaml-output --argjson volume_enabled 'true' '.limits_config.volume_enabled = $volume_enabled' \
    /etc/loki/config.yml > /tmp/config.$$.yml && sudo mv /tmp/config.$$.yml /etc/loki/config.yml

    sudo yq --yaml-output --arg alertmanager_url "http://<loki_instance_ip>:9093" '.ruler.alertmanager_url = $alertmanager_url' \
    /etc/loki/config.yml > /tmp/config.$$.yml && sudo mv /tmp/config.$$.yml /etc/loki/config.yml

    sudo sed -i 's/\/tmp\/loki/\/var\/lib\/loki/' /etc/loki/config.yml

    # Configure Loki Compactor
    sudo mkdir -p /var/lib/loki/compactor
    sudo chown loki:loki /var/lib/loki/compactor
    sudo chmod 770 /var/lib/loki/compactor

    sudo tee -a /etc/loki/config.yml > /dev/null <<EOF
    compactor:
    working_directory: /var/lib/loki/compactor
    compaction_interval: 30m
    retention_enabled: true
    retention_delete_delay: 30m
    retention_delete_worker_count: 150
    delete_request_store: filesystem
    EOF

    # Configure Compactor Retention Period to 3 days
    # https://grafana.com/docs/loki/latest/operations/storage/retention/
    sudo yq --yaml-output --arg retention_period '72h' '.limits_config.retention_period = $retention_period' \
    /etc/loki/config.yml > /tmp/config.$$.yml && sudo mv /tmp/config.$$.yml /etc/loki/config.yml

    sudo systemctl start loki
    sudo systemctl restart loki
    sudo systemctl status -l --no-pager loki
    sudo journalctl -u loki.service -f

    # install logcli for log cli actions - works with Loki
    wget https://github.com/grafana/loki/releases/download/v${loki_version}/logcli_${loki_version}_amd64.deb
    sudo dpkg -i logcli_${loki_version}_amd64.deb
    sudo rm logcli_${loki_version}_amd64.deb
    ```

1. [Tempo install - Traces ingestion](https://grafana.com/docs/tempo/latest/)
    Example script:

    ```sh
    sudo useradd --no-create-home --shell /bin/false tempo
    wget https://github.com/grafana/tempo/releases/download/v2.8.1/tempo_2.8.1_linux_amd64.deb
    sudo dpkg -i tempo_2.8.1_linux_amd64.deb
    sudo rm tempo_2.8.1_linux_amd64.deb
    sudo systemctl status -l --no-pager tempo
    ```

1. Custom container logs extraction script:

    ```sh
    # Create log extraction script
    sudo mkdir -p /var/lib/docker_custom_logs
    sudo chown -R loki:root /var/lib/docker_custom_logs
    sudo chmod -R 755 /var/lib/docker_custom_logs

    sudo tee /usr/local/bin/extract_docker_logs.sh > /dev/null <<EOF
    #!/bin/bash
    # This script extracts logs from Docker containers and saves them to a specified directory.
    timestamp=\$(date +"%Y_%m_%d")
    if [ ! -f /var/lib/docker_custom_logs/timestamp ]; then
        echo \$timestamp | tee /var/lib/docker_custom_logs/timestamp
    elif [ "\$(cat /var/lib/docker_custom_logs/timestamp)" != "\$timestamp" ]; then
        unlabelled_logs=\$(find /var/lib/docker_custom_logs -type f -name "*.log" ! -name "timestamp" ! -name "*.log.*")
        for log in \$unlabelled_logs; do
            new_log="/var/lib/docker_custom_logs/\$(basename \$log .log).log.\$timestamp"
            echo "Renaming \$log --> \$new_log"
            mv "\$log" "\$new_log"
        done
        echo \$timestamp | tee /var/lib/docker_custom_logs/timestamp
    else
        echo "Timestamp \$timestamp file already exists with the same value."
    fi

    ## cleanup old logs
    old_logs=\$(find /var/lib/docker_custom_logs -type f -name "*.log.*")
    for log in \$old_logs; do
        echo "Removing \$log"
        rm -f \$log
    done

    containers=\$(docker ps --format='{{.Names}}')
    for container in \$containers; do
        docker logs \$container 2>&1 | split -b 1M -d - /var/lib/docker_custom_logs/\${container}_
        # docker logs \$container 2>&1 | tee /var/lib/docker_custom_logs/\$container.log > /dev/null
        if [ \$? -ne 0 ]; then
            echo "Failed to extract logs for container: \$container"
        else
            echo "Logs extracted for container: \$container"
        fi
    done

    ## Rename logs without extension
    logs_no_ext=\$(find /var/lib/docker_custom_logs -type f -regex ".*_[0-9]+")
    for log in \$logs_no_ext; do
        # echo "Renaming \$log to \$log.log"
        mv "\$log" "\$log.log"
    done
    sleep 30
    EOF

    sudo chmod +x /usr/local/bin/extract_docker_logs.sh

    sudo tee /etc/systemd/system/docker-logs-extractor.service > /dev/null <<EOF
    [Unit]
    Description="Extract Docker Logs"
    After=docker.service

    [Service]
    Type=simple
    ExecStart=/usr/local/bin/extract_docker_logs.sh
    Restart=always

    [Install]
    WantedBy=multi-user.target
    EOF

    sudo systemctl daemon-reload
    sudo systemctl enable docker-logs-extractor.service
    sudo systemctl start docker-logs-extractor.service
    sudo systemctl status docker-logs-extractor.service -l --no-pager
    sudo journalctl -u docker-logs-extractor -f
    ```

1. [Grafana Alloy Install - Collects all telemetry](https://grafana.com/docs/alloy/latest/)

    ```sh
    # Install Grafana Alloy
    sudo apt install gpg
    sudo mkdir -p /etc/apt/keyrings/
    wget -q -O - https://apt.grafana.com/gpg.key | gpg --dearmor | sudo tee /etc/apt/keyrings/grafana.gpg > /dev/null
    echo "deb [signed-by=/etc/apt/keyrings/grafana.gpg] https://apt.grafana.com stable main" | sudo tee /etc/apt/sources.list.d/grafana.list
    sudo apt-get update
    sudo apt-get install alloy

    sudo chown -R alloy:alloy /var/lib/alloy/data
    sudo chmod -R 775 /var/lib/alloy/data

    sudo tee /etc/alloy/config.alloy <<EOF
    local.file_match "logs_match" {
    path_targets = [
            {
                __address__      = "localhost",
                __path__         = "/var/lib/docker_custom_logs/*inventory*.log",
                environment      = "production",
                host             = "$HOSTNAME",
                job              = "inventory",
            },
        ]
    }

    loki.source.file "logs_source" {
        targets               = local.file_match.logs_match.targets
        forward_to            = [loki.process.container_logs.receiver]
        legacy_positions_file = "/var/lib/promtail/promtail-positions.yaml"
    }

    loki.process "container_logs" {
        forward_to = [loki.write.default.receiver]

        stage.json {
            expressions = {
            ActionId   = "Scopes[3].ActionId",
            ActionName = "Scopes[3].ActionName",
            Category   = "",
            EventId    = "",
            LogLevel   = "",
            Message    = "",
            SpanId     = "Scopes[0].SpanId",
            TraceId    = "Scopes[0].TraceId",
            }
        }

        stage.timestamp {
            source            = "Timestamp"
            format            = "RFC3339Nano"
            location          = "UTC"
            action_on_failure = "fudge"
        }

        stage.labels {
            values = {
            ActionId   = null,
            ActionName = null,
            Category   = null,
            EventId    = null,
            LogLevel   = null,
            SpanId     = null,
            TraceId    = null,
            }
        }

        stage.output {
            source = "Message"
        }
    }

    loki.write "default" {
        endpoint {
            url = "http://<loki_instance_ip>:3100/loki/api/v1/push"
            name = "loki.write.default.receiver"
            batch_wait = "5s"
            batch_size = "1MiB"
            min_backoff_period = "500ms"
            max_backoff_retries = 10
            retry_on_http_429 = true
            max_backoff_period = "5s"
        }
        external_labels = {}
    }
    EOF

    sudo systemctl daemon-reload
    sudo systemctl start alloy
    sudo systemctl enable alloy
    sudo systemctl status alloy -l --no-pager
    sudo journalctl -u alloy -f
    ```

1. [Pyroscope install - for real time app profiling](https://grafana.com/docs/pyroscope/latest/get-started/)

    ```sh
    # Install Pyroscope
    docker pull grafana/pyroscope:latest
    docker network create pyroscope
    docker run --rm --name pyroscope --network=pyroscope -p 4040:4040 grafana/pyroscope:latest
    ```
