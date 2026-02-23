using System.Drawing;
using System.Windows.Forms;

namespace PT.PackageDefinitionsUI;

public interface IPropertyEditControl
{
    object GetValue();

    void SelectValue();

    void SetName(string a_name);

    void UpdateValue(object a_value, bool a_readonly);

    bool ShouldExpandByValue();

    Control GetCustomControl();
    Size GetCustomSize();
}