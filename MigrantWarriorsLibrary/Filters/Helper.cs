namespace MigrantWarriorsLibrary.Filters
{
    public class Helper
    {
        public Helper()
        {
        }

        public object CreateResponse(int status)
        {
            return new
            {
                status = status,
            };
        }
    }
}
