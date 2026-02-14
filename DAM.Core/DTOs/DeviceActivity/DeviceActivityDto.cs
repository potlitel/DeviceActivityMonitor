namespace DAM.Core.DTOs.DeviceActivity
{
    /// <summary>
    /// DTO que representa una actividad completa de un dispositivo en el sistema.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contiene información completa del ciclo de vida de una actividad,
    /// desde la inserción hasta la extracción del dispositivo.
    /// </para>
    /// <para>
    /// <b>Cálculos derivados:</b>
    /// <list type="bullet">
    /// <item><description>Espacio utilizado: <c>TotalCapacityMB - FinalAvailableMB</c></description></item>
    /// <item><description>Espacio liberado: <c>FinalAvailableMB - InitialAvailableMB</c></description></item>
    /// <item><description>Duración: <c>ExtractedAt - InsertedAt</c> (si ambos existen)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="Id">Identificador único de la actividad.</param>
    /// <param name="SerialNumber">Número de serie del dispositivo.</param>
    /// <param name="Model">Modelo del dispositivo.</param>
    /// <param name="TotalCapacityMB">Capacidad total en megabytes.</param>
    /// <param name="InsertedAt">Fecha y hora de inserción.</param>
    /// <param name="ExtractedAt">Fecha y hora de extracción (null si aún activo).</param>
    /// <param name="InitialAvailableMB">Espacio disponible al inicio.</param>
    /// <param name="FinalAvailableMB">Espacio disponible al final.</param>
    public record DeviceActivityDto(
    int Id,
    string SerialNumber,
    string Model,
    long TotalCapacityMB, 
    DateTime? InsertedAt,
    DateTime? ExtractedAt,
    long InitialAvailableMB,
    long FinalAvailableMB
);
}
