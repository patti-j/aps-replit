using System.Text;

namespace ExternalIntegrations
{
    internal class Utilities
    {
        internal static string StripHtml(string a_s)
        {
            StringBuilder st = new StringBuilder(a_s);
            st.Replace("<br>", "\n");
            st.Replace("<p>","");
            st.Replace("</p>","");
            return st.ToString();
        }

        
    }
}
