namespace MigrantWarriorsLibrary.Models
{
    class MongoSettings: IMongoSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
