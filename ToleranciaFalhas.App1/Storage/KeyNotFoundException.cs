using System;

public class KeyNotFoundException : Exception
{
    public KeyNotFoundException()
        : base("Unable to find a key in the dictionary")
    {
    }

    public KeyNotFoundException(string message)
        : base(message)
    {
    }

    public KeyNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}