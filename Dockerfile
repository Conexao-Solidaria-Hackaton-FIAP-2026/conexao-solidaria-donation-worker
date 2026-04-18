FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY src/DonationWorker/*.csproj ./src/DonationWorker/
RUN dotnet restore src/DonationWorker/DonationWorker.csproj
COPY . .
RUN dotnet publish src/DonationWorker/DonationWorker.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
EXPOSE 9091
ENTRYPOINT ["dotnet", "DonationWorker.dll"]