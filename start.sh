#!/bin/bash

# Script de inicialização rápida do Planning Poker

echo "🎴 Planning Poker - Iniciando aplicação..."
echo ""

# Verificar se o .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK não encontrado!"
    echo "Por favor, instale o .NET SDK 8.0 ou use o Dev Container"
    exit 1
fi

# Exibir versão do .NET
echo "✅ .NET SDK encontrado:"
dotnet --version
echo ""

# Restaurar dependências
echo "📦 Restaurando dependências..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Erro ao restaurar dependências"
    exit 1
fi

echo ""
echo "🔨 Compilando aplicação..."
dotnet build

if [ $? -ne 0 ]; then
    echo "❌ Erro ao compilar aplicação"
    exit 1
fi

echo ""
echo "✅ Compilação concluída com sucesso!"
echo ""
echo "🚀 Iniciando Planning Poker..."
echo ""
echo "📍 URLs disponíveis:"
echo "   HTTP:  http://localhost:5000"
echo "   HTTPS: https://localhost:5001"
echo ""
echo "💡 Pressione Ctrl+C para parar o servidor"
echo ""

# Executar a aplicação
dotnet run
