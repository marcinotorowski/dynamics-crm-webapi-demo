using System.Xml.Linq;

namespace Otor.DynamicsDemo.Domain
{
    /// <summary>
    /// A generator of fetchXml strings.
    /// </summary>
    public static class DynamicsCrmFetchXmlBuilder
    {
        /// <summary>
        /// Builds XML string used to fetch audit logs.
        /// </summary>
        /// <param name="lastDays">The number of last days to consider.</param>
        /// <param name="pageSize">The maximum number of elements returned on a single page.</param>
        /// <returns>XML string representing the fetchXml query. The value must be URL-encoded to be used with the Web API.</returns>
        public static string BuildForAuditLogs(int lastDays, int pageSize = 1000)
        {
            var fetchXml = new XElement("fetch", 
                new XAttribute("mapping", "logical"),
                new XAttribute("page", 1),
                new XAttribute("count", pageSize));

            var entity = new XElement("entity", 
                new XAttribute("name", "audit"));
            
            var objectId = new XElement("attribute", 
                new XAttribute("name", "objectid"), 
                new XAttribute("alias", "objectid"));
            
            var createdOn = new XElement("attribute", 
                new XAttribute("name", "createdon"), 
                new XAttribute("value", "FormattedValue"));

            var linkEntity = new XElement("link-entity", 
                new XAttribute("name", "systemuser"), 
                new XAttribute("to", "objectid"), 
                new XAttribute("link-type", "inner"));
            
            linkEntity.Add(new XElement("attribute", 
                new XAttribute("name", "fullname")));
            
            var filter = new XElement("filter", 
                new XAttribute("type", "and"));
            
            filter.Add(new XElement("condition", 
                new XAttribute("attribute", "createdon"), 
                new XAttribute("operator", "last-x-days"), 
                new XAttribute("value", lastDays)));

            entity.Add(objectId, createdOn, linkEntity, filter);
            fetchXml.Add(entity);

            return fetchXml.ToString(SaveOptions.DisableFormatting).Trim();
        }
    }
}
