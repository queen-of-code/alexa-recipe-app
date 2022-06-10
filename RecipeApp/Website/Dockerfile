FROM mcr.microsoft.com/dotnet/core/aspnet:6.0.5-bullseye-slim-arm64v8 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

# Our artifacts are already coming to us in the build system in the /out folder.

FROM base AS final
WORKDIR /app
COPY out/ .
ENTRYPOINT ["dotnet", "Website.dll"]