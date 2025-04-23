using System.Collections.Concurrent;
using ToleranciaFalhas.App1.Models;

namespace ToleranciaFalhas.App1.Database;

public class InMemoryDatabase : IDatabase<Guid, Order>
{
    private readonly ConcurrentDictionary<Guid, Order> _database = new();
    public Order Get(Guid key)
    {
        if (!_database.TryGetValue(key, out var data) || data == null)
        {
            throw new KeyNotFoundException("Unable to find " + key.ToString() + " in the dictionary");
        }
        return data;
    }

    public Guid Save(Order data)
    {
        Guid key = Guid.NewGuid();
        if (!_database.TryAdd(key, data))
        {
            throw new KeyCollisionException();
        }
        return key;
    }

    public void Update(Guid key, Order data)
    {
        _database.AddOrUpdate(key, data, (_, old) =>
        {
            data.SetTransactionKey(old.GetTransactionKey());
            return data;
        });
    }
}