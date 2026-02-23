namespace PT.Common.Http;

public static class PTHttpUtility
{
    public static string UriCombine(string a_uri1, string a_uri2)
    {
        a_uri1 = a_uri1.TrimEnd('/');
        a_uri2 = a_uri2.TrimStart('/');
        return string.Format("{0}/{1}", a_uri1, a_uri2);
    }

    public static string UriCombine(string a_uri1, int a_port)
    {
        a_uri1 = a_uri1.TrimEnd('/');
        return string.Format("{0}:{1}", a_uri1, a_port);
    }

    public static string EnsureTrailingSlash(string a_uri)
    {
        a_uri = a_uri.TrimEnd('/');
        return a_uri + '/';
    }
}