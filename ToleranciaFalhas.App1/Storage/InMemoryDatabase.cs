using System.Collections.Concurrent;
using ToleranciaFalhas.OrderService.Models;

namespace ToleranciaFalhas.OrderService.Database;

public class InMemoryDatabase : IDatabase<Guid, Order>
{
    private readonly ConcurrentDictionary<Guid, Order> _database = new();

    public Order Get(Guid id)
    {
        if (!_database.TryGetValue(id, out var data) || data == null)
        {
            throw new KeyNotFoundException("Unable to find " + id.ToString() + " in the dictionary");
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
            data.Id = old.Id;
            return data;
        });
    }
}