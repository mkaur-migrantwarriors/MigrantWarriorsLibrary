namespace MigrantWarriorsLibrary.Filters
{
    public class Helper
    {
        public Helper()
        {
        }

        public const string SUCCEEDED = "Succeeded";
        public const string FAILURE = "Failure";
        public const string ALREADYEXISTS = "Data is Already Added";
        public const string PENDINGVERIFICATION = "Data is successfully added but Aadhar verification is still pending";
        public const string SUCCESSFULLYADDED = "Aadhar Number is Valid and Data is successfully Added";
        public const string DATANOTADDED= "Unable to Add Data";

        public object CreateResponse(string status, string message)
        {
            return new
            {
                status = status,
                message = message
            };
        }
    }
}
