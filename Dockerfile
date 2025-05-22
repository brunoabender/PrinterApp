# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Força UTF-8 no runtime
ENV LANG C.UTF-8
ENV LC_ALL C.UTF-8

COPY --from=build /app/publish .

# Variáveis de ambiente padrão
ENV AppConfiguration__QueueCapacity=10 \
    AppConfiguration__NumberOfProducers=2 \
    AppConfiguration__MillisecondsPerPage=50 \
    AppConfiguration__JobGenerationSettings__MinJobCount=200 \
    AppConfiguration__JobGenerationSettings__MaxJobCount=400 \
    AppConfiguration__JobGenerationSettings__MinPageCount=300 \
    AppConfiguration__JobGenerationSettings__MaxPageCount=2000 \
    AppConfiguration__JobGenerationSettings__MinDelay=500 \
    AppConfiguration__JobGenerationSettings__MaxDelay=1000

ENTRYPOINT ["dotnet", "PrinterApp.dll"]