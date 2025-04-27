# Keycloak Authentication Setup

This guide explains how to set up Keycloak for authentication with the ZenFlow frontend application.

## Setting up Keycloak

1. **Start Keycloak Server**

   ```bash
   # Using Docker
   docker run -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:latest start-dev
   ```

2. **Access Keycloak Admin Console**

   - Go to: http://localhost:8080/admin
   - Login with admin / admin (or your credentials)

3. **Create a New Realm**

   - Click on "master" dropdown in the top-left corner
   - Select "Create Realm"
   - Name it "zenflow" and click "Create"

4. **Create a Client**

   - Navigate to "Clients" in the left menu
   - Click "Create client"
   - Fill in:
     - Client ID: `zenflow-frontend`
     - Name: `ZenFlow Frontend`
     - Client Authentication: ON (confidential access type)
     - Click "Next"
   - In Authentication flow:
     - Standard flow: ON (for authorization code flow)
     - Direct access grants: ON (optional)
     - Click "Next" and then "Save"

5. **Configure Client Settings**

   - Go to the "Settings" tab of your client
   - Set Valid redirect URIs:
     - `http://localhost:3000/*` (your Next.js app URL)
   - Set Web Origins:
     - `http://localhost:3000` (for CORS)
   - Click "Save"

6. **Get Client Secret**

   - Go to the "Credentials" tab
   - Copy the "Client secret" - you'll need this for your application

7. **Create a Test User** (Optional)

   - Go to "Users" in the left menu
   - Click "Add user"
   - Fill in the user details and click "Create"
   - Go to the "Credentials" tab for the user
   - Set a password and turn OFF "Temporary" if you don't want to change it on first login

## Configure Frontend Application

1. **Set Environment Variables**

   Create a `.env.local` file in the root of the frontend project with:

   ```env
   # Next-Auth
   NEXTAUTH_URL=http://localhost:3000
   NEXTAUTH_SECRET=your-generated-secret-here  # Generate with: openssl rand -base64 32
   
   # Keycloak
   KEYCLOAK_URL=http://localhost:8080
   KEYCLOAK_REALM=zenflow
   KEYCLOAK_CLIENT_ID=zenflow-frontend
   KEYCLOAK_CLIENT_SECRET=your-client-secret-from-keycloak
   
   # API
   NEXT_PUBLIC_API_URL=http://localhost:8080/api
   ```

2. **Start the Application**

   ```bash
   npm run dev
   ```

## Troubleshooting

- **"Code not valid" error**: This usually means your redirect URI doesn't match what's configured in Keycloak or your client secret is incorrect.
- **CORS errors**: Make sure your Web Origins in Keycloak client settings include your frontend URL.
- **JWT verification errors**: Check that your realm and client IDs match in both Keycloak and your app config.

## Security Considerations

- Always use HTTPS in production
- Keep your client secret safe (in environment variables, not in code)
- Consider configuring token lifetimes appropriately in Keycloak
- Use proper role-based access control for sensitive operations 