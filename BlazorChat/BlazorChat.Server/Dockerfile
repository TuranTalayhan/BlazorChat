FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BlazorChat/BlazorChat.Server/BlazorChat.Server.csproj", "BlazorChat/BlazorChat.Server/"]
COPY ["BlazorChat/BlazorChat.Shared/BlazorChat.Shared.csproj", "BlazorChat/BlazorChat.Shared/"]
RUN dotnet restore "BlazorChat/BlazorChat.Server/BlazorChat.Server.csproj"

COPY . .
WORKDIR "/src/BlazorChat/BlazorChat.Server"
RUN dotnet publish "BlazorChat.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Changed from 8.0 to 10.0
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorChat.Server.dll"]