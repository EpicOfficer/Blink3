using System.Collections;
using Blink3.Core.Interfaces;

namespace Blink3.Core.Helpers;

/// <inheritdoc />
public class DisposableCollection<T>(IEnumerable<T> items) : IDisposableCollection<T> where T : IDisposable
{
    private readonly LinkedList<T> _items = new(items);

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        foreach (T item in _items) item.Dispose();

        GC.SuppressFinalize(this);
    }
}