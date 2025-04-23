using System;

public class KeyCollisionException : Exception
{
    public KeyCollisionException()
        : base("A key collision occurred in the dictionary.")
    {
    }

    public KeyCollisionException(string message)
        : base(message)
    {
    }

    public KeyCollisionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}