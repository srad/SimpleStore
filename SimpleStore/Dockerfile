FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG TARGETARCH

WORKDIR /src

COPY ["SimpleStore/SimpleStore.csproj", "SimpleStore/"]
COPY ["SimpleStore.ServiceDefaults/SimpleStore.ServiceDefaults.csproj", "SimpleStore.ServiceDefaults/"]

RUN dotnet restore "SimpleStore/SimpleStore.csproj" -a $TARGETARCH
RUN dotnet restore "SimpleStore.ServiceDefaults/SimpleStore.ServiceDefaults.csproj" -a $TARGETARCH

WORKDIR "/src/SimpleStore"

COPY ["SimpleStore/", "SimpleStore/"]
COPY ["SimpleStore.ServiceDefaults/", "SimpleStore.ServiceDefaults/"]

RUN dotnet build "SimpleStore/SimpleStore.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "SimpleStore/SimpleStore.csproj" -c Release -o /app/publish /p:UseAppHost=false -a $TARGETARCH

FROM base AS final
WORKDIR /app

ENV STORAGE_DIRECTORY=/data/storage
ENV DB_PATH=/data/data.db
RUN mkdir -p $STORAGE_DIRECTORY

COPY --from=publish /app/publish .

ENV REALM=""

ENV OPENID_AUTHORITY=""
ENV OPENID_META_ADDRESS=""
ENV Realm=$REALM

ENV JwtSettings__Authority=$OPENID_AUTHORITY
ENV JwtSettings__MetadataAddress=$OPENID_META_ADDRESS

ENTRYPOINT ["dotnet", "SimpleStore.dll"]