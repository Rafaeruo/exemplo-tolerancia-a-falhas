namespace ToleranciaFalhas.App1.Database;

public interface IDatabase<TKey, TData> where TKey : new()
{
    public TKey Save(TData data);
    public TData Get(TKey key);
    public void Update(TKey key, TData data);
}