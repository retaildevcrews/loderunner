### Build and Test the App
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

#checkov:skip=CKV_DOCKER_2: No healthcheck is needed

### copy the source and tests
COPY ./LodeRunner.sln /src/
COPY ./LodeRunner /src/LodeRunner
COPY ./LodeRunner.API.sln /src/
COPY ./LodeRunner.API /src/LodeRunner.API
COPY ./LodeRunner.Data /src/LodeRunner.Data
COPY ./LodeRunner.Core /src/LodeRunner.Core
COPY ./LodeRunner.API.Test /src/LodeRunner.API.Test
COPY ./AppSettings /src/AppSettings

WORKDIR /src

# build the app and test
RUN dotnet test ./LodeRunner.API.Test/ --collect:"XPlat Code Coverage" --logger "console;verbosity=minimal" --filter "Category=Unit" && \
    dotnet publish LodeRunner.API.sln -c Release -o /app

###########################################################

### Build the runtime container
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS release

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

ENTRYPOINT [ "dotnet",  "LodeRunner.API.dll" ]
