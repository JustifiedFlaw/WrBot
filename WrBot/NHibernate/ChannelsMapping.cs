using FluentNHibernate.Mapping;

public class ChannelsMapping : ClassMap<ChannelItem>
{
    public ChannelsMapping()
    {
        this.Table("channels");
        this.Id(x => x.Name);
        this.Map(x => x.RunnerEnabled);
        this.Map(x => x.RunnerValue);
        this.Map(x => x.GameEnabled);
        this.Map(x => x.GameValue);
        this.Map(x => x.CategoryEnabled);
        this.Map(x => x.CategoryValue);
    }
}