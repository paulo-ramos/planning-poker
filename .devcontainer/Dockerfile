# Dev Container para Planning Poker com .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy

# Instalar dependências adicionais
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    git \
    curl \
    wget \
    ca-certificates \
    sudo \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

# Criar usuário vscode para desenvolvimento
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

# Configurar diretório de trabalho com permissões corretas
WORKDIR /workspace

# Dar permissões ao usuário vscode no diretório de trabalho
RUN chown -R $USERNAME:$USERNAME /workspace

# Mudar para o usuário vscode
USER $USERNAME

# Configurar Git para confiar no diretório de trabalho
RUN git config --global --add safe.directory /workspace

# Instalar ferramentas .NET globais úteis
RUN dotnet tool install --global dotnet-ef

# Tentar instalar dotnet-aspnet-codegenerator (opcional, pode falhar)
RUN dotnet tool install --global dotnet-aspnet-codegenerator || true

# Adicionar ferramentas ao PATH
ENV PATH="${PATH}:/home/${USERNAME}/.dotnet/tools"

# Expor portas para desenvolvimento
EXPOSE 5000 5001

CMD ["/bin/bash"]
