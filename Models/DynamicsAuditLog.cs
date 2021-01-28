using System;
using Newtonsoft.Json;

namespace Otor.DynamicsDemo.Models
{
    [JsonObject]
    public class DynamicsAuditLog
    {
        [JsonProperty("auditid")]
        public Guid? Auditid { get; set; }
        
        [JsonProperty("_objectid_value")]
        public Guid? ObjectId { get; set; }
        
        [JsonProperty("systemuser1.fullname")]
        public string UserName { get; set; }
        
        [JsonProperty("createdon")]
        public DateTime? CreatedOn { get; set; }

        public override string ToString()
        {
            return $"User='{this.UserName}', Date='{this.CreatedOn}'";
        }
    }
}
