# Keycloak Integration

This directory contains configuration files for Keycloak, an open-source Identity and Access Management solution used by ZenFlow.

## Files

- **realm-export.json**: Contains the realm configuration that gets imported when Keycloak starts
- **export-realm.sh**: Script to export your current Keycloak realm configuration

## Database Configuration

ZenFlow uses a dedicated Neon Tech PostgreSQL database for Keycloak:
- Database: zenflow-keycloak-db
- Host: ep-damp-shape-a14t1mjd-pooler.ap-southeast-1.aws.neon.tech
- Schema: public

This database is separate from the main application database to ensure proper separation of concerns and to follow security best practices.

## Usage

### Initial Setup

The Docker Compose configuration will automatically import the realm configuration from `realm-export.json` when Keycloak starts for the first time. The Keycloak server will connect to the dedicated PostgreSQL database specified in the configuration.

### Exporting Updated Configuration

After making changes to your Keycloak realm through the admin console, you can save those changes by running:

```bash
# Navigate to this directory
cd /path/to/ZenFlow.Shared/Infrastructure/Keycloak

# Make the script executable (if not already)
chmod +x export-realm.sh

# Run the export script
./export-realm.sh
```

This will export your current realm configuration and save it to `realm-export.json`.

## Architecture Benefits

Placing Keycloak configuration in ZenFlow.Shared provides:

1. **Modularity**: Keycloak configuration is treated as part of the shared infrastructure
2. **Reusability**: Multiple services can refer to the same Keycloak configuration
3. **Maintainability**: Centralizing IAM configuration makes it easier to update and manage
4. **Security**: Using a dedicated database for authentication concerns follows security best practices

## Configuration Notes

The realm configuration includes:
- Realm settings for "zenflow"
- Client configurations for API access
- Default roles (admin and user)
- Sample user accounts for testing 