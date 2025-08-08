using System.Collections.Generic;
using System.Text;

public static class StringBuilderPool
{
    // Maximum builders to keep in pool to avoid unbounded memory
    private const int MaxPoolSize = 64;
    private static readonly Stack<StringBuilder> _pool = new Stack<StringBuilder>(MaxPoolSize);

    /// <summary>
    /// Get a StringBuilder instance. Capacity is cleared before returning.
    /// </summary>
    public static StringBuilder Get()
    {
        lock(_pool)
        {
            if(_pool.Count > 0)
            {
                var sb = _pool.Pop();
                sb.Clear();
                return sb;
            }
        }
        return new StringBuilder();
    }

    /// <summary>
    /// Return a StringBuilder to the pool. Builders over MaxPoolSize are discarded.
    /// </summary>
    public static void Release(StringBuilder sb)
    {
        if(sb == null)
            return;
        sb.Clear();
        lock(_pool)
        {
            if(_pool.Count < MaxPoolSize)
                _pool.Push(sb);
        }
    }
} 