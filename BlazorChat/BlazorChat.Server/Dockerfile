
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy entire repo so referenced projects (Shared, etc.) are available
COPY . ./

RUN dotnet publish BlazorChat/BlazorChat.Server/BlazorChat.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# copy entrypoint and make it executable
COPY BlazorChat/BlazorChat.Server/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:7138
EXPOSE 7138

ENTRYPOINT ["/entrypoint.sh"]
