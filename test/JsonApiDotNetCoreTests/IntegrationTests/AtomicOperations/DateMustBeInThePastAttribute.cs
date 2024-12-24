using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class DateMustBeInThePastAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var targetedFields = validationContext.GetRequiredService<ITargetedFields>();

        if (targetedFields.Attributes.Any(attribute => attribute.Property.Name == validationContext.MemberName))
        {
            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(validationContext.MemberName!)!;

            if (propertyInfo.PropertyType == typeof(DateTimeOffset) || propertyInfo.PropertyType == typeof(DateTimeOffset?))
            {
                var typedValue = (DateTimeOffset?)propertyInfo.GetValue(validationContext.ObjectInstance);

                var timeProvider = validationContext.GetRequiredService<TimeProvider>();
                DateTimeOffset utcNow = timeProvider.GetUtcNow();

                if (typedValue >= utcNow)
                {
                    return new ValidationResult($"{validationContext.MemberName} must be in the past.");
                }
            }
        }

        return ValidationResult.Success;
    }
}
