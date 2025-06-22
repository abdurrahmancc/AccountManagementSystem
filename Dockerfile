# ------------------------------
# STAGE 1: Build Environment
# ------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["AccountManagementSystem.csproj", "./"]
RUN dotnet restore "AccountManagementSystem.csproj"

# Copy everything and build
COPY . .
RUN dotnet build "AccountManagementSystem.csproj" -c Release -o /app/build

# ------------------------------
# STAGE 2: Publish the app
# ------------------------------
FROM build AS publish
RUN dotnet publish "AccountManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ------------------------------
# STAGE 3: Final runtime image
# ------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Optional: You can define environment variables here if needed
# ENV ASPNETCORE_ENVIRONMENT=Production

# Expose default ports
EXPOSE 80
EXPOSE 443

# Copy published app from previous stage
COPY --from=publish /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "AccountManagementSystem.dll"]
