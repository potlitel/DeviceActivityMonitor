using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.DTOs.Heartbeat
{
    public record HeartbeatDto(
    string ServiceName,
    string MachineName,
    string Status,          // Running, Stopping, etc.
    int ActiveWatchers,     // Cantidad de dispositivos siendo monitoreados ahora
    double CpuUsagePercentage,
    long MemoryUsageMB,     // Consumo de RAM del proceso
    long UpTimeSeconds,     // Cuánto tiempo lleva el servicio sin reiniciarse
    string Version,         // Versión del ensamblado para control de despliegue
    DateTime Timestamp
);
}
