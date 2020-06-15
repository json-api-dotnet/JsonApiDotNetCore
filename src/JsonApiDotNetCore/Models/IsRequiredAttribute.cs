using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Models
{
    public sealed class IsRequiredAttribute : RequiredAttribute
    {
        private bool _isDisabled;

        public override bool IsValid(object value)
        {
            return _isDisabled || base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var httpContextAccessor = (IHttpContextAccessor)validationContext.GetRequiredService(typeof(IHttpContextAccessor));
            _isDisabled = httpContextAccessor.HttpContext.IsValidatorDisabled(validationContext.MemberName, validationContext.ObjectType.Name);
            return _isDisabled ? ValidationResult.Success : base.IsValid(value, validationContext);
        }
    }
}
