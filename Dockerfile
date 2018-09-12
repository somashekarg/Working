FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY OneDirect.sln ./
COPY OneDirect/OneDirect.csproj OneDirect/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/OneDirect
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "OneDirect.dll"]