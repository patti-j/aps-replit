using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace PT.Common;

public class Web
{
    [DllImport("wininet.dll", SetLastError = true)]
    public static extern bool InternetGetCookieEx(
        string url,
        string cookieName,
        StringBuilder cookieData,
        ref int size,
        int dwFlags,
        IntPtr lpReserved);

    private const int InternetCookieHttponly = 0x2000;

    /// <summary>
    /// Gets the URI cookie container.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns></returns>
    public static string GetCookieContrainerString(Uri uri)
    {
        //CookieContainer cookies = null;
        // Determine the size of the cookie
        int datasize = 8192 * 16;
        StringBuilder cookieData = new (datasize);
        if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
        {
            if (datasize < 0)
            {
                return null;
            }

            // Allocate stringbuilder large enough to hold the cookie
            cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(
                    uri.ToString(),
                    null,
                    cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
            {
                return null;
            }
        }

        return cookieData.ToString().Replace(';', ',');
    }

    [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

    /// <summary>
    /// </summary>
    /// <param name="a_uri"></param>
    /// <param name="a_cookieStr">comma seperated list of cookies (see Container.SetCookies comments and GetUriCookieContainer above)</param>
    public static void SetUriCookie(Uri a_uri, string a_cookieStr)
    {
        CookieContainer container = new ();
        container.SetCookies(a_uri, a_cookieStr);
        CookieCollection collection = container.GetCookies(a_uri);
        for (int i = 0; i < collection.Count; i++)
        {
            InternetSetCookie(a_uri.OriginalString, null, collection[i].ToString());
        }
    }

    public static async Task<bool> IsEndPointReachable(string a_url)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(a_url);
            request.Timeout = 15000;
            request.Method = "HEAD";
            using (WebResponse response = await request.GetResponseAsync())
            {
                return ((HttpWebResponse)response).StatusCode == HttpStatusCode.OK;
            }
        }
        catch (WebException)
        {
            return false;
        }
    }
}