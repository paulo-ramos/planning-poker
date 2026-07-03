# 🎴 Planning Poker - Aplicação em Tempo Real

Uma aplicação web moderna e reativa de Planning Poker desenvolvida com **Blazor Server (.NET 8)** para facilitar o planejamento de sprints em equipe.

![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)

## ✨ Características

- 🚀 **Tempo Real**: Atualização instantânea para todos os participantes usando Blazor InteractiveServer
- 💾 **Em Memória**: Gerenciamento de estado thread-safe com `ConcurrentDictionary` (sem banco de dados)
- 🎨 **Design Moderno**: Interface limpa e responsiva com Tailwind CSS
- 🔒 **Thread-Safe**: Serviço Singleton com suporte completo a concorrência
- 🎯 **Sequências Customizáveis**: Fibonacci, Modified Fibonacci, T-Shirt Sizes ou crie sua própria
- 📊 **Cálculo Automático**: Média aritmética dos votos numéricos
- 🎭 **Avatares Dinâmicos**: Geração automática de avatares via DiceBear API
- ⏰ **Limpeza Automática**: Salas inativas são removidas automaticamente após 2 horas

## 🏗️ Arquitetura

```
PlanningPoker/
├── .devcontainer/          # Configuração do Dev Container para Podman
│   ├── devcontainer.json
│   └── Dockerfile
├── Components/             # Componentes Blazor
│   ├── Layout/
│   │   └── MainLayout.razor
│   ├── Pages/
│   │   ├── Home.razor      # Tela de criação de sala
│   │   └── Room.razor      # Sala de votação em tempo real
│   ├── App.razor
│   ├── Routes.razor
│   └── _Imports.razor
├── Models/                 # Modelos de domínio
│   ├── Room.cs
│   ├── User.cs
│   └── Vote.cs
├── Services/              # Serviços da aplicação
│   └── PlanningPokerService.cs
├── wwwroot/               # Arquivos estáticos
│   └── app.css
├── Program.cs             # Configuração da aplicação
└── PlanningPoker.csproj   # Arquivo do projeto
```

## 🚀 Como Executar

### ⚡ Início Rápido (3 passos)

```bash
# 1. Verificar ambiente
./diagnose.sh

# 2. Abrir no VS Code
code .

# 3. No VS Code: F1 → "Dev Containers: Reopen in Container"
# Depois: dotnet run
```

📖 **Ver guia completo**: [QUICKSTART.md](QUICKSTART.md)

---

### Opção 1: Usando Dev Containers (Recomendado)

1. **Pré-requisitos**:
   - VS Code instalado
   - Extensão "Dev Containers" instalada
   - Podman instalado e configurado
   
   ✅ **Verificação automática**: Execute `./diagnose.sh`

2. **Abrir o projeto**:
   ```bash
   cd planning-poker
   code .
   ```

3. **Reabrir no Dev Container**:
   - Pressione `F1` ou `Ctrl+Shift+P`
   - Selecione: "Dev Containers: Reopen in Container"
   - Aguarde o container ser construído (primeira vez pode demorar)

4. **Executar a aplicação**:
   ```bash
   dotnet run
   ```

5. **Acessar**:
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001

#### 🔒 Configurações de Segurança (Podman + SELinux)

O projeto está otimizado para **Fedora** com **Podman** e **SELinux**:

- ✅ Flags SELinux configuradas (`:Z`, `--security-opt`)
- ✅ Mapeamento correto de UID/GID (`--userns=keep-id`)
- ✅ Usuário não-root (`vscode`) com permissões adequadas
- ✅ Sem problemas de "Permission denied" em volumes

📖 **Documentação detalhada**: 
- [SELINUX-SECURITY.md](SELINUX-SECURITY.md) - Guia completo de segurança
- [SECURITY-CHANGES.md](SECURITY-CHANGES.md) - Resumo das alterações
- [PODMAN.md](PODMAN.md) - Comandos úteis do Podman

### Opção 2: Execução Direta (sem container)

1. **Pré-requisitos**:
   - .NET SDK 8.0 ou superior instalado

2. **Restaurar dependências**:
   ```bash
   dotnet restore
   ```

3. **Executar**:
   ```bash
   dotnet run
   ```

4. **Acessar no navegador**:
   - http://localhost:5000

## 📖 Como Usar

### 1️⃣ Criar uma Sala

1. Acesse a página inicial
2. Preencha os campos:
   - **Seu Nome**: Como você será identificado
   - **Nome do Time**: Identificação da equipe
   - **Sprint**: Nome ou número da sprint
   - **Sequência de Votação**: Escolha entre as opções pré-definidas
3. Clique em "Criar Sala de Planning Poker"
4. Você será redirecionado para a sala como Moderador

### 2️⃣ Convidar Participantes

1. Na sala, clique no botão "🔗 Copiar Link"
2. Compartilhe o link com sua equipe
3. Cada pessoa que acessar o link digitará seu nome e entrará automaticamente

### 3️⃣ Votar

1. Cada participante escolhe um card com seu voto
2. Os votos ficam ocultos até serem revelados
3. Um indicador verde mostra quem já votou

### 4️⃣ Revelar e Analisar

1. O moderador clica em "👁️ Revelar Votos"
2. Todos os votos são exibidos simultaneamente
3. A média aritmética é calculada automaticamente (votos numéricos)

### 5️⃣ Nova Rodada

1. Clique em "🔄 Nova Rodada" para limpar os votos
2. O contador de rodadas é incrementado
3. Processo recomeça para a próxima história

## 🔧 Tecnologias Utilizadas

- **Framework**: .NET 8 / ASP.NET Core
- **UI**: Blazor Web App (Interactive Server)
- **Linguagem**: C# 12
- **CSS**: Tailwind CSS (via CDN)
- **Container**: Docker/Podman com Dev Containers
- **Persistência**: Em memória (thread-safe com `ConcurrentDictionary`)

## 🎯 Conceitos Implementados

### Thread-Safety
- Uso de `ConcurrentDictionary<TKey, TValue>` para gerenciar salas e usuários
- Serviço registrado como **Singleton** no DI container
- Operações atômicas para adicionar/remover votos

### Reatividade em Tempo Real
- Blazor Interactive Server com SignalR (implícito)
- Event system customizado (`RoomUpdated`) para notificações
- Auto-refresh dos componentes quando há mudanças

### Gerenciamento de Memória
- Timer periódico para limpeza de salas inativas
- Timestamp de última atividade em cada sala
- Remoção automática de salas sem atividade por 2+ horas

### Padrões de Design
- **Repository Pattern**: `PlanningPokerService` como abstração de dados
- **Observer Pattern**: Eventos para notificar mudanças
- **Factory Pattern**: Geração de avatares e IDs únicos

## 🛠️ Desenvolvimento

### Estrutura do Serviço Principal

```csharp
public class PlanningPokerService
{
    private readonly ConcurrentDictionary<string, Room> _rooms;
    
    // Gerenciamento de salas
    public Room CreateRoom(...)
    public Room? GetRoom(string roomId)
    
    // Gerenciamento de usuários
    public User? JoinRoom(string roomId, string userName)
    public bool LeaveRoom(string roomId, string userId)
    
    // Gerenciamento de votos
    public bool Vote(string roomId, string userId, string value)
    public bool RevealVotes(string roomId)
    public bool ResetVoting(string roomId)
    
    // Event para atualização em tempo real
    public event Action<string>? RoomUpdated;
}
```

### Modelos de Dados

- **Room**: Representa uma sala com time, sprint, sequência de votação e participantes
- **User**: Informações do usuário (nome, avatar, se é moderador)
- **Vote**: Registro de um voto individual

## 📝 Roadmap / Melhorias Futuras

- [ ] Implementar cópia do link da sala para clipboard via JSInterop
- [ ] Adicionar histórico de rodadas e votos anteriores
- [ ] Implementar modo espectador (observa sem votar)
- [ ] Adicionar temporizador para votação
- [ ] Exportar resultados da sessão (CSV/JSON)
- [ ] Temas customizáveis (dark mode, cores)
- [ ] Integração com Jira/Azure DevOps para importar histórias
- [ ] Persistência opcional em Redis/banco de dados
- [ ] Estatísticas da equipe (velocidade média, distribuição de votos)
- [ ] Suporte a múltiplos idiomas (i18n)

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. Fazer fork do projeto
2. Criar uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abrir um Pull Request

## 📄 Licença

Este projeto é livre para uso educacional e comercial.

## 👨‍💻 Autor

Desenvolvido com ❤️ usando Blazor e .NET 8

---

**⭐ Se este projeto foi útil, considere dar uma estrela!**
