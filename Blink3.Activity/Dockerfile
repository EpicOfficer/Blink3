FROM nginx:alpine AS base
EXPOSE 80

FROM node:21-alpine AS build
WORKDIR /app
COPY ["Blink3.Activity/package.json", "."]
RUN npm install
COPY ["Blink3.Activity", "."]
RUN npm run build

FROM base AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist .
COPY Blink3.Activity/nginx.conf /etc/nginx/nginx.conf