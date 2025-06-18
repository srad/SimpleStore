using System.Text.RegularExpressions;
using SimpleStore.Helpers.Interfaces;

namespace SimpleStore.Helpers;

public class StorageNameValidator: IValidator<string>
{
    private static readonly Regex StorageNamePattern = new("^[a-z0-9]+(-[_.a-z0-9]+)*(.[a-z0-9]+)*$", RegexOptions.Compiled);

    public bool IsValid(string input) => StorageNamePattern.IsMatch(input);
}