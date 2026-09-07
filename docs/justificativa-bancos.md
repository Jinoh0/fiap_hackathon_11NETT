# Justificativa dos bancos / persistência — Conexão Solidária

## Contexto

O edital exige justificar a escolha dos bancos de dados utilizados na arquitetura.
Para o MVP, adotamos **dois componentes de persistência complementares**:

1. **PostgreSQL 16** — fonte da verdade transacional (usuários, campanhas, doações).
2. **RabbitMQ 3.13** — persistência durável de eventos assíncronos (`DoacaoRecebidaEvent`).

## Por que PostgreSQL

- Modelo relacional adequado a regras de negócio (email único, FK campanha/doação, meta financeira decimal).
- Constraints e ACID garantem consistência no cadastro de doadores e no incremento de `ValorArrecadado`.
- Excelente suporte no ecossistema .NET via EF Core + Npgsql.
- Imagem leve (`postgres:16-alpine`) e madura em Kubernetes.
- Alternativa considerada: SQL Server — mais pesada no cluster local e menos conveniente para demo acadêmica.

API e Worker compartilham o database `conexao_campanhas` de propósito: o Worker precisa atualizar o total arrecadado da campanha (requisito do fluxo assíncrono). A separação de responsabilidade fica nos **microsserviços** (API publica evento; Worker consome e escreve), não em um segundo SGBD artificial.

## Por que RabbitMQ (além do banco relacional)

O brief obriga comunicação assíncrona: a API **não** atualiza o valor arrecadado diretamente.
O broker:

- Desacopla API e Worker.
- Oferece fila durável, ack/nack e Management UI (ideal para o vídeo de demonstração).
- É mais simples e econômico que Kafka para o volume do MVP.

## Resumo

| Persistência | Papel |
|---|---|
| PostgreSQL | Dados de negócio (CRUD + agregação de arrecadação) |
| RabbitMQ | Transporte e retenção de eventos de doação até o Worker processar |

Essa combinação atende escalabilidade, observabilidade e o requisito explícito de mensageria do hackathon.
