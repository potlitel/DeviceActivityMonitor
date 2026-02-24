using DAM.Core.DTOs.DevicePresence;
using DAM.Core.DTOs.Invoices;
using DAM.Core.Enums;

namespace DAM.Core.DTOs.DeviceActivity
{
    /// <summary>
    /// Data Transfer Object para la entidad DeviceActivity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Este DTO se utiliza para transferir datos de actividad de dispositivo entre capas,
    /// especialmente para consultas (queries) donde no se necesita la entidad completa.
    /// </para>
    /// <para>
    /// <b>⚠️ IMPORTANTE:</b> Cuando se utiliza para reconstruir una entidad (mapeo DTO → Entidad),
    /// se debe tener en cuenta que las colecciones anidadas pueden no estar completamente cargadas.
    /// </para>
    /// </remarks>
    public record DeviceActivityDto(
        int Id,
        string SerialNumber,
        string Model,
        long TotalCapacityMB,
        DateTime InsertedAt,  // Cambiado de nullable a non-nullable según entidad original
        DateTime? ExtractedAt,
        long InitialAvailableMB,
        long FinalAvailableMB,
        long MegabytesCopied,
        long MegabytesDeleted,
        ActivityStatus Status,
        string SpecialEvent,
        List<string> FilesCopied,
        List<string> FilesDeleted,
        TimeSpan? TimeInserted,
        ICollection<DevicePresenceDto>? PresenceHistory = null,
        ICollection<InvoiceDto>? Invoices = null)
    {
        /// <summary>
        /// Crea un DeviceActivityDto a partir de una entidad DeviceActivity.
        /// </summary>
        /// <param name="entity">La entidad de dominio a proyectar.</param>
        /// <returns>Un DTO poblado con los datos de la entidad.</returns>
        /// <remarks>
        /// Este método realiza una proyección explícita y controlada de la entidad a DTO.
        /// Las colecciones relacionadas se proyectan usando sus respectivos DTOs si existen.
        /// </remarks>
        /// <example>
        /// Uso en un Query Handler:
        /// <code>
        /// var entity = await context.DeviceActivities.FindAsync(id);
        /// var dto = DeviceActivityDto.FromEntity(entity);
        /// return dto;
        /// </code>
        /// </example>
        public static DeviceActivityDto FromEntity(Entities.DeviceActivity? entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "La entidad no puede ser nula");

            return new DeviceActivityDto(
                Id: entity.Id,
                SerialNumber: entity.SerialNumber,
                Model: entity.Model,
                TotalCapacityMB: entity.TotalCapacityMB,
                InsertedAt: entity.InsertedAt,
                ExtractedAt: entity.ExtractedAt,
                InitialAvailableMB: entity.InitialAvailableMB,
                FinalAvailableMB: entity.FinalAvailableMB,
                MegabytesCopied: entity.MegabytesCopied,
                MegabytesDeleted: entity.MegabytesDeleted,
                Status: entity.Status,
                SpecialEvent: entity.SpecialEvent,
                FilesCopied: entity.FilesCopied?.ToList() ?? new List<string>(),
                FilesDeleted: entity.FilesDeleted?.ToList() ?? new List<string>(),
                TimeInserted: entity.TimeInserted,
                PresenceHistory: entity.PresenceHistory?
                    .Select(DevicePresenceDto.FromEntity)
                    .ToList(),
                Invoices: entity.Invoices?
                    .Select(InvoiceDto.FromEntity)
                    .ToList()
            );
        }

        /// <summary>
        /// Convierte el DTO de vuelta a una entidad DeviceActivity.
        /// </summary>
        /// <returns>Una entidad DeviceActivity poblada con los datos del DTO.</returns>
        /// <remarks>
        /// <para>
        /// <b>⚠️ ADVERTENCIA DE USO:</b>
        /// </para>
        /// <para>
        /// Este método debe utilizarse SOLO en contextos muy específicos donde:
        /// <list type="bullet">
        /// <item><description>La entidad resultante NO será trackeada por EF Core</description></item>
        /// <item><description>Se necesita la entidad para lógica de negocio que no modifica el estado</description></item>
        /// <item><description>Se comprende que las colecciones pueden estar incompletas</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Para operaciones de escritura, siempre es preferible obtener la entidad directamente
        /// del repositorio en lugar de reconstruirla desde un DTO.
        /// </para>
        /// </remarks>
        public Entities.DeviceActivity ToEntity()
        {
            var entity = new Entities.DeviceActivity
            {
                Id = this.Id,
                SerialNumber = this.SerialNumber,
                Model = this.Model,
                TotalCapacityMB = this.TotalCapacityMB,
                InsertedAt = this.InsertedAt,
                ExtractedAt = this.ExtractedAt,
                InitialAvailableMB = this.InitialAvailableMB,
                FinalAvailableMB = this.FinalAvailableMB,
                MegabytesCopied = this.MegabytesCopied,
                MegabytesDeleted = this.MegabytesDeleted,
                Status = this.Status,
                SpecialEvent = this.SpecialEvent,
                FilesCopied = this.FilesCopied?.ToList() ?? new List<string>(),
                FilesDeleted = this.FilesDeleted?.ToList() ?? new List<string>()
            };

            // Restaurar el tiempo insertado si está disponible
            if (this.TimeInserted.HasValue)
            {
                // La propiedad TimeInserted es privada, pero podemos usar el método de la entidad
                // Nota: Esto asume que ExtractedAt está correctamente establecido
                if (entity.ExtractedAt.HasValue)
                {
                    entity.SetTimeInsertedDuration();
                }
            }

            // Restaurar colecciones relacionadas si existen
            if (this.PresenceHistory?.Any() == true)
            {
                entity.PresenceHistory = this.PresenceHistory
                    .Select(dto => dto.ToEntity(entity))
                    .ToList();
            }

            if (this.Invoices?.Any() == true)
            {
                entity.Invoices = this.Invoices
                    .Select(dto => dto.ToEntity(entity))
                    .ToList();
            }

            return entity;
        }
    }

    /// <summary>
    /// DTO para DevicePresence
    /// </summary>
    public record DevicePresenceDto(
        int Id,
        string SerialNumber,
        DateTime Timestamp,
        int DeviceActivityId)
    {
        public static DevicePresenceDto FromEntity(Entities.DevicePresence entity)
        {
            return new DevicePresenceDto(
                Id: entity.Id,
                SerialNumber: entity.SerialNumber,
                Timestamp: entity.Timestamp,
                DeviceActivityId: entity.DeviceActivityId
            );
        }

        public Entities.DevicePresence ToEntity(Entities.DeviceActivity activity)
        {
            return new Entities.DevicePresence
            {
                Id = this.Id,
                SerialNumber = this.SerialNumber,
                Timestamp = this.Timestamp,
                DeviceActivityId = this.DeviceActivityId,
                DeviceActivity = activity
            };
        }
    }

    /// <summary>
    /// DTO para Invoice
    /// </summary>
    public record InvoiceDto(
        int Id,
        string SerialNumber,
        DateTime Timestamp,
        decimal TotalAmount,
        string Description,
        int DeviceActivityId)
    {
        public static InvoiceDto FromEntity(Entities.Invoice entity)
        {
            return new InvoiceDto(
                Id: entity.Id,
                SerialNumber: entity.SerialNumber,
                Timestamp: entity.Timestamp,
                TotalAmount: entity.TotalAmount,
                Description: entity.Description,
                DeviceActivityId: entity.DeviceActivityId
            );
        }

        public Entities.Invoice ToEntity(Entities.DeviceActivity activity)
        {
            return new Entities.Invoice
            {
                Id = this.Id,
                SerialNumber = this.SerialNumber,
                Timestamp = this.Timestamp,
                TotalAmount = this.TotalAmount,
                Description = this.Description,
                DeviceActivityId = this.DeviceActivityId,
                DeviceActivity = activity
            };
        }
    }
}
