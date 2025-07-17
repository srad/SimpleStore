FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG TARGETARCH

WORKDIR /src

COPY ["SimpleStore.Admin/SimpleStore.Admin.csproj", "SimpleStore.Admin/"]
COPY ["SimpleStore.ServiceDefaults/SimpleStore.ServiceDefaults.csproj", "SimpleStore.ServiceDefaults/"]

RUN dotnet restore "SimpleStore.Admin/SimpleStore.Admin.csproj" -a $TARGETARCH
RUN dotnet restore "SimpleStore.ServiceDefaults/SimpleStore.ServiceDefaults.csproj" -a $TARGETARCH

WORKDIR "/src/SimpleStore.Admin"

COPY ["SimpleStore.Admin/", "SimpleStore.Admin/"]
COPY ["SimpleStore.ServiceDefaults/", "SimpleStore.ServiceDefaults/"]

RUN apt update
RUN apt install nodejs npm -y

WORKDIR "/src/SimpleStore.Admin/SimpleStore.Admin/wwwroot"
RUN npm install

WORKDIR "/src/SimpleStore.Admin"
RUN dotnet build "SimpleStore.Admin/SimpleStore.Admin.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "SimpleStore.Admin/SimpleStore.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false -a $TARGETARCH

FROM base AS final
WORKDIR /app

ENV API__Endpoint=""
ENV API__Key=""
ENV OpenId__Authority=""
ENV OpenId__ClientId=""
ENV OpenId__ClientSecret=""

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleStore.Admin.dll"]