@echo off

if not exist ".\Resources\bin" (
    md .\Resources\bin
)

if not exist ".\Resources\bin\swagger-codegen-cli.jar" (
    echo "Downloading swagger-codegen-cli ..."
    curl.exe  --output .\Resources\bin\swagger-codegen-cli.jar -L https://repo1.maven.org/maven2/io/swagger/codegen/v3/swagger-codegen-cli/3.0.40/swagger-codegen-cli-3.0.40.jar
)

java -jar .\Resources\bin\swagger-codegen-cli.jar generate -i https://localhost:44312/swagger/v1/swagger.json -D io.swagger.parser.util.RemoteUrl.trustAll=true -D io.swagger.v3.parser.util.RemoteUrl.trustAll=true -l html2 -o .\SimpleStore\Docs\v1 -c .\Resources\swagger-codegen.json --remove-operation-id-prefix getId

exit