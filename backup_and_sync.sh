#!/bin/bash

# ���� � ����� � ��������
BACKUP_DIR="/home/prod/backup/sql"

# ���� � docker-compose
DOCKER_COMPOSE_DIR="/home/OMarket"

# ��������� � ���������� � docker-compose
cd $DOCKER_COMPOSE_DIR

# ��������� ��������� ����������� ���� ������
docker-compose run --rm db_backup

# ������������� ����������� ������ � Google Drive
rclone sync /home/prod/appstorage/static/files GoogleDrive:/OMarketBot/static --delete-excluded

# ������������� ������� ���� ������ � Google Drive
rclone sync $BACKUP_DIR GoogleDrive:/OMarketBot/postgresql --delete-excluded

# ������� ������ ������ (���� �����)
find $BACKUP_DIR -type f -mtime +7 -name "*.sql" -exec rm {} \;