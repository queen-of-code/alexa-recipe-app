FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

FROM base AS final
WORKDIR /app
COPY out/ .
ENTRYPOINT ["dotnet", "RecipeAPI.dll"]
