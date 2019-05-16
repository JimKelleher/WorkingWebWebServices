using System.Net;
using System.Web.Http;

namespace WorkingWebWebServices
{
    public class ProxyController : ApiController
    {
        // NOTE: One-argument controllers conform to the following standard but this controller
        // is different.  Its argument is a URL which will contain slash characters.  Slash
        // characters are intrinsic to the routing process and, as such, URLs "break" it.
        // Question mark-based arguments have no such problem, thus, this issue is not an
        // inconvenience and can be safely ignored: 
        // GET: api/Proxy/request content
        //      api/Proxy/?request=request content

        // NOTE 2: Hall & Oates was chosen because it contains 1) space characters, replaced
        // by underscores and 2) an ampersand character, replaced by its hex representation.

        // This doesn't work:
        // http://localhost:60133/api/proxy/https://en.wikipedia.org/wiki/Hall_%26_Oates
        // This works:
        // http://localhost:60133/api/proxy/?request=https://en.wikipedia.org/wiki/Hall_%26_Oates

        // Get the Web Request Response (ie, my Proxy Engine) necessary to handle CORS violations by the Wikipedia API:
        public string Get(string request)
        {
            // Get the response:
            WebClient webClient = new WebClient();
            string response = webClient.DownloadString(request);

            //--------------------------------------------------------------------------------------------------
            // CORS-compliant XML HTTP GET request/response

            // CROSS-REFERENCE: PROXY TRANSIT ENCODING/DECODING 1 OF 3:
            // THIS IS:  C:\a_dev\ASP\WorkingWebWebServices\WorkingWebWebServices(ProxyController.cs)
            // SEE ALSO: C:\a_dev\ASP\WikipediaDiscography\WikipediaDiscography(WikipediaDiscography.js)
            // SEE ALSO: C:\a_dev\ASP\WorkingWebWebServices\WorkingWebWebServices(WorkingWebBrowserServices.js)

            // Encode the only characters that get messed up in transit:
            response = response.Replace("\"", "DOUBLEQUOTE");
            response = response.Replace("\\", "BACKSLASH");
            //--------------------------------------------------------------------------------------------------

            // Return it:
            return response;

        }

    }
}
