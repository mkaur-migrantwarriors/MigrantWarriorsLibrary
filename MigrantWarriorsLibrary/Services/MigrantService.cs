using Microsoft.VisualBasic.FileIO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using MigrantWarriorsLibrary.Models;
using MigrantWarriorsLibrary.Filters;

namespace MigrantWarriorsLibrary.Services
{
    public class MigrantService
    {
        private readonly IMongoCollection<Migrant> _migrants;
        private readonly Dictionary<long, Tuple<decimal, decimal>> _coordinatesData;

        public MigrantService(IMongoSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _migrants = database.GetCollection<Migrant>(settings.CollectionName);
            _coordinatesData = GetLatitudeLongitudeInfo();
        }

        public List<Migrant> Get() =>
            _migrants.Find(migrant => !string.IsNullOrEmpty(migrant.Id)).ToList();

        public Migrant Get(string id) =>
            _migrants.Find<Migrant>(migrant => migrant.Id == id).FirstOrDefault();

        public dynamic Create(Migrant migrant)
        {
            Helper helper = new Helper();
            try
            {
                List<Migrant> existingMigrant = Get().Where(m => m.AadharNumber + m.Phone == migrant.AadharNumber + migrant.Phone).ToList();
                if (existingMigrant.Count == 0)
                {
                    _migrants.InsertOne(migrant);
                    return helper.CreateResponse(Helper.SUCCEEDED, migrant.IsVerified ? Helper.SUCCESSFULLYADDED : Helper.PENDINGVERIFICATION);
                }

                return helper.CreateResponse(Helper.FAILURE, Helper.ALREADYEXISTS);
            }
            catch(Exception)
            {
                return helper.CreateResponse(Helper.FAILURE, Helper.DATANOTADDED);
            }
        }

        public void Update(string id, Migrant migrant) =>
            _migrants.ReplaceOne(migrant => migrant.Id == id, migrant);

        public void Remove(Migrant migrant) =>
            _migrants.DeleteOne(migrant => migrant.Id == migrant.Id);

        public void Remove(string id) =>
            _migrants.DeleteOne(mingrant => mingrant.Id == id);

        public Coordinates GetLatitudeLongitudeWithMigrantsCount(long pincode)
        {
            var coordinates = _coordinatesData.FirstOrDefault(t => t.Key == pincode).Value;
            var migrants = Get().Where(t => t.PinCode == pincode).Count();
            var data = new Coordinates();
            data.Latitude = coordinates.Item1;
            data.Longitude = coordinates.Item2;
            data.Count = migrants;
            return data;
        }

        public void UpdateMigrantData(out Migrant migrant, Migrant existingMigrant, bool isVerified)
        {
            migrant = existingMigrant;
            var completeInfo = new Dictionary<string, string>();
            var client = new RestClient($"https://pincode.saratchandra.in/api/pincode/{migrant.PinCode}");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            object obj = JsonConvert.DeserializeObject<object>(response.Content);
            var list = JObject.FromObject(obj).SelectToken("data[0]").ToList();
            foreach(JProperty listitem in list)
            {
                var key = listitem.Name;
                var value = ((JValue)listitem.First).Value;
                completeInfo.Add(key, value.ToString());
            }
            migrant.District = completeInfo["district"];
            migrant.State = completeInfo["state_name"];
            migrant.IsVerified = isVerified;
        }

        public List<Migrant> GetStateWiseData(string state)
        {
            return Get().Where(t => t.State.ToLower() == state.ToLower()).ToList();
        }

        private Dictionary<long, Tuple<decimal, decimal>> GetLatitudeLongitudeInfo()
        {
            var info = new Dictionary<long, Tuple<decimal, decimal>>();
            using (TextFieldParser parser = new TextFieldParser(@"Resources\Latitute_Longitude.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    var latitudeLogitude = new Tuple<decimal, decimal>(Convert.ToDecimal(fields[1]), Convert.ToDecimal(fields[2]));
                    info.Add(Convert.ToInt64(fields[0]), latitudeLogitude);
                }
            }
            return info;
        }
    }
}