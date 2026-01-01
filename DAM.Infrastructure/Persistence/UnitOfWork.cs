using DAM.Core.Interfaces;

namespace DAM.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación concreta de la unidad de trabajo utilizando Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Esta clase coordina el acceso a la base de datos a través de <see cref="DeviceActivityDbContext"/>
    /// y gestiona el ciclo de vida de las transacciones para asegurar la consistencia de los datos.
    /// </remarks>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DeviceActivityDbContext _context;

        /// <inheritdoc/>
        public IActivityRepository Activities { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UnitOfWork"/>.
        /// </summary>
        /// <param name="context">El contexto de base de datos de Entity Framework.</param>
        /// <param name="activityRepository">El repositorio específico para actividades de dispositivos.</param>
        public UnitOfWork(DeviceActivityDbContext context, IActivityRepository activityRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Activities = activityRepository;
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        /// <inheritdoc/>
        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        /// <inheritdoc/>
        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();

        /// <inheritdoc/>
        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

        /// <summary>
        /// Libera los recursos del contexto de base de datos subyacente.
        /// </summary>
        public void Dispose() => _context.Dispose();
    }
}
