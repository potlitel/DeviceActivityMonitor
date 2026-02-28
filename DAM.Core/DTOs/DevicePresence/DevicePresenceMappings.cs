using DAM.Core.Features.DevicePresence.Commands;

namespace DAM.Core.DTOs.DevicePresence
{
    /// <summary>
    /// Proporciona métodos de extensión para mapear comandos de presencia de dispositivos a entidades <see cref="DevicePresence"/>.
    /// </summary>
    /// <remarks>
    /// Estos mapeos son simples y directos ya que la entidad <see cref="DevicePresence"/> es principalmente
    /// un registro de auditoría con dependencia directa de una <see cref="DeviceActivity"/> existente.
    /// </remarks>
    public static class DevicePresenceMappings
    {
        /// <summary>
        /// Convierte un comando de creación de presencia en una entidad <see cref="DevicePresence"/>.
        /// </summary>
        /// <param name="cmd">El comando con los datos de presencia a registrar.</param>
        /// <returns>Una nueva instancia de <see cref="DevicePresence"/> lista para persistir.</returns>
        /// <remarks>
        /// Este mapeo es 1:1 ya que la entidad refleja exactamente la información del comando.
        /// La entidad resultante se asocia a una <see cref="DeviceActivity"/> existente mediante
        /// <see cref="DevicePresence.DeviceActivityId"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="cmd"/> es null.</exception>
        public static Entities.DevicePresence ToEntity(this CreateDevicePresenceCmd cmd)
        {
            return new Entities.DevicePresence
            {
                SerialNumber = cmd.SerialNumber,
                Timestamp = cmd.Timestamp,
                DeviceActivityId = cmd.DeviceActivityId
            };
        }
    }
}
