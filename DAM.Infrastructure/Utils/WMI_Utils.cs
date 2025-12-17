using System.Management;
using Microsoft.Extensions.Logging;

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
                                return disk["Model"]?.ToString() ?? "N/A";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fallo WMI al obtener Modelo para la unidad {DriveLetter}", driveLetter);
            }
            return "UNKNOWN_WMI";
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
                                        string serialNumber = physicalMedia["SerialNumber"]?.ToString().Trim();

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
            // Limpiamos la entrada para asegurar que el formato sea "X:" (requerido por WMI)
            string driveLetter = driveRoot.TrimEnd('\\');
            string volumeSerial = "NO_VOL_SERIAL";

            // -------------------------------------------------------------------------
            // PASO 1: Obtener el VolumeSerialNumber (Capa Lógica/Software)
            // -------------------------------------------------------------------------
            // Este ID es generado por Windows al formatear la unidad. Es muy estable
            // y se mantiene igual aunque el dispositivo se cambie de puerto USB.
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '{driveLetter}'"))
                {
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        // Extraemos el ID hexadecimal del volumen (ej: "A1B2C3D4")
                        volumeSerial = disk["VolumeSerialNumber"]?.ToString() ?? "NO_VOL_SERIAL";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("No se pudo obtener VolumeSerialNumber para la unidad {Drive}. Error: {Msg}", driveLetter, ex.Message);
            }

            // -------------------------------------------------------------------------
            // PASO 2: Intentar obtener el Serial Number de Hardware (Capa Física)
            // -------------------------------------------------------------------------
            // Realizamos un "WMI Deep Dive" navegando por las asociaciones del sistema:
            // Disco Lógico (E:) -> Partición -> Disco Físico (Hardware)
            try
            {
                // A. Buscamos la partición asociada a la letra de unidad
                string queryLogicalToPartition = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";

                using (var searcherLogicalDisk = new ManagementObjectSearcher(queryLogicalToPartition))
                {
                    foreach (ManagementObject partition in searcherLogicalDisk.Get())
                    {
                        // B. Buscamos el disco físico que contiene esa partición
                        string queryPartitionToDisk = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_PartitionToDisk";

                        using (var searcherPartition = new ManagementObjectSearcher(queryPartitionToDisk))
                        {
                            foreach (ManagementObject disk in searcherPartition.Get())
                            {
                                // C. Intentamos obtener el SerialNumber real del fabricante (disponible en Win 8/10/11)
                                string hardwareSN = disk["SerialNumber"]?.ToString()?.Trim();

                                // D. Fallback de Hardware: Si SerialNumber es nulo, usamos PNPDeviceID.
                                // En dispositivos USB, el PNPDeviceID suele terminar con el número de serie único del chip.
                                if (string.IsNullOrEmpty(hardwareSN))
                                {
                                    hardwareSN = disk["PNPDeviceID"]?.ToString();
                                }

                                // Si logramos obtener cualquiera de los dos identificadores de hardware, lo priorizamos
                                if (!string.IsNullOrEmpty(hardwareSN))
                                {
                                    return hardwareSN;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Registramos el error pero no interrumpimos el flujo, para permitir el fallback al Paso 3
                logger.LogError(ex, "Error en la cadena de consulta WMI para la unidad {Drive}", driveLetter);
            }

            // -------------------------------------------------------------------------
            // PASO 3: Red de Seguridad (Fallback Persistente)
            // -------------------------------------------------------------------------
            // Si la consulta de hardware falló (por permisos o incompatibilidad del driver),
            // retornamos el VolumeID obtenido en el Paso 1. 
            // Esto garantiza que el mismo USB siempre devuelva el mismo ID en esta máquina.
            return $"VOLID_{volumeSerial}";
        }
    }
}
