using System;

public class OnBotLeftChannelArgs : EventArgs
{
    public string Channel { get; set; }

    public OnBotLeftChannelArgs()
    {
    }
}