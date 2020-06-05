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
        private readonly Dictionary<string, string[]> _statesDistricts;
        private readonly Dictionary<long, Tuple<string, string>> _pincodeRegions;

        public MigrantService(IMongoSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _migrants = database.GetCollection<Migrant>(settings.CollectionName);
            _coordinatesData = GetLatitudeLongitudeInfo();
            _statesDistricts = GetStatesWithDistricts();
            _pincodeRegions = GetStatesDistrictWithPincode();
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
                if (migrant.State == null && migrant.District == null)
                {
                    return helper.CreateResponse(405);
                }
                if (existingMigrant.Count == 0)
                {
                    _migrants.InsertOne(migrant);
                    return helper.CreateResponse(migrant.IsVerified ? 401 : 402);
                }

                return helper.CreateResponse(403);
            }
            catch(Exception)
            {
                return helper.CreateResponse(404);
            }
        }

        public void Update(string id, Migrant migrant) =>
            _migrants.ReplaceOne(migrant => migrant.Id == id, migrant);

        public void Remove(Migrant migrant) =>
            _migrants.DeleteOne(migrant => migrant.Id == migrant.Id);

        public void Remove(string id) =>
            _migrants.DeleteOne(mingrant => mingrant.Id == id);

        public List<Coordinates> GetLatitudeLongitudeWithMigrantsCount()
        {
            var listOfData = Get();
            var pincodes = listOfData.Select(x => x.PinCode).ToList().Distinct();
            var data = new List<Coordinates>();
            foreach(var pincode in pincodes)
            {
                var coordinate = new Coordinates();
                var latLong = _coordinatesData.FirstOrDefault(t => t.Key == pincode).Value;
                coordinate.Latitude = latLong.Item1;
                coordinate.Longitude = latLong.Item2;
                coordinate.Count = listOfData.Where(x => x.PinCode == pincode).Count();
                data.Add(coordinate);
            }

            return data;
        }

        public void UpdateMigrantData(out Migrant migrant, Migrant existingMigrant, bool isVerified)
        {
            migrant = existingMigrant;
            var pincode_statedistrict = _pincodeRegions.FirstOrDefault(t => t.Key == existingMigrant.PinCode).Value;
            if (pincode_statedistrict == null)
            {
                migrant.District = null;
                migrant.State = null;
            }
            else
            {
            migrant.District = pincode_statedistrict.Item1;
            migrant.State = pincode_statedistrict.Item2;
            }
            migrant.IsVerified = isVerified;
            migrant.RegisteredOn = DateTime.Now;
        }

        public List<Migrant> GetStateWiseData(string state)
        {
            return Get().Where(t => t.State.ToLower() == state.ToLower()).ToList();
        }

        public UIData GetUIPanelCounts()
        {
            var listOfdata = Get();
            ModeOfRegistration modeCount = new ModeOfRegistration
            {
                Whatsapp = listOfdata.Where(x => x.Mode == "Whatsapp").Count(),
                Telegram = listOfdata.Where(x => x.Mode == "Telegram").Count(),
                SMS = listOfdata.Where(x => x.Mode == "SMS").Count(),
                WebForm = listOfdata.Where(x => x.Mode == "Web Form").Count(),
                PhoneCall = listOfdata.Where(x => x.Mode == "Phone call").Count(),
            };
            return new UIData
            {
                Verified = listOfdata.Where(x => x.IsVerified == true).Count(),
                UnVerified = listOfdata.Where(x => x.IsVerified == false).Count(),
                Female = listOfdata.Where(x => x.Gender == "Female").Count(),
                Male = listOfdata.Where(x => x.Gender == "Male").Count(),
                RegistrationModeCount = modeCount
            };
        }

        public Dictionary<string, int> GetCountForAllStates()
        {
            var listOfdata = Get();
            Dictionary<string, int> data = new Dictionary<string, int>();
            foreach(var state in _statesDistricts)
            {
                var count = listOfdata.Where(x => x.State.ToLower() == state.Key.ToLower()).Count();
                data.Add(state.Key, count);
            }
            return data;
        }

        public string[] GetDistricts(string stateName)
        {
            return _statesDistricts.FirstOrDefault(x => x.Key.ToLower() == stateName.ToLower()).Value.ToArray();
        }

        public dynamic GetLast7DaysVerifiedUnverifiedCount(string state, string district)
        {
            var listOfData = state != null ? Get().FindAll (x => (district != null ? x.District.ToString().ToLower() == district.ToLower() : x.State.ToString().ToLower() == state.ToLower()) && x.RegisteredOn >= DateTime.Now.AddDays(-7)).ToList()
                : Get().FindAll(x => x.RegisteredOn >= DateTime.Now.AddDays(-7)).ToList();
            return new
            {
                Verified = listOfData.Where(x => x.IsVerified).Count(),
                UnVerified = listOfData.Where(x => !x.IsVerified).Count()
            };

        }

        public Dictionary<string, int> GetSkillsCount(string state, string district, bool isTopFive = false)
        {
            Helper helper = new Helper();
            var data = new Dictionary<string, int>();
            var listOfData = state != null ? Get().FindAll(x => district != null ? x.District.ToString().ToLower() == district.ToLower() : x.State.ToString().ToLower() == state.ToLower()).ToList()
                : Get().ToList();
            foreach(string skill in helper.Skills)
            {
                data.Add(skill, listOfData.Where(x => x.Skill.Contains(skill)).Count());
            }

            if (isTopFive)
            {
                return data.OrderByDescending(pair => pair.Value).Take(5)
                   .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            else
            {
                return data.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }

        public Dictionary<string, int> GetGenderWiseInTopFiveStates(string gender)
        {
            var listOfData = Get();
            var info = new Dictionary<string, int>();
            var genderData = listOfData.Where(x => x.Gender.ToLower() == gender.ToLower()).ToList();
            foreach (var stateDistrict in _statesDistricts)
            {
                var countInState = genderData.Where(x => x.State.ToLower() == stateDistrict.Key.ToLower()).Count();
                info.Add(stateDistrict.Key, countInState);
            }

            return info.OrderByDescending(pair => pair.Value).Take(5)
                   .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public dynamic GetTopFiveRegistration(string state, string district)
        {
            var listOfData = state != null ? Get().FindAll(x => district != null ? x.District.ToString().ToLower() == district.ToLower() : x.State.ToString().ToLower() == state.ToLower()).ToList()
                        : Get().ToList();

            return new ModeOfRegistration
            {
                Whatsapp = listOfData.Where(x => x.Mode == "Whatsapp").Count(),
                Telegram = listOfData.Where(x => x.Mode == "Telegram").Count(),
                SMS = listOfData.Where(x => x.Mode == "SMS").Count(),
                WebForm = listOfData.Where(x => x.Mode == "Web Form").Count(),
                PhoneCall = listOfData.Where(x => x.Mode == "Phone call").Count(),
            };
        }

        public Dictionary<string, object> GetTopFiveSkillsVerifiedUnverifiedCount(string state, string district)
        {
            var info = new Dictionary<string, object>();
            var listOfData = state != null ? Get().FindAll(x => district != null ? x.District.ToString().ToLower() == district.ToLower() : x.State.ToString().ToLower() == state.ToLower()).ToList()
            : Get().ToList();
            var top5Skills = GetSkillsCount(state, district, true);
            foreach(var skill in top5Skills)
            {
                var verifiedUnverifiedCount = new
                {
                    verified = listOfData.Where(x => x.IsVerified && x.Skill.Contains(skill.Key)).Count(),
                    unverified = listOfData.Where(x => !x.IsVerified && x.Skill.Contains(skill.Key)).Count()
                };
                info.Add(skill.Key.ToString(), verifiedUnverifiedCount);
            }

            return info;
        }

        private Dictionary<string, string[]> GetStatesWithDistricts()
        {
            var info = new Dictionary<string, string[]>();
            using (TextFieldParser parser = new TextFieldParser(@"Resources\States-Districts.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    info.Add(fields[0], fields.Skip(1).ToArray());
                }
            }
            return info;
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

        private Dictionary<long, Tuple<string, string>> GetStatesDistrictWithPincode()
        {
            var info = new Dictionary<long, Tuple<string, string>>();
            using (TextFieldParser parser = new TextFieldParser(@"Resources\Pincode_StatesDistrict.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    var region = new Tuple<string, string>(fields[1], fields[2]);
                    if (!info.ContainsKey(Convert.ToInt64(fields[0])))
                    {
                        info.Add(Convert.ToInt64(fields[0]), region);
                    }
                }
            }
            return info;
        }
    }
}