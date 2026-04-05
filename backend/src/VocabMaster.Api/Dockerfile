FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY VocabMaster.sln .
COPY src/VocabMaster.Api/VocabMaster.Api.csproj src/VocabMaster.Api/
COPY src/VocabMaster.Domain/VocabMaster.Domain.csproj src/VocabMaster.Domain/
COPY src/VocabMaster.Application/VocabMaster.Application.csproj src/VocabMaster.Application/
COPY src/VocabMaster.Infrastructure/VocabMaster.Infrastructure.csproj src/VocabMaster.Infrastructure/

RUN dotnet restore src/VocabMaster.Api/VocabMaster.Api.csproj

COPY . .
RUN dotnet publish src/VocabMaster.Api/VocabMaster.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VocabMaster.Api.dll"]
