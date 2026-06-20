using System.ComponentModel.DataAnnotations;
using BusinessKit.Shared.Constants;

namespace BusinessKit.Application.BusinessSettings.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AllowedCurrencyAttribute : ValidationAttribute
{
    public AllowedCurrencyAttribute()
        : base($"Currency must be one of: {CurrencyCodes.AllowedList}.")
    {
    }

    public override bool IsValid(object? value) =>
        value is string str && CurrencyCodes.IsValid(str);
}
