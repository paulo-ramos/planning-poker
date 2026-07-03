// Funções JavaScript para Planning Poker

/**
 * Copia texto para o clipboard
 */
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Erro ao copiar para o clipboard:', err);
        return false;
    }
};

/**
 * Armazena o userId no sessionStorage
 */
window.saveUserId = (roomId, userId) => {
    sessionStorage.setItem(`room_${roomId}_userId`, userId);
};

/**
 * Recupera o userId do sessionStorage
 */
window.getUserId = (roomId) => {
    return sessionStorage.getItem(`room_${roomId}_userId`);
};

/**
 * Remove o userId do sessionStorage
 */
window.removeUserId = (roomId) => {
    sessionStorage.removeItem(`room_${roomId}_userId`);
};

/**
 * Mostra uma notificação toast
 */
window.showToast = (message, type = 'success') => {
    // Criar elemento de toast
    const toast = document.createElement('div');
    toast.className = `fixed top-4 right-4 px-6 py-3 rounded-xl shadow-lg text-white font-semibold z-50 transition-all duration-300 ${
        type === 'success' ? 'bg-green-600' : 'bg-red-600'
    }`;
    toast.textContent = message;
    toast.style.opacity = '0';
    toast.style.transform = 'translateY(-20px)';

    document.body.appendChild(toast);

    // Animação de entrada
    setTimeout(() => {
        toast.style.opacity = '1';
        toast.style.transform = 'translateY(0)';
    }, 10);

    // Remover após 3 segundos
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(-20px)';
        setTimeout(() => {
            document.body.removeChild(toast);
        }, 300);
    }, 3000);
};

/**
 * Anima o disparo do canhão
 */
window.animateCannonShot = (fromUserId, toUserId, projectile) => {
    const targetElement = document.querySelector(`[data-user-id="${toUserId}"]`);
    const cannonElement = document.getElementById('cannon-icon');

    if (!targetElement || !cannonElement) {
        console.log('Target ou canhão não encontrado');
        return;
    }

    // Posição do canhão
    const cannonRect = cannonElement.getBoundingClientRect();
    const cannonX = cannonRect.left + cannonRect.width / 2;
    const cannonY = cannonRect.top + cannonRect.height / 2;

    // Posição do alvo
    const targetRect = targetElement.getBoundingClientRect();
    const targetX = targetRect.left + targetRect.width / 2;
    const targetY = targetRect.top + targetRect.height / 2;

    // Criar projétil
    const projectileElement = document.createElement('div');
    projectileElement.textContent = projectile;
    projectileElement.className = 'fixed text-4xl pointer-events-none z-[60]';
    projectileElement.style.left = `${cannonX}px`;
    projectileElement.style.top = `${cannonY}px`;
    projectileElement.style.transition = 'all 0.8s cubic-bezier(0.25, 0.46, 0.45, 0.94)';

    document.body.appendChild(projectileElement);

    // Efeito de disparo no canhão
    cannonElement.style.transform = 'rotate(-45deg) scale(1.3)';
    setTimeout(() => {
        cannonElement.style.transform = 'rotate(0deg) scale(1)';
    }, 200);

    // Animar projétil
    setTimeout(() => {
        projectileElement.style.left = `${targetX}px`;
        projectileElement.style.top = `${targetY}px`;
        projectileElement.style.transform = 'rotate(360deg) scale(1.5)';
    }, 50);

    // Impacto no alvo
    setTimeout(() => {
        // Efeito de shake no alvo
        targetElement.style.animation = 'shake 0.5s';

        // Criar explosão de partículas
        for (let i = 0; i < 8; i++) {
            const particle = document.createElement('div');
            particle.textContent = projectile;
            particle.className = 'fixed text-2xl pointer-events-none z-[60]';
            particle.style.left = `${targetX}px`;
            particle.style.top = `${targetY}px`;

            const angle = (Math.PI * 2 * i) / 8;
            const distance = 80 + Math.random() * 40;
            const endX = targetX + Math.cos(angle) * distance;
            const endY = targetY + Math.sin(angle) * distance;

            particle.style.transition = 'all 0.6s ease-out';
            document.body.appendChild(particle);

            setTimeout(() => {
                particle.style.left = `${endX}px`;
                particle.style.top = `${endY}px`;
                particle.style.opacity = '0';
                particle.style.transform = `scale(0.5) rotate(${Math.random() * 360}deg)`;
            }, 50);

            setTimeout(() => {
                document.body.removeChild(particle);
            }, 700);
        }

        // Remover projétil principal
        document.body.removeChild(projectileElement);

        // Remover animação do alvo
        setTimeout(() => {
            targetElement.style.animation = '';
        }, 500);
    }, 850);
};

// Adicionar estilo CSS para animação de shake
if (!document.getElementById('cannon-styles')) {
    const style = document.createElement('style');
    style.id = 'cannon-styles';
    style.textContent = `
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
            20%, 40%, 60%, 80% { transform: translateX(5px); }
        }
        #cannon-icon {
            transition: transform 0.2s ease;
        }
    `;
    document.head.appendChild(style);
}

