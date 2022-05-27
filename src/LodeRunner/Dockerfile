### build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

# Copy the source
COPY ./LodeRunner.sln /src/
COPY ./LodeRunner /src/LodeRunner
COPY ./LodeRunner.Data /src/LodeRunner.Data
COPY ./LodeRunner.Core /src/LodeRunner.Core
COPY ./LodeRunner.Test /src/LodeRunner.Test
COPY ./AppSettings /src/AppSettings

### Build the release app
WORKDIR /src
RUN dotnet test ./LodeRunner.Test/ --collect:"XPlat Code Coverage" --logger "console;verbosity=minimal" --filter "Category=Unit" && \
    dotnet publish -c Release -o /app

    
###########################################################


### build the runtime container
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime

### create a user
### dotnet needs a home directory
RUN addgroup -S ngsa && \
    adduser -S ngsa -G ngsa && \
    mkdir -p /home/ngsa && \
    chown -R ngsa:ngsa /home/ngsa

WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /app/TestFiles && \
    cp *.json TestFiles && \
    cp perfTargets.txt TestFiles && \
    chown -R ngsa:ngsa /app

WORKDIR /app/TestFiles
EXPOSE 8080

# run as the ngsa user
USER ngsa

ENTRYPOINT [ "dotnet",  "../LodeRunner.dll" ]
