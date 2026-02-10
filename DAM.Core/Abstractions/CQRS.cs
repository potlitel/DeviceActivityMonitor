namespace DAM.Core.Abstractions;

/// <summary> Representa una intención de cambio de estado en el sistema. </summary>
public interface ICommand<out TResponse> { }

/// <summary> Representa una solicitud de información sin efectos secundarios. </summary>
public interface IQuery<out TResponse> { }

/// <summary> Lógica de ejecución para un comando específico. </summary>
public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken ct);
}

/// <summary> Lógica de ejecución para una consulta específica. </summary>
public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken ct);
}