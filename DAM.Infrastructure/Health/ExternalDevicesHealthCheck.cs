//using DAM.Core.Interfaces;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using Microsoft.Extensions.Logging;
//using System.Collections.Concurrent;

///// <summary>
///// Health Check especializado para monitorear el estado de los dispositivos externos
///// y el Worker Service que los gestiona.
///// </summary>
//public class ExternalDevicesHealthCheck : IHealthCheck
//{
//    private readonly IDevicePersistenceService _persistenceService;
//    private readonly IDeviceMonitor _deviceMonitor;
//    private readonly ILogger<ExternalDevicesHealthCheck> _logger;
//    private readonly IUnitOfWork _unitOfWork;

//    // Acceso a los watchers activos (necesitarás exponerlos o inyectar el worker)
//    private readonly ConcurrentDictionary<string, DeviceActivityWatcher> _activeWatchers;

//    public ExternalDevicesHealthCheck(
//        IDevicePersistenceService persistenceService,
//        IDeviceMonitor deviceMonitor,
//        IUnitOfWork unitOfWork,
//        ILogger<ExternalDevicesHealthCheck> logger)
//    {
//        _persistenceService = persistenceService;
//        _deviceMonitor = deviceMonitor;
//        _unitOfWork = unitOfWork;
//        _logger = logger;

//        // Necesitas una forma de acceder a los watchers activos del Worker
//        // Opción 1: Hacer que Worker exponga una propiedad estática o singleton
//        // Opción 2: Crear un servicio registrado como Singleton que mantenga el estado
//        _activeWatchers = Worker.ActiveWatchers; // Asumiendo que expones esto
//    }

//    public async Task<HealthCheckResult> CheckHealthAsync(
//        HealthCheckContext context,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            var startTime = DateTime.UtcNow;
//            var issues = new List<string>();
//            var warnings = new List<string>();
//            var data = new Dictionary<string, object>();

//            // 📊 1. MÉTRICAS DE WATCHERS ACTIVOS
//            var activeWatchersCount = _activeWatchers?.Count ?? 0;
//            data["active_watchers_count"] = activeWatchersCount;
//            data["active_devices"] = _activeWatchers?.Keys.ToList() ?? new List<string>();

//            // ⏱️ 2. TIEMPO DESDE ÚLTIMO HEARTBEAT
//            var lastHeartbeat = await GetLastHeartbeatAsync();
//            var timeSinceLastHeartbeat = DateTime.UtcNow - lastHeartbeat;
//            data["last_heartbeat"] = lastHeartbeat;
//            data["seconds_since_last_heartbeat"] = timeSinceLastHeartbeat.TotalSeconds;

//            if (timeSinceLastHeartbeat > TimeSpan.FromMinutes(2))
//            {
//                issues.Add($"⚠️ Último heartbeat hace {timeSinceLastHeartbeat.TotalSeconds:F0} segundos");
//            }

//            // 📝 3. ACTIVIDADES PENDIENTES DE RECUPERACIÓN
//            var pendingActivities = await _unitOfWork.Activities.GetActivitiesMissingInvoicesAsync();
//            var pendingCount = pendingActivities?.Count() ?? 0;
//            data["pending_activities_count"] = pendingCount;

//            if (pendingCount > 10)
//            {
//                issues.Add($"❌ {pendingCount} actividades pendientes de recuperación (crítico)");
//            }
//            else if (pendingCount > 5)
//            {
//                warnings.Add($"⚠️ {pendingCount} actividades pendientes de recuperación");
//            }

//            // 📈 4. DISPOSITIVOS INACTIVOS (usando tu lógica original pero mejorada)
//            var cutoffTime = DateTime.UtcNow.AddMinutes(-5);
//            var inactiveDevices = await _unitOfWork.DevicePresences
//                .GetDevicesInactiveSinceAsync(cutoffTime);

//            var inactiveCount = inactiveDevices?.Count() ?? 0;
//            var totalDevices = await _unitOfWork.DevicePresences.GetTotalDevicesCountAsync();

//            data["inactive_devices_count"] = inactiveCount;
//            data["total_devices_known"] = totalDevices;
//            data["inactivity_threshold_minutes"] = 5;

//            var inactivePercentage = totalDevices > 0
//                ? (inactiveCount * 100.0 / totalDevices)
//                : 0;
//            data["inactive_percentage"] = Math.Round(inactivePercentage, 2);

//            if (inactivePercentage > 30)
//            {
//                issues.Add($"❌ {inactivePercentage:F1}% de dispositivos inactivos (umbral: 30%)");
//            }
//            else if (inactivePercentage > 15)
//            {
//                warnings.Add($"⚠️ {inactivePercentage:F1}% de dispositivos inactivos");
//            }

//            // 🔄 5. ESTADO DEL MONITOR WMI
//            var isMonitoring = _deviceMonitor?.IsMonitoring ?? false;
//            data["wmi_monitoring_active"] = isMonitoring;

//            if (!isMonitoring)
//            {
//                issues.Add("❌ Monitor WMI no está activo");
//            }

//            // ⏳ 6. TIEMPO DE ACTIVIDAD DEL WORKER
//            var workerUptime = GetWorkerUptime();
//            data["worker_uptime_seconds"] = workerUptime.TotalSeconds;
//            data["worker_uptime"] = workerUptime.ToString();

//            if (workerUptime < TimeSpan.FromMinutes(1))
//            {
//                warnings.Add($"⚠️ Worker recién iniciado (hace {workerUptime.TotalSeconds:F0}s)");
//            }

//            // 💾 7. ACTIVIDADES RECIENTES (última hora)
//            var lastHourActivities = await _unitOfWork.Activities
//                .GetActivitiesCountSinceAsync(DateTime.UtcNow.AddHours(-1));
//            data["activities_last_hour"] = lastHourActivities;

//            // 📊 8. TASA DE ERRORES
//            var errorRate = await GetErrorRateLastHourAsync();
//            data["error_rate_last_hour"] = errorRate;

//            if (errorRate > 10)
//            {
//                issues.Add($"❌ Alta tasa de errores: {errorRate} en la última hora");
//            }
//            else if (errorRate > 5)
//            {
//                warnings.Add($"⚠️ Tasa de errores elevada: {errorRate} en la última hora");
//            }

//            // 🎯 9. DETERMINAR ESTADO GENERAL
//            var totalDuration = DateTime.UtcNow - startTime;
//            data["check_duration_ms"] = totalDuration.TotalMilliseconds;

//            // Registrar métricas
//            _logger.LogInformation(
//                "Health Check - Activos: {Active}, Inactivos: {Inactive}, Pendientes: {Pending}, Errores: {Errors}",
//                activeWatchersCount, inactiveCount, pendingCount, errorRate);

//            // Construir descripción
//            var description = BuildDescription(activeWatchersCount, inactiveCount, pendingCount, issues, warnings);

//            // Determinar resultado según severidad
//            if (issues.Any(i => i.StartsWith("❌")))
//            {
//                return HealthCheckResult.Unhealthy(
//                    description,
//                    data: data);
//            }

//            if (issues.Any() || warnings.Count > 2)
//            {
//                return HealthCheckResult.Degraded(
//                    description,
//                    data: data);
//            }

//            if (warnings.Any())
//            {
//                return HealthCheckResult.Healthy(
//                    description + " (con advertencias menores)",
//                    data: data);
//            }

//            return HealthCheckResult.Healthy(
//                $"✅ {activeWatchersCount} dispositivos activos, {inactiveCount} inactivos - Todo OK",
//                data: data);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error crítico en health check de dispositivos externos");

//            return HealthCheckResult.Unhealthy(
//                "Error al verificar estado de dispositivos",
//                ex,
//                data: new Dictionary<string, object>
//                {
//                    ["error"] = ex.Message,
//                    ["error_type"] = ex.GetType().Name
//                });
//        }
//    }

//    #region Métodos de ayuda

//    private async Task<DateTime> GetLastHeartbeatAsync()
//    {
//        try
//        {
//            return await _unitOfWork.Heartbeats.GetLastHeartbeatTimestampAsync();
//        }
//        catch
//        {
//            return DateTime.MinValue;
//        }
//    }

//    private TimeSpan GetWorkerUptime()
//    {
//        // Necesitas una forma de obtener el start time del worker
//        // Podrías registrarlo en un servicio Singleton cuando el worker inicia
//        return Worker.StartTime.HasValue
//            ? DateTime.UtcNow - Worker.StartTime.Value
//            : TimeSpan.Zero;
//    }

//    private async Task<int> GetErrorRateLastHourAsync()
//    {
//        try
//        {
//            return await _unitOfWork.Errors.GetErrorCountSinceAsync(DateTime.UtcNow.AddHours(-1));
//        }
//        catch
//        {
//            return 0;
//        }
//    }

//    private string BuildDescription(int active, int inactive, int pending, List<string> issues, List<string> warnings)
//    {
//        var parts = new List<string> { $"📱 {active} activos, {inactive} inactivos" };

//        if (pending > 0)
//            parts.Add($"📝 {pending} pendientes");

//        if (issues.Any())
//            parts.Add($"❌ {issues.Count} problemas");

//        if (warnings.Any())
//            parts.Add($"⚠️ {warnings.Count} advertencias");

//        return string.Join(" | ", parts);
//    }

//    #endregion
//}