using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace JsonApiDotNetCore.Models.CustomValidators
{
    public class RequiredIfEnabled : RequiredAttribute
    {
        public bool Disabled { get; set; }

        public override bool IsValid(object value)
        {
            if (Disabled)
            {
                return true;
            }

            return base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var itemKey = this.CreateKey(validationContext.ObjectType.Name, validationContext.MemberName);
            var httpContextAccessor = (IHttpContextAccessor)validationContext.GetRequiredService(typeof(IHttpContextAccessor));
            if (httpContextAccessor.HttpContext.Items.ContainsKey(itemKey))
            {
                Disabled = true;
            }
              
            if (Disabled)
            {
                return ValidationResult.Success;
            }

            return base.IsValid(value, validationContext);
        }

        private string CreateKey(string model, string propertyName)
        {
            return string.Format("DisableValidation_{0}_{1}", model, propertyName);
        }
    }
}
