using System;

namespace Wra10Core2023.Util;

/// <summary>
/// End of line exception
/// </summary>
public class EOLException : Exception
{
    public EOLException(string msg) : base(msg) { }
}
