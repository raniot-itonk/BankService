FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY RanIoTCA.crt /usr/local/share/ca-certificates/RanIoTCA.crt
RUN update-ca-certificates

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["BankService/BankService.csproj", "BankService/"]
RUN dotnet restore "BankService/BankService.csproj"
COPY . .
WORKDIR "/src/BankService"
RUN dotnet build "BankService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BankService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BankService.dll"]