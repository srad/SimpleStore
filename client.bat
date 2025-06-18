@echo off

if not exist ".\Resources\bin" (
    md .\Resources\bin
)

if not exist ".\Resources\bin\nswag" (
    echo "Downloading nswag ..."
    curl.exe  --output .\Resources\bin\nswag.zip -L https://github.com/RicoSuter/NSwag/releases/download/v14.4.0/NSwag.zip
    md .\Resources\bin\nswag
    tar -xf .\Resources\bin\nswag.zip -C .\Resources\bin\nswag
    del .\Resources\bin\nswag.zip
)

.\Resources\bin\nswag\Net80\dotnet-nswag.exe openapi2csclient /input:https://localhost:44312/swagger/v1/swagger.json /GenerateNativeRecords:true /classname:SimpleStoreClient /namespace:SimpleStore.Admin.Services.v1 /output:.\SimpleStore.Admin\Services\v1\StorageClient.cs /ArrayType:System.Collections.Generic.List /ArrayInstanceType:System.Collections.Generic.List /JsonLibrary:SystemTextJson

exit
