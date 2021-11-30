using System.Collections.Generic;

public interface IChannelService
{
    ChannelSettings[] GetAll();
    void Update(ChannelSettings channel);
    void Delete(string channel);
    void Add(ChannelSettings channel);
}