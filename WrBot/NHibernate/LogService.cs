using System;
using NHibernate;

public class LogService : ILogService
{
    private ISessionFactory sessionFactory;

    public LogService(ISessionFactory sessionFactory)
    {
        this.sessionFactory = sessionFactory;
    }

    public void Add(DateTimeOffset timestamp, string message)
    {
        using (var session = this.sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
             var logItem = new LogItem 
             {
                 Timestamp = timestamp.UtcDateTime,
                 Message = message
             };

             session.SaveOrUpdate(logItem);

             transaction.Commit();
        }
    }
}