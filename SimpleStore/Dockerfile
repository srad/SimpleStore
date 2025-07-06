FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG TARGETARCH

WORKDIR /src
COPY ["SimpleStore.csproj", "SimpleStore/"]
RUN dotnet restore "SimpleStore/SimpleStore.csproj" -a $TARGETARCH
WORKDIR "/src/SimpleStore"
COPY . .
RUN dotnet build "SimpleStore.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "SimpleStore.csproj" -c Release -o /app/publish /p:UseAppHost=false -a $TARGETARCH

FROM base AS final
WORKDIR /app

ENV STORAGE_DIRECTORY=/data/storage
ENV DB_PATH=/data/data.db
RUN mkdir -p $STORAGE_DIRECTORY
CMD /bin/sh -c "echo Storing file in: $STORAGE_DIRECTORY. Storing data in: $DB_PATH"

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleStore.dll"]