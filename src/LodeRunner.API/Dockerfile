### Build and Test the App
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

### copy the source and tests
COPY . /src

WORKDIR /src

# build the app
RUN dotnet publish -c Release -o /app

###########################################################

### Build the runtime container
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS release

### if port is changed, also update value in Config
EXPOSE 8080
WORKDIR /app

### create a user
### dotnet needs a home directory
RUN addgroup -S relayrunner && \
    adduser -S relayrunner -G relayrunner && \
    mkdir -p /home/relayrunner && \
    chown -R relayrunner:relayrunner /home/relayrunner

### run as relayrunner user
USER relayrunner

### copy the app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "aspnetapp.dll" ]