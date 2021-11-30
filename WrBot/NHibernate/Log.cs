using System;

public class LogItem
{
    public virtual int Id { get; set; }
    public virtual DateTime Timestamp { get; set; }
    public virtual string Message { get; set; }
}