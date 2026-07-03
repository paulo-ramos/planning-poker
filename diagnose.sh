#!/bin/bash

# Script de diagnóstico para Dev Container com Podman e SELinux
# Verifica configurações e identifica problemas potenciais

echo "🔍 Planning Poker - Diagnóstico de Ambiente"
echo "=============================================="
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para verificações
check_ok() {
    echo -e "${GREEN}✓${NC} $1"
}

check_warn() {
    echo -e "${YELLOW}⚠${NC} $1"
}

check_error() {
    echo -e "${RED}✗${NC} $1"
}

# 1. Verificar Sistema Operacional
echo "📋 Sistema Operacional:"
if [ -f /etc/os-release ]; then
    . /etc/os-release
    echo "   Distro: $NAME $VERSION"
    if [[ "$ID" == "fedora" ]]; then
        check_ok "Fedora detectado - configurações otimizadas para este sistema"
    else
        check_warn "Sistema: $ID - configurações foram otimizadas para Fedora"
    fi
else
    check_warn "Não foi possível detectar a distribuição"
fi
echo ""

# 2. Verificar Podman
echo "🐳 Podman:"
if command -v podman &> /dev/null; then
    PODMAN_VERSION=$(podman --version)
    check_ok "Podman instalado: $PODMAN_VERSION"
    
    # Verificar se pode executar sem sudo
    if podman ps &> /dev/null; then
        check_ok "Podman funciona sem sudo (rootless)"
    else
        check_error "Podman requer sudo ou não está configurado corretamente"
        echo "   Execute: sudo usermod -aG podman $USER && newgrp podman"
    fi
else
    check_error "Podman não está instalado"
    echo "   Execute: sudo dnf install -y podman podman-docker"
    exit 1
fi
echo ""

# 3. Verificar .NET SDK
echo "⚙️  .NET SDK:"
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    check_ok ".NET SDK instalado: $DOTNET_VERSION"
    
    if [[ "$DOTNET_VERSION" == 8.* ]]; then
        check_ok ".NET 8 detectado - versão correta para o projeto"
    else
        check_warn ".NET versão $DOTNET_VERSION - projeto requer .NET 8"
    fi
else
    check_warn ".NET SDK não instalado no host (OK se usar Dev Container)"
fi
echo ""

# 4. Verificar SELinux
echo "🔒 SELinux:"
if command -v getenforce &> /dev/null; then
    SELINUX_STATUS=$(getenforce)
    echo "   Status: $SELINUX_STATUS"
    
    case "$SELINUX_STATUS" in
        "Enforcing")
            check_ok "SELinux está ativo - configurações do projeto incluem suporte"
            ;;
        "Permissive")
            check_warn "SELinux está em modo permissivo (apenas alerta, não bloqueia)"
            ;;
        "Disabled")
            check_warn "SELinux está desabilitado - flags SELinux no projeto são desnecessárias"
            ;;
    esac
else
    check_warn "SELinux não está disponível neste sistema"
fi
echo ""

# 5. Verificar UID/GID
echo "👤 Usuário e Permissões:"
CURRENT_UID=$(id -u)
CURRENT_GID=$(id -g)
echo "   UID: $CURRENT_UID"
echo "   GID: $CURRENT_GID"

if [ "$CURRENT_UID" -eq 1000 ] && [ "$CURRENT_GID" -eq 1000 ]; then
    check_ok "UID/GID padrão (1000) - compatível com usuário 'vscode' no container"
else
    check_warn "UID/GID não padrão - você pode ter problemas de permissão"
    echo "   Considere ajustar USER_UID/USER_GID no Dockerfile para: $CURRENT_UID/$CURRENT_GID"
fi
echo ""

# 6. Verificar VS Code
echo "💻 Visual Studio Code:"
if command -v code &> /dev/null; then
    check_ok "VS Code está instalado"
    
    # Verificar extensão Dev Containers
    if code --list-extensions | grep -q "ms-vscode-remote.remote-containers"; then
        check_ok "Extensão Dev Containers está instalada"
    else
        check_warn "Extensão Dev Containers não encontrada"
        echo "   Instale: code --install-extension ms-vscode-remote.remote-containers"
    fi
else
    check_warn "VS Code não encontrado no PATH"
fi
echo ""

# 7. Verificar Permissões do Diretório
echo "📁 Permissões do Projeto:"
PROJECT_DIR="/home/paulo/Projects/planning-poker"

if [ -d "$PROJECT_DIR" ]; then
    PROJECT_OWNER=$(stat -c '%U' "$PROJECT_DIR")
    PROJECT_PERMS=$(stat -c '%a' "$PROJECT_DIR")
    
    echo "   Proprietário: $PROJECT_OWNER"
    echo "   Permissões: $PROJECT_PERMS"
    
    if [ "$PROJECT_OWNER" == "$USER" ]; then
        check_ok "Você é o proprietário do diretório do projeto"
    else
        check_error "Proprietário do diretório: $PROJECT_OWNER (esperado: $USER)"
    fi
    
    if [ -w "$PROJECT_DIR" ]; then
        check_ok "Você tem permissão de escrita no diretório"
    else
        check_error "Sem permissão de escrita no diretório do projeto"
    fi
    
    # Verificar contexto SELinux
    if command -v ls &> /dev/null && ls --version 2>&1 | grep -q "SELinux"; then
        SELINUX_CONTEXT=$(ls -Zd "$PROJECT_DIR" 2>/dev/null | awk '{print $1}')
        if [ -n "$SELINUX_CONTEXT" ]; then
            echo "   Contexto SELinux: $SELINUX_CONTEXT"
        fi
    fi
else
    check_error "Diretório do projeto não encontrado: $PROJECT_DIR"
fi
echo ""

# 8. Verificar Containers Existentes
echo "📦 Containers Existentes:"
if podman ps -a --format "{{.Names}}" 2>/dev/null | grep -q "planning-poker"; then
    check_warn "Containers planning-poker existentes encontrados:"
    podman ps -a --filter "name=planning-poker" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    echo ""
    echo "   Para limpar: podman rm -f \$(podman ps -aq --filter 'name=planning-poker')"
else
    check_ok "Nenhum container planning-poker existente"
fi
echo ""

# 9. Verificar Portas
echo "🌐 Portas Disponíveis:"
for PORT in 5000 5001; do
    if ss -tuln 2>/dev/null | grep -q ":$PORT "; then
        check_warn "Porta $PORT já está em uso"
        echo "   Processo usando a porta:"
        sudo ss -tulpn 2>/dev/null | grep ":$PORT " || echo "   (não foi possível determinar o processo)"
    else
        check_ok "Porta $PORT está disponível"
    fi
done
echo ""

# 10. Verificar Arquivos do Projeto
echo "📄 Arquivos Essenciais:"
ESSENTIAL_FILES=(
    ".devcontainer/devcontainer.json"
    ".devcontainer/Dockerfile"
    "PlanningPoker.csproj"
    "Program.cs"
    "Components/App.razor"
)

for FILE in "${ESSENTIAL_FILES[@]}"; do
    if [ -f "$PROJECT_DIR/$FILE" ]; then
        check_ok "$FILE"
    else
        check_error "$FILE não encontrado"
    fi
done
echo ""

# 11. Resumo e Recomendações
echo "=============================================="
echo "📊 RESUMO E RECOMENDAÇÕES"
echo "=============================================="
echo ""

# Contador de problemas
PROBLEMS=0

# Verificações críticas
if ! command -v podman &> /dev/null; then
    echo "🔴 CRÍTICO: Instale o Podman antes de continuar"
    PROBLEMS=$((PROBLEMS+1))
fi

if ! podman ps &> /dev/null 2>&1; then
    echo "🔴 CRÍTICO: Configure permissões do Podman"
    echo "   sudo usermod -aG podman $USER"
    echo "   newgrp podman"
    PROBLEMS=$((PROBLEMS+1))
fi

if [ ! -f "$PROJECT_DIR/.devcontainer/devcontainer.json" ]; then
    echo "🔴 CRÍTICO: Arquivos do Dev Container não encontrados"
    PROBLEMS=$((PROBLEMS+1))
fi

if command -v getenforce &> /dev/null && [ "$(getenforce)" == "Enforcing" ]; then
    echo "ℹ️  SELinux está ativo - as configurações do projeto incluem suporte completo"
    echo "   As flags :Z e --security-opt=label=disable foram aplicadas"
fi

if [ "$CURRENT_UID" -ne 1000 ]; then
    echo "⚠️  AVISO: Seu UID ($CURRENT_UID) difere do padrão (1000)"
    echo "   Você pode ter problemas de permissão dentro do container"
    echo "   Solução: Ajustar USER_UID no .devcontainer/Dockerfile"
fi

echo ""

if [ $PROBLEMS -eq 0 ]; then
    echo -e "${GREEN}✅ Sistema pronto para usar Dev Containers!${NC}"
    echo ""
    echo "Para começar:"
    echo "1. Abra o VS Code no diretório do projeto:"
    echo "   cd $PROJECT_DIR && code ."
    echo ""
    echo "2. Pressione F1 e selecione:"
    echo "   'Dev Containers: Reopen in Container'"
    echo ""
    echo "3. Aguarde o build do container (primeira vez demora)"
    echo ""
    echo "4. Execute a aplicação:"
    echo "   dotnet run"
else
    echo -e "${RED}⚠️  $PROBLEMS problema(s) crítico(s) encontrado(s)${NC}"
    echo "   Corrija os problemas acima antes de continuar"
fi

echo ""
echo "📚 Documentação adicional:"
echo "   - README.md: Guia completo do projeto"
echo "   - PODMAN.md: Comandos e troubleshooting do Podman"
echo "   - SELINUX-SECURITY.md: Configurações de segurança e SELinux"
echo ""
