using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Otor.DynamicsDemo.Domain;
using Otor.DynamicsDemo.Models;
using Otor.DynamicsDemo.Web;

namespace Otor.DynamicsDemo
{
    public class Program
    {
        // your Dynamics CRM URL
        private const string DynamicsCrmUrl = "https://XXXXXXX.crm4.dynamics.com";

        // tenant ID or name
        private const string Tenant = "00000000-0000-0000-0000-000000000000";

        // application or client ID
        private const string ApplicationOrClientId = "00000000-0000-0000-0000-000000000000";

        // client secret
        private const string ClientSecret = "abcdefghijklmnopqrstuwxyz";
        
        /// <summary>
        /// A demo showing how to perform a paginated request to Dynamics Web API with
        /// OAuth2 authentication.
        /// </summary>
        public static async Task<int> Main(string[] args)
        {
            var authParams = new OAuth2Params(Tenant, Guid.Parse(ApplicationOrClientId), ClientSecret)
            {
                Resource = DynamicsCrmUrl
            };
            
            var request = new DynamicsCrmWebApiRequest(authParams);

            const int perPage = 200;
            const int lastDays = 30;
            
            var fetchXml = DynamicsCrmFetchXmlBuilder.BuildForAuditLogs(lastDays, perPage);
            var encoded = HttpUtility.UrlEncode(fetchXml).Replace("+", "%20");
            
            var pageUri = new Uri(DynamicsCrmUrl.TrimEnd('/') + "/api/data/v9.1/audits?fetchXml=" + encoded);// HttpUtility.UrlEncode(fetchXml).Replace("+", "%20"));

            var page = 1;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var allElements = new List<DynamicsAuditLog>();
            
            while (pageUri != null)
            {
                Console.WriteLine("{0} Getting page #{1} with no more than {2} entries...", stopWatch.Elapsed, page++, perPage);
                using var response = await request.RequestAsync(pageUri);
                try
                {
                    var elements = await response.GetElementsAsync<DynamicsAuditLog>();
                    Console.WriteLine("{0} Received {1} entries.", stopWatch.Elapsed, elements.Count);
                    allElements.AddRange(elements);
                    pageUri = await response.GetNextPageUrlAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Error: {1}", stopWatch.Elapsed, e);
                    return 1;
                }
            }
            
            Console.WriteLine("{0} Finished! Received {1} entries on {2} pages.", stopWatch.Elapsed, allElements.Count, page - 1);
            stopWatch.Stop();

            Console.WriteLine("First 5 items:");
            foreach (var item in allElements.Take(5))
            {
                Console.WriteLine(" * {0}", item);
            }

            return 0;
        } 
    }
}
