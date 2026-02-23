using System.Collections;

namespace PT.Common.Localization;

/// <summary>
/// Google Translation Utility Class (c)Peter A. Bromberg 2005
/// </summary>
public enum LangPair
{
    EnglishToGerman,
    EnglishToSpanish,
    EnglishToFrench,
    EnglishToItalian,
    EnglishToPortuguese,
    EnglishToJapanese,
    EnglishToKorean,
    EnglishToChineseSimplified,
    GermanToEnglish,
    GermanToFrench,
    SpanishToEnglish,
    FrenchToEnglish,
    FrenchToGerman,
    ItalianToEnglish,
    PortugueseToEnglish,
    JapaneseToEnglish,
    KoreanToEnglish,
    ChineseSimplifiedToEnglish
}

public class TranslateUtil
{
    private TranslateUtil() { }

    public static ArrayList GetLangPairs()
    {
        ArrayList al = new ();
        Array vals = Enum.GetValues(typeof(LangPair));
        al.Add("Please Select");
        foreach (object o in vals)
        {
            al.Add(o.ToString());
        }

        return al;
    }

    //public static string GetTranslatedText(string textToTranslate, string displayLanguage)
    //{
    //    string languageText = displayLanguage;
    //    if (displayLanguage != "Chinese_PRC")
    //    {
    //        languageText = languageText.Split("-".ToCharArray())[0]; //not working with the country specifier for other languages
    //    }
    //    //check here for supported translations: http://translate.google.com/translate_t#

    //    string strLangPair = "en%7C" + languageText;

    //    WebWagon.HTMLPage ww = new WebWagon.HTMLPage();
    //    ww.LoadSource("http://translate.google.com/translate_t?text=" + textToTranslate + "&langpair=" + strLangPair);

    //    string[] stuff = ww.GetTagsByName("div");
    //    string hh = "";
    //    for (int i = 0; i < stuff.Length; i++)
    //    {
    //        string jj = stuff[i];
    //        if (jj.Contains("id=result_box"))
    //        {
    //            string[] pieces = jj.Split(">".ToCharArray());
    //            string[] pieces2 = pieces[1].Split("<".ToCharArray());
    //            hh = pieces2[0];
    //            //Regex findData = new Regex(@"<(?.*).*>(?.*)");
    //            //Match foundData = findData.Match(jj);
    //            //hh = foundData.Groups["text"].Value;
    //            break;
    //        }
    //    }
    //    return hh;

    //    //foreach (string s in stuff)
    //    //{
    //    //    int index = s.IndexOf("<div id=result_box dir=ltr>");
    //    //    int indexOfEnd = s.IndexOf("</div>");

    //    //    if (index > -1)
    //    //    {
    //    //        string answer = s.Substring(27, indexOfEnd - 27);
    //    //        return answer;
    //    //    }
    //    //}

    //    //return "";

    //    //Regex findData = new Regex(@"<(?<tag>.*).*>(?<text>.*)</\k<tag>>");
    //    //Match foundData = findData.Match(stuff[0]);
    //    //return foundData.Groups["text"].Value ;
    //}
}