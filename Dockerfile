FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "EthernaIndex.sln"
RUN dotnet build "EthernaIndex.sln" -c Release -o /app/build
RUN dotnet test "EthernaIndex.sln" -c Release

FROM build AS publish
RUN dotnet publish "EthernaIndex.sln" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EthernaIndex.dll"]