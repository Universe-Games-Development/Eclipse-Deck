using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class PriorityQueue<TKey, TValue> {
    private readonly SortedDictionary<TKey, Queue<TValue>> _queue = new();
    private readonly IComparer<TKey> _keyComparer;

    public int Count {
        get {
            int count = 0;
            foreach (var queue in _queue.Values) {
                count += queue.Count;
            }
            return count;
        }
    }

    public PriorityQueue(IComparer<TKey> keyComparer = null) {
        _keyComparer = keyComparer ?? Comparer<TKey>.Default;
        _queue = new SortedDictionary<TKey, Queue<TValue>>(
            Comparer<TKey>.Create((x, y) => _keyComparer.Compare(y, x)) // Reverse for highest priority first
        );
    }

    public void Enqueue(TKey priority, TValue value) {
        if (!_queue.ContainsKey(priority)) {
            _queue[priority] = new Queue<TValue>();
        }
        _queue[priority].Enqueue(value);
    }

    public TValue Dequeue() {
        if (_queue.Count == 0) {
            throw new InvalidOperationException("The queue is empty.");
        }

        var maxPriority = _queue.Keys.First(); // Already sorted in descending order
        var dequeuedValue = _queue[maxPriority].Dequeue();

        if (_queue[maxPriority].Count == 0) {
            _queue.Remove(maxPriority);
        }

        return dequeuedValue;
    }

    public TValue Peek() {
        if (_queue.Count == 0) {
            throw new InvalidOperationException("The queue is empty.");
        }

        var maxPriority = _queue.Keys.First();
        return _queue[maxPriority].Peek();
    }

    public void Clear() {
        _queue.Clear();
    }

    public bool IsEmpty() {
        return _queue.Count == 0;
    }

    public bool TryDequeue(out TValue value) {
        if (IsEmpty()) {
            value = default;
            return false;
        }

        value = Dequeue();
        return true;
    }

    public IEnumerable<TValue> GetAllItems() {
        foreach (var kvp in _queue) {
            foreach (var item in kvp.Value) {
                yield return item;
            }
        }
    }

    public List<TValue> RemoveItems(Func<TValue, bool> predicate) {
        var removedItems = new List<TValue>();
        var keysToRemove = new List<TKey>();

        foreach (var kvp in _queue.ToList()) {
            var priority = kvp.Key;
            var queue = kvp.Value;
            var tempItems = new List<TValue>();

            while (queue.Count > 0) {
                var item = queue.Dequeue();
                if (predicate(item)) {
                    removedItems.Add(item);
                } else {
                    tempItems.Add(item);
                }
            }

            // Re-enqueue items that weren't removed
            foreach (var item in tempItems) {
                queue.Enqueue(item);
            }

            // Mark empty queues for removal
            if (queue.Count == 0) {
                keysToRemove.Add(priority);
            }
        }

        // Remove empty priority queues
        foreach (var key in keysToRemove) {
            _queue.Remove(key);
        }

        return removedItems;
    }
}

public class ConcurrentPriorityQueue<TKey, TValue> : IDisposable where TKey : notnull {
    private readonly ConcurrentDictionary<TKey, ConcurrentQueue<TValue>> _queues = new();
    private readonly IComparer<TKey> _keyComparer;
    private readonly ReaderWriterLockSlim _priorityLock = new();
    private readonly SortedSet<TKey> _priorities;
    private int _count;
    private bool _disposed = false;

    public int Count => Volatile.Read(ref _count);

    public ConcurrentPriorityQueue(IComparer<TKey> keyComparer = null) {
        _keyComparer = keyComparer ?? Comparer<TKey>.Default;
        _priorities = new SortedSet<TKey>(Comparer<TKey>.Create((x, y) => _keyComparer.Compare(y, x))); // Descending
    }

    public void Enqueue(TKey priority, TValue value) {
        ThrowIfDisposed();

        var queue = _queues.GetOrAdd(priority, key => {
            _priorityLock.EnterWriteLock();
            try {
                _priorities.Add(key);
            } finally {
                _priorityLock.ExitWriteLock();
            }
            return new ConcurrentQueue<TValue>();
        });

        queue.Enqueue(value);
        Interlocked.Increment(ref _count);
    }

    public bool TryDequeue(out TValue value) {
        ThrowIfDisposed();
        value = default!;

        _priorityLock.EnterReadLock();
        try {
            foreach (var priority in _priorities) {
                if (_queues.TryGetValue(priority, out var queue) && queue.TryDequeue(out value!)) {
                    Interlocked.Decrement(ref _count);

                    // Schedule cleanup if queue becomes empty
                    if (queue.IsEmpty) {
                        Task.Run(() => CleanupEmptyQueue(priority, queue));
                    }

                    return true;
                }
            }
        } finally {
            _priorityLock.ExitReadLock();
        }

        return false;
    }

    public bool TryPeek(out TValue value) {
        ThrowIfDisposed();
        value = default!;

        _priorityLock.EnterReadLock();
        try {
            foreach (var priority in _priorities) {
                if (_queues.TryGetValue(priority, out var queue) && queue.TryPeek(out value!)) {
                    return true;
                }
            }
        } finally {
            _priorityLock.ExitReadLock();
        }

        return false;
    }

    private void CleanupEmptyQueue(TKey priority, ConcurrentQueue<TValue> queue) {
        // Double-check if the queue is still empty
        if (queue.IsEmpty && _queues.TryGetValue(priority, out var currentQueue) && currentQueue == queue) {
            // Use a small delay to avoid aggressive cleanup
            Thread.Sleep(10); // Configurable delay

            if (queue.IsEmpty && _queues.TryRemove(priority, out _)) {
                _priorityLock.EnterWriteLock();
                try {
                    _priorities.Remove(priority);
                } finally {
                    _priorityLock.ExitWriteLock();
                }
            }
        }
    }

    public bool IsEmpty => Count == 0;

    public IEnumerable<TValue> GetAllItems() {
        ThrowIfDisposed();

        _priorityLock.EnterReadLock();
        try {
            // Create a snapshot to avoid concurrent modification during iteration
            var prioritySnapshot = _priorities.ToList();

            foreach (var priority in prioritySnapshot) {
                if (_queues.TryGetValue(priority, out var queue)) {
                    // Snapshot the queue contents
                    var items = queue.ToArray();
                    foreach (var item in items) {
                        yield return item;
                    }
                }
            }
        } finally {
            _priorityLock.ExitReadLock();
        }
    }

    public List<TValue> RemoveItems(Func<TValue, bool> predicate) {
        ThrowIfDisposed();

        var removedItems = new List<TValue>();
        var keysToCheck = new List<TKey>();

        // Get all priorities safely
        _priorityLock.EnterReadLock();
        try {
            keysToCheck.AddRange(_priorities);
        } finally {
            _priorityLock.ExitReadLock();
        }

        foreach (var priority in keysToCheck) {
            if (_queues.TryGetValue(priority, out var queue)) {
                var tempList = new List<TValue>();
                var removedFromThisQueue = 0;

                // Dequeue all items temporarily
                while (queue.TryDequeue(out var item)) {
                    if (predicate(item)) {
                        removedItems.Add(item);
                        removedFromThisQueue++;
                    } else {
                        tempList.Add(item);
                    }
                }

                // Re-enqueue items that weren't removed
                foreach (var item in tempList) {
                    queue.Enqueue(item);
                }

                // Update global count
                if (removedFromThisQueue > 0) {
                    Interlocked.Add(ref _count, -removedFromThisQueue);
                }

                // Schedule cleanup if needed
                if (queue.IsEmpty) {
                    Task.Run(() => CleanupEmptyQueue(priority, queue));
                }
            }
        }

        return removedItems;
    }

    public void Clear() {
        ThrowIfDisposed();

        _priorityLock.EnterWriteLock();
        try {
            _queues.Clear();
            _priorities.Clear();
            Interlocked.Exchange(ref _count, 0);
        } finally {
            _priorityLock.ExitWriteLock();
        }
    }

    public QueueSnapshot GetSnapshot() {
        ThrowIfDisposed();

        _priorityLock.EnterReadLock();
        try {
            var snapshot = new QueueSnapshot {
                TotalCount = Count,
                Priorities = _priorities.ToDictionary(
                    p => p,
                    p => _queues.TryGetValue(p, out var q) ? q.Count : 0
                ),
                Items = GetAllItems().ToList()
            };

            return snapshot;
        } finally {
            _priorityLock.ExitReadLock();
        }
    }

    public bool TryBulkEnqueue(TKey priority, IEnumerable<TValue> values) {
        ThrowIfDisposed();

        var queue = _queues.GetOrAdd(priority, key => {
            _priorityLock.EnterWriteLock();
            try {
                _priorities.Add(key);
            } finally {
                _priorityLock.ExitWriteLock();
            }
            return new ConcurrentQueue<TValue>();
        });

        var count = 0;
        foreach (var value in values) {
            queue.Enqueue(value);
            count++;
        }

        if (count > 0) {
            Interlocked.Add(ref _count, count);
            return true;
        }

        return false;
    }

    private void ThrowIfDisposed() {
        if (_disposed) {
            throw new ObjectDisposedException("Concurrent queue is disposed");
        }
    }

    public void Dispose() {
        if (!_disposed) {
            _priorityLock?.Dispose();
            _disposed = true;
        }
    }

    public class QueueSnapshot {
        public int TotalCount { get; set; }
        public Dictionary<TKey, int> Priorities { get; set; } = new();
        public List<TValue> Items { get; set; } = new();
    }
}
