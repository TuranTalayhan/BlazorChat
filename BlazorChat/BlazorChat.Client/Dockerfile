FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BlazorChat/BlazorChat.Client/BlazorChat.Client.csproj", "BlazorChat/BlazorChat.Client/"]
COPY ["BlazorChat/BlazorChat.Shared/BlazorChat.Shared.csproj", "BlazorChat/BlazorChat.Shared/"]
RUN dotnet restore "BlazorChat/BlazorChat.Client/BlazorChat.Client.csproj"

COPY . .
WORKDIR "/src/BlazorChat/BlazorChat.Client"
RUN dotnet publish "BlazorChat.Client.csproj" -c Release -o /app/dist

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

RUN rm -rf ./*
COPY --from=build /app/dist/wwwroot .
COPY BlazorChat/BlazorChat.Client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]