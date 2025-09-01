FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ReadYourWritesConsistency.Database/ReadYourWritesConsistency.Database.sqlproj", "ReadYourWritesConsistency.Database/"]
COPY ReadYourWritesConsistency.Database/ ReadYourWritesConsistency.Database/
WORKDIR /src/ReadYourWritesConsistency.Database
RUN dotnet build -c Release

FROM mcr.microsoft.com/mssql-tools:latest
WORKDIR /usr/src/sqltools
RUN apt-get update && apt-get install -y wget unzip libicu-dev libssl-dev curl gnupg \
    && wget https://aka.ms/sqlpackage-linux -O sqlpackage.zip \
    && unzip sqlpackage.zip -d /opt/sqlpackage \
    && chmod +x /opt/sqlpackage/sqlpackage \
    && ln -s /opt/sqlpackage/sqlpackage /usr/local/bin/sqlpackage \
    && apt-get clean && rm -rf /var/lib/apt/lists/*
COPY --from=build /src/ReadYourWritesConsistency.Database/bin/Release/*.dacpac .
COPY tools/db-deploy/deploy.sh .
COPY tools/db-deploy/scripts ./scripts
RUN chmod +x ./deploy.sh
CMD ["./deploy.sh"]