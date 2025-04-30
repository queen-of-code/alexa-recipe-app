FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM base AS final
WORKDIR /app
COPY out/ .
ENTRYPOINT ["dotnet", "RecipeAPI.dll"]