using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;

public class ChannelService : IChannelService
{
    private ISessionFactory SessionFactory;

    public ChannelService(ISessionFactory sessionFactory)
    {
        this.SessionFactory = sessionFactory;
    }

    public ChannelSettings[] GetAll()
    {
        using (var session = SessionFactory.OpenSession())
        {
             var channels = session.Query<ChannelItem>();
             return channels.Select(c => new ChannelSettings
             {
                 Name = c.Name,
                 Runner = new DefaultValueSettings(c.Name)
                 {
                     Enabled = c.RunnerEnabled,
                     Value = c.RunnerValue
                 },
                 Game = new DefaultValueSettings(c.Name)
                 {
                     Enabled = c.GameEnabled,
                     Value = c.GameValue
                 },
                 Category = new DefaultValueSettings(c.Name)
                 {
                     Enabled = c.CategoryEnabled,
                     Value = c.CategoryValue
                 }
             }).ToArray();
        }
    }

    public void Update(ChannelSettings channel)
    {
        using (var session = this.SessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            var channelItem = Map(channel);

            session.Merge(channelItem);

            transaction.Commit();
        }
    }

    public void Delete(string channel)
    {
        var queryString = $"DELETE FROM {typeof(ChannelItem)} where name = :name";
            
        using (var session = SessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            session.CreateQuery(queryString)
                .SetParameter("name", channel)
                .ExecuteUpdate();

            transaction.Commit();
        }
    }

    public void Add(ChannelSettings channel)
    {
        using (var session = SessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            var channelItem = Map(channel);

            session.SaveOrUpdate(channelItem);

            transaction.Commit();
        }
    }

    private static ChannelItem Map(ChannelSettings channel)
    {
        return new ChannelItem
        {
            Name = channel.Name,
            RunnerEnabled = channel.Runner.Enabled,
            RunnerValue = channel.Runner.Value,
            GameEnabled = channel.Game.Enabled,
            GameValue = channel.Game.Value,
            CategoryEnabled = channel.Category.Enabled,
            CategoryValue = channel.Category.Value
        };
    }
}