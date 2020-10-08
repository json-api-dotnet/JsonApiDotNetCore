using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class HouseholdInsurance : Identifiable<string>
    {
        public override string Id 
        {
            get => $"{Address}-{Excess}";
            set
            {
                var split = value.Split('-');
                Address = split[0];
                Address = split[1];
            }
        }
        
        [Attr]
        [Required]
        public string Address { get; set; }
        
        [Attr]
        [Required]
        public Excess Excess { get; set; }

        [Attr]
        public decimal MonthlyFee { get; set; }
        
        [HasOne]
        public Person Person { get; set; }
    }
    
    public enum Excess
    {
        None,
        Low,
        High,
    }
}
