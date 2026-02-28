namespace DAM.Frontend.Core.Models.Auth
{
    /// <summary>
    /// 🔐 Respuesta para la configuración inicial de 2FA
    /// </summary>
    public class Setup2FAResponse
    {
        /// <summary>
        /// 🔑 Secreto compartido para la app autenticadora
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// 📱 URI del código QR (formato otpauth://)
        /// </summary>
        public string QrCodeUri { get; set; } = string.Empty;

        /// <summary>
        /// 🔢 Códigos de respaldo de un solo uso (guardar en lugar seguro)
        /// </summary>
        public List<string> BackupCodes { get; set; } = new();

        /// <summary>
        /// ⏰ Tiempo restante para escanear el QR (segundos)
        /// </summary>
        public int ExpiresInSeconds { get; set; } = 300;
    }
}
