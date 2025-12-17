using DAM.Core.Constants;
using Microsoft.Extensions.Logging;
using System.Management;

namespace DAM.Infrastructure.Utils
{
    public static class WMI_Utils
    {
        /// <summary>
        /// Obtiene el nombre del modelo de hardware del dispositivo asociado a una letra de unidad.
        /// Ejemplo: "Kingston DataTraveler 3.0 USB Device" o "Samsung SSD 970 EVO Plus 1TB".
        /// </summary>
        /// <param name="driveRoot">La raíz de la unidad (ej. "E:\\" o "E:").</param>
        /// <param name="logger">Instancia de ILogger para registrar errores de consulta.</param>
        /// <returns>El nombre del modelo del fabricante o "UNKNOWN_MODEL" si no se encuentra.</returns>
        public static string GetModel(string driveRoot, ILogger logger)
        {
            var disk = GetDiskDriveObject(driveRoot, logger);
            return disk?["Model"]?.ToString()?.Trim() ?? DataConstants.UnknownWmi;
        }

        /// <summary>
        /// Obtiene el Serial Number (mejorado) con lógica de fallback y GUID en caso de error crítico.
        /// </summary>
        /// <remarks>
        /// Marcada como obsoleta porque genera GUIDs aleatorios que rompen la persistencia del ID.
        /// </remarks>
        [Obsolete("Use GetPersistentSerialNumber en su lugar para garantizar identificadores constantes.")]
        public static string GetSerialNumberImproved(string driveRoot, ILogger logger)
        {
            string driveLetter = driveRoot.TrimEnd('\\');

            try
            {
                string queryPhysicalMedia = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";

                using (var searcher = new ManagementObjectSearcher(queryPhysicalMedia))
                {
                    foreach (ManagementObject partition in searcher.Get())
                    {
                        string queryDiskDrive = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_PartitionToDisk";
                        using (var searcherDisk = new ManagementObjectSearcher(queryDiskDrive))
                        {
                            foreach (ManagementObject disk in searcherDisk.Get())
                            {
                                string queryPhysical = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{disk["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskMedia";
                                using (var searcherPhysical = new ManagementObjectSearcher(queryPhysical))
                                {
                                    foreach (ManagementObject physicalMedia in searcherPhysical.Get())
                                    {
                                        string? serialNumber = physicalMedia["SerialNumber"]?.ToString()!.Trim();

                                        if (!string.IsNullOrEmpty(serialNumber) && serialNumber != "0")
                                        {
                                            return serialNumber;
                                        }
                                        else
                                        {
                                            string pnpID = disk["PNPDeviceID"]?.ToString() ?? "N/A";
                                            string signature = disk["Signature"]?.ToString() ?? "N/A";
                                            return $"PNP_{pnpID}_{signature}";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fallo WMI al obtener Serial Number para la unidad {DriveLetter}", driveLetter);
            }

            return $"WMI_FAIL_{Guid.NewGuid()}";
        }

        /// <summary>
        /// Versión estándar de obtención de Serial Number.
        /// </summary>
        /// <remarks>
        /// Marcada como obsoleta porque retorna 'UNKNOWN_WMI' en fallos, causando colisiones de ID.
        /// </remarks>
        [Obsolete("Use GetPersistentSerialNumber en su lugar. Esta función no maneja fallos de forma única.")]
        public static string GetSerialNumber(string driveRoot, ILogger logger)
        {
            string driveLetter = driveRoot.TrimEnd('\\');
            try
            {
                string queryLogicalDisk = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";

                using (var searcherLogicalDisk = new ManagementObjectSearcher(queryLogicalDisk))
                {
                    foreach (ManagementObject partition in searcherLogicalDisk.Get())
                    {
                        string queryPartition = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_PartitionToDisk";

                        using (var searcherPartition = new ManagementObjectSearcher(queryPartition))
                        {
                            foreach (ManagementObject disk in searcherPartition.Get())
                            {
                                return (disk["PNPDeviceID"]?.ToString() ?? "N/A") + "_" + (disk["Signature"]?.ToString() ?? "N/A");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fallo WMI al obtener Serial Number para la unidad {DriveLetter}", driveLetter);
            }
            return "UNKNOWN_WMI";
        }

        /// <summary>
        /// Obtiene un identificador persistente para una unidad externa.
        /// Intenta primero obtener el Serial Number de hardware; si falla, recurre al Volume Serial Number.
        /// </summary>
        /// <param name="driveRoot">La raíz de la unidad (ej. "E:\\" o "E:").</param>
        /// <param name="logger">Instancia de ILogger para registrar advertencias o errores.</param>
        /// <returns>
        /// Un string con el Serial Number de hardware, el PNPDeviceID o el VolumeID. 
        /// Nunca retorna un valor aleatorio para garantizar consistencia.
        /// </returns>
        public static string GetPersistentSerialNumber(string driveRoot, ILogger logger)
        {
            string driveLetter = driveRoot.TrimEnd('\\');
            string volumeSerial = GetVolumeSerialNumber(driveLetter, logger);

            var disk = GetDiskDriveObject(driveLetter, logger);
            if (disk != null)
            {
                // Intentamos SerialNumber físico (Win 8+) o PNPDeviceID
                #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                string hardwareSN = disk["SerialNumber"]?.ToString()?.Trim()
                                    ?? disk["PNPDeviceID"]?.ToString()?.Trim();
                #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                if (!string.IsNullOrEmpty(hardwareSN)) return hardwareSN;
            }

            return $"VOLID_{volumeSerial}";
        }

        // -------------------------------------------------------------------------
        // MÉTODOS DE APOYO (Lógica Compartida)
        // -------------------------------------------------------------------------

        /// <summary>
        /// Navega por las asociaciones jerárquicas de WMI para localizar el objeto físico <c>Win32_DiskDrive</c> 
        /// correspondiente a una letra de unidad lógica.
        /// </summary>
        /// <param name="driveLetter">La letra de la unidad lógica (ej. "E:").</param>
        /// <param name="logger">Instancia de <see cref="ILogger"/> para el registro de errores de diagnóstico.</param>
        /// <returns>
        /// Un objeto <see cref="ManagementObject"/> que representa el disco físico si se encuentra; 
        /// de lo contrario, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// El método realiza una resolución en dos pasos:
        /// <list type="number">
        /// <item>Mapea el disco lógico a su partición correspondiente mediante <c>Win32_LogicalDiskToPartition</c>.</item>
        /// <item>Mapea dicha partición al dispositivo físico final mediante <c>Win32_PartitionToDisk</c>.</item>
        /// </list>
        /// Se recomienda invocar el método <c>Dispose()</c> sobre el objeto retornado si no se va a utilizar más.
        /// </remarks>
        private static ManagementObject GetDiskDriveObject(string driveLetter, ILogger logger)
        {
            try
            {
                string cleanLetter = driveLetter.TrimEnd('\\');

                // 1. Lógica -> Partición
                string q1 = string.Format(WmiQueries.LogicalDiskToPartition, cleanLetter);
                using var searcher1 = new ManagementObjectSearcher(q1);

                foreach (ManagementObject partition in searcher1.Get())
                {
                    // 2. Partición -> Disco Físico
                    string q2 = string.Format(WmiQueries.PartitionToDisk, partition["DeviceID"]);
                    using var searcher2 = new ManagementObjectSearcher(q2);

                    foreach (ManagementObject disk in searcher2.Get())
                    {
                        // Retornamos el objeto. Nota: El llamador debe gestionar el Dispose o usar 'using'
                        return disk;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, Messages.Wmi.ProcessError, driveLetter);
            }
            return null!;
        }

        /// <summary>
        /// Obtiene el número de serie del volumen lógico (identificador de software) asignado por el sistema de archivos.
        /// </summary>
        /// <param name="driveLetter">La letra de la unidad lógica a consultar (ej. "C:").</param>
        /// <param name="logger">Instancia de <see cref="ILogger"/> para registrar advertencias.</param>
        /// <returns>
        /// Una cadena que representa el <c>VolumeSerialNumber</c>. 
        /// Retorna "NO_VOL_SERIAL" si el valor no está disponible o la consulta falla.
        /// </returns>
        /// <remarks>
        /// A diferencia del número de serie físico (quemado en el hardware), este valor es generado 
        /// al formatear la unidad y puede cambiar si el volumen es recreado.
        /// </remarks>
        private static string GetVolumeSerialNumber(string driveLetter, ILogger logger)
        {
            try
            {
                string query = string.Format(WmiQueries.VolumeSerial, driveLetter);
                using var searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject disk in searcher.Get())
                {
                    return disk["VolumeSerialNumber"]?.ToString() ?? DataConstants.NotAvailable;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(Messages.Watcher.DriveInfoError, driveLetter);
            }
            return DataConstants.NotAvailable;
        }
    }
}
