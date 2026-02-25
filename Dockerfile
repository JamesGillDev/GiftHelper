FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GiftHelper.sln ./
COPY src/GiftHelper.Web/GiftHelper.Web.csproj src/GiftHelper.Web/
COPY src/GiftHelper.Data/GiftHelper.Data.csproj src/GiftHelper.Data/
COPY src/GiftHelper.Domain/GiftHelper.Domain.csproj src/GiftHelper.Domain/

RUN dotnet restore GiftHelper.sln

COPY . .
RUN dotnet publish src/GiftHelper.Web/GiftHelper.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet GiftHelper.Web.dll --urls http://0.0.0.0:${PORT:-10000}"]
