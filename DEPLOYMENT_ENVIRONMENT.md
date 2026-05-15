# Deployment Environment Variables

Set these values in Azure Container Apps for the deployed services. Do not commit real secrets in `appsettings.json`.

## All Authenticated Services

Use the same JWT values in Auth, Application, Job, Profile, and Notification services:

```text
Jwt__Key=<strong-32-plus-character-signing-key>
Jwt__Issuer=JobPortalApp
Jwt__Audience=JobPortalUsers
AllowedOrigins=<frontend-origin-or-*>

# Admin Configuration (AuthService Only)
DefaultAdmin__Email=<your-admin-email>
DefaultAdmin__Password=<your-admin-password>
DefaultAdmin__FullName=<your-admin-name>
```

## Service-Specific Values

```text
ConnectionStrings__DefaultConnection=<service-database-connection-string>
ServiceBus__ConnectionString=<azure-service-bus-connection-string>
Google__ClientId=<google-oauth-client-id>
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__Username=<smtp-username>
EmailSettings__Password=<smtp-app-password>
EmailSettings__FromEmail=<sender-email>
AzureStorage__ConnectionString=<storage-connection-string>
AzureStorage__ContainerName=resumes
```

The local Docker setup uses RabbitMQ for development. Azure deployment uses `ServiceBus__ConnectionString`.
