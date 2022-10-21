FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["PostService/PostService.csproj", "PostService/"]
RUN dotnet restore "./PostService/PostService.csproj"
WORKDIR "/src/PostService"
COPY . .
RUN dotnet build "PostService/PostService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PostService/PostService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "PostService.dll" ]