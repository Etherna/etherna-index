FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN curl -SLO https://deb.nodesource.com/nsolid_setup_deb.sh
RUN chmod 500 nsolid_setup_deb.sh
RUN ./nsolid_setup_deb.sh 20
RUN apt-get install -y nodejs
WORKDIR /src
COPY . .
RUN dotnet restore "EthernaIndex.sln"
RUN dotnet build "EthernaIndex.sln" -c Release -o /app/build
RUN dotnet test "EthernaIndex.sln" -c Release

FROM build AS publish
RUN dotnet publish "EthernaIndex.sln" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EthernaIndex.dll"]