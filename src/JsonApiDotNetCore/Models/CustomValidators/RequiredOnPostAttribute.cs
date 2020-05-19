using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace JsonApiDotNetCore.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredOnPostAttribute : ValidationAttribute
    {
        private string Error { get; set; }

        /// <summary>
        /// Validates that the value is not null or empty on POST operations. 
        /// </summary>
        /// <param name="error"></param>
        public RequiredOnPostAttribute(string error = null)
        {
            Error = error;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var httpContextAccessor = (IHttpContextAccessor)validationContext.GetService(typeof(IHttpContextAccessor));
            var request = httpContextAccessor.HttpContext.Request;
            if (request.Method == "POST")
            {
                if (Error == null)
                {
                    Error = string.Format("{0} is required.", validationContext.MemberName);
                }

                if (value == null)
                {
                    return new ValidationResult(Error);
                }

                var propertyType = value.GetType();
                if (propertyType.Equals(typeof(System.String)))
                {
                    if (string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        return new ValidationResult(Error);
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}
