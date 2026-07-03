# 🔒 Configurações de Segurança e Permissões - Podman + SELinux

Este documento explica as configurações de segurança aplicadas ao Dev Container para garantir compatibilidade total com **Podman** e **SELinux** no Fedora.

## 🎯 Problemas Comuns Resolvidos

### 1. Permissões de Escrita no Volume

**Problema**: No Fedora com SELinux habilitado, o Podman pode bloquear operações de escrita no volume montado.

**Solução Implementada**:
- Flag `:Z` nos mounts de volume (relabel com contexto privado do container)
- `--security-opt=label=disable` para desabilitar labels do SELinux no container

### 2. Mapeamento de UID/GID

**Problema**: O UID do usuário dentro do container pode não corresponder ao UID do host.

**Solução Implementada**:
- `--userns=keep-id` mantém o UID do host dentro do container
- Usuário `vscode` criado com UID/GID 1000 (padrão)
- Permissões corretas no diretório `/workspace`

## 📋 Configurações Aplicadas

### devcontainer.json

```json
{
  "runArgs": [
    "--name=planning-poker-devcontainer",
    "--userns=keep-id",              // Mantém UID do host
    "--security-opt=label=disable"   // Desabilita SELinux labels
  ],
  "workspaceMount": "source=${localWorkspaceFolder},target=/workspace,type=bind,Z",
  "remoteUser": "vscode"             // Usuário não-root dentro do container
}
```

### Flags Explicadas

| Flag | Propósito | Impacto |
|------|-----------|---------|
| `--userns=keep-id` | Mapeia seu UID do host para o container | Evita problemas de permissão de arquivos |
| `--security-opt=label=disable` | Desabilita labels do SELinux | Permite escrita no volume sem restrições |
| `:Z` no mount | Relabel privado do SELinux | Apenas este container pode acessar |
| `remoteUser: vscode` | Executa como usuário não-root | Segurança adicional |

## 🔍 Verificação de Configuração

### Verificar SELinux Status

```bash
# Verificar se SELinux está ativo
getenforce

# Deve retornar: Enforcing, Permissive ou Disabled
```

### Verificar Permissões no Container

```bash
# Dentro do container
whoami
# Deve retornar: vscode

id
# Deve mostrar uid=1000(vscode) gid=1000(vscode)

# Testar permissão de escrita
touch /workspace/test.txt && rm /workspace/test.txt
# Não deve dar erro
```

### Verificar Labels do SELinux

```bash
# No host, verificar contexto do volume
ls -Z /home/paulo/Projects/planning-poker

# Deve mostrar algo como:
# unconfined_u:object_r:container_file_t:s0:c123,c456
```

## 🛠️ Solução de Problemas

### Problema: "Permission denied" ao criar arquivos

**Diagnóstico**:
```bash
# Dentro do container
ls -la /workspace
# Verificar owner e permissões
```

**Solução 1**: Verificar se SELinux está bloqueando
```bash
# No host
sudo ausearch -m avc -ts recent | grep podman
# Se houver resultados, o SELinux está bloqueando
```

**Solução 2**: Ajustar contexto do SELinux
```bash
# No host, aplicar contexto correto
sudo chcon -R -t container_file_t /home/paulo/Projects/planning-poker
```

**Solução 3**: Recriar container com flag adicional
```bash
# Adicionar ao runArgs no devcontainer.json
"--privileged"  // ÚLTIMA OPÇÃO - reduz segurança
```

### Problema: UID mismatch entre host e container

**Diagnóstico**:
```bash
# No host
id -u
# Anotar o UID

# Dentro do container
id -u
# Deve ser o mesmo
```

**Solução**: Ajustar UID no Dockerfile
```dockerfile
ARG USER_UID=1000
ARG USER_GID=1000

# Ou usar o UID do seu usuário host:
# ARG USER_UID=<seu-uid>
# ARG USER_GID=<seu-gid>
```

### Problema: Container não inicia

**Diagnóstico**:
```bash
# Ver logs do Podman
podman logs planning-poker-devcontainer
```

**Soluções Comuns**:
```bash
# 1. Remover container e imagem
podman rm -f planning-poker-devcontainer
podman rmi localhost/planning-poker

# 2. Rebuild completo no VS Code
# F1 > Dev Containers: Rebuild Container Without Cache

# 3. Verificar permissões do socket Podman
ls -la /run/user/$(id -u)/podman/podman.sock
```

## 🔐 Níveis de Segurança

### Configuração Atual (Recomendada)

✅ **Balanço entre segurança e usabilidade**
- Usuário não-root (vscode)
- SELinux labels desabilitados apenas no container
- Mapeamento correto de UID/GID
- Isolamento de namespaces

### Alternativa: Máxima Segurança

```json
{
  "runArgs": [
    "--userns=keep-id",
    // Remover: "--security-opt=label=disable"
  ],
  "workspaceMount": "source=${localWorkspaceFolder},target=/workspace,type=bind,z"
  // Usar :z (minúsculo) em vez de :Z
}
```

⚠️ Pode causar problemas de permissão, mas mantém SELinux totalmente ativo.

### Alternativa: Desenvolvimento Rápido

```json
{
  "runArgs": [
    "--userns=keep-id",
    "--privileged",  // Acesso completo ao sistema
    "--security-opt=label=disable"
  ]
}
```

⚠️ Menos seguro, mas elimina quase todos os problemas de permissão.

## 📚 Referências

### Documentação Oficial

- [Podman e SELinux](https://docs.podman.io/en/latest/markdown/podman-run.1.html#security-opt-option)
- [Dev Containers com Podman](https://code.visualstudio.com/remote/advancedcontainers/docker-options)
- [SELinux para Containers](https://access.redhat.com/documentation/en-us/red_hat_enterprise_linux/8/html/using_selinux/using-selinux-with-containers_using-selinux)

### Comandos Úteis

```bash
# Verificar contexto de segurança
podman inspect planning-poker-devcontainer | grep -i selinux

# Ver processos do container com contexto SELinux
ps axZ | grep planning-poker

# Restaurar contextos padrão do SELinux
sudo restorecon -R /home/paulo/Projects/planning-poker

# Modo permissivo temporário (desenvolvimento)
sudo setenforce 0  # Temporário até reboot
sudo setenforce 1  # Voltar para enforcing
```

## ✅ Checklist de Verificação

Antes de reportar problemas, verifique:

- [ ] SELinux está ativo? (`getenforce`)
- [ ] Podman está atualizado? (`podman --version`)
- [ ] UID do host corresponde ao UID no container? (`id -u`)
- [ ] Permissões do diretório do projeto? (`ls -la ../planning-poker`)
- [ ] Container foi reconstruído após mudanças? (Rebuild Container)
- [ ] Logs do container mostram erros? (`podman logs`)
- [ ] Extensões do VS Code estão atualizadas?

## 🚨 Importante para Fedora

O **Fedora** vem com SELinux em modo **Enforcing** por padrão. As configurações neste projeto foram otimizadas especificamente para:

1. ✅ Fedora 38, 39, 40+
2. ✅ Podman 4.x e 5.x
3. ✅ VS Code com extensão Dev Containers
4. ✅ SELinux em modo Enforcing

Se você usar outra distribuição (Ubuntu, Debian, etc.), pode remover as flags relacionadas ao SELinux do `devcontainer.json`.

## 💡 Dicas Finais

1. **Sempre use `--userns=keep-id`** com Podman (evita problemas de UID)
2. **A flag `:Z` é exclusiva do container** (não compartilha com outros containers)
3. **Use `:z` (minúsculo)** se múltiplos containers precisam acessar o mesmo volume
4. **`--security-opt=label=disable`** é segura para desenvolvimento local
5. **Não use `--privileged`** a menos que absolutamente necessário

---

**🎯 Com estas configurações, seu ambiente está otimizado para desenvolvimento no Fedora com Podman e SELinux!**
