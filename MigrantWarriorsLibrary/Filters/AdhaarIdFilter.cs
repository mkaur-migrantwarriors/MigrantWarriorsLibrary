using RestSharp;

namespace MigrantWarriorsLibrary.Filters
{
    public class AdhaarIdFilter
    {
        public AdhaarIdFilter()
        {
        }

        public bool IsValidAadhaarNumber(long aadharNumber)
        {
            var client = new RestClient($"https://aadhaarnumber-verify.p.rapidapi.com/Uidverify?uidnumber={aadharNumber}&clientid=111&method=uidverify&txn_id=123456");
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-rapidapi-host", "aadhaarnumber-verify.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "e70acd8db1msh792c9ebba5d001fp1aaf35jsn1b5c56c47ac4");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return response.Content.Contains("Succeeded");
            }
            return false;
        }
    }
}
