using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Web.Configuration;
using System;

// NOTE: Math is a static class, not a namespace:
using static System.Math;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// Unfortunately, this API is subject to this limit.  I must make all test/run cycles count.
// Test in small batches, ones, threes, elevens at the most.  When possible, test from imported
// files instead of the Google server.
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// {
//  "error": {
//   "errors": [
//    {
//     "domain": "usageLimits",
//     "reason": "dailyLimitExceeded",
//     "message": "This API requires billing to be enabled on the project. Visit https://console.developers.google.com/billing?project=124542758258 to enable billing.",
//     "extendedHelp": "https://console.developers.google.com/billing?project=124542758258"
//    }
//   ],
//   "code": 403,
//   "message": "This API requires billing to be enabled on the project. Visit https://console.developers.google.com/billing?project=124542758258 to enable billing."
//  }
// }
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------

namespace WorkingWebWebServices.GoogleImage
{
    public class GoogleImageHelper
    {
        // These will be filled in the constructor:

        // Web.config-supplied URLs, URL prefixes and settings:
        public static string strCUSTOM_SEARCH_PREFIX;
        public static string strCUSTOM_SEARCH_ENGINE_ID;
        public static string strDEVELOPER_API_KEY;
        public static string strSEARCH_TYPE;
        public static int intGOOGLE_PAGE_MAXIMUM;

        // This is an argument to the constructor which has been saved:
        public static int intNumberOfImagesDesired;

        // This is the part of the query that won't change from page to page:
        public static string strCUSTOM_SEARCH_BASE;

        // This will do our HTTP Gets:
        public static WebClient webClient;

        // This is a JSON array to hold our images:
        public static JArray googleImageJsonArray;
        
        // These drive the creation loop:
        public static int intPageMaximum;
        public static int intPages;
        public static int intLastPageItems;

        // We will do our testing with static files so as not to detract from our Google daily quota:
        public static bool boolTest = false;

        // Constructor:
        public GoogleImageHelper(int intNumberOfImagesDesiredArg)
        {
            // This is the argument to the constructor which has been saved:
            GoogleImageHelper.intNumberOfImagesDesired = intNumberOfImagesDesiredArg;

            // Get the application-run settings and set them as "global" variables:
            GetWebConfigConstants();

            // Assemble the parts of the query that won't change from page to page:
            GoogleImageHelper.strCUSTOM_SEARCH_BASE =
                                 GoogleImageHelper.strCUSTOM_SEARCH_PREFIX +
                "?key="        + GoogleImageHelper.strDEVELOPER_API_KEY +
                "&cx="         + GoogleImageHelper.strCUSTOM_SEARCH_ENGINE_ID +
                "&searchType=" + GoogleImageHelper.strSEARCH_TYPE;

            // Set the run loop parameter:
            GoogleImageHelper.intPageMaximum =
                Min(GoogleImageHelper.intNumberOfImagesDesired, GoogleImageHelper.intGOOGLE_PAGE_MAXIMUM);

            // Determine the number of Google pages we will retrieve:
            GoogleImageHelper.intPages         = GoogleImageHelper.intNumberOfImagesDesired / GoogleImageHelper.intGOOGLE_PAGE_MAXIMUM;
            GoogleImageHelper.intLastPageItems = GoogleImageHelper.intNumberOfImagesDesired % GoogleImageHelper.intGOOGLE_PAGE_MAXIMUM;

            if (GoogleImageHelper.intLastPageItems > 0) { GoogleImageHelper.intPages++; }

            // Instantiate these objects:
            GoogleImageHelper.webClient = new WebClient();
            GoogleImageHelper.googleImageJsonArray = new JArray();

        }

        public int AssembleFinalResult(string strResponse)
        {
            // In C#, you can't convert to JSON without a "schema" implemented as C# classes.
            // This very hand utility converts JSON to C# classes: http://json2csharp.com/

            // Instantiate the container class:
            RootObject rootObject = new RootObject { };

            // Populate the container with the Google Custom Search query response:
            JsonConvert.PopulateObject(strResponse, rootObject);

            // Now we can use object notation to refer to the fields we care about.

            // NOTE: This query returns approximately 100 fields but I only want two
            // of them.  I attempted to use a schema that only carried those two fields
            // but retained the overall structure.  This was unsuccessful.
            int intTotalResults = Int32.Parse(rootObject.queries.request[0].totalResults);

            if (intTotalResults > 0)
            {
                // Load the JSON array to hold our images:
                for (int i = 0; i < rootObject.items.Count; i++)
                {
                    GoogleImageHelper.googleImageJsonArray.Add(rootObject.items[i].link);
                }

            }

            // Return the number of images found:
            return intTotalResults;

        }

        public string GetGoogleImageCustomSearchResponse(int intPage, string strCustomSearch)
        {
            string strResponseInputFile = ""; // init
            string strResponse;

            if (boolTest == true)
            {
                // Constant:
                // NOTE: I tried the prescribed method of specifying files to default to the current directory.  I was unsuccessful:
                string strRESPONSE_INPUT_FILE_FOLDER = @"C:\a_dev\ASP\WorkingWebWebServices\WorkingWebWebServices\GoogleImage\test\";

                // Assign the appropriate test file contents in a way that simulates the real flow:
                switch (GoogleImageHelper.intNumberOfImagesDesired)
                {
                    case 1:
                      // These simulate known error conditions:
                      //strResponseInputFile = "error_daily_limit.json";
                      //strResponseInputFile = "error_no_values.json";

                      //strResponseInputFile = "entries_1_page_1_of_1.json";
                        break;

                    case 3:
                        strResponseInputFile = "entries_3_page_1_of_1.json";
                        break;

                    case 10:
                        strResponseInputFile = "entries_10_page_1_of_1.json";
                        break;

                    case 13:

                        // NOTE: intPage is zero-based.
                        if (intPage == 0)
                        { strResponseInputFile = "entries_13_page_1_of_2.json"; }
                        else if (intPage == 1)
                        { strResponseInputFile = "entries_13_page_2_of_2.json"; }
                        break;

                }

                // Get the test file contents:
                strResponse = File.ReadAllText(strRESPONSE_INPUT_FILE_FOLDER + strResponseInputFile);

            }
            else
            {
                // Submit the Google Custom Search query and get the response:
                strResponse = GoogleImageHelper.webClient.DownloadString(strCustomSearch);
            }

            // Return the result:
            return strResponse;

        }

        public void GetWebConfigConstants()
        {
            // Fill variables with values from Web.Config making them, effectively, constants:
            GoogleImageHelper.strCUSTOM_SEARCH_PREFIX    = WebConfigurationManager.AppSettings["CUSTOM_SEARCH_PREFIX"];
            GoogleImageHelper.strCUSTOM_SEARCH_ENGINE_ID = WebConfigurationManager.AppSettings["CUSTOM_SEARCH_ENGINE_ID"];
            GoogleImageHelper.strDEVELOPER_API_KEY       = WebConfigurationManager.AppSettings["DEVELOPER_API_KEY"];
            GoogleImageHelper.strSEARCH_TYPE             = WebConfigurationManager.AppSettings["SEARCH_TYPE"];
            GoogleImageHelper.intGOOGLE_PAGE_MAXIMUM     = Int32.Parse(WebConfigurationManager.AppSettings["GOOGLE_PAGE_MAXIMUM"]);

        }

    }

}