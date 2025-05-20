public interface IElasticSearchService<T> where T : class
{
    Task IndexAsync(T item, string? indexOverride = null);
    Task<IEnumerable<T>> SearchAsync(string keyword, string[] fields, string? indexOverride = null);
    Task UpdateAsync(string id, T item, string? indexOverride = null);
    Task DeleteDocumentAsync(string id, string? indexOverride = null);
}