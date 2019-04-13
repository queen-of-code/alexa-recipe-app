FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

#FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
#WORKDIR /src
#COPY ["Website.csproj", "Website/"]
#COPY ["../LambdaAPI/Core/Core.csproj", "LambdaAPI/Core/"]
#RUN dotnet restore "Website/Website.csproj"
#COPY . .
#WORKDIR "/src/Website"
#RUN dotnet build "Website.csproj" -c Release -o /app

#FROM build AS publish
#RUN dotnet publish "Website.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY out/ .
ENTRYPOINT ["dotnet", "Website.dll"]