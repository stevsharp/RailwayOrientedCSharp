namespace RailwayOrientedCSharp.Model;

using System;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"    [LOG] {message}");
        Console.ResetColor();
    }
}
