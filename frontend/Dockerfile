# Etapa 1: Build con Node
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
COPY .npmrc ./
RUN npm ci --legacy-peer-deps
COPY . .
RUN npm run build

# Etapa 2: Servir con Nginx
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]