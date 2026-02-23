namespace PT.Common.Localization;

public interface ILocalizationLanguageModule : IDisposable
{
    string CultureInfo { get; }
    string LanguageKey { get; }
    void TrackMissingItems();
    void TrackUnLocalizedItems();
    Dictionary<string, string> GetMissingItemsDictionary();
    Dictionary<string, string> GetUnLocalizedItemsDictionary();
    string Localize(string a_textToLocalize);
    string LocalizeError(string a_errorCode);
    bool DefaultModule { get; }
    string GetHelpDocsErrorCodeUrl(string a_code);
    string GetHelpUrlString(string a_topic);
    void AddTranslations(ILocalizationLanguageModuleTranslations a_translations);
}

public interface ILocalizationLanguageModuleTranslations
{
    string LanguageKey { get; }
    Dictionary<string, string> GenerateLocalizationDictionary();
    Dictionary<string, string> GenerateErrorLocalizationDictionary();
    Dictionary<string, string> GenerateHelpTopicLocalizationDictionary();
}