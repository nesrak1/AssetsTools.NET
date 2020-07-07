using System;

namespace Plurally.Localization
{
    public interface ILocalization
    {
        bool IsPlural(string word);
        bool IsSingular(string word);
        string Pluralize(string word);
        string Singularize(string word);
    }
}
