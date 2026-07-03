# 🐳 Guia de Uso com Podman

Este documento contém instruções específicas para executar a aplicação Planning Poker usando **Podman** em vez de Docker.

## 📋 Pré-requisitos

### Instalação do Podman no Fedora

```bash
# Instalar Podman
sudo dnf install -y podman podman-docker

# Verificar instalação
podman --version
```

### Configurar Podman para o VS Code

O VS Code Dev Containers funciona nativamente com Podman. Basta configurar:

```bash
# Criar link simbólico do Docker para Podman (opcional)
sudo ln -s /usr/bin/podman /usr/bin/docker

# Ou configurar o VS Code para usar Podman diretamente
```

### Configuração do VS Code

Adicione ao seu `settings.json` do VS Code:

```json
{
  "dev.containers.dockerPath": "podman",
  "dev.containers.dockerComposePath": "podman-compose"
}
```

## 🚀 Executando com Dev Containers

### 1. Abrir o Projeto

```bash
cd planning-poker
code .
```

### 2. Reabrir no Container

- Pressione `F1` ou `Ctrl+Shift+P`
- Digite: **"Dev Containers: Reopen in Container"**
- Aguarde o build do container (primeira vez demora mais)

### 3. Verificar o Container

```bash
# Dentro do container, verificar .NET
dotnet --version

# Deve mostrar: 8.0.x
```

### 4. Executar a Aplicação

```bash
# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar
dotnet run
```

## 🔧 Comandos Úteis do Podman

### Gerenciar Containers

```bash
# Listar containers em execução
podman ps

# Listar todos os containers
podman ps -a

# Parar um container
podman stop <container-id>

# Remover um container
podman rm <container-id>

# Ver logs de um container
podman logs <container-id>

# Executar comando dentro do container
podman exec -it <container-id> /bin/bash
```

### Gerenciar Imagens

```bash
# Listar imagens
podman images

# Remover uma imagem
podman rmi <image-id>

# Limpar imagens não utilizadas
podman image prune

# Limpar tudo (CUIDADO!)
podman system prune -a
```

### Problemas Comuns e Soluções

#### 1. Porta já em uso

```bash
# Verificar portas em uso
sudo ss -tulpn | grep :5000

# Matar processo usando a porta
sudo kill -9 <PID>
```

#### 2. Permissões no Podman

Se encontrar problemas de permissão:

```bash
# Adicionar seu usuário ao grupo podman
sudo usermod -aG podman $USER

# Recarregar grupos
newgrp podman

# Logout e login novamente
```

#### 3. Erro de rede no container

```bash
# Recriar a rede padrão do Podman
podman network rm podman
podman network create podman
```

#### 4. Container não inicia

```bash
# Ver logs detalhados
podman logs -f <container-id>

# Rebuild do container
# No VS Code: F1 > Dev Containers: Rebuild Container
```

## 🔒 Executando Rootless

Podman suporta execução rootless (sem privilégios de root):

```bash
# Habilitar lingering para seu usuário (mantém containers rodando após logout)
loginctl enable-linger $USER

# Configurar recursos de usuário
echo "user.max_user_namespaces=28633" | sudo tee -a /etc/sysctl.d/99-podman.conf
sudo sysctl -p /etc/sysctl.d/99-podman.conf
```

## 📊 Monitoramento

### Ver uso de recursos

```bash
# Estatísticas de containers
podman stats

# Inspeção detalhada
podman inspect <container-id>

# Ver processos dentro do container
podman top <container-id>
```

## 🧹 Limpeza e Manutenção

### Limpeza regular

```bash
# Remover containers parados
podman container prune

# Remover imagens não utilizadas
podman image prune

# Remover volumes não utilizados
podman volume prune

# Limpeza completa (CUIDADO: remove tudo!)
podman system prune -a --volumes
```

## 🆚 Diferenças: Podman vs Docker

| Característica | Podman | Docker |
|----------------|--------|---------|
| Daemon | Não usa daemon | Usa daemon |
| Root | Pode rodar rootless | Requer root |
| Pods | Suporte nativo | Não suporta |
| Compatibilidade | 99% compatível com Docker | - |
| SystemD | Integração nativa | Requer configuração |

## 📚 Recursos Adicionais

- [Documentação Oficial do Podman](https://docs.podman.io/)
- [Podman Desktop](https://podman-desktop.io/) - Interface gráfica
- [Tutorial de Migração Docker → Podman](https://podman.io/getting-started/docker)

## 💡 Dicas

1. **Alias para Docker**: Se você está acostumado com Docker, adicione ao `~/.bashrc`:
   ```bash
   alias docker=podman
   alias docker-compose=podman-compose
   ```

2. **Auto-update de containers**:
   ```bash
   # Habilitar auto-update
   podman auto-update
   ```

3. **Backup de containers**:
   ```bash
   # Salvar imagem
   podman save -o planning-poker.tar localhost/planning-poker
   
   # Carregar imagem
   podman load -i planning-poker.tar
   ```

4. **Integração com SystemD**:
   ```bash
   # Gerar service unit
   podman generate systemd --name planning-poker > ~/.config/systemd/user/planning-poker.service
   
   # Habilitar e iniciar
   systemctl --user enable --now planning-poker.service
   ```

## 🐛 Debug

Se algo não funcionar:

1. Verifique os logs: `podman logs <container>`
2. Inspecione o container: `podman inspect <container>`
3. Execute shell interativo: `podman exec -it <container> /bin/bash`
4. Reconstrua o container limpo: No VS Code, use "Rebuild Container"

---

**✨ Dica Final**: Podman é drop-in replacement para Docker. A maioria dos comandos funciona identicamente!
