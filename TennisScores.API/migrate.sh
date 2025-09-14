#!/bin/bash
# migrate.sh

# Variables to connect to database
# Ensure you have a .env file with the following variables defined:
# DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD
export $(grep -v '^#' .env | xargs)

# Start migration
dotnet ef database update --project ../TennisScores.Infrastructure --startup-project ../TennisScores.API
