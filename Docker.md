# Dockerize the components

- Docker compose contains following components
  - API (Native published, linux x-64, internal port 80, no need to expose external port)
  - Web (React served with NGINX, all /api routed to API server, internal port 80, external port 3000)
  - SQL Server (After starting the server it should publish the database project with building dacpac via dotnet build only)
    - Also should execute seed data stored procedure
- All the connection string should in in docker compose file
