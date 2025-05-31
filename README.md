
# API de Otimização de Embalagem - Loja do Seu Manoel

## Descrição
Esta API tem como objetivo otimizar o processo de embalagem de produtos para a loja de jogos do Seu Manoel. Ela recebe uma lista de pedidos com os produtos e suas dimensões e retorna a melhor combinação de caixas para cada pedido. A API também inclui funcionalidades de autenticação de usuário e gerenciamento de dados de pedidos e caixas.

---

## Requisitos
- **Docker e Docker Compose:** Para executar a API e o banco de dados SQL Server em contêineres.
---


## Como Executar e Testar a API

### 1. Execute com Docker Compose

```bash
docker compose up --build
```

### 2. Acessos Disponíveis
- **API:** [http://localhost:8001](http://localhost:8001)
- **Banco de Dados (SQL Server ):** porta `8002`
- **Swagger UI:** [http://localhost:8001/swagger/index.html](http://localhost:8001/swagger/index.html)

---

## Autenticação e Uso dos Endpoints

### 🔑 Login

**Endpoint:** `POST /api/auth/login`  
**Exemplo de corpo:**

```json
{
  "username": "teste",
  "password": "senha123"}
```

> 🔑 Use teste e senha123 para realizar o login


**Resposta de Sucesso:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn..."
}
```

---

### 🔑 Autenticação no Swagger

1. Clique no botão **Authorize** no Swagger.
2. No campo `Value` insira:

```
Bearer SEU_TOKEN_AQUI
```

3. Clique em **Authorize** e depois em **Close**.

---

## 🧪 Testes Unitários

Para executar os testes:

```bash
dotnet test
```

Execute no diretório da solução onde está o projeto `TesteUnitario`.



