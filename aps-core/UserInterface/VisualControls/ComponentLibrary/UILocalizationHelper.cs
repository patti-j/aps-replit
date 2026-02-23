using System.Data;
using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Controls;
using DevExpress.XtraBars.InternalItems;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ButtonPanel;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraTab;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;

using PT.APSCommon.Extensions;
using PT.Common.Localization;

using ComboBox = System.Windows.Forms.ComboBox;
using ControlBase = DevExpress.Utils.Controls.ControlBase;
using Localizer = PT.Common.Localization.Localizer;

namespace PT.ComponentLibrary;

public class UILocalizationHelper
{
    public static void LocalizeToolbarManager(BarManager a_tools)
    {
        foreach (Bar bar in a_tools.Bars)
        {
            LocalizeBar(bar);
        }

        foreach (RepositoryItem repositoryItem in a_tools.RepositoryItems)
        {
            if (repositoryItem is RepositoryItemToggleSwitch toggleSwitch)
            {
                toggleSwitch.OnText = toggleSwitch.OnText.Localize();
                toggleSwitch.OffText = toggleSwitch.OffText.Localize();
            }
        }

        for (int toolI = 0; toolI < a_tools.Items.Count; toolI++)
        {
            BarItem barItem = a_tools.Items[toolI];
            barItem.Caption = barItem.Caption.Localize(); //TODO: Probably should localize from a static value like Name.
            barItem.Hint = barItem.Hint.Localize();
            LocalizeSuperTip(barItem.SuperTip);
        }
    }

    public static void LocalizeControlsRecursively(Control.ControlCollection a_controls, ToolTip a_tooltip)
    {
        for (int i = 0; i < a_controls.Count; i++)
        {
            Control control = a_controls[i];
            LocalizeControlToolTip(control, a_tooltip);
        }

        LocalizeControlsRecursively(a_controls);
    }

    private static HashSet<Control> GetHashForDontLocalize(Control[] dontLocalizeThese)
    {
        HashSet<Control> dontLocalizeTheseHash = new ();
        for (int i = 0; i < dontLocalizeThese.Length; i++)
        {
            Control control = dontLocalizeThese[i];
            dontLocalizeTheseHash.Add(control);
        }

        return dontLocalizeTheseHash;
    }

    public static void LocalizeControlsRecursively(Control.ControlCollection a_controls, params Control[] dontLocalizeThese)
    {
        HashSet<Control> dontLocalizeTheseHash = GetHashForDontLocalize(dontLocalizeThese);
        LocalizeControlsRecursively(a_controls, dontLocalizeTheseHash);
    }

    public static void LocalizeSuperTip(SuperToolTip a_superTip)
    {
        if (a_superTip != null)
        {
            foreach (BaseToolTipItem superTipItem in a_superTip.Items)
            {
                if (superTipItem is ToolTipTitleItem)
                {
                    (superTipItem as ToolTipTitleItem).Text = (superTipItem as ToolTipTitleItem).Text.Localize();
                }
                else if (superTipItem is ToolTipItem)
                {
                    (superTipItem as ToolTipItem).Text = (superTipItem as ToolTipItem).Text.Localize();
                }
            }
        }
    }

    public static void LocalizeToolTip(BaseControl a_baseControl)
    {
        if (!string.IsNullOrEmpty(a_baseControl.ToolTip))
        {
            a_baseControl.ToolTip = a_baseControl.ToolTip.Localize();
        }
    }

    private static void LocalizeControlBase(ControlBase a_base)
    {
        a_base.Text = a_base.Text.Localize();
    }

    private static void LocalizeControlsRecursively(Control.ControlCollection a_controls, HashSet<Control> a_dontLocalizeThese)
    {
        for (int i = 0; i < a_controls.Count; i++)
        {
            Control control = a_controls[i];
            LocalizeControl(control, a_dontLocalizeThese);
        }
    }

    public static void LocalizeControl(Control a_control)
    {
        LocalizeControl(a_control, new HashSet<Control>());
    }

    private static void LocalizeControl(Control control, HashSet<Control> dontLocalizeThese)
    {
        if (!dontLocalizeThese.Contains(control))
        {
            if (control is ILocalizable)
            {
                ((ILocalizable)control).Localize();
                return;
            }

            if (control is RibbonControl rControl)
            {
                foreach (object ribbonBarItem in rControl.Items)
                {
                    if (ribbonBarItem is RibbonExpandCollapseItem recItem)
                    {
                        LocalizeSuperTip(recItem.SuperTip);
                    }
                }
            }
            else if (control is Button btn)
            {
                LocalizeButton(btn);
                LocalizeControlsRecursively(btn.Controls);
            }
            else if (control is DropDownButton ddBtn)
            {
                LocalizeButton(ddBtn);
                LocalizeToolTip(ddBtn);
                LocalizeSuperTip(ddBtn.SuperTip);
            }
            else if (control is SimpleButton simpleBtn)
            {
                LocalizeButton(simpleBtn);
                LocalizeToolTip(simpleBtn);
                LocalizeSuperTip(simpleBtn.SuperTip);
            }
            else if (control is SplitGroupPanel)
            {
                LocalizeControlsRecursively(((SplitGroupPanel)control).Controls);
            }
            else if (control is GroupControl)
            {
                GroupControl c = control as GroupControl;
                c.Text = c.Text.Localize();

                if (c.CustomHeaderButtons.Count > 0)
                {
                    LocalizeCustomHeaderButtons(c.CustomHeaderButtons);
                }

                LocalizeControlsRecursively(c.Controls, dontLocalizeThese);
            }
            else if (control is PanelControl)
            {
                LocalizePanel((PanelControl)control, dontLocalizeThese);
            }
            else if (control is DateEdit dateEdit)
            {
                LocalizeControlsRecursively(dateEdit.Controls, dontLocalizeThese);
            }
            else if (control is TimeEdit timeEdit)
            {
                LocalizeControlsRecursively(timeEdit.Controls, dontLocalizeThese);
            }
            else if (control is PictureEdit pictureEdit)
            {
                LocalizeToolTip(pictureEdit);
                LocalizeSuperTip(pictureEdit.SuperTip);
            }
            else if (control is TabPane)
            {
                TabPane t = (TabPane)control;
                t.Text = t.Text.Localize();
                LocalizeControlsRecursively(t.Controls, dontLocalizeThese);
            }
            else if (control is TabNavigationPage)
            {
                TabNavigationPage page = (TabNavigationPage)control;
                page.PageText = page.PageText.Localize();
                page.Caption = page.Caption.Localize();
                LocalizeControlsRecursively(page.Controls, dontLocalizeThese);
            }
            else if (control is NavigationFrame)
            {
                NavigationFrame frame = (NavigationFrame)control;
                frame.Text = frame.Text.Localize();
                if (frame.Controls.Count > 0)
                {
                    LocalizeControlsRecursively(frame.Controls, dontLocalizeThese);
                }
            }
            else if (control is AccordionControl)
            {
                AccordionControl a = (AccordionControl)control;
                LocalizeAccordionControl(a);
            }
            else if (control is TrackBarControl)
            {
                TrackBarControl t = control as TrackBarControl;
                foreach (TrackBarLabel label in t.Properties.Labels)
                {
                    label.Label = label.Label.Localize();
                }

                LocalizeSuperTip(t.SuperTip);
                LocalizeToolTip(t);
            }
            else if (control is Label)
            {
                LocalizeLabel((Label)control);
            }
            else if (control is GroupBox)
            {
                LocalizeGroupBox((GroupBox)control, dontLocalizeThese);
            }
            else if (control is ListBox)
            {
                LocalizeListBox((ListBox)control);
            }
            else if (control is ComboBox)
            {
                LocalizeComboBox((ComboBox)control);
            }
            else if (control is Panel)
            {
                LocalizePanel((Panel)control, dontLocalizeThese);
            }
            else if (control is XtraScrollableControl)
            {
                LocalizeControlsRecursively(((XtraScrollableControl)control).Controls, dontLocalizeThese);
            }
            else if (control is SplitContainer)
            {
                LocalizeSplitContainer((SplitContainer)control, dontLocalizeThese);
            }
            else if (control is CheckBox)
            {
                LocalizeCheckBox((CheckBox)control);
            }
            else if (control is TreeView)
            {
                foreach (TreeNode node in ((TreeView)control).Nodes)
                {
                    LocalizeTreeNode(node);
                }
            }
            else if (control is TabControl)
            {
                LocalizeTabControl((TabControl)control, dontLocalizeThese);
            }
            else if (control is XtraTabControl)
            {
                LocalizeTabControl((XtraTabControl)control, dontLocalizeThese);
            }
            else if (control is RadioButton)
            {
                LocalizeRadioButton((RadioButton)control);
            }
            else if (control is RadioGroup)
            {
                foreach (RadioGroupItem item in (control as RadioGroup).Properties.Items)
                {
                    item.Description = item.Description.Localize();
                }
            }
            else if (control is ToggleSwitch)
            {
                ToggleSwitch t = (ToggleSwitch)control;
                t.Properties.OnText = t.Properties.OnText.Localize();
                t.Properties.OffText = t.Properties.OffText.Localize();
                LocalizeSuperTip(t.SuperTip);
                LocalizeToolTip(t);
            }
            else if (control is NavigationPane)
            {
                NavigationPane p = (NavigationPane)control;
                foreach (NavigationPageBase page in p.Pages)
                {
                    page.PageText = page.PageText.Localize();
                    page.Caption = page.Caption.Localize();
                    LocalizeControlsRecursively(page.Controls);
                }
            }
            else if (control is ImageComboBoxEdit)
            {
                ImageComboBoxEdit c = (ImageComboBoxEdit)control;
                LocalizeSuperTip(c.SuperTip);
                LocalizeToolTip(c);

                if (c.Tag != null && c.Tag.ToString() == "LanguageSelector_DoNotLocalize")
                {
                    // This was intentionally left blank because I want localization 
                    // to trigger even if the control's tag was not set.
                    // I was also extra specific with the condition above to make sure it wouldn't be an issue
                }
                else
                {
                    foreach (ImageComboBoxItem item in c.Properties.Items)
                    {
                        item.Description = item.Description.Localize();
                    }
                }
            }
            else if (control is ComboBoxEdit)
            {
                ComboBoxEdit c = control as ComboBoxEdit;
                LocalizeComboBoxEdit(c);
            }
            else if (control is ButtonEdit)
            {
                ButtonEdit edit = control as ButtonEdit;
                LocalizeBaseControl(edit);
                EditorButtonCollection collection = edit.Properties.Buttons;
                LocalizeButtonCollection(collection);
                LocalizeToolTip(edit);
            }
            else if (control is TextEdit c)
            {
                c.Properties.NullValuePrompt = c.Properties.NullValuePrompt.Localize();
            }
            else if (control is BaseControl)
            {
                LocalizeBaseControl(control as BaseControl);
            }
            else if (control is ControlBase)
            {
                LocalizeControlBase(control as ControlBase);
            }
            else if (control is BarDockControl)
            {
                LocalizeBarDockControl((BarDockControl)control);
            }
            else if (control is DataGridView)
            {
                LocalizeDataGridView((DataGridView)control);
            }
            else if (control is GridControl)
            {
                LocalizeGridControlRecursively((GridControl)control);
            }
            else if (control is BarDockControl)
            {
                LocalizeControlsRecursively(((BarDockControl)control).Controls);
            }
            else if (control is DataGridView)
            {
                LocalizeDataGridControl((DataGridView)control);
            }
            else if (control is DevExpress.XtraGrid.Controls.FindControl)
            {
                LocalizeFindControlRecursively((DevExpress.XtraGrid.Controls.FindControl)control);
            }
            //Gantt controls should be localized manually from the parent. Otherwise if gantt is referenced here it must be included in many project unnecessarily.
            //else if (control is Gantt)
            //{
            //    LocalizeControlsRecursively(((Gantt)control).Controls);
            //}
            else if (control is TreeList)
            {
                LocalizeTreeList((TreeList)control);
            }
            else if (control is FindControl)
            {
                LocalizeFindControlRecursively((FindControl)control);
            }
            else if (control is MRUEdit mruEdit)
            {
                LocalizeFindControlRecursively(mruEdit);
            }
            else if (control is TextBoxMaskBox maskBox)
            {
                maskBox.EditText = maskBox.EditText.Localize();
            }
            else if (control is ContainerControl)
            {
                LocalizeControlsRecursively(((ContainerControl)control).Controls);
            }
            else if (control is ChartControl)
            {
                LocalizeChartControl((ChartControl)control);
            }
            else
            {
                //Controls to ignore because there is nothing to localize.
                #if DEBUG
                if (control is TextBox) { }
                else if (control is NumericUpDown) { }
                else if (control is Splitter) { }
                else if (control is RichTextBox) { }
                else if (control is BarDockControl) { }
                else if (control.GetType().Name == "PlantsGantt") { }
                else if (control.GetType().Name == "BaseChartControl") { }
                else if (control is OfficeNavigationBar) { }
                else if (control.GetType().Name == "TabbedBrowser") { }
                else if (control.GetType().Name == "ScenarioKpiChart") { }
                else if (control is PictureBox) { }
                else if (control is DateTimePicker) { }
                else if (control.GetType().Name == "IGWinTooltip") { }
                #endif
                //Control not found. Possibly should be added here.
            }

            //else if (control is ListBox)
            //    LocalizeListBox(prefix,(ListBox)control);
        }
    }

    private static void LocalizeCustomHeaderButtons(BaseButtonCollection a_customHeaderButtons)
    {
        foreach (IBaseButton customHeaderButton in a_customHeaderButtons)
        {
            customHeaderButton.Properties.Caption = customHeaderButton.Properties.Caption.Localize();
            LocalizeSuperTip(customHeaderButton.Properties.SuperTip);
        }
    }

    private static void LocalizeTreeList(TreeList a_treeList)
    {
        a_treeList.Caption = a_treeList.Caption.Localize();

        // Cannot use the iterator due to some timing or access (or maybe something else?) issues with ColumnChooserDialog.cs
        for (int i = 0; i < a_treeList.Nodes.Count; i++)
        {
            TreeListNode node = a_treeList.Nodes[i];
            if (node[0] is string rootNodeCaption)
            {
                node[0] = rootNodeCaption.Localize();
            }

            LocalizeTreeListNode(node);
        }

        foreach (TreeListColumn col in a_treeList.Columns)
        {
            col.Caption = col.Caption.Localize();
        }
    }

    private static void LocalizeTreeListNode(TreeListNode a_node)
    {
        for (int i = 0; i < a_node.Nodes.Count; i++)
        {
            TreeListNode node = a_node.Nodes[i];
            if (node[0] is string nodeCaption)
            {
                node[0] = nodeCaption.Localize();
            }

            LocalizeTreeListNode(node);
        }
    }

    public static void LocalizeBar(Bar a_control)
    {
        a_control.BarName = a_control.BarName.Localize();
    }

    private static void LocalizeChartControl(ChartControl control)
    {
        control.Legend.Title.Text = control.Legend.Title.Text.Localize();
        foreach (CustomLegendItem legendCustomItem in control.Legend.CustomItems)
        {
            legendCustomItem.Text = legendCustomItem.Text.Localize();
        }

        foreach (Legend controlLegend in control.Legends)
        {
            controlLegend.Title.Text = controlLegend.Title.Text.Localize();
        }

        foreach (Series controlSeries in control.Series)
        {
            controlSeries.Name = controlSeries.Name.Localize();
        }

        foreach (ChartTitle controlTitle in control.Titles)
        {
            controlTitle.Text = controlTitle.Text.Localize();
        }

        if (control.Diagram is XYDiagram xyDiagram)
        {
            xyDiagram.AxisX.Title.Text = xyDiagram.AxisX.Title.Text.Localize();
            xyDiagram.AxisY.Title.Text = xyDiagram.AxisY.Title.Text.Localize();
            foreach (SecondaryAxisX x in xyDiagram.SecondaryAxesX)
            {
                x.Title.Text = x.Title.Text.Localize();
            }

            foreach (SecondaryAxisY y in xyDiagram.SecondaryAxesY)
            {
                y.Title.Text = y.Title.Text.Localize();
            }

            foreach (XYDiagramPane xyDiagramPane in xyDiagram.Panes)
            {
                xyDiagramPane.Title.Text = xyDiagramPane.Title.Text.Localize();
            }
        }
    }

    private static void LocalizeDataGridView(DataGridView a_dataGridView)
    {
        foreach (DataGridViewColumn col in a_dataGridView.Columns)
        {
            col.HeaderText = col.HeaderText.Localize();
        }
    }

    public static void LocalizeButtonCollection(EditorButtonCollection collection)
    {
        foreach (EditorButton button in collection)
        {
            if (button != null)
            {
                button.Caption = button.Caption.Localize();
                LocalizeSuperTip(button.SuperTip);
            }
        }
    }

    private static void LocalizeBarDockControl(BarDockControl a_barDockControl)
    {
        foreach (DockedBarControl dockedBarControl in a_barDockControl.Controls)
        {
            Bar bar = dockedBarControl.Bar;
            BarManager manager = bar.Manager;
            foreach (BarItem item in manager.Items)
            {
                if (item is BarCheckItem)
                {
                    item.Caption = item.Caption.Localize();
                }

                if (item is BarSubItem)
                {
                    item.Caption = item.Caption.Localize();
                }
            }
        }
    }

    private static void LocalizeLayoutControl(LayoutGroupItemCollection a_rootItems)
    {
        foreach (BaseLayoutItem baseItem in a_rootItems)
        {
            if (baseItem is LayoutControlItem)
            {
                LayoutControlItem item = baseItem as LayoutControlItem;
                LocalizeLayoutControlItem(item);
            }

            if (baseItem is LayoutControlGroup)
            {
                LayoutControlGroup group = baseItem as LayoutControlGroup;
                LocalizeLayoutControlGroup(group);
            }

            if (baseItem is TabbedControlGroup)
            {
                TabbedControlGroup tab = baseItem as TabbedControlGroup;
                LocalizeTabbedControlGroup(tab);
            }
        }
    }

    public static void LocalizeLayoutControlGroup(LayoutControlGroup a_group)
    {
        a_group.Text = a_group.Text.Localize();
        LocalizeLayoutControl(a_group.Items);
    }

    private static void LocalizeTabbedControlGroup(TabbedControlGroup a_tab)
    {
        a_tab.Text = a_tab.Text.Localize();
    }

    private static void LocalizeLayoutControlItem(LayoutControlItem a_item)
    {
        a_item.Text = a_item.Text.Localize();
    }

    public static void LocalizeSimpleButton(SimpleButton simpleButton)
    {
        simpleButton.Text = Localizer.GetString(simpleButton.Text);
    }

    private static void LocalizeFindControlRecursively(object a_findControl)
    {
        if (a_findControl != null)
        {
            switch (a_findControl.GetType().FullName)
            {
                case "DevExpress.XtraTreeList.FindControl":
                    ((FindControl)a_findControl).ClearButton.Text = ((FindControl)a_findControl).ClearButton.Text.Localize();
                    ((FindControl)a_findControl).CloseButton.Text = ((FindControl)a_findControl).CloseButton.Text.Localize();
                    ((FindControl)a_findControl).FindButton.Text = ((FindControl)a_findControl).FindButton.Text.Localize();
                    LocalizeControlsRecursively(((FindControl)a_findControl).Controls);
                    LocalizeControlsRecursively(((FindControl)a_findControl).FindEdit.Controls);
                    break;
                case "DevExpress.XtraGrid.Controls.FindControl":
                    ((DevExpress.XtraGrid.Controls.FindControl)a_findControl).ClearButton.Text = ((DevExpress.XtraGrid.Controls.FindControl)a_findControl).ClearButton.Text.Localize();
                    ((DevExpress.XtraGrid.Controls.FindControl)a_findControl).CloseButton.Text = ((DevExpress.XtraGrid.Controls.FindControl)a_findControl).CloseButton.Text.Localize();
                    ((DevExpress.XtraGrid.Controls.FindControl)a_findControl).FindButton.Text = ((DevExpress.XtraGrid.Controls.FindControl)a_findControl).FindButton.Text.Localize();
                    LocalizeControlsRecursively(((DevExpress.XtraGrid.Controls.FindControl)a_findControl).Controls);
                    break;
            }
        }
    }

    private static void LocalizeDataGridControl(DataGridView a_dataGrid)
    {
        foreach (DataGridViewColumn column in a_dataGrid.Columns)
        {
            column.HeaderText = column.Name.Localize();
        }
    }

    public static void LocalizeGridControlRecursively(GridControl a_gridControl)
    {
        foreach (BaseView baseView in a_gridControl.Views)
        {
            if (baseView is ColumnView cv)
            {
                foreach (DevExpress.XtraGrid.Columns.GridColumn column in cv.Columns)
                {
                    //Unbound grids are localized via a module
                    //Other columns Caption isn't set, so devexpress will display FieldName
                    //If it hasn't been localized, set the caption based on the FieldName
                    if (string.IsNullOrEmpty(column.Caption))
                    {
                        column.Caption = column.FieldName.Localize();
                    }
                }
            }
        }

        LocalizeControlsRecursively(a_gridControl.Controls);
    }

    private static void LocalizeBaseControl(BaseControl a_control)
    {
        LocalizeSuperTip(a_control.SuperTip);
        LocalizeToolTip(a_control);

        //Some controls' value should not be changed
        if (a_control is SpinEdit || a_control is ColorPickEdit)
        {
            return;
        }

        ControlBase controlBase = a_control;
        string localizedText = controlBase.Text.Localize();
        if (controlBase.Text != localizedText)
        {
            //Check for difference to avoid unnecessary change events
            controlBase.Text = localizedText;
        }
    }

    private static void LocalizeTreeNode(TreeNode a_node)
    {
        a_node.Text = a_node.Text.Localize();
        a_node.ToolTipText = a_node.ToolTipText.Localize();
        foreach (TreeNode node in a_node.Nodes)
        {
            LocalizeTreeNode(node);
        }
    }

    public static void LocalizeAccordionControl(AccordionControl a_control)
    {
        a_control.Text = a_control.Text.Localize();
        foreach (AccordionControlElement element in a_control.Elements)
        {
            LocalizeAccordionElement(element);
        }
    }

    private static void LocalizeAccordionElement(AccordionControlElement a_element)
    {
        a_element.Text = a_element.Text.Localize();
        LocalizeSuperTip(a_element.SuperTip);
        if (a_element.Style == ElementStyle.Group)
        {
            foreach (AccordionControlElement element in a_element.Elements)
            {
                LocalizeAccordionElement(element);
            }
        }
        else if (a_element.Style == ElementStyle.Item)
        {
            if (a_element.ContentContainer != null)
            {
                LocalizeControlsRecursively(a_element.ContentContainer.Controls);
            }
        }
    }

    public static void LocalizeControlToolTip(Control a_control, ToolTip a_tooltip)
    {
        string toolTipStr = a_tooltip.GetToolTip(a_control);
        if (!string.IsNullOrEmpty(toolTipStr))
        {
            //string name = String.Format("{0}_{1}_Tooltip", a_prefix, a_control.Name);
            a_tooltip.SetToolTip(a_control, Localizer.GetString(toolTipStr));
        }
    }

    public static void LocalizeButton(Button button)
    {
        //string name = String.Format("{0}_{1}", prefix, button.Name);
        button.Text = Localizer.GetString(button.Text);
    }

    public static void LocalizeButton(SimpleButton button)
    {
        button.Text = Localizer.GetString(button.Text);
    }

    public static void LocalizeButton(DropDownButton button)
    {
        button.Text = Localizer.GetString(button.Text);
    }

    public static void LocalizeFormCaption(Form form)
    {
        //string name = String.Format("{0}_Caption", form.GetType().Name);
        form.Text = Localizer.GetString(form.Text);
    }

    public static void LocalizeFormIncludingCaption(Form a_form)
    {
        LocalizeFormCaption(a_form);
        LocalizeControlsRecursively(a_form.Controls);
    }

    public static void LocalizeFormIncludingCaption(Form a_form, params Control[] dontLocalizeThese)
    {
        LocalizeFormCaption(a_form);
        LocalizeControlsRecursively(a_form.Controls, dontLocalizeThese);
    }

    public static void LocalizeUserControl(UserControl a_control)
    {
        LocalizeControlsRecursively(a_control.Controls);
    }

    public static void LocalizeUserControl(UserControl a_control, params Control[] dontLocalizeThese)
    {
        LocalizeControlsRecursively(a_control.Controls, dontLocalizeThese);
    }

    public static void LocalizeLabel(Label label)
    {
        //string name = String.Format("{0}_{1}", prefix, label.Name);
        label.Text = Localizer.GetString(label.Text);
    }

    public static void LocalizeTextBox(TextBox textBox)
    {
        //string name = String.Format("{0}_{1}", prefix, textBox.Name);
        textBox.Text = Localizer.GetString(textBox.Text);
    }

    public static void LocalizeRadioButton(RadioButton radioButton)
    {
        //string name = String.Format("{0}_{1}", prefix, radioButton.Name);
        radioButton.Text = Localizer.GetString(radioButton.Text);
    }

    public static void LocalizeCheckBox(CheckBox checkBox)
    {
        //string name = String.Format("{0}_{1}", prefix, checkBox.Name);
        checkBox.Text = Localizer.GetString(checkBox.Text);
    }

    public static void LocalizeGroupBox(GroupBox groupBox, HashSet<Control> dontLocalizeThese)
    {
        //string name = String.Format("{0}_{1}", prefix, groupBox.Name);
        groupBox.Text = Localizer.GetString(groupBox.Text);

        LocalizeControlsRecursively(groupBox.Controls, dontLocalizeThese);
    }

    public static void LocalizeListBox(ListBox listBox)
    {
        if (listBox.Items.Count > 0 && listBox.Items[0] is string)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                string itemStr = listBox.Items[i].ToString();
                listBox.Items[i] = Localizer.GetString(itemStr);
            }
        }
    }

    public static void LocalizeComboBox(ComboBox comboBox)
    {
        if (comboBox.Items.Count > 0 && comboBox.Items[0] is string)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                string itemStr = comboBox.Items[i].ToString();
                comboBox.Items[i] = Localizer.GetString(itemStr);
            }
        }
    }

    public static void LocalizeComboBoxEdit(ComboBoxEdit a_comboBoxEdit)
    {
        a_comboBoxEdit.Text = a_comboBoxEdit.Text.Localize();
        LocalizeButtonCollection(a_comboBoxEdit.Properties.Buttons);
        if (a_comboBoxEdit.Properties.Items.Count > 0)
        {
            if (a_comboBoxEdit.Properties.Items[0] is string || a_comboBoxEdit.Properties.Items[0] is Enum)
            {
                a_comboBoxEdit.Text = a_comboBoxEdit.Text.Localize();
                for (int i = 0; i < a_comboBoxEdit.Properties.Items.Count; i++)
                {
                    string itemStr = a_comboBoxEdit.Properties.Items[i].ToString();
                    a_comboBoxEdit.Properties.Items[i] = Localizer.GetString(itemStr);
                }
            }
        }
    }

    public static void LocalizeSplitContainer(SplitContainer splitContainer, HashSet<Control> dontLocalizeThese)
    {
        LocalizeControlsRecursively(splitContainer.Controls, dontLocalizeThese);
        LocalizeControlsRecursively(splitContainer.Panel1.Controls, dontLocalizeThese);
        LocalizeControlsRecursively(splitContainer.Panel2.Controls, dontLocalizeThese);
    }

    public static void LocalizePanel(Panel panel, HashSet<Control> dontLocalizeThese)
    {
        LocalizeControlsRecursively(panel.Controls, dontLocalizeThese);
    }

    public static void LocalizePanel(PanelControl panel, HashSet<Control> dontLocalizeThese)
    {
        panel.Text = panel.Text.Localize();
        LocalizeControlsRecursively(panel.Controls, dontLocalizeThese);
    }

    public static void LocalizeTabControl(TabControl tabControl, HashSet<Control> dontLocalizeThese = null)
    {
        if (dontLocalizeThese == null)
        {
            dontLocalizeThese = new HashSet<Control>();
        }

        for (int tabI = 0; tabI < tabControl.TabPages.Count; tabI++)
        {
            TabPage tabPage = tabControl.TabPages[tabI];
            tabPage.Text = Localizer.GetString(tabPage.Text);

            LocalizeControlsRecursively(tabPage.Controls, dontLocalizeThese);
        }
    }

    public static void LocalizeTabControl(XtraTabControl tabControl, HashSet<Control> dontLocalizeThese = null)
    {
        if (dontLocalizeThese == null)
        {
            dontLocalizeThese = new HashSet<Control>();
        }

        for (int tabI = 0; tabI < tabControl.TabPages.Count; tabI++)
        {
            XtraTabPage tabPage = tabControl.TabPages[tabI];
            tabPage.Text = Localizer.GetString(tabPage.Text);
            LocalizeSuperTip(tabPage.SuperTip);

            LocalizeControlsRecursively(tabPage.Controls, dontLocalizeThese);
        }
    }

    public static void LocalizeDataSet(DataSet a_dataSet)
    {
        foreach (DataTable table in a_dataSet.Tables)
        {
            LocalizeDataTable(table);
        }
    }

    public static void LocalizeDataTable(DataTable a_dataTable)
    {
        foreach (DataColumn column in a_dataTable.Columns)
        {
            column.Caption = column.ColumnName.Localize();
        }
    }

    public static readonly string AttributeSuffixCode = "_ptattcode";
    public static readonly string AttributeSuffixNbr = "_ptattnbr";
    public static readonly string AttributeSuffixColor = "_ptattclr";
    public static readonly string UDFSuffix = "_ptudf";

    public const string DO_NOT_LOCALIZE_LIST_TAG = "Do not localize these values lists.  They contain data not localizable text.";
}