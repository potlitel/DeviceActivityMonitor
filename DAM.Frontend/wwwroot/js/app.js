// 📁 DAM.Frontend/wwwroot/js/app.js
window.damTheme = {
    // 🎬 Inicializar tema con animación
    initTheme: function () {
        console.log('🎨 DAM Theme Manager initialized');
        this.applyThemeTransition();
    },

    // ✨ Transición suave entre temas
    applyThemeTransition: function () {
        const style = document.createElement('style');
        style.textContent = `
            * {
                transition: background-color 0.3s ease, 
                            color 0.2s ease,
                            border-color 0.2s ease,
                            box-shadow 0.2s ease !important;
            }
            .dam-logo {
                animation: pulse 2s infinite;
            }
            @keyframes pulse {
                0% { opacity: 1; }
                50% { opacity: 0.8; }
                100% { opacity: 1; }
            }
        `;
        document.head.appendChild(style);
    },

    // ☀️🌙 Actualizar emoji en tiempo real
    updateThemeEmoji: function (emoji) {
        const button = document.querySelector('button[title*="Cambiar tema"]');
        if (button) {
            button.innerHTML = emoji;
            button.style.transform = 'scale(1.2)';
            setTimeout(() => {
                button.style.transform = 'scale(1)';
            }, 200);
        }
    },

    // 📱 Detectar preferencia del sistema
    getSystemTheme: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches
            ? 'dark'
            : 'light';
    },

    // 🕐 Obtener hora del sistema (JS local)
    getCurrentHour: function () {
        const date = new Date();
        return date.getHours();
    }
};

// Inicializar
window.damTheme.initTheme();

// Escuchar cambios de tema del sistema
window.matchMedia('(prefers-color-scheme: dark)')
    .addEventListener('change', e => {
        console.log('📱 Preferencia del sistema cambiada:',
            e.matches ? 'oscuro' : 'claro');
    });