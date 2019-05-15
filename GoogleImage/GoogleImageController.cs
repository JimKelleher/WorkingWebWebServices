using Newtonsoft.Json.Linq;
using System.Web.Http;
using WorkingWebWebServices.GoogleImage;

namespace WorkingWebWebServices
{
    public class GoogleImageController : ApiController
    {
        // This is production:
        // http://www.workingweb.info/Utility/WorkingWebWebServices/api/GoogleImage/?request=paul+simon&number=1

        // Use these to test with our static test files.  First, you must set GoogleImageHelper.boolTest to true.

        // Get values:
        // http://localhost:60133/api/GoogleImage/?request=elton+john&number=1
        // http://localhost:60133/api/GoogleImage/?request=elton+john&number=10
        // http://localhost:60133/api/GoogleImage/?request=elton+john&number=13

        // No values:
        // http://localhost:60133/api/GoogleImage/?request=%22jameshkelleherxyzsmithohboy%22&number=1
        
        // Get the Web Request Response from querying Google, via Google Custom Search:
        public string Get(string request, int number)
        {
            // "Rename" this argument:
            int intNumberOfImagesDesired = number;

            // This class contains our helper stuff.  Its constructor computes all the information
            // we will need:
            GoogleImageHelper googleImageHelper = new GoogleImageHelper(intNumberOfImagesDesired);

            // Init:
            var strResponse = "";
            bool boolErrorFound = false;
            int intTotalResults = 0;

            // Page loop:
            for (int intPage = 0; intPage < GoogleImageHelper.intPages; intPage++)
            {
                // Results: 1, 11, 21, etc:
                int intFirstItemOnPage = (intPage * GoogleImageHelper.intGOOGLE_PAGE_MAXIMUM) + 1;

                // Determine how many items will be on the page we are about to retrieve:
                int intItems;

                // For the last page:
                if (intPage == (GoogleImageHelper.intPages - 1) && GoogleImageHelper.intLastPageItems != 0)
                    { intItems = GoogleImageHelper.intLastPageItems; }
                // For all other pages:
                else
                    { intItems = GoogleImageHelper.intGOOGLE_PAGE_MAXIMUM; }

                // Assemble the parts of the query that will change from page to page:
                string strCustomSearch =
                    GoogleImageHelper.strCUSTOM_SEARCH_BASE +
                    "&q=" + request +
                    "&num=" + intItems.ToString() +
                    "&start=" + intFirstItemOnPage.ToString();

                // Submit the Google Custom Search query and get the response (or a simulation thereof):
                strResponse =
                    googleImageHelper.GetGoogleImageCustomSearchResponse(
                        intPage,        // this relates only to the test process
                        strCustomSearch
                    );

                if (strResponse.IndexOf("error") > -1)
                {
                    // Set the error flag and terminate the loop:
                    boolErrorFound = true;
                    break;
                }
                else
                {
                    // Continue the processing and the loop:

                    // Boil down the approximately 100 fields of the Google Custom Search query response
                    // to the two fields we care about.  Load the JSON array that holds our images:
                    intTotalResults = googleImageHelper.AssembleFinalResult(strResponse);

                }

            }

            if (boolErrorFound == true)
            {
                // NOTE: Google's errors come in JSON format.  Rather than defining a schema for
                // the error and examining it, we will just pass it on as is and leave it for
                // the client (JavaScript) to handle:
                return strResponse;

            }
            else if (intTotalResults == 0) {

                // Return an empty JSON array:
                return "{\"images\":[]}";
            }
            else
            {
                // 1) JSON array to 2) JSON object to 3) JSON string:
                JObject googleImageJsonObject = new JObject();
                googleImageJsonObject["images"] = GoogleImageHelper.googleImageJsonArray;
                string googleImageJsonString = googleImageJsonObject.ToString();

                //---------------------------------------------------------------------------------------------
                // {
                //  "images":[
                //      "http://www.mtishows.com/sites/default/files/profile/elton-john.jpg?download=1",
                //      "http://www.lasvegastravelersinfo.com/images/2016/artist/Elton-John.jpg",
                //      "http://digitalmediawire.com/wp-content/uploads/2016/12/Elton-John.jpg"
                //  ]
                // }
                //---------------------------------------------------------------------------------------------

                // Return the final result, a JSON string containing our Google Image URLs:
                return googleImageJsonString;

            }

        }

    }
}
