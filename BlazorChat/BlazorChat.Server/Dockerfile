FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy the .csproj files for BOTH projects to restore dependencies
# This is the secret sauce for Docker build caching
COPY ["BlazorChat/BlazorChat.Server/BlazorChat.Server.csproj", "BlazorChat.Server/"]
COPY ["BlazorChat/BlazorChat.Shared/BlazorChat.Shared.csproj", "BlazorChat.Shared/"]

# 2. Restore using the Server project (it will automatically restore Shared)
RUN dotnet restore "BlazorChat.Server/BlazorChat.Server.csproj"

# 3. Copy EVERYTHING else
COPY . .

# 4. Build from the Server directory
WORKDIR "/src/BlazorChat/BlazorChat.Server"
RUN dotnet publish "BlazorChat.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# 5. Copy the entrypoint script from your host into the runtime image
COPY BlazorChat/BlazorChat.Server/entrypoint.sh ./entrypoint.sh

# 6. Ensure the script has Linux execution permissions
RUN chmod +x ./entrypoint.sh

ENV ASPNETCORE_URLS=http://+:7138
EXPOSE 7138

# 7. Reference the script in the current WORKDIR (/app)
ENTRYPOINT ["./entrypoint.sh"]