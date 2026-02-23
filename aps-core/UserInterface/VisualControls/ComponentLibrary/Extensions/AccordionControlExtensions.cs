using System.Drawing;

using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraBars.Navigation;

using PT.APSCommon.Extensions;

namespace PT.ComponentLibrary.Extensions;

public static class AccordionControlExtensions
{
    public static AccordionControlElement AddItemToGroup(this AccordionControl a_accordion, string a_groupKey, string a_categoryKey)
    {
        AccordionControlElement group = null;
        //Check if the group already exists
        foreach (AccordionControlElement element in a_accordion.Elements)
        {
            if (element.Name == a_groupKey)
            {
                group = element;
                break;
            }
        }

        if (group == null)
        {
            //First add a new group
            group = new AccordionControlElement
            {
                Style = ElementStyle.Group,
                Name = a_groupKey,
                Expanded = true,
                Text = a_groupKey.Localize()
            };

            a_accordion.Elements.Add(group);
        }

        //Add a category element
        AccordionControlElement newCategory = new ()
        {
            Style = ElementStyle.Item,
            Name = a_categoryKey,
            Text = a_categoryKey.Localize()
        };

        group.Elements.Add(newCategory);

        return newCategory;
    }

    public static AccordionControlElement AddSubGroup(this AccordionControl a_accordion, string a_groupKey, string a_categoryKey)
    {
        //Check if category group exists
        foreach (AccordionControlElement element in a_accordion.Elements)
        {
            if (element.Name == a_groupKey)
            {
                return element;
            }
        }

        AccordionControlElement group = null;
        //Check if the group already exists
        foreach (AccordionControlElement element in a_accordion.Elements)
        {
            if (element.Name == a_groupKey)
            {
                group = element;
                break;
            }
        }

        if (group == null)
        {
            //First add a new group
            group = new AccordionControlElement
            {
                Style = ElementStyle.Group,
                Name = a_groupKey,
                Expanded = true,
                Text = a_groupKey.Localize()
            };

            a_accordion.Elements.Add(group);
        }

        //Add a category element
        AccordionControlElement newCategory = new ()
        {
            Style = ElementStyle.Group,
            Name = a_categoryKey,
            Text = a_categoryKey.Localize()
        };

        group.Elements.Add(newCategory);

        return newCategory;
    }

    public static AccordionControlElement AddGroup(this AccordionControl a_accordion, AccordionControlElement a_parent, string a_groupKey, string a_groupCaption, SvgImage a_image, string a_description, int a_position, object a_tag)
    {
        //Check if category group exists
        foreach (AccordionControlElement element in a_accordion.Elements)
        {
            if (element.Name == a_groupKey)
            {
                return element;
            }
        }

        //First add a new group
        AccordionControlElement group = new ()
        {
            Style = ElementStyle.Group,
            Name = a_groupKey,
            Text = a_groupCaption,
            Expanded = true
        };

        group.ImageOptions.SvgImage = a_image;

        group.HeaderTemplate.SetImagePosition(0, HeaderElementAlignment.Left);
        group.HeaderTemplate.SetTextPosition(1, HeaderElementAlignment.Left);
        group.HeaderTemplate.SetContextButtonsPosition(2, HeaderElementAlignment.Right);
        group.HeaderTemplate.SetHeaderControlPosition(3, HeaderElementAlignment.Right);

        // Create an object to initialize the SuperToolTip. 
        group.SuperTip = new SuperToolTip();
        SuperToolTipSetupArgs args = new ();
        args.Title.Text = group.Text;
        args.Contents.Text = a_description;
        args.Contents.ImageOptions.SvgImageSize = new Size(40, 40);
        args.Contents.ImageOptions.SvgImage = group.ImageOptions.SvgImage;
        group.SuperTip.Setup(args);
        group.Tag = a_tag;

        if (a_parent != null)
        {
            a_parent.Elements.Insert(a_position, group);
        }
        else
        {
            a_accordion.Elements.Add(group);
        }

        return group;
    }

    public static AccordionControlElement GetElement(this AccordionControl a_accordion, string a_groupName)
    {
        foreach (AccordionControlElement controlElement in a_accordion.Elements)
        {
            if (controlElement.Name == a_groupName)
            {
                return controlElement;
            }
        }

        return null;
    }

    public static void HideElements(this AccordionControl a_accordion, List<string> a_elementKeys)
    {
        foreach (AccordionControlElement element in a_accordion.Elements)
        {
            element.Visible = !a_elementKeys.Contains(element.Name);
            if (element.Visible)
            {
                HideElements(element, a_elementKeys);
            }
        }
    }

    public static void HideElements(this AccordionControlElement a_element, List<string> a_elementKeys)
    {
        foreach (AccordionControlElement element in a_element.Elements)
        {
            element.Visible = !a_elementKeys.Contains(element.Name);
            if (element.Visible)
            {
                HideElements(element, a_elementKeys);
            }
        }
    }

    public static void HighlightElements(this AccordionControlElement a_element, List<string> a_elementKeys)
    {
        foreach (AccordionControlElement element in a_element.Elements)
        {
            // Check if the element's Name or Tag matches one of the keys
            if (a_elementKeys.Contains(element.Name) ||
                (element.Tag != null && a_elementKeys.Contains(element.Tag.ToString())))
            {
                // Apply highlight styling to matching elements
                element.Appearance.Normal.BackColor = Color.FromArgb(64, Color.LightBlue);
                element.Appearance.Normal.ForeColor = Color.White;
                element.Appearance.Normal.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
            else
            {
                // Reset the appearance for non-matching elements
                element.Appearance.Normal.BackColor = Color.Transparent;
                element.Appearance.Normal.ForeColor = Color.Black;
                element.Appearance.Normal.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }
        }
    }

    public static void ShowItem(this AccordionControl a_accordion, string a_itemName)
    {
        foreach (AccordionControlElement element in a_accordion.Elements)
        {
            if (element.Name == a_itemName)
            {
                element.Visible = true;
                break;
            }
        }
    }

    public static void SelectFirstItemElement(this AccordionControl a_accordion)
    {
        foreach (AccordionControlElement group in a_accordion.Elements)
        {
            foreach (AccordionControlElement elem in group.Elements)
            {
                if (elem is AccordionControlElement item && item.Style == ElementStyle.Item)
                {
                    a_accordion.SelectElement(item);
                    return;
                }
            }
        }
    }
}