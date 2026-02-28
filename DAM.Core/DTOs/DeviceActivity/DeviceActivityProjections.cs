using DeviceActivityEntity = DAM.Core.Entities.DeviceActivity;

namespace DAM.Core.DTOs.DeviceActivity
{
    /// <summary>
    /// Métodos de extensión para proyecciones de DeviceActivity a DTO.
    /// </summary>
    public static class DeviceActivityProjections
    {
        /// <summary>
        /// Proyecta una consulta de DeviceActivity a DeviceActivityDto para listados.
        /// </summary>
        /// <remarks>
        /// Esta proyección es optimizada para listados paginados, por lo que NO incluye
        /// colecciones relacionadas para evitar sobrecarga en la consulta.
        /// </remarks>
        public static IQueryable<DeviceActivityDto> ToListDto(this IQueryable<DeviceActivityEntity> query)
        {
            return query.Select(entity => new DeviceActivityDto(
                entity.Id,                     
                entity.SerialNumber,           
                entity.Model,
                entity.TotalCapacityMB,
                entity.InsertedAt,
                entity.ExtractedAt,
                entity.InitialAvailableMB,
                entity.FinalAvailableMB,
                entity.MegabytesCopied,
                entity.MegabytesDeleted,
                entity.Status,
                entity.SpecialEvent ?? string.Empty,
                entity.FilesCopied ?? new List<string>(),
                entity.FilesDeleted ?? new List<string>(),
                entity.TimeInserted,
                null,
                null 
            ));
        }

        /// <summary>
        /// Proyecta una consulta de DeviceActivity a DeviceActivityDto para vista detallada.
        /// </summary>
        /// <remarks>
        /// Esta proyección incluye colecciones relacionadas, por lo que debe usarse
        /// solo cuando se necesitan todos los datos de la actividad.
        /// </remarks>
        public static IQueryable<DeviceActivityDto> ToDetailDto(this IQueryable<DeviceActivityEntity> query)
        {
            return query.Select(entity => new DeviceActivityDto(
                entity.Id,
                entity.SerialNumber,
                entity.Model,
                entity.TotalCapacityMB,
                entity.InsertedAt,
                entity.ExtractedAt,
                entity.InitialAvailableMB,
                entity.FinalAvailableMB,
                entity.MegabytesCopied,
                entity.MegabytesDeleted,
                entity.Status,
                entity.SpecialEvent ?? string.Empty,
                entity.FilesCopied ?? new List<string>(),
                entity.FilesDeleted ?? new List<string>(),
                entity.TimeInserted,
                entity.PresenceHistory.Select(p => new DevicePresenceDto(
                    p.Id,
                    p.SerialNumber,
                    p.Timestamp,
                    p.DeviceActivityId
                )).ToList(),
                entity.Invoices.Select(i => new InvoiceDto(
                    i.Id,
                    i.SerialNumber,
                    i.Timestamp,
                    i.TotalAmount,
                    i.Description,
                    i.DeviceActivityId
                )).ToList()
            ));
        }

        /// <summary>
        /// Método de utilidad para obtener la función de mapeo para ToPaginatedListAsync.
        /// </summary>
        public static Func<DeviceActivityDto, DeviceActivityDto> IdentityMap()
        {
            return dto => dto;
        }
    }
}
