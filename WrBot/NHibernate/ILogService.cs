using System;

public interface ILogService
{
    void Add(DateTimeOffset timestamp, string message);
}