using System.Collections.Generic;

namespace MigrantWarriorsLibrary.Filters
{
    public class Helper
    {
        public List<string> Skills = new List<string>()
        {
            "Carpentor",
            "Electrician",
            "Mason",
            "Driver",
            "Gardener",
            "Farmer",
            "Construction Labour",
            "Sweeper",
            "Foundryman",
            "Porter",
            "Security Guard",
            "Household Help",
            "Beautician",
            "Fitter",
            "Painter",
            "Plumber",
            "Textile labour",
            "Factory worker",
            "Motor mechanic",
            "Street vendor",
            "Hawker",
            "Plastic factory labour",
            "Leather work",
            "Printing work",
            "agriculture help",
            "Office help",
            "Blacksmith",
            "Shoe maker",
            "Weldor",
            "Tailor",
            "Rickshaw Puller",
            "Waste/scrap picker",
            "Beedi factory worker",
            "Brick Kiln work",
            "Butchery",
            "Electroplating",
            "Fish processing",
            "Gem cutting",
            "Matches manufacture",
            "Mineral and mines work",
            "Scavenging",
            "Stone crushing",
            "Tobacco processing",
            "Other"
        };

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
