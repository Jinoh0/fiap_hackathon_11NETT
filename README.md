# Conexão Solidária

MVP da plataforma digital da ONG **Esperança Solidária** para gestão de campanhas e doações, construído para o Hackathon 11NETT.

Arquitetura: **microsserviços .NET 8**, **PostgreSQL**, **RabbitMQ**, **Kubernetes**, **Prometheus + Grafana** e **GitHub Actions**.

## Arquitetura em uma frase

A API autentica usuários, gerencia campanhas e **publica** doações; o Worker **consome** a fila e atualiza o valor arrecadado — a API nunca incrementa o total diretamente.

Veja o diagrama em [docs/arquitetura.md](docs/arquitetura.md) e a justificativa dos bancos em [docs/justificativa-bancos.md](docs/justificativa-bancos.md).

## Pré-requisitos

- Docker + Docker Compose **ou** cluster Kubernetes (Kind / Minikube / Docker Desktop)
- .NET 8 SDK (apenas para desenvolvimento local sem containers)
- `kubectl` (para o caminho Kubernetes)

## Subir com Docker Compose (recomendado para correção rápida)

```bash
docker compose up --build -d
```

Se a rede bridge do Docker falhar no seu ambiente (raro), use:

```bash
docker compose -f docker-compose.host.yml up --build -d
```

Serviços:

| URL | Uso |
|---|---|
| http://localhost:43123/swagger | API + Swagger |
| http://localhost:15672 | RabbitMQ Management (`guest` / `guest`) |
| http://localhost:3000 | Grafana (`admin` / `admin`) |
| http://localhost:9090 | Prometheus |
| http://localhost:43123/health | Health check |
| http://localhost:43123/metrics | Métricas da API |

Parar:

```bash
docker compose down -v
```

## Subir no Kubernetes

1. Crie o cluster (exemplo com Kind):

```bash
kind create cluster --name conexao
```

2. Construa e carregue as imagens:

```bash
docker build -t conexao-solidaria/campanhas-api:latest -f src/Campanhas.Api/Dockerfile .
docker build -t conexao-solidaria/doacoes-worker:latest -f src/Doacoes.Worker/Dockerfile .
kind load docker-image conexao-solidaria/campanhas-api:latest --name conexao
kind load docker-image conexao-solidaria/doacoes-worker:latest --name conexao
```

3. Aplique os manifests:

```bash
kubectl apply -f k8s/
kubectl get pods -n conexao-solidaria -w
```

4. Acesse a API (NodePort `30080`):

```bash
kubectl get svc -n conexao-solidaria
# API: http://localhost:30080/swagger  (Kind: pode exigir port-forward)
kubectl port-forward -n conexao-solidaria svc/campanhas-api 43123:8080
kubectl port-forward -n conexao-solidaria svc/grafana 3000:3000
kubectl port-forward -n conexao-solidaria svc/rabbitmq 15672:15672
```

## Credenciais seed

| Perfil | Email | Senha |
|---|---|---|
| GestorONG | `gestor@esperanca.org` | `Gestor@123` |
| Doador | cadastre via `POST /api/auth/register` | — |

CPF válido de exemplo para testes: `529.982.247-25`

## Roteiro de demonstração (API)

```bash
# 1) Login gestor
curl -s -X POST http://localhost:43123/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"gestor@esperanca.org","senha":"Gestor@123"}'

# 2) Criar campanha (use o token do gestor)
curl -s -X POST http://localhost:43123/api/campanhas \
  -H "Authorization: Bearer TOKEN_GESTOR" \
  -H 'Content-Type: application/json' \
  -d '{
    "titulo":"Kits Escolares 2026",
    "descricao":"Material para crianças acolhidas",
    "dataInicio":"2026-09-01T00:00:00Z",
    "dataFim":"2026-12-31T23:59:59Z",
    "metaFinanceira":10000,
    "status":"Ativa"
  }'

# 3) Registrar doador
curl -s -X POST http://localhost:43123/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{
    "nomeCompleto":"Maria Doadora",
    "email":"maria@email.com",
    "cpf":"529.982.247-25",
    "senha":"Doador@123"
  }'

# 4) Doar (token do doador) — API só publica evento
curl -s -X POST http://localhost:43123/api/doacoes \
  -H "Authorization: Bearer TOKEN_DOADOR" \
  -H 'Content-Type: application/json' \
  -d '{"idCampanha":"GUID_DA_CAMPANHA","valorDoacao":150.00}'

# 5) Conferir na UI do RabbitMQ a mensagem / fila doacao.recebida.queue

# 6) Painel público (após o Worker processar)
curl -s http://localhost:43123/api/campanhas/publicas
```

## CI/CD

O workflow [.github/workflows/ci.yml](.github/workflows/ci.yml) roda em todo push na branch principal:

1. `dotnet restore` / `build` / `test`
2. Build das imagens Docker da API e do Worker

## Estrutura

```
src/Campanhas.Api          # API JWT + campanhas + doações (publish)
src/Doacoes.Worker         # Consumer RabbitMQ
src/ConexaoSolidaria.Domain
tests/ConexaoSolidaria.Domain.Tests
k8s/                       # Deployments, Services, ConfigMaps, Secrets
observability/             # Prometheus + Grafana dashboard
docs/                      # Arquitetura e justificativa de bancos
```

## Endpoints principais

| Método | Rota | Acesso |
|---|---|---|
| POST | `/api/auth/register` | Público (Doador) |
| POST | `/api/auth/login` | Público |
| POST/PUT/GET | `/api/campanhas` | GestorONG |
| GET | `/api/campanhas/publicas` | Público |
| POST | `/api/doacoes` | Doador |
| GET | `/health` | Público |
| GET | `/metrics` | Público |

## Relatório de entrega (modelo)

Preencha e envie na data da entrega:

- Nome do grupo:
- Participantes e usernames no Discord:
- Link da documentação: `docs/`
- Link do(s) repositório(s):
- Link do vídeo:
