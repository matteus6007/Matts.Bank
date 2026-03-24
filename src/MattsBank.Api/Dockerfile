FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_VERSION=1.0.0
ARG SSL_CERT_DIR=/etc/ssl/certs/
ARG CERT_FILE_PATH
ARG CERT_FILE

# Add the CA Certificate to the container
ADD "*$CERT_FILE_PATH/$CERT_FILE" /tmp/
RUN if [ -f "/tmp/$CERT_FILE" ] ; then cp /tmp/$CERT_FILE $SSL_CERT_DIR ; fi
RUN update-ca-certificates

WORKDIR /workspace

# restore
COPY ["NuGet.Config", "src/"]
COPY ["src/MattsBank.Api/MattsBank.Api.csproj", "src/MattsBank.Api/"]
COPY ["src/MattsBank.Domain/MattsBank.Domain.csproj", "src/MattsBank.Domain/"]
COPY ["src/MattsBank.Infrastructure/MattsBank.Infrastructure.csproj", "src/MattsBank.Infrastructure/"]
RUN dotnet restore "src/MattsBank.Api/MattsBank.Api.csproj" --configfile "src/NuGet.Config"

# build
COPY . .
WORKDIR "/workspace/src/MattsBank.Api"
RUN dotnet build "MattsBank.Api.csproj" -c Release --no-restore -o /app/build /p:Version=$BUILD_VERSION

# publish
FROM build AS publish
ARG BUILD_VERSION

RUN dotnet publish "MattsBank.Api.csproj" -c Release --no-restore -o /app/publish /p:Version=$BUILD_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MattsBank.Api.dll"]
