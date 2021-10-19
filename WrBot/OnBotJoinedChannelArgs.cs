using System;

public class OnBotJoinedChannelArgs : EventArgs
{
    public string Channel { get; set; }

    public OnBotJoinedChannelArgs()
    {
    }
}