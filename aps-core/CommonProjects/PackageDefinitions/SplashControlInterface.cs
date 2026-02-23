namespace PT.PackageDefinitions;

/// <summary>
/// Summary description for SplashControlInterface.
/// </summary>
public interface ISplashControlInterface : IDisposable
{
    void SetVersionLabel(string a_text);
    void SetStatusLabel(string a_text);
    void SetWarningLabel(string a_text, bool a_highlight);

    void CalculateWarningText();

    void PrepareHide();
}