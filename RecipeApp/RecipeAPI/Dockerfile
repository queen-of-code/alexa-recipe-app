FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RecipeAPI/RecipeAPI.csproj", "RecipeAPI/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "RecipeAPI/RecipeAPI.csproj"

COPY . .
WORKDIR "/src/RecipeAPI"
RUN dotnet publish "RecipeAPI.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RecipeAPI.dll"]
