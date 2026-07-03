# 🚀 Início Rápido - Planning Poker

## ⚡ TL;DR - Início em 3 Comandos

```bash
# 1. Diagnóstico do sistema
./diagnose.sh

# 2. Abrir no VS Code
code .

# 3. No VS Code: F1 → "Dev Containers: Reopen in Container"
# Aguarde o build e execute: dotnet run
```

---

## 📋 Pré-requisitos Mínimos

| Requisito | Comando de Verificação | Como Instalar (Fedora) |
|-----------|------------------------|------------------------|
| **Podman** | `podman --version` | `sudo dnf install podman podman-docker` |
| **VS Code** | `code --version` | [code.visualstudio.com](https://code.visualstudio.com) |
| **Extensão Dev Containers** | `code --list-extensions \| grep remote-containers` | No VS Code: Extensions → "Dev Containers" |

---

## 🏃 Primeira Execução

### 1️⃣ Verificar Sistema

```bash
cd /home/paulo/Projects/planning-poker
./diagnose.sh
```

**Resultado esperado**: ✅ Sistema pronto para usar Dev Containers

---

### 2️⃣ Abrir no VS Code

```bash
code .
```

---

### 3️⃣ Iniciar Dev Container

No VS Code:
1. Pressione `F1` (ou `Ctrl+Shift+P`)
2. Digite: `Dev Containers: Reopen in Container`
3. Enter

**⏱️ Primeira vez**: ~5-10 minutos (build da imagem)  
**⚡ Próximas vezes**: ~30 segundos

---

### 4️⃣ Executar Aplicação

No terminal do VS Code (dentro do container):

```bash
dotnet run
```

**URLs**:
- 🌐 HTTP: http://localhost:5000
- 🔒 HTTPS: https://localhost:5001

---

## 🔥 Comandos Úteis

### Dentro do Container (VS Code Terminal)

```bash
# Executar aplicação
dotnet run

# Executar com hot reload
dotnet watch run

# Compilar
dotnet build

# Limpar build
dotnet clean

# Restaurar pacotes
dotnet restore
```

---

### No Host (Terminal do Fedora)

```bash
# Ver containers rodando
podman ps

# Ver logs do container
podman logs planning-poker-devcontainer

# Parar container
podman stop planning-poker-devcontainer

# Remover container
podman rm planning-poker-devcontainer

# Limpar tudo (cuidado!)
podman system prune -a
```

---

## 🎯 Fluxo de Desenvolvimento

```
┌─────────────────────────────────────────────────┐
│  1. Abrir VS Code                               │
│     code .                                      │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  2. Reabrir no Container                        │
│     F1 → "Reopen in Container"                  │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  3. Aguardar Build (primeira vez)               │
│     • Download da imagem base                   │
│     • Instalação do .NET SDK                    │
│     • Setup do usuário vscode                   │
│     • Restaurar dependências                    │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  4. Desenvolvimento                             │
│     • Editar código                             │
│     • dotnet watch run (hot reload)             │
│     • Testar no navegador                       │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  5. Commit & Push                               │
│     • Git funciona normalmente                  │
│     • Arquivos pertencem ao seu usuário         │
└─────────────────────────────────────────────────┘
```

---

## 🐛 Troubleshooting Rápido

### ❌ Erro: "Permission denied" ao escrever

**Solução**:
```bash
# No host
sudo chcon -R -t container_file_t /home/paulo/Projects/planning-poker
```

---

### ❌ Porta 5000 já em uso

**Solução**:
```bash
# Verificar o que está usando a porta
sudo ss -tulpn | grep :5000

# Matar processo (substitua <PID>)
sudo kill -9 <PID>
```

---

### ❌ Container não inicia

**Solução**:
```bash
# Limpar e reconstruir
podman rm -f planning-poker-devcontainer
# No VS Code: F1 → "Rebuild Container Without Cache"
```

---

### ❌ SELinux bloqueando

**Diagnóstico**:
```bash
sudo ausearch -m avc -ts recent | grep podman
```

**Solução Temporária**:
```bash
sudo setenforce 0  # Apenas para teste
# Desenvolva e depois reverta:
sudo setenforce 1
```

---

## 📖 Estrutura do Projeto

```
planning-poker/
├── .devcontainer/          # 🐳 Configuração do Dev Container
│   ├── devcontainer.json   #    • Configurações do VS Code + Podman
│   └── Dockerfile          #    • Imagem .NET 8 + usuário vscode
│
├── Components/             # ⚛️ Componentes Blazor
│   ├── Pages/
│   │   ├── Home.razor      #    • Tela de criação de sala
│   │   └── Room.razor      #    • Sala de votação em tempo real
│   └── Layout/
│       └── MainLayout.razor
│
├── Models/                 # 📦 Modelos de domínio
│   ├── Room.cs            #    • Sala de Planning Poker
│   ├── User.cs            #    • Usuário/Participante
│   └── Vote.cs            #    • Voto individual
│
├── Services/              # ⚙️ Lógica de negócio
│   └── PlanningPokerService.cs  # • Singleton thread-safe
│
├── wwwroot/               # 🎨 Assets estáticos
│   └── app.css           #    • Estilos Tailwind CSS
│
├── Program.cs             # 🚀 Entry point da aplicação
├── PlanningPoker.csproj   # 📄 Arquivo do projeto .NET
│
└── Scripts/               # 🔧 Utilitários
    ├── start.sh          #    • Inicialização rápida
    └── diagnose.sh       #    • Diagnóstico do ambiente
```

---

## 🎮 Como Usar a Aplicação

### 1. Criar Sala

1. Acesse http://localhost:5000
2. Preencha:
   - Seu Nome
   - Nome do Time
   - Nome da Sprint
   - Sequência de Votação (Fibonacci, etc)
3. Clique "Criar Sala"

---

### 2. Convidar Participantes

1. Na sala, clique "🔗 Copiar Link"
2. Envie o link para sua equipe
3. Cada pessoa:
   - Acessa o link
   - Digita seu nome
   - Entra na sala automaticamente

---

### 3. Votar

1. Cada participante clica em um card (1, 2, 3, 5, 8...)
2. Status muda para "Votou" (voto oculto)
3. Aguarda outros votarem

---

### 4. Revelar

1. Moderador clica "👁️ Revelar Votos"
2. Todos os votos aparecem
3. Média é calculada automaticamente

---

### 5. Nova Rodada

1. Moderador clica "🔄 Nova Rodada"
2. Votos são limpos
3. Processo recomeça

---

## 🎨 Tecnologias

- **Backend**: C# / .NET 8
- **Frontend**: Blazor Server (Interactive)
- **Tempo Real**: SignalR (implícito no Blazor)
- **Estilos**: Tailwind CSS
- **Container**: Podman + Dev Containers
- **Estado**: Em memória (ConcurrentDictionary)

---

## 📚 Documentação Completa

| Documento | Conteúdo |
|-----------|----------|
| **README.md** | Documentação completa do projeto |
| **PODMAN.md** | Guia do Podman para Fedora |
| **SELINUX-SECURITY.md** | Configurações de segurança detalhadas |
| **SECURITY-CHANGES.md** | Resumo das alterações de segurança |
| **Este arquivo** | Início rápido e referência |

---

## 💡 Dicas

1. **Use `dotnet watch run`** para hot reload automático
2. **Compartilhe o link da sala** facilmente com QR code (futuro)
3. **SELinux está ativo?** Não se preocupe - configurado automaticamente
4. **Container não inicia?** Execute `./diagnose.sh` primeiro
5. **Problemas de permissão?** Verifique se UID = 1000 (`id -u`)

---

## 🆘 Precisa de Ajuda?

```bash
# Execute o diagnóstico completo
./diagnose.sh

# Veja os logs do container
podman logs planning-poker-devcontainer

# Verifique erros do .NET
dotnet build
```

**Documentação detalhada**: Leia os arquivos `.md` na raiz do projeto

---

**🎉 Pronto para começar? Execute `./diagnose.sh` e depois `code .`**
