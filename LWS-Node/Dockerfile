FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LWS-Node/LWS-Node.csproj", "LWS-Node/"]
RUN dotnet restore "LWS-Node/LWS-Node.csproj"
COPY . .
WORKDIR "/src/LWS-Node"
RUN dotnet build "LWS-Node.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LWS-Node.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LWS-Node.dll"]
