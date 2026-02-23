using System.Globalization;

using PT.Common.Exceptions;

namespace PT.Common.Localization;

public interface ILocalizeController
{
    void RegisterLocalizableControl(ILocalizable a_control);
}

public class Localizer
{
    static Localizer()
    {
        //Todo 
        //SetLocalizedLanguage(DisplayLanguageFromCulture(Thread.CurrentThread.CurrentUICulture));
    }

    public Localizer(ILocalizeController a_uiControl)
    {
        m_uiControl = a_uiControl;
    }

    private static ILocalizeController m_uiControl;

    [Obsolete("TODO: Get a reference to the actual implementation of ILocalizeController")]
    public static void RegisterLocalizableControl(ILocalizable a_control)
    {
        m_uiControl.RegisterLocalizableControl(a_control);
    }

    /// <summary>
    /// Loads a localization language module into the application.
    /// </summary>
    /// <param name="a_languageModule">The <see cref="ILocalizationLanguageModule" /> to load.</param>
    public static void LoadLanguage(ILocalizationLanguageModule a_languageModule)
    {
        if (!s_languageModules.TryGetValue(a_languageModule.LanguageKey, out ILocalizationLanguageModule module))
        {
            s_languageModules.Add(a_languageModule.LanguageKey, a_languageModule);
        }

        //TODO: Duplicates have to be ignored due to Localizer being static. Both the UI and system package managers load Localization modules. 
        //Track missing items; use this for testing purposes to develop the localization dictionaries
        #if Localization
            a_languageModule.TrackMissingItems();
            a_languageModule.TrackUnLocalizedItems();
        #endif

        //This is the default language. Initialize as primary
        if (a_languageModule.DefaultModule)
        {
            s_activeLanguageModule = a_languageModule;
        }

        if (a_languageModule is ILocalizationLanguageModuleTranslations translationsObject)
        {
            LoadLanguageTranslations(translationsObject);
        }
    }

    public static void LoadLanguageTranslations(ILocalizationLanguageModuleTranslations a_module)
    {
        if (s_languageModules.TryGetValue(a_module.LanguageKey, out ILocalizationLanguageModule module))
        {
            module.AddTranslations(a_module);
        }
    }

    private static readonly Dictionary<string, ILocalizationLanguageModule> s_languageModules = new ();

    public static Dictionary<string, string> GetSupportedLanguagesConversion()
    {
        Dictionary<string, string> conversionInfo = new ();
        foreach (KeyValuePair<string, ILocalizationLanguageModule> languageModule in s_languageModules)
        {
            conversionInfo.Add(languageModule.Key, languageModule.Value.CultureInfo);
        }

        return conversionInfo;
    }

    #region Culture
    public static string ExceptionLoggingDirectory = string.Empty; //this has to be static to be used.
    private static int s_errorsLeftToLog = 2;

    public static string CultureNameFromDisplayLanguage(string a_displayLanguage)
    {
        switch (a_displayLanguage)
        {
            case "German":
                return "de-DE";
            case "Polish":
                return "pl-PL";
            case "Chinese_PRC":
                return "zh-CN";
            case "French":
                return "fr-FR";
            case "Spanish":
                return "es-ES";
            case "Japanese":
                return "ja-JP";
            case "Dutch":
                return "nl-NL";
            case "Italian":
                return "it-IT";
            case "English_GB":
                return "en-US";
            case "Indonesian":
                return "id-ID";
            case "Turkish":
                return "tr-TR";
            case "Portuguese":
                return "pt-BR";
            #if DEBUG
            case "InvariantCulture":
                return "*";
            #endif
            default:
                return "en-US";
        }
    }

    public static string DisplayLanguageFromCulture(CultureInfo a_culture)
    {
        switch (a_culture.Name)
        {
            case "de-DE":
                return "German";
            case "pl-PL":
                return "Polish";
            case "zh-CN":
                return "Chinese_PRC";
            case "fr-FR":
                return "French";
            case "es-ES":
                return "Spanish";
            case "ja-JP":
                return "Japanese";
            case "nl-NL":
                return "Dutch";
            case "it-IT":
                return "Italian";
            case "en-GB":
                return "English";
            case "en-US":
                return "English";
            case "id-ID":
                return "Indonesian";
            case "tr-TR":
                return "Turkish";
            case "pt-BR":
                return "Portuguese";
            #if DEBUG
            case "*":
                return "InvariantCulture";
            #endif
            default:
                return "English";
        }
    }

    /// <summary>
    /// </summary>
    /// <returns>Whether the current user's language has localized help messages</returns>
    public static bool LocalizedHelpContentAvailable()
    {
        return s_localizedCulture == "English";
    }

    // Other Cultures: http://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.aspx

    private static string s_localizedCulture = "English";

    public static string CurrentLanguage => s_localizedCulture; // get language from culture

    /// <summary>
    /// Sets the localized culture for text localization
    /// </summary>
    public static void SetLocalizedLanguage(string a_dispLanguage)
    {
        if (!s_languageModules.ContainsKey(a_dispLanguage))
        {
            LogException(new LocalizationException("Localization for the specified language has not been loaded"), null);
            return;
        }

        s_localizedCulture = a_dispLanguage;
        s_activeLanguageModule = s_languageModules[s_localizedCulture];
        s_userTextOverrider = new UserTextOverrider(a_dispLanguage);
    }

    public static void SetLocalizedLanguage(CultureInfo a_culture)
    {
        SetLocalizedLanguage(DisplayLanguageFromCulture(a_culture));
        //s_userTextOverrider = new UserTextOverrider(s_currentLanguage); text override aren't used at this point
    }

    public static void SetLocalizationPath(string a_workingDirectory)
    {
        s_localizationPath = Path.Combine(a_workingDirectory, "Localization");
    }

    private static string s_localizationPath = "";

    public static string LocalizationPath => s_localizationPath;

    private static UserTextOverrider s_userTextOverrider;

    public static CultureInfo CurrentCulture
    {
        set => Thread.CurrentThread.CurrentUICulture = value;
        get => Thread.CurrentThread.CurrentUICulture;
    }
    #endregion Culture

    private static ILocalizationLanguageModule s_activeLanguageModule;

    /// <summary>
    /// Returns the string specified based on the Current Language setting.
    /// The language must be set before this is called or an Exception is thrown.
    /// //The name is now the caption rather than the control name.
    /// </summary>
    /// <param name="a_defaultDisplayText"></param>
    public static string GetString(string a_defaultDisplayText)
    {
        string returnString;

        try
        {
            #if DEBUG
            if (!RuntimeStatus.IsRuntime)
            {
                return a_defaultDisplayText;
            }
            #endif

            //TODO: Packages load localization packages without UI.
            if (s_activeLanguageModule == null)
            {
                return a_defaultDisplayText;
            }

            if (string.IsNullOrWhiteSpace(a_defaultDisplayText))
            {
                return string.Empty;
            }

            // localize the text using the the localized culture
            returnString = s_activeLanguageModule.Localize(a_defaultDisplayText);

            //Check if string contains shortcut ampersand and clean string
            if (string.IsNullOrWhiteSpace(returnString))
            {
                if (a_defaultDisplayText.Contains("&") && !a_defaultDisplayText.Contains("&&"))
                {
                    returnString = s_activeLanguageModule.Localize(a_defaultDisplayText.Replace("&", ""));
                }
            }

            //Check if string contains underscore at beginning
            if (string.IsNullOrWhiteSpace(returnString))
            {
                if (a_defaultDisplayText.StartsWith("_"))
                {
                    returnString = LocalizeStringWithUnderscore(a_defaultDisplayText);
                }
            }

            //Check if string contains ellipsis at end and clean string
            if (string.IsNullOrWhiteSpace(returnString))
            {
                if (a_defaultDisplayText.EndsWith("…​") || a_defaultDisplayText.EndsWith("..."))
                {
                    a_defaultDisplayText = a_defaultDisplayText.Replace("…", "...");
                    while (a_defaultDisplayText.EndsWith("."))
                    {
                        //Doesn't TrimEnd just remove all the occurrences of this character anyways?
                        // I feel like this while loop is unnecessary
                        a_defaultDisplayText = a_defaultDisplayText.TrimEnd('.');
                    }

                    returnString = s_activeLanguageModule.Localize(a_defaultDisplayText);

                    if (string.IsNullOrWhiteSpace(returnString))
                    {
                        //Check if cleaned string has colon at end and clean string
                        if (a_defaultDisplayText.EndsWith(":"))
                        {
                            returnString = LocalizeStringWithColon(a_defaultDisplayText);
                        }

                        if (!string.IsNullOrWhiteSpace(returnString))
                        {
                            returnString += "…​";
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(returnString))
            {
                // This if statement below must be after the one above that has EndsWith("...") || EndsWith("…")
                // because this would strip the three periods as ellipsis. This will handle any singular periods,
                // but it treats multiple periods as a singular period. 
                if (a_defaultDisplayText.EndsWith('.'))
                {
                    returnString = s_activeLanguageModule.Localize(a_defaultDisplayText.TrimEnd('.'));
                    if (!string.IsNullOrWhiteSpace(returnString))
                    {
                        returnString += ".";
                    }
                }
            }

            //Check if string contains colon at end and clean string
            if (string.IsNullOrWhiteSpace(returnString))
            {
                if (a_defaultDisplayText.EndsWith(":"))
                {
                    returnString = LocalizeStringWithColon(a_defaultDisplayText);
                }
            }

            //Check if string surrounded by greater/lesser than symbols and clean string
            if (string.IsNullOrWhiteSpace(returnString))
            {
                if (a_defaultDisplayText.StartsWith("<") && a_defaultDisplayText.EndsWith(">"))
                {
                    returnString = LocalizeStringWithGreaterLesserThanSymbols(a_defaultDisplayText);
                }
            }

            //If not found, attempt to localize in english. Default to text key if not found.
            if (string.IsNullOrWhiteSpace(returnString))
            {
                if (CurrentLanguage != "English")
                {
                    returnString = s_languageModules["English"].Localize(a_defaultDisplayText);
                }

                if (string.IsNullOrWhiteSpace(returnString))
                {
                    returnString = a_defaultDisplayText;
                }
            }

            //Apply user substitution of text at the end so that they can see in the UI the value they need to put in the file.

            if (s_userTextOverrider != null && s_userTextOverrider.ContainsString(returnString))
            {
                returnString = s_userTextOverrider.GetOverrideString(returnString);
            }

            if (returnString == null)
            {
                returnString = ""; //don't want to return any nulls;  Can cause problem in GetStringNoAmpersand and maybe other places.
            }
        }
        catch (Exception e) //(LocalizationException)
        {
            returnString = a_defaultDisplayText;
            LogException(e, a_defaultDisplayText);
        }

        returnString = AddLineBreaks(returnString);
        return returnString;
    }

    /// <summary>
    /// Returns the error string specified based on the Current Language setting.
    /// The language must be set before this is called or an Exception is thrown.
    /// </summary>
    /// <param name="a_errorCode">4 digit error code string</param>
    /// <param name="a_stringParameters">array of objects that will be used in ToString()</param>
    /// <param name="a_appendHelpURL">[Bool] whether a help url will be appended to the message</param>
    public static string GetErrorString(string a_errorCode, object[] a_stringParameters = null, bool a_appendHelpURL = false)
    {
        //TODO: Packages Handle errors before packages have been loaded
        if (s_activeLanguageModule == null)
        {
            return a_errorCode;
        }

        //If the message is not a code, just return the string (it should have already been translated)
        //This is required when multi line strings are translated and then sent through an exception.
        if (a_errorCode.Length > 4)
        {
            return a_errorCode;
        }

        string returnStringErrorCode = string.Format("{0} {1}:{2}", "ERROR".Localize(), a_errorCode, Environment.NewLine);
        string nonUsefulDisplayDescription = string.Format("{0}: {1}", "Unknown Error".Localize(), a_errorCode);

        try
        {
            #if DEBUG
            // This needs to be uncommented to get a list of strings without translations.
            //CheckAllLanguageStrings(name, defaultDisplayText);
            #endif
            s_activeLanguageModule = s_languageModules[s_localizedCulture];
            string returnString = s_activeLanguageModule.LocalizeError(a_errorCode);

            //Attempt to get the english translation of an error message if it isn't found in the active language module.
            if (string.IsNullOrEmpty(returnString) && CurrentLanguage != "English")
            {
                returnString = s_languageModules["English"].LocalizeError(a_errorCode);
            }

            if (string.IsNullOrEmpty(returnString))
            {
                #if DEBUG
                //This is to get a list of the items not in the resource file for the language.  They need to be added in their language.
                //if (CurrentCulture.Name != CultureNameFromDisplayLanguage(displayLanguages.English))
                //    AddMissingStringIfNew(CurrentLanguage, name, defaultDisplayText);
                #endif
                if (a_appendHelpURL)
                {
                    returnString = nonUsefulDisplayDescription + s_activeLanguageModule.GetHelpDocsErrorCodeUrl(a_errorCode);
                }
                else
                {
                    returnString = nonUsefulDisplayDescription;
                }
            }
            else
            {
                //Apply the string parameters if there are any
                returnString = SafeString.Format(returnString, a_stringParameters);

                if (a_appendHelpURL)
                {
                    returnString = returnString + Environment.NewLine + s_activeLanguageModule.GetHelpDocsErrorCodeUrl(a_errorCode);
                }
                else
                {
                    returnString = returnStringErrorCode + returnString;
                }
            }

            returnString = AddLineBreaks(returnString);
            return returnString;
        }
        catch (Exception e) //(LocalizationException)
        {
            //Could not find the localization files. Return the unlocalized string prefaced by an error code
            string errorToLog = "Error 0002:" + Environment.NewLine + "Error: " + a_errorCode;

            //Build a list of the values that were to be formatted so that some information can be seen
            if (a_stringParameters != null)
            {
                foreach (string p in a_stringParameters)
                {
                    if (p != null)
                    {
                        errorToLog += p + Environment.NewLine;
                    }
                    else
                    {
                        errorToLog += "null" + Environment.NewLine;
                    }
                }
            }

            LogException(e, errorToLog);
            return nonUsefulDisplayDescription + Environment.NewLine + errorToLog;
        }
    }

    #region Localization Helper
    // The functions in this region will actually call the language module's localize function
    // then some string processing is done to add non-language-specific symbols.
    private static string LocalizeStringWithUnderscore(string a_defaultDisplayText)
    {
        string returnString = s_activeLanguageModule.Localize(a_defaultDisplayText.TrimStart('_'));
        if (!string.IsNullOrWhiteSpace(returnString))
        {
            returnString = string.Format("_{0}", returnString);
        }

        return returnString;
    }

    private static string LocalizeStringWithColon(string a_defaultDisplayText)
    {
        string returnString = s_activeLanguageModule.Localize(a_defaultDisplayText.TrimEnd(':'));
        if (!string.IsNullOrWhiteSpace(returnString))
        {
            returnString += ":";
        }

        return returnString;
    }

    private static string LocalizeStringWithGreaterLesserThanSymbols(string a_defaultDisplayText)
    {
        string returnString = s_activeLanguageModule.Localize(a_defaultDisplayText.TrimStart('<').TrimEnd('>'));
        if (!string.IsNullOrWhiteSpace(returnString))
        {
            returnString = string.Format("<{0}>", returnString);
        }

        return returnString;
    }
    #endregion

    #region String Processing Helpers
    // The functions in this region just process strings and return them; no localization is technically done. 

    /// <summary>
    /// Strip out ampersands which can be used in menus.
    /// </summary>
    public static string GetStringNoAmpersands(string a_defaultDisplayText)
    {
        return GetString(a_defaultDisplayText).Replace("&", "");
    }

    /// <summary>
    /// Add new lines.The localization text is not stored with line breaks.
    /// </summary>
    private static string AddLineBreaks(string a_returnString)
    {
        if (string.IsNullOrWhiteSpace(a_returnString))
        {
            return a_returnString;
        }

        a_returnString = a_returnString.Replace(@"\r\n", Environment.NewLine);
        a_returnString = a_returnString.Replace(@"\n", Environment.NewLine);
        a_returnString = a_returnString.Replace("[NL]", Environment.NewLine);
        return a_returnString;
    }
    #endregion

    #region Testing Helpers
    // TODO?: Implement an enable/disable option interface for generating localization dictionary
    //       The below function doesn't appear to be currently used, but it does seem potentially useful to implement 
    //       something that can just be turned on and off for generating the localization dictionaries. 
    public static bool ReportMissingTranslations()
    {
        bool missingEntriesExist = false;
        System.Text.StringBuilder sb = new ();
        Dictionary<string, string> missingItemsDictionary = s_activeLanguageModule.GetMissingItemsDictionary();
        foreach (string key in missingItemsDictionary.Keys)
        {
            missingEntriesExist = true;
            sb.AppendLine(key);
        }

        System.IO.File.WriteAllText(@"C:\Temp\newEntries.txt", sb.ToString());

        return missingEntriesExist;
    }
    #endregion

    #region Help Links
    //private const string c_unsupportedLanguageURL = "http://help.apsportal.com/advanced-topics/error-codes/unsupported-language-error";

    /// <summary>
    /// Returns URL string pointing to a help page related to the passed topic
    /// </summary>
    /// <param name="a_topic">Help Topic</param>
    /// <returns>URL String</returns>
    public static string GetUrlString(string a_topic)
    {
        //Build and return URL string. Will return just base url if referenced topic is null.
        string referencedTopic = s_activeLanguageModule.GetHelpUrlString(a_topic);
        return referencedTopic;
    }

    public static string GetHelpUrl()
    {
        return s_activeLanguageModule?.GetHelpDocsErrorCodeUrl("");
    }

    // ShowHelp moved to PT.APSCommon.Windows.LocalizerUIHelper

    #endregion

    #region Logging And Exceptions
    /// <summary>
    /// Uses the common logger to log to the Alerts/Exceptions Folder
    /// </summary>
    private static void LogException(Exception a_e, string a_localizingString)
    {
        if (ExceptionLoggingDirectory != string.Empty && s_errorsLeftToLog > 0)
        {
            string errorMessage = "";
            if (!string.IsNullOrEmpty(a_localizingString))
            {
                errorMessage = "Attempting to localize: " + a_localizingString;
            }

            if (s_errorsLeftToLog == 1)
            {
                errorMessage = "Additional localization issues will not be logged for this session" + Environment.NewLine + errorMessage;
            }

            s_errorsLeftToLog--;
            Common.File.SimpleExceptionLogger.LogException(ExceptionLoggingDirectory + @"\Exceptions.log", a_e, errorMessage);
        }
    }

    public class PTLocalizationException : PTHandleableException
    {
        public PTLocalizationException(string a_message) : base(a_message) { }
    }
    #endregion
}