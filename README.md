# PrinterApp 🖨️

Aplicação console .NET multithread simulando um sistema de fila de impressão com múltiplos produtores e um consumidor (`Printer`). O sistema é configurável via `appsettings.json` ou variáveis de ambiente, e suporta execução local e em container Docker.

Seguindo essa premissa, o projeto foi desenvolvido para ser totalmente configurável. As configurações estão distribuídas de forma apropriada conforme o ambiente de execução:

No Docker, as variáveis estão definidas no Dockerfile via blocos ENV;

Para execução local, os valores estão em appsettings.json;

Para execução via Visual Studio com Docker, as variáveis estão em launchSettings.json.

Implementei testes de unidade e integração (estes últimos se aproximando mais de testes de aceitação), com possibilidade de capturar e validar a saída do console dentro dos testes.

Do ponto de vista arquitetural, gostaria de ter adotado mais o Result Pattern em vez do uso direto de exceções. Isso deixaria o código mais legível e evitarias blocos try/catch, que impactam a performance e complexidade de leitura.

Simulei a leitura de configurações via variáveis de ambiente como se fossem chaves vindas de cofres, semelhante ao uso de Application Settings no Azure.

Incluí comandos para execução com Docker e Docker Compose, e escrevi os testes utilizando a biblioteca Shouldly (em vez do FluentAssertions, que passará a ser pago). Se necessário, posso reescrevê-los com MSTest ou outra framework.

Também considerei estruturar o projeto em estilo Vertical Slice Architecture, mas dado o escopo limitado de funcionalidades, optei por manter a estrutura mais enxuta e direta.

Meu objetivo foi entregar uma solução compacta, funcional, extensível e fácil de testar.

---

## 🛠️ Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Windows Terminal ou PowerShell (UTF-8 compatível)

---

## 🚀 Executando localmente (Por Favor, entrar na pasta que está o projeto)

```bash
docker compose run printer
```

Você verá:

```
Digite ENTER para parar a execução.
```

Pressione ENTER quando desejar parar os producers e o printer. Ele vai tentar encerrar todos os Producers e pode demorar um pouco para ter uma resposta já que depende das ENV que estão configuradas no docker file.

---

## 🐳 Rodando com Docker

### ✅ Build da imagem (Por Favor, entrar na pasta que está o projeto)

```bash
docker build -t printer-app -f Dockerfile.UTF8 .
```

### ▶️ Executar interativamente (Por Favor, entrar na pasta que está o projeto)

```bash
docker run -it printer-app
```

Isso mantém o console aberto para que você possa pressionar ENTER e encerrar o sistema normalmente.

---

## 🐳 Usando Docker Compose (Por Favor, entrar na pasta que está o projeto)

### Arquivos necessários:

- `docker-compose.yml`
- `docker-compose.override.yml`

### ✅ Subir com console interativo

```bash
docker compose run printer
```

> ⚠️ Use `run` (e não `up`) para manter o terminal interativo.

---

## ⚙️ Configurações disponíveis via ENV

| Variável                                     | Exemplo |
|----------------------------------------------|---------|
| `AppConfiguration__QueueCapacity`            | 1000    |
| `AppConfiguration__NumberOfProducers`        | 4       |
| `AppConfiguration__MillisecondsPerPage`      | 250     |
| `AppConfiguration__RandomizerSettings__MinJobCount` | 200 |
| `AppConfiguration__RandomizerSettings__MaxJobCount` | 400 |
| `AppConfiguration__RandomizerSettings__MinPageCount` | 300 |
| `AppConfiguration__RandomizerSettings__MaxPageCount` | 2000 |
| `AppConfiguration__RandomizerSettings__MinDelay` | 500 |
| `AppConfiguration__RandomizerSettings__MaxDelay` | 1000 |

Você pode sobrescrever essas variáveis com `-e` no `docker run`, ou no `docker-compose.override.yml`.

Embora não estivesse na premissa do projeto, tornar o sistema configurável pareceu uma evolução natural. Por isso, implementei essa capacidade no projeto para avaliar seu comportamento em diferentess cenários.

---

## 📦 Build clean + execução

```bash
docker compose down --remove-orphans
docker compose build --no-cache
docker compose run printer
```

---

## 📄 Licença

Este projeto é apenas para fins de teste técnico e demonstração.