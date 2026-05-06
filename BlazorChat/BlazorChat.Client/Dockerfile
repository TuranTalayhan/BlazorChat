FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Restore only (for caching)
COPY ["BlazorChat/BlazorChat.Client/BlazorChat.Client.csproj", "BlazorChat/BlazorChat.Client/"]
# Copy Shared project if referenced!
# COPY ["BlazorChat/BlazorChat.Shared/BlazorChat.Shared.csproj", "BlazorChat/BlazorChat.Shared/"]
RUN dotnet restore "BlazorChat/BlazorChat.Client/BlazorChat.Client.csproj"

# 2. Copy and Build
COPY . .
RUN dotnet publish "BlazorChat/BlazorChat.Client/BlazorChat.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY BlazorChat/BlazorChat.Client/nginx/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 80