namespace DAM.Frontend.Core.Models.Common
{
    /// <summary>
    /// 📋 Resultado paginado genérico para todas las consultas
    /// </summary>
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedList() { }

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    /// <summary>
    /// 🏗️ Contrato base inmutable para cualquier operación de filtrado paginado.
    /// Utiliza records para garantizar la integridad de los datos durante el ciclo de vida de la petición.
    /// </summary>
    /// <param name="PageNumber">Índice de la página solicitado (basado en 1).</param>
    /// <param name="PageSize">Cantidad de registros por página.</param>
    public abstract record BaseFilter(int PageNumber = 1, int PageSize = 10);
}
