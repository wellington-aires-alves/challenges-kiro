# 📋 TaskFlow - Gerenciador de Tarefas

Aplicação de gerenciamento de tarefas (To-Do) com autenticação JWT, desenvolvida em .NET 9 com ASP.NET Core, Razor Pages e API REST.

---

## 📚 Documentação do Projeto

### 🎯 Início Rápido
- **Este arquivo (README.md)** - Guia completo de uso, instalação e testes

### 📋 Especificações (`.kiro/specs/taskflow/`)
- **[requirements.md](.kiro/specs/taskflow/requirements.md)** - Requisitos funcionais detalhados
- **[design.md](.kiro/specs/taskflow/design.md)** - Arquitetura e decisões de design
- **[tasks.md](.kiro/specs/taskflow/tasks.md)** - Plano de implementação e status

### 📝 Auditoria e Mudanças
- **[CHANGELOG.md](.kiro/specs/taskflow/CHANGELOG.md)** - Histórico de mudanças nos specs
- **[AUDIT_SUMMARY.md](.kiro/specs/taskflow/AUDIT_SUMMARY.md)** - Resumo da última auditoria

### 🎓 Orientações (`.kiro/steering/`)
- **[tech.md](.kiro/steering/tech.md)** - Stack tecnológica e comandos
- **[structure.md](.kiro/steering/structure.md)** - Estrutura do projeto e convenções
- **[product.md](.kiro/steering/product.md)** - Visão do produto e princípios

### 🪝 Hooks (`.kiro/hooks/`)
- **[dotnet-build-on-save.kiro.hook](.kiro/hooks/dotnet-build-on-save.kiro.hook)** - Build automático ao salvar arquivos `.cs`
- **[dotnet-test-on-save.kiro.hook](.kiro/hooks/dotnet-test-on-save.kiro.hook)** - Testes automáticos ao salvar arquivos de teste

---

## ✅ Status Atual

**Última atualização:** 23 de abril de 2026

```
✅ Projeto: 100% completo e funcional
✅ Testes: 37/37 passando (100%)
✅ Documentação: Sincronizada com código
✅ Build: Sem erros
```

---

## 🎯 Sobre o Projeto

TaskFlow permite que usuários se cadastrem, façam login e gerenciem suas tarefas pessoais através de operações CRUD completas, com interface web integrada e API REST.

### Características Principais

- ✅ Autenticação JWT com expiração configurável
- ✅ CRUD completo de tarefas
- ✅ Frontend integrado com Razor Pages
- ✅ API REST documentada
- ✅ Persistência em memória (ideal para desenvolvimento)
- ✅ Testes automatizados (unitários e de integração)
- ✅ Arquitetura em camadas (API, Core, Tests)

---

## 📋 Pré-requisitos

- **Windows** (qualquer versão recente)
- **.NET 9 SDK** instalado
  - Baixe em: https://dotnet.microsoft.com/download
  - Verifique a instalação: `dotnet --version`
- **Navegador web** moderno
- **Cliente HTTP** para testes de API (opcional):
  - [Postman](https://www.postman.com/downloads/)
  - [Insomnia](https://insomnia.rest/download)
  - cURL (linha de comando)

---

## 🚀 Início Rápido

### Forma Mais Fácil (Menu Interativo) - RECOMENDADO

1. Abra o **Explorador de Arquivos**
2. Navegue até a pasta do projeto
3. Dê um **duplo clique** em `menu.bat`
4. Escolha a opção desejada (digite o número e pressione Enter)
5. Para executar a aplicação, escolha opção **1** ou **2**
6. Abra o navegador em https://localhost:5001

### Execução Direta

1. Dê um **duplo clique** em `run-taskflow.bat`
2. Aguarde a aplicação iniciar
3. Acesse https://localhost:5001

### Modo Desenvolvimento (Hot Reload)

1. Dê um **duplo clique** em `run-dev.bat`
2. Edite os arquivos do projeto
3. As alterações serão aplicadas automaticamente
4. Recarregue a página no navegador

---

## 🎯 Scripts Disponíveis

### **menu.bat** - Menu Interativo (RECOMENDADO)

Menu interativo com todas as opções:

```bash
menu.bat
```

**Oferece:**
- 🎮 Interface interativa com menu de opções
- ✅ Acesso rápido a todas as funcionalidades
- ✅ Ideal para quem prefere não usar linha de comando
- ✅ Navegação simples e intuitiva

---

### **run-taskflow.bat** - Executar Aplicação (Produção)

Executa a aplicação em modo de produção:

```bash
run-taskflow.bat
```

**O que faz:**
- ✅ Verifica se o .NET está instalado
- ✅ Restaura as dependências do projeto
- ✅ Compila o projeto em modo Release
- ✅ Inicia a aplicação

**Acesse em:**
- https://localhost:5001
- http://localhost:5000

---

### **run-dev.bat** - Modo Desenvolvimento

Desenvolvimento com recarga automática:

```bash
run-dev.bat
```

**O que faz:**
- ✅ Inicia a aplicação em modo watch
- ✅ Recarrega automaticamente quando você salva alterações
- ✅ Ideal para desenvolvimento

---

### **run-tests.bat** - Executar Testes

Executa todos os testes:

```bash
run-tests.bat
```

**O que faz:**
- ✅ Executa todos os testes unitários e de integração
- ✅ Mostra relatório detalhado dos resultados
- ✅ Indica se algum teste falhou

---

### **clean.bat** - Limpar Build

Limpa arquivos de compilação:

```bash
clean.bat
```

**O que faz:**
- ✅ Remove pastas `bin/` e `obj/`
- ✅ Limpa arquivos temporários de build
- ✅ Útil quando há problemas de compilação

---

## 🔧 Comandos Manuais (Alternativa)

Se preferir usar o terminal diretamente:

```bash
# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar
dotnet run --project TaskFlow.API

# Executar em modo watch (desenvolvimento)
dotnet watch run --project TaskFlow.API

# Executar testes
dotnet test

# Executar testes com detalhes
dotnet test --verbosity detailed

# Limpar build
dotnet clean
```

---

## 📝 Primeira Utilização

### Via Frontend (Razor Pages)

1. **Registre um usuário:**
   - Acesse https://localhost:5001/register
   - Preencha: nome de usuário, e-mail e senha (mínimo 6 caracteres)
   - Clique em "Cadastrar"

2. **Faça login:**
   - Você será redirecionado automaticamente
   - Ou acesse https://localhost:5001/login

3. **Crie suas tarefas:**
   - Na página inicial, preencha o formulário
   - Clique em "Criar Tarefa"
   - Gerencie suas tarefas (editar, excluir, marcar como concluída)

4. **Gerencie suas tarefas:**
   - **Criar**: Preencha título (obrigatório) e descrição (opcional)
   - **Visualizar**: Veja todas as suas tarefas com status e data
   - **Editar**: Clique em "Editar" para modificar título, descrição ou status
   - **Excluir**: Clique em "Excluir" e confirme a ação
   - **Logout**: Clique em "Sair" para encerrar a sessão

---

## 🧪 Testando a API REST

### Configuração Inicial

A API estará disponível em:
- **HTTP**: http://localhost:5000/api
- **HTTPS**: https://localhost:5001/api

### Endpoints Disponíveis

#### 1. Registrar Usuário

```bash
POST /api/auth/register
Content-Type: application/json

{
  "username": "usuario1",
  "email": "usuario1@example.com",
  "password": "Senha@123"
}
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "usuario1"
}
```

**Possíveis erros:**
- `400 Bad Request`: Validação falhou (senha fraca, email inválido)
- `409 Conflict`: Email ou username já cadastrado

---

#### 2. Fazer Login

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario1@example.com",
  "password": "Senha@123"
}
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "usuario1"
}
```

⚠️ **Importante:** Copie o token para usar nas próximas requisições.

**Possíveis erros:**
- `401 Unauthorized`: Credenciais inválidas
- `400 Bad Request`: Campos obrigatórios faltando

---

#### 3. Criar Tarefa

```bash
POST /api/tasks
Content-Type: application/json
Authorization: Bearer SEU_TOKEN_AQUI

{
  "title": "Minha primeira tarefa",
  "description": "Descrição da tarefa"
}
```

**Resposta (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Minha primeira tarefa",
  "description": "Descrição da tarefa",
  "status": "Pendente",
  "createdAt": "2026-04-23T10:30:00"
}
```

**Possíveis erros:**
- `400 Bad Request`: Título vazio ou muito longo (máx. 200 caracteres)
- `401 Unauthorized`: Token inválido ou expirado

---

#### 4. Listar Tarefas

```bash
GET /api/tasks
Authorization: Bearer SEU_TOKEN_AQUI
```

**Resposta (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Minha primeira tarefa",
    "description": "Descrição da tarefa",
    "status": "Pendente",
    "createdAt": "2026-04-23T10:30:00"
  }
]
```

---

#### 5. Atualizar Tarefa

```bash
PUT /api/tasks/{id}
Content-Type: application/json
Authorization: Bearer SEU_TOKEN_AQUI

{
  "title": "Tarefa atualizada",
  "description": "Nova descrição",
  "status": "Concluida"
}
```

**Resposta (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Tarefa atualizada",
  "description": "Nova descrição",
  "status": "Concluida",
  "createdAt": "2026-04-23T10:30:00"
}
```

**Possíveis erros:**
- `400 Bad Request`: Validação falhou
- `401 Unauthorized`: Token inválido
- `403 Forbidden`: Tarefa pertence a outro usuário
- `404 Not Found`: Tarefa não existe

---

#### 6. Deletar Tarefa

```bash
DELETE /api/tasks/{id}
Authorization: Bearer SEU_TOKEN_AQUI
```

**Resposta:** Status 204 (No Content)

**Possíveis erros:**
- `401 Unauthorized`: Token inválido
- `403 Forbidden`: Tarefa pertence a outro usuário
- `404 Not Found`: Tarefa não existe

---

### Testando com cURL

#### Fluxo Completo

```bash
# 1. Registrar usuário
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"usuario1\",\"email\":\"usuario1@example.com\",\"password\":\"Senha@123\"}"

# 2. Fazer login (copie o token retornado)
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"usuario1@example.com\",\"password\":\"Senha@123\"}"

# 3. Criar tarefa (substitua TOKEN pelo token obtido)
curl -X POST http://localhost:5000/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d "{\"title\":\"Minha tarefa\",\"description\":\"Descrição\"}"

# 4. Listar tarefas
curl -X GET http://localhost:5000/api/tasks \
  -H "Authorization: Bearer TOKEN"

# 5. Atualizar tarefa (substitua ID pelo id da tarefa)
curl -X PUT http://localhost:5000/api/tasks/ID \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d "{\"title\":\"Tarefa atualizada\",\"description\":\"Nova descrição\",\"status\":\"Concluida\"}"

# 6. Deletar tarefa
curl -X DELETE http://localhost:5000/api/tasks/ID \
  -H "Authorization: Bearer TOKEN"
```

---

### Testando com Postman

#### Configurar Variáveis de Ambiente

- `baseUrl`: `http://localhost:5000`
- `token`: (será preenchido após login)

#### Criar Coleção

**1. Register**
- Método: `POST`
- URL: `{{baseUrl}}/api/auth/register`
- Body (JSON):
```json
{
  "username": "usuario1",
  "email": "usuario1@example.com",
  "password": "Senha@123"
}
```

**2. Login**
- Método: `POST`
- URL: `{{baseUrl}}/api/auth/login`
- Body (JSON):
```json
{
  "email": "usuario1@example.com",
  "password": "Senha@123"
}
```
- Script (Tests tab) para salvar o token:
```javascript
pm.environment.set("token", pm.response.json().token);
```

**3. Create Task**
- Método: `POST`
- URL: `{{baseUrl}}/api/tasks`
- Headers: `Authorization: Bearer {{token}}`
- Body (JSON):
```json
{
  "title": "Minha tarefa",
  "description": "Descrição da tarefa"
}
```

**4. Get All Tasks**
- Método: `GET`
- URL: `{{baseUrl}}/api/tasks`
- Headers: `Authorization: Bearer {{token}}`

**5. Update Task**
- Método: `PUT`
- URL: `{{baseUrl}}/api/tasks/{{taskId}}`
- Headers: `Authorization: Bearer {{token}}`
- Body (JSON):
```json
{
  "title": "Tarefa atualizada",
  "description": "Nova descrição",
  "status": "Concluida"
}
```

**6. Delete Task**
- Método: `DELETE`
- URL: `{{baseUrl}}/api/tasks/{{taskId}}`
- Headers: `Authorization: Bearer {{token}}`

---

## 📊 Cenários de Teste

### Cenário 1: Fluxo Completo via Frontend

1. Acesse http://localhost:5000
2. Registre novo usuário
3. Faça login
4. Crie 3 tarefas diferentes
5. Visualize todas as tarefas (deve retornar 3)
6. Edite 2 tarefas para status "Concluída"
7. Delete 1 tarefa
8. Visualize novamente (deve retornar 2)
9. Faça logout

### Cenário 2: Testes de Segurança

1. **Acesso sem token** → 401 Unauthorized
2. **Email duplicado** → 409 Conflict
3. **Username duplicado** → 409 Conflict
4. **Credenciais inválidas** → 401 Unauthorized
5. **Tarefa de outro usuário** → 403 Forbidden

### Cenário 3: Testes de Validação

1. **Tarefa sem título** → 400 Bad Request
2. **Título muito longo** (>200 chars) → 400 Bad Request
3. **Status inválido** → 400 Bad Request
4. **Tarefa inexistente** → 404 Not Found
5. **Senha muito curta** (<6 chars) → 400 Bad Request
6. **Email inválido** → 400 Bad Request

---

## 🏗️ Estrutura do Projeto

```
TaskFlow.sln
├── TaskFlow.API/             # Projeto principal — API e configuração
│   ├── Controllers/          # Controllers HTTP (endpoints REST)
│   ├── Pages/                # Razor Pages (frontend)
│   ├── Middleware/           # Middlewares customizados
│   ├── Program.cs            # Ponto de entrada e configuração
│   └── appsettings.json      # Configurações da aplicação
│
├── TaskFlow.Core/            # Núcleo da aplicação
│   ├── Entities/             # Modelos de domínio
│   ├── Interfaces/           # Contratos de repositórios e serviços
│   ├── Services/             # Lógica de negócio
│   └── DTOs/                 # Objetos de transferência de dados
│
├── TaskFlow.Tests/           # Testes automatizados
│   ├── Unit/                 # Testes unitários
│   └── Integration/          # Testes de integração
│
└── Scripts/                  # Scripts de execução (.bat)
    ├── menu.bat
    ├── run-taskflow.bat
    ├── run-dev.bat
    ├── run-tests.bat
    └── clean.bat
```

---

## 🤖 Configuração do MCP GitHub (Kiro)

O projeto utiliza o servidor MCP do GitHub para integração com o Kiro. A configuração fica em `.kiro/settings/mcp.json`.

### Pré-requisitos

1. **Instalar `uv`** (gerenciador de pacotes Python que fornece o `uvx`):
   ```bash
   pip install uv
   ```
   Ou consulte o guia oficial: https://docs.astral.sh/uv/getting-started/installation/

2. **Gerar um Personal Access Token (classic)** no GitHub:
   - Acesse: https://github.com/settings/tokens/new
   - Marque o escopo **`repo`** (Full control of private repositories)
   - Copie o token gerado

### Configuração

Crie ou edite o arquivo `.kiro/settings/mcp.json` com o seguinte conteúdo:

```json
{
  "mcpServers": {
    "github": {
      "command": "C:/Users/<seu-usuario>/.local/bin/uvx.exe",
      "args": [
        "mcp-github"
      ],
      "env": {
        "GITHUB_TOKEN": "<seu-github-token>"
      },
      "disabled": false,
      "autoApprove": [
        "create_repository",
        "search_repositories"
      ]
    }
  }
}
```

> **Atenção:** Substitua `<seu-usuario>` pelo seu nome de usuário do Windows e `<seu-github-token>` pelo token gerado. **Nunca commite tokens reais no repositório.**

### Observações

- O caminho do `uvx.exe` pode variar. Após instalar o `uv`, verifique com: `where uvx`
- O servidor reconecta automaticamente quando a configuração é salva
- `autoApprove` define quais ferramentas não precisam de confirmação manual no Kiro

---

## 🪝 Agent Hooks

Hooks são automações que disparam ações do agente Kiro em resposta a eventos do IDE. Os hooks do projeto ficam em `.kiro/hooks/`.

### Build on C# Save

**Arquivo:** `.kiro/hooks/dotnet-build-on-save.kiro.hook`

| Campo | Valor |
|---|---|
| Evento | `fileEdited` |
| Padrão de arquivo | `**/*.cs` |
| Ação | `runCommand` |
| Comando | `dotnet build` |
| Timeout | 60 segundos |

**O que faz:** Executa `dotnet build` automaticamente sempre que qualquer arquivo `.cs` do projeto for salvo. Garante feedback imediato sobre erros de compilação durante o desenvolvimento.

---

### Test on Test File Save

**Arquivo:** `.kiro/hooks/dotnet-test-on-save.kiro.hook`

| Campo | Valor |
|---|---|
| Evento | `fileEdited` |
| Padrão de arquivo | `**/TaskFlow.Tests/**/*.cs` |
| Ação | `runCommand` |
| Comando | `dotnet test` |
| Timeout | 120 segundos |

**O que faz:** Executa `dotnet test` automaticamente sempre que um arquivo de teste dentro de `TaskFlow.Tests/` for salvo. Mantém a suíte de testes sempre verificada durante o desenvolvimento de novos testes.

---

## ⚠️ Solução de Problemas

### Erro: ".NET não encontrado"
- Instale o .NET 9 SDK: https://dotnet.microsoft.com/download
- Reinicie o terminal/prompt após a instalação

### Erro: "Porta já em uso"
- Outra aplicação está usando a porta 5000 ou 5001
- Feche outras instâncias do TaskFlow
- Ou edite `TaskFlow.API/Properties/launchSettings.json` para usar outras portas

### Erro de compilação
1. Execute `clean.bat` para limpar o build
2. Execute `run-taskflow.bat` novamente

### Aplicação não abre no navegador
- Abra manualmente: https://localhost:5001
- Verifique se o firewall não está bloqueando

### Token expirado
- Faça login novamente para obter um novo token
- O token expira após 3 minutos por padrão

### Testes falhando
1. Verifique se a aplicação não está rodando (conflito de porta)
2. Limpar e recompilar:
```bash
dotnet clean
dotnet build
dotnet test
```

### Não consegue fazer login via frontend
1. Abra o DevTools do navegador (F12)
2. Vá para a aba "Application" ou "Storage"
3. Limpe os cookies e tente novamente
4. Se a aplicação foi reiniciada, registre novamente (dados em memória)

---

## 🛑 Parar a Aplicação

- Pressione **Ctrl + C** no terminal
- Ou feche a janela do prompt de comando

---

## 🔐 Segurança

### Boas Práticas Implementadas

- ✅ Senhas com hash BCrypt (nunca armazenadas em texto simples)
- ✅ Autenticação JWT com expiração configurável
- ✅ Cookies HttpOnly para sessão
- ✅ Validação de entrada em todos os endpoints
- ✅ Isolamento de dados por usuário
- ✅ Middleware de tratamento de exceções global

### Recomendações para Produção

- 🔒 Usar HTTPS obrigatoriamente
- 🔒 Implementar rate limiting
- 🔒 Adicionar CORS restritivo
- 🔒 Usar variáveis de ambiente para chaves secretas
- 🔒 Implementar logging e monitoramento
- 🔒 Migrar para banco de dados persistente
- 🔒 Fazer backup regular dos dados
- 🔒 Implementar autenticação multi-fator (MFA)

---

## 💡 Dicas

- Use `run-dev.bat` durante o desenvolvimento para aproveitar o Hot Reload
- Execute `run-tests.bat` antes de fazer commits importantes
- Use `clean.bat` se encontrar problemas estranhos de compilação
- A aplicação usa armazenamento em memória - os dados são perdidos ao reiniciar
- Use variáveis de ambiente do Postman para facilitar os testes
- Mantenha o console da aplicação visível para monitorar logs
- Use DevTools do navegador para debugar problemas de frontend
- Mantenha a chave JWT segura — nunca a exponha em repositórios públicos

---

## 📚 Stack Tecnológica

- **Plataforma**: .NET 9
- **Framework Web**: ASP.NET Core 9 (Web API + Razor Pages)
- **Autenticação**: JWT Bearer
- **Persistência**: In-memory (desenvolvimento)
- **Frontend**: Razor Pages
- **Testes**: xUnit + FsCheck
- **Hash de Senha**: BCrypt.Net

---

## 📖 Recursos Adicionais

- [Documentação do .NET](https://docs.microsoft.com/dotnet)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Documentação JWT](https://jwt.io/introduction)
- [Documentação xUnit](https://xunit.net/)
- [Documentação Razor Pages](https://docs.microsoft.com/aspnet/core/razor-pages)
- [REST API Best Practices](https://restfulapi.net/)
- [OWASP Security Guidelines](https://owasp.org/www-project-top-ten/)

---

## 📝 Checklist de Validação

Use este checklist para garantir que tudo está funcionando:

- [ ] Aplicação compila sem erros
- [ ] Aplicação inicia nas portas corretas
- [ ] Página inicial carrega
- [ ] Registro de usuário funciona
- [ ] Login funciona e retorna token
- [ ] Criar tarefa funciona
- [ ] Listar tarefas funciona
- [ ] Editar tarefa funciona
- [ ] Deletar tarefa funciona
- [ ] Logout funciona
- [ ] API: Todos os endpoints respondem corretamente
- [ ] API: Autenticação funciona
- [ ] API: Validações funcionam
- [ ] Todos os testes automatizados passam

---

## 🎯 Próximos Passos

1. **Explorar o código** para entender a arquitetura
2. **Adicionar novos testes** para cobrir mais cenários
3. **Implementar novas funcionalidades**:
   - Filtros de tarefas
   - Paginação
   - Busca
   - Categorias/tags
   - Notificações
4. **Melhorar o frontend**:
   - Validação no cliente
   - Design aprimorado
   - Indicadores de carregamento
5. **Migrar para banco de dados real**:
   - Entity Framework Core
   - SQL Server, PostgreSQL ou SQLite
6. **Adicionar documentação**:
   - Swagger/OpenAPI
   - Comentários XML

---

## ⚠️ Importante

- Os dados são armazenados em memória
- Ao reiniciar a aplicação, os dados são perdidos
- Isso é normal para ambiente de desenvolvimento
- Para produção, migre para um banco de dados persistente

---

**Desenvolvido com ❤️ usando .NET 9 e ASP.NET Core**

**Bom desenvolvimento! 🎉**
