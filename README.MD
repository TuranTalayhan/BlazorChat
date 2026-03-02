# BlazorChat: Developer Documentation

This repository contains a real-time chat application built with Blazor Web App (Auto/Server), Entity Framework Core, and SignalR.
## Prerequisites

Before managing the database, you must have the Entity Framework Core Tools installed on your machine. Run this command in your terminal:
```bash
dotnet tool install --global dotnet-ef
```
## Database Workflow (EF Core)

We use Migrations to keep the database schema in sync across different development environments.

### 1. Initial Setup

If you have just cloned the repository and need to create your local database file (blazorchat.db):
```bash
# Navigate to the Server project
cd BlazorChat.Server

# Apply all migrations to create your local database
dotnet ef database update
```

### 2. Changing the Schema

Whenever you modify an Entity (e.g., adding a new property to User.cs or creating a Message.cs), follow these steps:
```bash
# 1. Generate the migration files (run from Solution root)
dotnet ef migrations add <DescriptionOfChange> --project BlazorChat.Server

# 2. Apply the change to your local database file
dotnet ef database update --project BlazorChat.Server
```

### 3. Reverting Changes

If you created a migration but haven't pushed it to Git yet and want to delete the files and revert the database snapshot:
```bash
dotnet ef migrations remove --project BlazorChat.Server
```
