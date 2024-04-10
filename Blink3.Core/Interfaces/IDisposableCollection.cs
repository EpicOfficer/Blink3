namespace Blink3.Core.Interfaces;

/// <summary>
///     Represents a collection of disposable objects that can be iterated and disposed.
/// </summary>
/// <typeparam name="T">The type of the disposable objects in the collection.</typeparam>
public interface IDisposableCollection<out T> : IEnumerable<T>, IDisposable where T : IDisposable
{
}