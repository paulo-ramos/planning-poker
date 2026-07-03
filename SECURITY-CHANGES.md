# ✅ Configurações de Segurança - Resumo das Alterações

## 🎯 Objetivo

Garantir compatibilidade total com **Podman** e **SELinux** no Fedora, evitando problemas de permissão de escrita em volumes.

## 🔧 Alterações Aplicadas

### 1. `.devcontainer/devcontainer.json`

#### ✅ Adicionado ao `runArgs`:
```json
"runArgs": [
  "--name=planning-poker-devcontainer",
  "--userns=keep-id",              // ✅ Mantém UID do host no container
  "--security-opt=label=disable"   // ✅ Desabilita labels SELinux no container
]
```

**Por quê?**
- `--userns=keep-id`: Resolve conflitos de UID entre host e container
- `--security-opt=label=disable`: Permite escrita no volume sem bloqueios do SELinux

#### ✅ Configuração de Mounts:
```json
"workspaceMount": "source=${localWorkspaceFolder},target=/workspace,type=bind,Z",
"workspaceFolder": "/workspace"
```

**Por quê?**
- `:Z`: Aplica relabel privado do SELinux ao volume
- Garante que apenas este container tenha acesso ao volume

#### ✅ `remoteUser` Confirmado:
```json
"remoteUser": "vscode"
```

**Status**: ✅ Configurado corretamente - executa como usuário não-root

---

### 2. `.devcontainer/Dockerfile`

#### ✅ Permissões no Workspace:
```dockerfile
# Configurar diretório de trabalho com permissões corretas
WORKDIR /workspace

# Dar permissões ao usuário vscode no diretório de trabalho
RUN chown -R $USERNAME:$USERNAME /workspace

# Mudar para o usuário vscode
USER $USERNAME
```

**Por quê?**
- Garante que o usuário `vscode` (UID 1000) tenha permissões completas
- Evita "Permission denied" ao criar/modificar arquivos

#### ✅ Usuário vscode:
```dockerfile
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME
```

**Status**: ✅ Criado corretamente com UID/GID 1000 (padrão Fedora)

---

## 🔍 Verificação das Configurações

### Teste Rápido

Execute o script de diagnóstico:

```bash
./diagnose.sh
```

Este script verifica:
- ✅ Podman instalado e configurado
- ✅ SELinux status e contexto
- ✅ UID/GID compatíveis
- ✅ Permissões do diretório
- ✅ VS Code e extensões
- ✅ Portas disponíveis
- ✅ Arquivos essenciais do projeto

---

## 📊 Comparação: Antes vs Depois

| Aspecto | ❌ Antes | ✅ Depois |
|---------|---------|-----------|
| SELinux | Bloqueava escrita | Labels desabilitados no container |
| UID Mapping | Potencial conflito | `--userns=keep-id` resolve |
| Permissões /workspace | Não garantidas | `chown` explícito aplicado |
| Volume Mount | Sem flags SELinux | Flag `:Z` aplicada |
| Segurança | Padrão | Balanceada (não-root + SELinux ajustado) |

---

## 🚨 Problemas Conhecidos e Soluções

### Problema 1: "Permission denied" ao escrever arquivos

**Causa**: SELinux bloqueando operações no volume

**Solução Aplicada**:
- ✅ `--security-opt=label=disable` no `runArgs`
- ✅ Flag `:Z` no mount do workspace

**Teste**:
```bash
# Dentro do container
touch /workspace/test.txt && rm /workspace/test.txt
# Não deve dar erro
```

---

### Problema 2: Arquivos criados pertencem a root

**Causa**: UID mismatch entre host e container

**Solução Aplicada**:
- ✅ `--userns=keep-id` mantém UID do host
- ✅ Usuário `vscode` com UID 1000
- ✅ `chown` no diretório `/workspace`

**Teste**:
```bash
# Dentro do container
whoami           # Deve retornar: vscode
id -u            # Deve retornar: 1000 (ou seu UID)
ls -la /workspace   # Arquivos devem pertencer a vscode
```

---

### Problema 3: SELinux ainda está bloqueando

**Causa Rara**: Configuração não foi aplicada corretamente

**Diagnóstico**:
```bash
# No host
sudo ausearch -m avc -ts recent | grep podman
```

**Solução Adicional**:
```bash
# Aplicar contexto correto manualmente
sudo chcon -R -t container_file_t /home/paulo/Projects/planning-poker

# Ou temporariamente colocar SELinux em permissive
sudo setenforce 0  # Apenas para teste - reverta depois
```

---

## 🎓 Entendendo as Flags

### `--userns=keep-id`

**O que faz**: Mapeia seu UID do host (ex: 1000) para o mesmo UID dentro do container

**Sem a flag**:
- Host: paulo (UID 1000)
- Container: vscode aparece como UID diferente
- Arquivos criados no volume ficam com owner errado

**Com a flag**:
- Host: paulo (UID 1000)
- Container: vscode (UID 1000) 
- Arquivos mantêm permissões corretas

---

### `--security-opt=label=disable`

**O que faz**: Desabilita labels do SELinux apenas dentro deste container

**Importante**: 
- ❌ Não desabilita SELinux no host
- ✅ Permite que o container escreva no volume
- ✅ Ainda mantém isolamento de processos

**Alternativas**:
- Sem esta flag: Use `:z` em vez de `:Z` no mount (menos seguro)
- Mais restritivo: Remover flag e configurar contextos SELinux manualmente

---

### Flag `:Z` no Mount

**O que faz**: Relabel privado - apenas este container pode acessar

**Opções**:
- `:Z` (uppercase): Relabel privado - recomendado para containers únicos
- `:z` (lowercase): Relabel compartilhado - para múltiplos containers no mesmo volume
- Sem flag: SELinux pode bloquear acesso

---

## 📚 Documentação Adicional

- 📄 **SELINUX-SECURITY.md**: Guia completo de segurança e troubleshooting
- 📄 **PODMAN.md**: Comandos úteis do Podman para Fedora
- 📄 **README.md**: Documentação geral do projeto

---

## ✅ Checklist de Validação

Antes de começar o desenvolvimento:

- [ ] Executar `./diagnose.sh` sem erros críticos
- [ ] Confirmar SELinux status (`getenforce`)
- [ ] Verificar UID do host (`id -u`) = 1000
- [ ] Podman funciona sem sudo (`podman ps`)
- [ ] VS Code com extensão Dev Containers instalada
- [ ] Portas 5000 e 5001 disponíveis

---

## 🎯 Próximos Passos

1. **Execute o diagnóstico**:
   ```bash
   ./diagnose.sh
   ```

2. **Abra no VS Code**:
   ```bash
   code .
   ```

3. **Reabra no Container**:
   - Pressione `F1`
   - Selecione: "Dev Containers: Reopen in Container"

4. **Teste a aplicação**:
   ```bash
   dotnet run
   ```

5. **Acesse no navegador**:
   - http://localhost:5000

---

**🎉 Configuração completa e otimizada para Fedora + Podman + SELinux!**
