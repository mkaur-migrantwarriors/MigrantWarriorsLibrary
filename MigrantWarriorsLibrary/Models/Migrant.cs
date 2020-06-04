using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MigrantWarriorsLibrary.Models
{
    public class Migrant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        public long AadharNumber { get; set; }
        public long Phone { get; set; }
        public string Address { get; set; }
        public long PinCode { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string Gender { get; set; }
        public bool IsVerified { get; set; }
        public string Mode { get; set; }
        public DateTime RegisteredOn { get; set; }
        public List<string> Skill { get; set; }
    }
}
