FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["myflow/myflow.csproj", "myflow/"]
RUN dotnet restore "myflow/myflow.csproj"
COPY . .
WORKDIR "/src/myflow"
RUN dotnet build "myflow.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "myflow.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "myflow.dll"]
