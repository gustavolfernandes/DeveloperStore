# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

## Running the API

The backend lives under `template/backend`, targets **.NET 8** and uses **PostgreSQL**.
Both ways below apply the EF Core migrations automatically on startup.

### Prerequisites
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (Docker Desktop on Windows/macOS)

### Option 1 — .NET Aspire (recommended)
The Aspire AppHost provisions the PostgreSQL container and injects its connection string
into the API automatically.

```bash
cd template/backend
dotnet dev-certs https --trust            # once, for the HTTPS profile
dotnet run --project src/Ambev.DeveloperEvaluation.AppHost
```

The Aspire dashboard opens and the API starts with the database already connected.

### Option 2 — Docker Compose
The database credentials are not committed in plaintext; only an AES-256-encrypted file
(`secrets/db.env.enc`) is versioned. Decrypt it into a local `.env` (git-ignored) before
starting the stack:

```bash
cd template/backend
openssl enc -d -aes-256-cbc -pbkdf2 -a -in secrets/db.env.enc -out .env -pass pass:developer-evaluation-demo
docker compose up -d --build
```

The API is then available at `http://localhost:8080` and Swagger at `http://localhost:8080/swagger`.

#### How the secrets are handled
`docker-compose.yml` references the credentials through variables (`${POSTGRES_PASSWORD}`, …)
that Docker Compose reads from the decrypted `.env`; the API receives the connection string the
same way. The committed `secrets/db.env.enc` is just the `.env` encrypted with OpenSSL:

```bash
# Encrypt (after editing .env)
openssl enc -aes-256-cbc -pbkdf2 -salt -a -in .env -out secrets/db.env.enc -pass pass:developer-evaluation-demo

# Decrypt ("break" it open to run)
openssl enc -d -aes-256-cbc -pbkdf2 -a -in secrets/db.env.enc -out .env -pass pass:developer-evaluation-demo
```

> Keeping the passphrase in this README makes the encryption a demonstration of the technique,
> not real protection — anyone with the repository can decrypt it. In a real deployment the
> passphrase/key lives outside the repository (a secrets manager or KMS) and only the encrypted
> file is committed.

### Authentication
All `/api/sales/*` endpoints require a JWT:

1. `POST /api/users` to register a user (`status: 1`, `role: 1`; phone in E.164, e.g. `+5511987654321`).
2. `POST /api/auth` with the e-mail and password — the response `data.token` is the bearer token.
3. Send it as `Authorization: Bearer <token>`, or use the **Authorize** button in Swagger.

### Tests
```bash
cd template/backend
dotnet test                               # unit + integration + functional
```

The functional tests start a throwaway PostgreSQL container via Testcontainers and require
Docker. To run only the suites that do not need Docker:

```bash
dotnet test --filter "Category!=Functional"
```
