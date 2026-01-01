namespace DAM.Core.Interfaces
{
    /// <summary>
    /// Define el contrato para el patrón Unit of Work (Unidad de Trabajo).
    /// Centraliza la gestión de repositorios y el control de transacciones para asegurar la atomicidad de las operaciones.
    /// </summary>
    /// <remarks>
    /// Este patrón permite coordinar múltiples repositorios compartiendo un mismo contexto de base de datos.
    /// Implementa <see cref="IDisposable"/> para liberar correctamente los recursos de la conexión y transacciones.
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Repositorio para la gestión y persistencia de actividades (<see cref="IActivityRepository"/>).
        /// </summary>
        IActivityRepository Activities { get; }

        /// <summary>
        /// Persiste de forma asíncrona todos los cambios realizados en el contexto actual hacia la base de datos.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado contiene el número de entradas afectadas en la base de datos.</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Inicia una nueva transacción asíncrona en la base de datos.
        /// </summary>
        /// <remarks>
        /// Debe utilizarse para envolver múltiples operaciones que deben completarse de forma exitosa como un bloque único.
        /// </remarks>
        Task BeginTransactionAsync();

        /// <summary>
        /// Confirma de forma asíncrona la transacción actual, consolidando todos los cambios realizados.
        /// </summary>
        /// <exception cref="InvalidOperationException">Se lanza si no hay una transacción activa.</exception>
        Task CommitTransactionAsync();

        /// <summary>
        /// Revierte de forma asíncrona todos los cambios realizados durante la transacción actual en caso de error.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
