using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Handlers
{
    public class ResponseHandler : DelegatingHandler
    {
        private int counter = 1;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            //if (response.Headers.Any(c => /*c.Key.Equals("x-ms-resource-unit") || */
            //                              c.Key.Equals("x-ms-throttle-limit-percentage") || 
            //                              c.Key.Equals("x-ms-throttle-scope") || 
            //                              c.Key.Equals("x-ms-throttle-information")))
            //{
            //    var xMsResourceUnit = response.Headers.FirstOrDefault(c => c.Key.Equals("x-ms-resource-unit"));
            //    var xMsThrottleLimitPercentage = response.Headers.FirstOrDefault(c => c.Key.Equals("x-ms-throttle-limit-percentage"));
            //    var xMsThrottleScope = response.Headers.FirstOrDefault(c => c.Key.Equals("x-ms-throttle-scope"));
            //    var xMsThrottleШnformation = response.Headers.FirstOrDefault(c => c.Key.Equals("x-ms-throttle-information"));
            //}

            if (response.Headers.Any(c => c.Key.Contains("throttle") || c.Key.Contains("limit")))
            {

            }

            if (response.Content != null)
            {
                if (response.Headers.TryGetValues("x-ms-throttle-limit-percentage", out IEnumerable<string>? value1))
                {

                }
                if (response.Headers.TryGetValues("x-ms-throttle-scope", out IEnumerable<string>? value12))
                {

                }
                if (response.Headers.TryGetValues("x-ms-throttle-information", out IEnumerable<string>? value3))
                {

                }

                string responseContent = await response.Content.ReadAsStringAsync();


                if (responseContent.Contains("header"))
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.AppendLine("Response Header: ");
                    stringBuilder.AppendLine(response.Headers.ToString());
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("Response Content: ");
                    stringBuilder.AppendLine(responseContent);
                    
                    File.WriteAllTextAsync(@$"C:\AzureLogs\AzureLogs{counter}.txt", stringBuilder.ToString());

                    counter++;
                }

                if (responseContent.Contains("throttle") || responseContent.Contains("limit"))
                {

                }

            }

            return response;
        }

    }
}
