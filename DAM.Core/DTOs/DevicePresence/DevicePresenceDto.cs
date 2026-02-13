using DAM.Core.DTOs.DeviceActivity;

namespace DAM.Core.DTOs.DevicePresence
{
    /// <summary>
    /// DTO que representa un evento de presencia de dispositivo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Los eventos de presencia son registros históricos de cuándo un dispositivo
    /// fue detectado en el sistema, independientemente de su actividad.
    /// </para>
    /// <para>
    /// <b>Relaciones:</b>
    /// <list type="bullet">
    /// <item><description>Puede estar asociado a una <see cref="DeviceActivityDto"/> si ocurrió durante una actividad</description></item>
    /// <item><description>Múltiples presencias pueden referenciar la misma actividad</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="Id">Identificador único del evento de presencia.</param>
    /// <param name="SerialNumber">Número de serie del dispositivo.</param>
    /// <param name="Timestamp">Marca temporal del evento.</param>
    /// <param name="ActivityDto">Actividad asociada (opcional).</param>
    public record DevicePresenceDto(
    int Id,
    string SerialNumber,
    DateTime Timestamp,
    DeviceActivityDto? ActivityDto
);
}
