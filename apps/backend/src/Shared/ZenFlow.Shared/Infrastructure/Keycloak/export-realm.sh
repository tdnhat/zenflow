#!/bin/bash

# Export the Keycloak realm configuration to a JSON file
# Usage: ./export-realm.sh

# Configuration
KEYCLOAK_CONTAINER="identity"
KEYCLOAK_ADMIN="admin"
KEYCLOAK_ADMIN_PASSWORD="admin"
REALM_NAME="zenflow"
OUTPUT_FILE="$(dirname "$0")/realm-export.json"

echo "Exporting Keycloak realm configuration..."

# Run the export command inside the Keycloak container
docker exec -it $KEYCLOAK_CONTAINER /opt/keycloak/bin/kc.sh \
    export \
    --dir /opt/keycloak/data/export \
    --realm $REALM_NAME \
    --users realm_file

# Copy the export file from the container to the host
docker cp $KEYCLOAK_CONTAINER:/opt/keycloak/data/export/$REALM_NAME-realm.json $OUTPUT_FILE

echo "Export completed: $OUTPUT_FILE" 