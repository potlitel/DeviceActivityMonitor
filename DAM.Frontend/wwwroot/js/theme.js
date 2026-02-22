// 📁 DAM.Frontend/wwwroot/js/theme.js

/**
 * 🌓 Theme Helper - Manejo completo de temas con detección de sistema y persistencia
 */
window.themeHelper = {
    /**
     * 🔍 Detectar tema del sistema
     * @returns {string} 'dark' | 'light'
     */
    getSystemTheme: function () {
        try {
            const isDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
            console.log('🎨 Tema del sistema detectado:', isDark ? 'oscuro' : 'claro');
            return isDark ? 'dark' : 'light';
        } catch (e) {
            console.warn('⚠️ No se pudo detectar tema del sistema, usando claro por defecto', e);
            return 'light';
        }
    },

    /**
     * 🎨 Aplicar tema visual
     * @param {string} theme 'dark' | 'light'
     */
    setTheme: function (theme) {
        try {
            // Validar parámetro
            if (theme !== 'dark' && theme !== 'light') {
                console.warn('⚠️ Tema inválido, usando claro:', theme);
                theme = 'light';
            }

            const isDark = theme === 'dark';

            // Remover clases existentes
            document.documentElement.classList.remove(
                'dark-theme', 'light-theme',
                'mud-theme-dark', 'mud-theme-light'
            );

            // Agregar nuevas clases
            document.documentElement.classList.add(isDark ? 'dark-theme' : 'light-theme');
            document.documentElement.classList.add(isDark ? 'mud-theme-dark' : 'mud-theme-light');

            // Opcional: cambiar color de fondo para elementos que no usen MudBlazor
            document.body.style.backgroundColor = isDark ? '#1e1e1e' : '#ffffff';
            document.body.style.color = isDark ? '#ffffff' : '#000000';

            console.log('✅ Tema aplicado:', theme);

            // Disparar evento personalizado para otros componentes
            window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
        } catch (e) {
            console.error('❌ Error aplicando tema:', e);
        }
    },

    /**
     * 🔄 Alternar tema actual
     * @returns {string} Nuevo tema aplicado
     */
    toggleTheme: function () {
        const currentTheme = document.documentElement.classList.contains('dark-theme') ? 'dark' : 'light';
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(newTheme);

        // Guardar en localStorage
        try {
            localStorage.setItem('user_theme_preference', newTheme === 'dark' ? 'true' : 'false');
            console.log('💾 Preferencia guardada en localStorage:', newTheme);
        } catch (e) {
            console.warn('⚠️ No se pudo guardar preferencia en localStorage:', e);
        }

        return newTheme;
    },

    /**
     * 🎬 Inicializar tema desde localStorage o sistema
     */
    initialize: function () {
        try {
            // Intentar cargar desde localStorage
            const savedTheme = localStorage.getItem('user_theme_preference');

            if (savedTheme !== null) {
                const isDark = savedTheme === 'true';
                this.setTheme(isDark ? 'dark' : 'light');
                console.log('📦 Tema cargado de localStorage:', isDark ? 'oscuro' : 'claro');
            } else {
                // Usar tema del sistema
                const systemTheme = this.getSystemTheme();
                this.setTheme(systemTheme);
                console.log('🖥️ Tema basado en sistema:', systemTheme);
            }

            // Configurar listener para cambios del sistema
            this.setupSystemThemeListener();
        } catch (e) {
            console.error('❌ Error en inicialización de tema:', e);
            this.setTheme('light'); // Fallback seguro
        }
    },

    /**
     * 📡 Configurar listener para cambios de tema del sistema
     */
    setupSystemThemeListener: function () {
        try {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

            // Verificar si ya hay un listener para no duplicar
            if (this._mediaQueryListener) {
                mediaQuery.removeEventListener('change', this._mediaQueryListener);
            }

            this._mediaQueryListener = (e) => {
                // Solo cambiar si no hay preferencia guardada
                const hasPreference = localStorage.getItem('user_theme_preference') !== null;
                if (!hasPreference) {
                    const newTheme = e.matches ? 'dark' : 'light';
                    console.log('🔄 Tema del sistema cambiado a:', newTheme);
                    this.setTheme(newTheme);

                    // Notificar a Blazor si hay referencia
                    if (this._dotNetRef) {
                        this._dotNetRef.invokeMethodAsync('SystemThemeChanged', newTheme)
                            .catch(err => console.warn('⚠️ Error notificando a Blazor:', err));
                    }
                }
            };

            mediaQuery.addEventListener('change', this._mediaQueryListener);
            console.log('📡 Listener de tema del sistema configurado');
        } catch (e) {
            console.warn('⚠️ No se pudo configurar listener de tema del sistema:', e);
        }
    },

    /**
     * 🔗 Registrar referencia de .NET para callbacks
     * @param {Object} dotNetRef - Referencia de DotNetObjectReference
     */
    registerDotNetReference: function (dotNetRef) {
        this._dotNetRef = dotNetRef;
        console.log('🔗 Referencia .NET registrada para callbacks de tema');
    },

    /**
     * 🔌 Desregistrar referencia de .NET
     */
    unregisterDotNetReference: function () {
        this._dotNetRef = null;
        console.log('🔌 Referencia .NET desregistrada');
    },

    /**
     * 🧹 Limpiar listeners (para cleanup)
     */
    dispose: function () {
        try {
            if (this._mediaQueryListener) {
                const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
                mediaQuery.removeEventListener('change', this._mediaQueryListener);
                this._mediaQueryListener = null;
            }
            this._dotNetRef = null;
            console.log('🧹 ThemeHelper cleanup completado');
        } catch (e) {
            console.warn('⚠️ Error en cleanup:', e);
        }
    },

    /**
     * 📊 Obtener tema actual
     * @returns {string} 'dark' | 'light'
     */
    getCurrentTheme: function () {
        return document.documentElement.classList.contains('dark-theme') ? 'dark' : 'light';
    },

    /**
     * 🔍 Verificar si hay preferencia guardada
     * @returns {boolean}
     */
    hasSavedPreference: function () {
        return localStorage.getItem('user_theme_preference') !== null;
    },

    /**
     * 🗑️ Limpiar preferencia guardada
     */
    clearSavedPreference: function () {
        try {
            localStorage.removeItem('user_theme_preference');
            console.log('🗑️ Preferencia de tema eliminada');

            // Volver al tema del sistema
            const systemTheme = this.getSystemTheme();
            this.setTheme(systemTheme);
        } catch (e) {
            console.warn('⚠️ No se pudo limpiar preferencia:', e);
        }
    }
};

// Interfaz para compatibilidad con código existente
window.mudThemeProvider = {
    setTheme: function (theme) {
        window.themeHelper.setTheme(theme);
    }
};

window.updateThemeEmoji = function (emoji) {
    // Buscar el botón de tema y actualizar su contenido si existe
    try {
        const themeButtons = document.querySelectorAll('[title*="Cambiar tema"], [title*="tema"], [title*="Theme"]');
        themeButtons.forEach(btn => {
            if (btn && (btn.tagName === 'BUTTON' || btn.classList.contains('mud-icon-button'))) {
                btn.innerHTML = emoji;
            }
        });
        console.log('🎯 Emoji de tema actualizado:', emoji);
    } catch (e) {
        // Ignorar errores - es solo cosmetico
    }
};

// Inicializar cuando el DOM esté listo
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.themeHelper.initialize();
    });
} else {
    // DOM ya está cargado
    window.themeHelper.initialize();
}

// Cleanup al descargar la página
window.addEventListener('beforeunload', () => {
    window.themeHelper.dispose();
});