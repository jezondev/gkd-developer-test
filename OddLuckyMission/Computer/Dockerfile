#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FalconOnboardComputer/FalconOnboardComputer.csproj", "FalconOnboardComputer/"]
RUN dotnet restore "FalconOnboardComputer/FalconOnboardComputer.csproj"
COPY . .
WORKDIR "/src/FalconOnboardComputer"
RUN dotnet build "FalconOnboardComputer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FalconOnboardComputer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FalconOnboardComputer.dll"]