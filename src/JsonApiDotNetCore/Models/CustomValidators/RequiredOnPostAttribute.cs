using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredOnPostAttribute : ValidationAttribute
    {     
        public bool AllowEmptyStrings { get; set; }

        /// <summary>
        /// Validates that the value is not null or empty on POST operations. 
        /// </summary>
        /// <param name="allowEmptyStrings">Allow empty strings</param>
        public RequiredOnPostAttribute(bool allowEmptyStrings = false)
        {
            AllowEmptyStrings = allowEmptyStrings;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var httpContextAccessor = (IHttpContextAccessor)validationContext.GetRequiredService(typeof(IHttpContextAccessor));
            if (httpContextAccessor.HttpContext.Request.Method == "POST")
            {
                var additionaError = string.Empty;
                if (!AllowEmptyStrings)
                {
                    additionaError = " or empty";
                }

                if (ErrorMessage == null)
                {
                    ErrorMessage = $"The field {validationContext.MemberName} is required and cannot be null{additionaError}.";
                }

                if (value == null)
                {
                    return new ValidationResult(ErrorMessage);
                }

                if (!AllowEmptyStrings)
                {
                    if (value is string stringValue && string.IsNullOrEmpty(stringValue))
                    {
                        return new ValidationResult(ErrorMessage);
                    }
                }
            }
            return ValidationResult.Success;     
        }
    }
}
