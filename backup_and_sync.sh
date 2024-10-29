#!/bin/bash

# Путь к папке с бэкапами
BACKUP_DIR="/home/prod/backup/sql"

# Путь к docker-compose
DOCKER_COMPOSE_DIR="/home/OMarket"

# Перейдите в директорию с docker-compose
cd $DOCKER_COMPOSE_DIR

# Запустите резервное копирование базы данных
docker-compose run --rm db_backup

# Синхронизация статических файлов с Google Drive
rclone sync /home/prod/appstorage/static/files GoogleDrive:/OMarketBot/static --delete-excluded

# Синхронизация бэкапов базы данных с Google Drive
rclone sync $BACKUP_DIR GoogleDrive:/OMarketBot/postgresql --delete-excluded

# Удаляем старые бэкапы (если нужно)
find $BACKUP_DIR -type f -mtime +7 -name "*.sql" -exec rm {} \;