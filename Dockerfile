FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs
WORKDIR /src
COPY . .
RUN dotnet restore "EthernaIndex.sln"
RUN dotnet build "EthernaIndex.sln" -c Release -o /app/build
RUN dotnet test "EthernaIndex.sln" -c Release

FROM build AS publish
RUN dotnet publish "EthernaIndex.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EthernaIndex.dll"]