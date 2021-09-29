### Build and Test the App
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

### copy the source and tests
COPY ./LodeRunner.API.sln /src/
COPY ./LodeRunner.API /src/LodeRunner.API
COPY ./LodeRunner.Data /src/LodeRunner.Data
COPY ./LodeRunner.Core /src/LodeRunner.Core

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
RUN addgroup -S ngsa && \
    adduser -S ngsa -G ngsa && \
    mkdir -p /home/ngsa && \
    chown -R ngsa:ngsa /home/ngsa

### run as ngsa user
USER ngsa

### copy the app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "aspnetapp.dll" ]