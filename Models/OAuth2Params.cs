using System;

namespace Otor.DynamicsDemo.Models
{
    /// <summary>
    /// A class that holds parameters for OAuth2.
    /// </summary>
    public class OAuth2Params
    {
        public OAuth2Params(string tenant, Guid clientId, string clientSecret)
        {
            this.Tenant = tenant;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
        }
        
        /// <summary>
        /// The id of the tenant - this can be either GUID or a name.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// The id of the client or the application.
        /// </summary>
        public Guid ClientId { get; set; }
        
        /// <summary>
        /// The client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The resource.
        /// </summary>
        /// <remarks>
        /// For Dynamics API authenticated against Microsoft Graph, this must be set to the URL
        /// of Dynamics CRM instance, for example https://marcin.crm4.dynamics.com
        /// </remarks>
        public string Resource { get; set; }
        
        /// <summary>
        /// The scope.
        /// </summary>
        public string Scope { get; set; } = "https://graph.microsoft.com/.default";
    }
}
