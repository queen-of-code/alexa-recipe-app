FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM base AS final
WORKDIR /app
COPY out/ .
ENTRYPOINT ["dotnet", "RecipeAPI.dll"]