FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MinabetBotsWeb.csproj", "./"]
RUN dotnet restore "MinabetBotsWeb.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "MinabetBotsWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MinabetBotsWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinabetBotsWeb.dll"]
