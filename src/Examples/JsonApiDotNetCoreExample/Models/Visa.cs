using System;

namespace JsonApiDotNetCoreExample.Models
{
    public class Visa
    {
        public int Id { get; set; }
        public string CountryCode { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
