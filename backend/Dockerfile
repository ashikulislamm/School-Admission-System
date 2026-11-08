# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so this layer is cached until a csproj changes
COPY Directory.Build.props ./
COPY src/Admission.Api/Admission.Api.csproj src/Admission.Api/
RUN dotnet restore src/Admission.Api/Admission.Api.csproj

COPY src/ src/
RUN dotnet publish src/Admission.Api/Admission.Api.csproj -c Release -o /app --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# The app listens on $PORT if set (Render), otherwise 8080.
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "Admission.Api.dll"]
