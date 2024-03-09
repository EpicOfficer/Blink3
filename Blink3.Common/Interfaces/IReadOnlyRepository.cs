namespace Blink3.Common.Interfaces;

public interface IReadOnlyRepository<T> where T : class
{
    Task<T?> GetByIdAsync(params object[] keyValues);
    Task<IReadOnlyCollection<T>> GetAllAsync();
}