using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Models.CustomValidators
{
    public class IsRequiredAttribute : RequiredAttribute
    {
        private bool _isDisabled;

        public override bool IsValid(object value)
        {
            return _isDisabled || base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var httpContextAccessor = (IHttpContextAccessor)validationContext.GetRequiredService(typeof(IHttpContextAccessor));
            _isDisabled = httpContextAccessor.HttpContext.IsValidatorDisabled(validationContext.ObjectType.Name, validationContext.MemberName);
            return _isDisabled ? ValidationResult.Success : base.IsValid(value, validationContext);
        }
    }
}
