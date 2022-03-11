### build prod app
FROM mcr.microsoft.com/vscode/devcontainers/javascript-node:14 AS build
ENV NODE_ENV production
WORKDIR /app

# copy project meta data
COPY ["package.json", "package-lock.json", "./"]

# install dependencies
RUN npm ci --only=production

# copy app 
COPY . ./ 

# build release app
RUN npm run build-prod

###########################################################

### build prod nginx container
FROM nginx:stable-alpine AS nginx-prod
LABEL nginx=prod

COPY --from=build /app/build /usr/share/nginx/html
COPY ./start-nginx.sh /
COPY ./nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

STOPSIGNAL SIGQUIT

ENTRYPOINT ["/start-nginx.sh"]
