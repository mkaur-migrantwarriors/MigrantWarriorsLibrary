namespace MigrantWarriorsLibrary.Models
{
    public interface IMongoSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
