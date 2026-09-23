using System;

namespace DanmaobTisax.Domain.Exceptions;

public class InvalidPlanStateException : InvalidOperationException
{
    public InvalidPlanStateException(string message) : base(message)
    {
    }
}
