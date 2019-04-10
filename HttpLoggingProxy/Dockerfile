FROM microsoft/dotnet:2.2-sdk
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./

RUN dotnet restore -s http://nuget.srv.symbotic.corp/nuget/Common -s https://api.nuget.org/v3/index.json

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/HttpLoggingProxy.dll"]