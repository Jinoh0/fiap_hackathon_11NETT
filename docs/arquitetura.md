# Diagrama de Arquitetura — Conexão Solidária

```mermaid
flowchart TB
  subgraph clients [Clientes]
    Postman[Postman_Swagger]
  end

  subgraph k8s [Kubernetes_Namespace_conexao_solidaria]
    Api[Campanhas_Api_JWT_RBAC]
    Worker[Doacoes_Worker]
    Pg[(PostgreSQL)]
    Rmq[RabbitMQ]
    Prom[Prometheus]
    Graf[Grafana]
  end

  Postman -->|HTTPS_HTTP| Api
  Api -->|CRUD_auth_doacao| Pg
  Api -->|"publish DoacaoRecebidaEvent"| Rmq
  Rmq -->|consume| Worker
  Worker -->|incrementa ValorArrecadado| Pg
  Api -->|/metrics /health| Prom
  Worker -->|/metrics| Prom
  Graf --> Prom
```

## Microsserviços

| Serviço | Responsabilidade |
|---|---|
| **Campanhas.Api** | JWT/RBAC, cadastro de doador, gestão de campanhas, painel público, recebe intenção de doação e publica evento |
| **Doacoes.Worker** | Consome fila RabbitMQ e atualiza `ValorArrecadado` |

## Fluxo de doação (obrigatório no vídeo)

1. Doador autentica e envia `POST /api/doacoes`.
2. API valida campanha `Ativa`, grava doação `Pendente` e publica `DoacaoRecebidaEvent`.
3. Mensagem aparece na fila `doacao.recebida.queue` (Management UI).
4. Worker processa, marca doação `Processada` e soma o valor na campanha.
5. `GET /api/campanhas/publicas` reflete o novo total arrecadado.
