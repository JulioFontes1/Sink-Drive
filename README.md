# SinkDrive

Aplicação web desenvolvida como projeto acadêmico para centralizar o acesso a arquivos de múltiplas contas Google Drive em uma única interface.

## Objetivos do projeto

- Permitir o cadastro e a autenticação de usuários do SinkDrive.
- Permitir a conexão de múltiplas contas Google por usuário.
- Exibir arquivos e pastas das contas conectadas.
- Permitir o envio e o download de arquivos.
- Identificar a conta Google associada a cada arquivo.
- Simplificar o gerenciamento de arquivos de diferentes contas em uma única aplicação.

## Tecnologias utilizadas

- **Linguagem:** C#
- **Framework:** .NET 10
- **Aplicação web:** ASP.NET Core Razor Pages
- **Autenticação:** ASP.NET Core Identity
- **ORM:** Entity Framework Core
- **Banco de dados:** PostgreSQL
- **Containerização:** Docker e Docker Compose
- **Autenticação Google:** OAuth 2.0
- **Arquivos:** Google Drive API
- **Interface:** HTML, CSS e Bootstrap

## Arquitetura do projeto

### Aplicação web

A aplicação utiliza ASP.NET Core Razor Pages para organizar as páginas e a lógica de interação com o usuário.

A interface utiliza um tema escuro, com layouts separados para as páginas de autenticação e para a área principal da aplicação.

### Autenticação local

O ASP.NET Core Identity gerencia o cadastro, a autenticação e os dados dos usuários do SinkDrive.

A entidade `ApplicationUser` estende `IdentityUser` e adiciona o campo opcional `DisplayName`.

O sistema possui páginas para cadastro, login e logout. As páginas privadas podem ser protegidas para permitir acesso somente a usuários autenticados.

### Contas Google

A entidade `GoogleAccount` representa uma conta Google vinculada a um usuário do SinkDrive.

Cada conta possui:

- `Id`: identificador interno da associação.
- `GoogleId`: identificador único da conta Google.
- `Email`: endereço de e-mail da conta.
- `UserId`: identificador do usuário SinkDrive associado.
- `User`: propriedade de navegação para o usuário relacionado.
- `RefreshToken`: campo opcional destinado à persistência do token de atualização OAuth.

Um usuário SinkDrive poderá conectar várias contas Google. Cada conta Google cadastrada deverá estar associada a um único usuário SinkDrive.

A implementação do fluxo de conexão e gerenciamento dessas contas ainda está planejada.

### Banco de dados

O PostgreSQL é executado em um container Docker.

O acesso ao banco é realizado por meio do Entity Framework Core, utilizando a classe `ApplicationDbContext`.

O ASP.NET Core Identity utiliza o mesmo contexto para persistir os dados de autenticação e gerenciamento de usuários.

As migrations do Entity Framework Core são utilizadas para versionar e aplicar alterações na estrutura do banco de dados.

### Google Drive

Os arquivos e as pastas não serão armazenados no PostgreSQL. Seus metadados serão consultados por meio da Google Drive API.

O OAuth 2.0 será utilizado para autorizar o acesso às contas Google conectadas, respeitando as permissões concedidas pelo usuário.

A integração funcional com a Google Drive API ainda está em desenvolvimento.

## Estrutura principal

```text
SinkDrive/
├── Data/
│   └── ApplicationDbContext.cs
├── Migrations/
├── Models/
│   ├── ApplicationUser.cs
│   └── GoogleAccount.cs
├── Pages/
│   ├── Account/
│   │   ├── Login.cshtml
│   │   ├── Login.cshtml.cs
│   │   ├── Register.cshtml
│   │   ├── Register.cshtml.cs
│   │   ├── Logout.cshtml
│   │   └── Logout.cshtml.cs
│   ├── Shared/
│   │   ├── _AuthLayout.cshtml
│   │   └── _Layout.cshtml
│   └── Index.cshtml
├── wwwroot/
│   └── css/
│       ├── auth.css
│       └── site.css
├── .gitignore
├── compose.yaml
├── Program.cs
├── SinkDrive.csproj
└── README.md
```

## Histórico de desenvolvimento

### Etapa 1 — Configuração inicial

- [x] Configuração do projeto ASP.NET Core.
- [x] Configuração do PostgreSQL com Docker Compose.
- [x] Configuração da persistência da senha do banco por meio de variável de ambiente.
- [x] Configuração do User Secrets para armazenar a connection string localmente.
- [x] Configuração inicial do `.gitignore`.

### Etapa 2 — Persistência e identidade

- [x] Instalação dos pacotes do Entity Framework Core.
- [x] Instalação do provedor PostgreSQL para Entity Framework Core.
- [x] Configuração do ASP.NET Core Identity.
- [x] Criação da entidade `ApplicationUser`.
- [x] Criação da entidade `GoogleAccount`.
- [x] Configuração do relacionamento entre usuários e contas Google.
- [x] Definição de unicidade para o `GoogleId`.
- [x] Criação e aplicação das migrations.
- [x] Verificação das tabelas no PostgreSQL.

### Etapa 3 — Versionamento e documentação

- [x] Inicialização do repositório Git local.
- [x] Criação do README inicial.
- [x] Criação e configuração do repositório remoto no GitHub.
- [ ] Atualização da documentação conforme a evolução do projeto.

### Etapa 4 — Autenticação local

- [x] Implementação do cadastro de usuários.
- [x] Implementação do login.
- [x] Implementação do logout.
- [x] Validação dos dados de cadastro.
- [x] Configuração dos caminhos de login e acesso negado.
- [x] Restrição da página inicial a usuários autenticados.

### Etapa 5 — Interface inicial

- [x] Criação de um layout exclusivo para autenticação.
- [x] Desenvolvimento dos estilos de login e cadastro em tema escuro.
- [x] Criação do layout principal com barra lateral e cabeçalho.
- [x] Implementação do botão de logout no layout principal.
- [x] Exibição do `DisplayName`, utilizando o nome de usuário como alternativa.
- [ ] Desenvolvimento do dashboard com informações reais das contas conectadas.
- [ ] Implementação das páginas de gerenciamento de contas e arquivos.

### Próximas etapas

#### Integração com o Google

- [ ] Implementar o fluxo OAuth 2.0 para conexão de contas.
- [ ] Associar contas Google ao usuário SinkDrive autenticado.
- [ ] Tratar contas Google já vinculadas.
- [ ] Proteger tokens de autorização armazenados.
- [ ] Implementar a desconexão de contas Google.

#### Gerenciamento de arquivos

- [ ] Consultar arquivos e pastas da Google Drive API.
- [ ] Exibir arquivos e pastas das contas conectadas.
- [ ] Identificar a conta de origem de cada arquivo.
- [ ] Implementar upload de arquivos.
- [ ] Implementar download de arquivos.

#### Interface e testes

- [ ] Desenvolver o dashboard com dados reais.
- [ ] Implementar as páginas de gerenciamento de contas Google.
- [ ] Implementar mensagens de erro e sucesso.
- [ ] Testar os fluxos de autenticação e conexão.
- [ ] Testar o acesso a arquivos de diferentes contas.
- [ ] Revisar a responsividade da interface.

## Como executar o ambiente de desenvolvimento

### Pré-requisitos

- SDK do .NET 10.
- Docker com Docker Compose.
- Git.

### 1. Configurar as variáveis de ambiente

Crie um arquivo `.env` na raiz do projeto com a variável `POSTGRES_PASSWORD` definida, conforme a configuração do Docker Compose.

### 2. Configurar a connection string

Configure a connection string `ConnectionStrings:DefaultConnection` utilizando o User Secrets do ASP.NET Core.

### 3. Iniciar o banco de dados

```bash
docker compose up -d
```

### 4. Aplicar as migrations

```bash
dotnet ef database update
```

### 5. Executar a aplicação

```bash
dotnet run
```

A aplicação informará no terminal o endereço local em que está sendo executada.

## Segurança

- Não versionar senhas, tokens OAuth ou arquivos de credenciais.
- Manter segredos fora do código-fonte.
- Utilizar HTTPS nos ambientes apropriados.
- Solicitar apenas as permissões necessárias para acessar o Google Drive.
- Proteger os tokens de autorização persistidos no banco.
- Validar as permissões do usuário antes de realizar operações sobre contas conectadas.

## Status atual

O projeto possui a estrutura de persistência configurada, com PostgreSQL, Entity Framework Core e ASP.NET Core Identity. As entidades `ApplicationUser` e `GoogleAccount` estão definidas, e as migrations iniciais foram aplicadas.

O cadastro, o login e o logout de usuários estão implementados. A página inicial está protegida contra acesso não autenticado, e a interface inicial possui layouts separados para autenticação e para a área principal, com tema escuro.

O gerenciamento de contas Google, a integração funcional com OAuth 2.0 e a Google Drive API, e as operações de upload, download e listagem de arquivos permanecem como etapas futuras do desenvolvimento.
