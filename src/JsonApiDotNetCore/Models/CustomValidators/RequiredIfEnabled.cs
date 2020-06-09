using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace JsonApiDotNetCore.Models.CustomValidators
{
    public class Required : RequiredAttribute
    {
        private bool Disabled { get; set; }

        public override bool IsValid(object value)
        {
            return Disabled || base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            CheckDisableKey(validationContext);
            return Disabled ? ValidationResult.Success : base.IsValid(value, validationContext);
        }

        private void CheckDisableKey(ValidationContext validationContext)
        {
            var httpContextAccessor = (IHttpContextAccessor)validationContext.GetRequiredService(typeof(IHttpContextAccessor));
            Disabled = httpContextAccessor.HttpContext.Items.ContainsKey($"DisableValidation_{validationContext.ObjectType.Name}_{validationContext.MemberName}") 
                       || httpContextAccessor.HttpContext.Items.ContainsKey($"DisableValidation_{validationContext.ObjectType.Name}_Relation");
        }
    }
}
