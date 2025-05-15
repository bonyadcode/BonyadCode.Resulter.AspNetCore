using Microsoft.AspNetCore.Identity;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace BonyadCode.Resulter;

public static class ProblemDetailsExtensionBuilder
{
    public static Dictionary<string, string[]> OneKeyWithMultipleValuesError(string key, IEnumerable<string> valueList)
    {
        return new Dictionary<string, string[]> { { key, valueList.ToArray() } };
    }

    public static Dictionary<string, string[]> OneKeyWithOneValueError(string key, string value)
    {
        return new Dictionary<string, string[]> { { key, [value] } };
    }

    public static Dictionary<string, string[]> MultipleKeysWithOneValueError(IEnumerable<string> keyList, string value)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var key in keyList)
        {
            errors[key] = [value];
        }

        return errors;
    }

    public static Dictionary<string, string[]> MultipleKeysWithMultipleValuesError(IEnumerable<string> keyList,
        IList<string> valueList)
    {
        var errors = new Dictionary<string, string[]>();
        var i = 0;
        foreach (var key in keyList)
        {
            errors[key] = [valueList[i]];
            i++;
        }

        return errors;
    }

    public static Dictionary<string, string[]> IdentityError(IdentityResult identityResult)
    {
        return identityResult.Errors.ToDictionary(ie => ie.Code, ie => new[] { ie.Description });
    }

    public static Dictionary<string, string[]> ValidationResultError(ValidationResult validationResult)
    {
        return validationResult.Errors.ToDictionary(
            vf => !vf.PropertyName.Contains('.')
                ? vf.PropertyName
                : vf.PropertyName.Split('.')[vf.PropertyName.Split('.').Length - 1],
            vf => new[] { vf.ErrorMessage });
    }

    public static Dictionary<string, string[]> ValidationError(string propertyName, IList<string> invalidItems)
    {
        return new Dictionary<string, string[]> { { propertyName, invalidItems.ToArray() } };
    }

    public static Dictionary<string, string[]> ExceptionError(Exception ex)
    {
        var dictionary = new Dictionary<string, string[]>();
        foreach (var property in ex.GetType().GetProperties())
        {
            dictionary[property.Name] = [property.GetValue(ex)?.ToString() ?? string.Empty];
        }

        return dictionary;
    }
}