using System.Drawing;

using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class Campaign
{
    private const string c_alternateCampaignLabel = "AlternateUOM";

    public Campaign(ResourceBlock a_block)
    {
        description = a_block.Batch.FirstActivity.Operation.ManufacturingOrder.ProductName;
        start = a_block.StartDateTime;
        Add(a_block);
        Item item = a_block.Batch.FirstActivity.Operation.ManufacturingOrder.Product;
        if (item != null)
        {
            ItemGroup = item.ItemGroup;
            AddAlternateCampaignLabel(item);
        }
    }

    /// <summary>
    /// Given an item, it determines whether an alternate quantity/uom should be
    /// displayed on Campaign label or not. This was added for LambWeston in v11.
    /// There will be more flexible and better designed solution in v12.
    /// </summary>
    /// <param name="a_item"></param>
    private void AddAlternateCampaignLabel(Item a_item)
    {
        //UDF TODO: Restore
        //if (a_item?.UserFields == null)
        //{
        //    return;
        //}

        //UserField udf = null;
        //for (int i = 0; i < a_item.UserFields.Count; i++)
        //{
        //    UserField tmpUdf = a_item.UserFields[i];
        //    if (tmpUdf.Name.StartsWith(c_alternateCampaignLabel))
        //    {
        //        udf = tmpUdf;
        //        break;
        //    }
        //}

        //if (udf == null || udf.UDFDataType != UserField.UDFTypes.Double)
        //{
        //    return;
        //}

        //string[] nameParts = udf.Name.Split('_'); // UDF should be named like 'AlternateCampaignLabel_cases'
        //if (nameParts.Length != 2)
        //{
        //    return;
        //}

        //AlternateLabel = nameParts[1];
        //AlternateLabelMultiplier = (double)udf.DataValue;
    }

    private string m_itemGroup;

    public string ItemGroup
    {
        get => m_itemGroup;
        private set => m_itemGroup = value;
    }

    private string description;

    public string Description
    {
        get => description;
        set => description = value;
    }

    private DateTime start;

    public DateTime Start
    {
        get => start;
        set => start = value;
    }

    private DateTime end;

    public DateTime End
    {
        get => end;
        set => end = value;
    }

    private decimal totalRequiredQty;

    public decimal TotalRequiredQty
    {
        get => totalRequiredQty;
        set => totalRequiredQty = value;
    }

    /// <summary>
    /// If not null or empty, a second line will be displayed on campaign labels with quantity followed by this label.
    /// </summary>
    public string AlternateLabel { get; private set; }

    /// <summary>
    /// If displaying alternate label, this value will be multiplied by campaign quantity to arrive at the alternate label quantity.
    /// </summary>
    public double AlternateLabelMultiplier { get; private set; }

    private string uom;

    public string UOM
    {
        get => uom;
        set => uom = value;
    }

    private Color fillColor = Color.CornflowerBlue;

    public Color FillColor
    {
        get => fillColor;
        set => fillColor = value;
    }

    internal void Add(ResourceBlock a_block)
    {
        End = a_block.EndDateTime;
        TotalRequiredQty += a_block.Batch.FirstActivity.RequiredFinishQty;
    }
}