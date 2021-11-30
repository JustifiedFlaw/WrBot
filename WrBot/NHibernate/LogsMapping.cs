using FluentNHibernate.Mapping;

public class LogsMapping : ClassMap<LogItem>
{
    public LogsMapping()
    {
        this.Table("logs");
        this.Id(x => x.Id).GeneratedBy.Increment();
        this.Map(x => x.Timestamp);
        this.Map(x => x.Message);
    }
}