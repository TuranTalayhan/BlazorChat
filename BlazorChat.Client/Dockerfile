FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy entire repository so project references (Shared) can be resolved
COPY . ./

RUN dotnet publish BlazorChat/BlazorChat.Client/BlazorChat.Client.csproj -c Release -o /app/publish

FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# SPA fallback
COPY BlazorChat.Client/nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
