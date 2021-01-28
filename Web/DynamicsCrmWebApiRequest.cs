using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.DynamicsDemo.Models;

namespace Otor.DynamicsDemo.Web
{
    /// <summary>
    /// A class to perform requests to the API with a shared authorization token.
    /// </summary>
    public class DynamicsCrmWebApiRequest
    {
        private readonly OAuth2Params _authenticationParameters;
        private string _accessToken;
        private DateTime _tokenExpiration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsCrmWebApiRequest"/> class.
        /// </summary>
        /// <param name="authenticationParameters">The parameters used for OAuth2.</param>
        public DynamicsCrmWebApiRequest(OAuth2Params authenticationParameters)
        {
            this._authenticationParameters = authenticationParameters;
        }

        /// <summary>
        /// Performs authorization (OAuth2) using given OAuth2 parameters.
        /// </summary>
        /// <param name="parameters">The parameters for authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A value task that finishes once the authorization is complete.</returns>
        public async ValueTask Authorize(OAuth2Params parameters, CancellationToken cancellationToken = default)
        {
            var url = new Uri($"https://login.microsoftonline.com/{parameters.Tenant}/oauth2/token");

            var request = (HttpWebRequest)WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.Headers.Add("ContentType", "application/x-www-form-urlencoded");

            var body = new Dictionary<string, string>
            {
                ["tenant"] = parameters.Tenant,
                ["client_id"] = parameters.ClientId.ToString("D"),
                ["grant_type"] = "client_credentials"
            };

            if (!string.IsNullOrEmpty(parameters.Resource))
            {
                body["resource"] = parameters.Resource;
            }

            if (!string.IsNullOrEmpty(parameters.Scope))
            {
                body["scope"] = parameters.Scope;
            }

            if (!string.IsNullOrEmpty(parameters.ClientSecret))
            {
                body["client_secret"] = parameters.ClientSecret;
            }

            await using (var requestStream = request.GetRequestStream())
            {
                var bodyAsBytes = await new FormUrlEncodedContent(body).ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                await requestStream.WriteAsync(bodyAsBytes, 0, bodyAsBytes.Length, cancellationToken).ConfigureAwait(false);
            }

            using var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            await using (var responseStream = response.GetResponseStream())
            {
                using (var streamReader = new StreamReader(responseStream))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var responseParsed = await JToken.LoadAsync(jsonReader, cancellationToken).ConfigureAwait(false);

                        if (responseParsed == null)
                        {
                            throw new InvalidOperationException("Response may not be empty.");
                        }

                        var expiresOn = responseParsed["expires_on"]?.Value<int>() ?? 0;
                        var accessToken = responseParsed["access_token"]?.Value<string>();

                        var expiresOnDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(expiresOn);

                        this._accessToken = accessToken;
                        this._tokenExpiration = expiresOnDate;
                    }
                }
            }
        }

        /// <summary>
        /// Performs an HTTP request + authorization via OAuth2 and returns the task that returns the response object.
        /// </summary>
        /// <param name="uri">The URL of the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A hot-running task that returns the response object.</returns>
        /// <seealso cref="DynamicsCrmWebApiResponse"/>
        public async Task<DynamicsCrmWebApiResponse> RequestAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (this._accessToken == null || this._tokenExpiration < DateTime.UtcNow)
            {
                await this.Authorize(this._authenticationParameters, cancellationToken).ConfigureAwait(false);
            }

            var request = (HttpWebRequest)WebRequest.Create(uri);
            // request.Method = "POST";
            request.Headers["Authorization"] = "Bearer " + this._accessToken;
            request.Headers.Add("Accept", "application/json");

            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);
                return new DynamicsCrmWebApiResponse((HttpWebResponse)response);
            }
            catch (WebException e)
            {
                if (e.Response is not HttpWebResponse httpWebResponse || httpWebResponse.StatusCode != HttpStatusCode.BadRequest)
                {
                    throw;
                }
                
                await using (var s = httpWebResponse.GetResponseStream())
                {
                    if (s == null)
                    {
                        throw;
                    }
                    
                    using var streamReader = new StreamReader(s);
                    var text = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                    var exception = JToken.Parse(text);

                    // Sample output
                    // {"error":{"code":"0x0","message":"Invalid object provided in the request."}}
                    var message = exception["error"]?["message"]?.Value<string>();
                    var errorCode = exception["error"]?["code"]?.Value<string>() ?? "0x0";

                    throw new WebException($"{httpWebResponse.StatusCode:D} {httpWebResponse.StatusCode:G}: {message} (error code {errorCode})");
                }
            }
        }
    }
}
