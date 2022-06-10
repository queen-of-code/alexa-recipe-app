FROM mcr.microsoft.com/dotnet/core/aspnet:6.0.5-bullseye-slim-arm64v8 AS base
WORKDIR /app
EXPOSE 80

FROM base AS final
WORKDIR /app
COPY out/ .
ENTRYPOINT ["dotnet", "RecipeAPI.dll"]