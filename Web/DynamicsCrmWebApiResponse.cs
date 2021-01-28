using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Otor.DynamicsDemo.Web
{
    /// <summary>
    /// The class that takes the HTTP response, handles any app-specific errors and provides
    /// convenient methods for paging and enumerating the returned elements.
    /// </summary>
    public class DynamicsCrmWebApiResponse : IDisposable
    {
        private const string FetchXmlUrlParam = "fetchXml";
        private const string FetchXmlPagingCookie = "@Microsoft.Dynamics.CRM.fetchxmlpagingcookie";
        private const string PagingCookieHtmlRequest = "paging-cookie";
        private const string PagingCookieHtmlResponse = "pagingcookie";
        private const string PageHtml = "page";
        
        private readonly Uri _request;
        private readonly HttpWebResponse _response;
        private JObject _parsedJsonResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsCrmWebApiResponse"/> class.
        /// </summary>
        /// <param name="response">The HTTP Web response.</param>
        public DynamicsCrmWebApiResponse(HttpWebResponse response)
        {
            this._request = response.ResponseUri;
            this._response = response;
        }
        
        /// <summary>
        /// Returns the URL pointing to the next chunk of items.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A hot running task that returns the URL to the next page.</returns>
        /// <remarks>If this method returns null, then it means that there are no more pages to query.</remarks>
        public async Task<Uri> GetNextPageUrlAsync(CancellationToken cancellationToken = default)
        {
            if (this._parsedJsonResponse == null)
            {
                await this.ReadContentAsync(cancellationToken).ConfigureAwait(false);
            }
            
            var pagingCookie = this._parsedJsonResponse[FetchXmlPagingCookie]?.Value<string>();
            if (string.IsNullOrEmpty(pagingCookie))
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var pagingCookieResponseHtml = HttpUtility.HtmlDecode(pagingCookie);

            var html = XElement.Parse(pagingCookieResponseHtml);

            // The cookie is stored in the attribute "pagingcookie"
            pagingCookie = html.Attribute(PagingCookieHtmlResponse)?.Value;

            // The value must be decoded twice!
            pagingCookie = HttpUtility.UrlDecode(pagingCookie);
            pagingCookie = HttpUtility.UrlDecode(pagingCookie);

            var queryParse = HttpUtility.ParseQueryString(this._request.Query);

            // Let's take the current fetchXml from the URL and upgrade it with new cookie information
            var currentFetch = queryParse.Get(FetchXmlUrlParam);

            if (currentFetch == null)
            {
                throw new InvalidOperationException("fetchXml must be present in the query at this point.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var bodyAsXml = XElement.Parse(currentFetch);
            bodyAsXml.SetAttributeValue(PagingCookieHtmlRequest, pagingCookie);

            var page = int.Parse(bodyAsXml.Attribute(PageHtml)?.Value ?? "0");
            bodyAsXml.SetAttributeValue(PageHtml, page + 1);

            var newQuery = "?" + string.Join("&", queryParse.AllKeys.Select(paramName =>
            {
                string escapedName, escapedValue;

                if (paramName == FetchXmlUrlParam)
                {
                    // if the parameter is fetchXml, we update it with a new value (encoded).
                    escapedName = HttpUtility.UrlEncode(paramName);
                    escapedValue = HttpUtility.UrlEncode(bodyAsXml.ToString(SaveOptions.DisableFormatting));
                }
                else
                {
                    // for any other URL param just take it as-is.
                    escapedName = HttpUtility.UrlEncode(paramName);
                    escapedValue = HttpUtility.UrlEncode(queryParse[paramName]);
                }

                return $"{escapedName}={escapedValue}";
            }));
            
            cancellationToken.ThrowIfCancellationRequested();

            // Now build the new URI
            var uriBuilder = new UriBuilder(this._request)
            {
                Query = newQuery
            };

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Returns a read-only collection of elements returned by Dynamics CRM API.
        /// </summary>
        /// <typeparam name="T">The type of the element after deserialization from JSON.</typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A hot-running task that returns a readonly collection of elements of a given type (after deserialization).</returns>
        public async Task<IReadOnlyCollection<T>> GetElementsAsync<T>(CancellationToken cancellationToken = default)
        {
            if (this._parsedJsonResponse == null)
            {
                await this.ReadContentAsync(cancellationToken).ConfigureAwait(false);
            }

            var objects = (JArray)this._parsedJsonResponse["value"];
            return objects?.Select(json => json.ToObject<T>()).ToList() ?? new List<T>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this._response?.Dispose();
        }

        private async Task ReadContentAsync(CancellationToken cancellationToken)
        {
            await using (var stream = this._response.GetResponseStream())
            {
                using var streamReader = new StreamReader(stream);
                var jsonReader = new JsonTextReader(streamReader);
                this._parsedJsonResponse = await JObject.LoadAsync(jsonReader, cancellationToken).ConfigureAwait(false);

                if (this._parsedJsonResponse == null)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}