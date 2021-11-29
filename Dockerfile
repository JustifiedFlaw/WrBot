FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy everything and build
COPY . .
RUN dotnet restore ./WrBot/WrBot.csproj
RUN dotnet publish -c Release -o out ./WrBot/WrBot.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
COPY ./WrBot/appsettings.json /app/

# Run the app on container startup
# Use your project name for the second parameter
# e.g. MyProject.dll
ENTRYPOINT [ "dotnet", "WrBot.dll" ]