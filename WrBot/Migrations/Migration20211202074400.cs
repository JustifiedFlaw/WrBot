using FluentMigrator;

[Migration(20211202074400)]
public class AddChannelsAndLogsTables : Migration
{
    public override void Down()
    {
        this.Delete.Table("Channels");
        this.Delete.Table("Logs");
    }

    public override void Up()
    {
        this.Create.Table("Channels")
            .WithColumn("name").AsCustom("character varying").PrimaryKey().NotNullable()
            .WithColumn("runnerenabled").AsBoolean()
            .WithColumn("runnervalue").AsCustom("character varying").Nullable()
            .WithColumn("gameenabled").AsBoolean()
            .WithColumn("gamevalue").AsCustom("character varying").Nullable()
            .WithColumn("categoryenabled").AsBoolean()
            .WithColumn("categoryvalue").AsCustom("character varying").Nullable();

        this.Create.Table("Logs")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity().NotNullable()
            .WithColumn("timestamp").AsDateTime().NotNullable()
            .WithColumn("message").AsAnsiString().NotNullable();
    }
}