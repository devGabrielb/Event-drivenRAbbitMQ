FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UserService/UserService.csproj", "UserService/"]
RUN dotnet restore "./UserService/UserService.csproj"
WORKDIR "/src/UserService"
COPY . .
RUN dotnet build "UserService/UserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService/UserService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "UserService.dll" ]