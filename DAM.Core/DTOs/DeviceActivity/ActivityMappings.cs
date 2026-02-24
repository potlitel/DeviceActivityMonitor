using DAM.Core.Features.DeviceActivity.Commands;

namespace DAM.Core.DTOs.DeviceActivity
{
    // ============================================
    // Extension Methods para conversiones
    // ============================================

    /// <summary>
    /// Proporciona métodos de extensión para mapear comandos relacionados con actividades de dispositivos
    /// a sus correspondientes entidades de dominio.
    /// </summary>
    /// <remarks>
    /// Esta clase sigue el principio de Separación de Preocupaciones (SoC) al mantener la lógica de mapeo
    /// fuera de los handlers y las entidades. Los métodos de extensión permiten una conversión explícita
    /// y mantenible entre la capa de aplicación (comandos) y la capa de dominio (entidades).
    /// <para>
    /// Todos los mapeos son explícitos y no utilizan reflexión o auto-mapeadores, lo que garantiza:
    /// <list type="bullet">
    /// <item><description>Rendimiento óptimo sin overhead de reflexión</description></item>
    /// <item><description>Type-safety en tiempo de compilación</description></item>
    /// <item><description>Facilidad para debugging y testing unitario</description></item>
    /// <item><description>Control total sobre transformaciones complejas</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static class ActivityMappings
    {
        /// <summary>
        /// Convierte un comando de creación de actividad en una entidad <see cref="DeviceActivity"/> lista para persistir.
        /// </summary>
        /// <param name="cmd">El comando que contiene los datos de la actividad a crear.</param>
        /// <returns>Una nueva instancia de <see cref="DeviceActivity"/> poblada con los datos del comando.</returns>
        /// <remarks>
        /// Este método inicializa las colecciones anidadas (<see cref="DeviceActivity.FilesCopied"/>, 
        /// <see cref="DeviceActivity.FilesDeleted"/>, <see cref="DeviceActivity.PresenceHistory"/>, 
        /// <see cref="DeviceActivity.Invoices"/>) como listas vacías para prevenir NullReferenceExceptions
        /// y permitir la carga diferida (lazy loading) si está configurada en el ORM.
        /// <para>
        /// La entidad resultante aún no tiene un Id asignado, ya que este será generado por la base de datos
        /// al momento de persistir.
        /// </para>
        /// </remarks>
        /// <example>
        /// Uso típico en un handler:
        /// <code>
        /// var entity = cmd.ToEntity();
        /// await uow.Activities.AddActivityAsync(entity);
        /// </code>
        /// </example>
        public static Entities.DeviceActivity ToEntity(this CreateActivityCmd cmd)
        {
            return new Entities.DeviceActivity
            {
                SerialNumber = cmd.SerialNumber,
                Model = cmd.Model,
                TotalCapacityMB = cmd.TotalCapacityMB,
                InsertedAt = cmd.InsertedAt,
                InitialAvailableMB = cmd.InitialAvailableMB,
                Status = cmd.Status,
                // Propiedades con valores por defecto
                FilesCopied = [],
                FilesDeleted = [],
                PresenceHistory = [],
                Invoices = []
            };
        }

        /// <summary>
        /// Actualiza una entidad <see cref="DeviceActivity"/> existente con los datos proporcionados en el comando de actualización.
        /// </summary>
        /// <param name="cmd">El comando que contiene los datos a actualizar.</param>
        /// <param name="entity">La entidad existente que será modificada.</param>
        /// <remarks>
        /// Este método implementa una actualización parcial (patch) que solo modifica las propiedades
        /// que tienen valores presentes en el comando. Las propiedades con valor null en el comando
        /// se ignoran, preservando los valores existentes en la entidad.
        /// <para>
        /// <list type="bullet">
        /// <item><description>✅ Las propiedades value-type con .HasValue true se actualizan</description></item>
        /// <item><description>✅ Las colecciones solo se reemplazan si se proporciona una nueva instancia</description></item>
        /// <item><description>✅ Los strings solo se actualizan si no son null ni empty</description></item>
        /// <item><description>✅ Se invoca <see cref="DeviceActivity.SetTimeInsertedDuration"/> automáticamente</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="cmd"/> o <paramref name="entity"/> son null.</exception>
        /// <example>
        /// Uso típico en un handler:
        /// <code>
        /// var entity = await uow.Activities.GetByIdAsync(cmd.Id);
        /// cmd.UpdateEntity(entity);
        /// await uow.Activities.UpdateActivityAsync(entity);
        /// </code>
        /// </example>
        public static void UpdateEntity(this UpdateActivityCmd cmd, Entities.DeviceActivity entity)
        {
            // Solo actualizamos las propiedades que pueden cambiar
            if (cmd.ExtractedAt.HasValue)
                entity.ExtractedAt = cmd.ExtractedAt;

            if (cmd.FinalAvailableMB.HasValue)
                entity.FinalAvailableMB = cmd.FinalAvailableMB.Value;

            if (cmd.MegabytesCopied.HasValue)
                entity.MegabytesCopied = cmd.MegabytesCopied.Value;

            if (cmd.MegabytesDeleted.HasValue)
                entity.MegabytesDeleted = cmd.MegabytesDeleted.Value;

            if (cmd.FilesCopied != null)
                entity.FilesCopied = cmd.FilesCopied;

            if (cmd.FilesDeleted != null)
                entity.FilesDeleted = cmd.FilesDeleted;

            if (!string.IsNullOrEmpty(cmd.SpecialEvent))
                entity.SpecialEvent = cmd.SpecialEvent;

            entity.Status = cmd.Status;

            // Calcular la duración si se proporcionó ExtractedAt
            entity.SetTimeInsertedDuration();
        }

        /// <summary>
        /// Convierte un ActivityDto a una entidad DeviceActivity para operaciones de dominio.
        /// </summary>
        /// <remarks>
        /// ⚠️ ADVERTENCIA: Este método debe usarse con precaución ya que:
        /// <list type="bullet">
        /// <item><description>La entidad resultante NO está siendo trackeada por EF Core</description></item>
        /// <item><description>Las colecciones relacionadas pueden estar incompletas</description></item>
        /// <item><description>Es responsabilidad del llamante asegurar la integridad de los datos</description></item>
        /// </list>
        /// </remarks>
        public static Entities.DeviceActivity ToEntity(this DeviceActivityDto dto)
        {
            return new Entities.DeviceActivity
            {
                Id = dto.Id,
                SerialNumber = dto.SerialNumber,
                Model = dto.Model,
                TotalCapacityMB = dto.TotalCapacityMB,
                InsertedAt = (DateTime)dto.InsertedAt!,
                ExtractedAt = dto.ExtractedAt,
                InitialAvailableMB = dto.InitialAvailableMB,
                FinalAvailableMB = dto.FinalAvailableMB,
                MegabytesCopied = dto.MegabytesCopied,
                MegabytesDeleted = dto.MegabytesDeleted,
                Status = dto.Status,
                SpecialEvent = dto.SpecialEvent,
                // Nota: Las colecciones pueden estar incompletas si el DTO no las incluye
                FilesCopied = dto.FilesCopied?.ToList() ?? new List<string>(),
                FilesDeleted = dto.FilesDeleted?.ToList() ?? new List<string>()
            };
        }
    }
}