namespace core.Domain;

public class LogsEntity : BaseEntity
{

    public string? Uname { get; set; }

    public required string Event { get; set; }

    public required DateTimeOffset Time { get; set; }

}

