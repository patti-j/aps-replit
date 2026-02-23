using Inventory = PT.APIDefinitions.RequestsAndResponses.DataDtos.Inventory;
using Item = PT.APIDefinitions.RequestsAndResponses.DataDtos.Item;
using Warehouse = PT.APIDefinitions.RequestsAndResponses.DataDtos.Warehouse;

namespace PT.PlanetTogetherAPI;

// This class depends on the PT.Scheduler project and so couldn't be in APIDefinitions
// TODO: Add fields as needed
internal static class DtoMapper
{
    internal static Inventory ToInventoryDto(Scheduler.Inventory a_entity, List<Scheduler.Warehouse> a_warehousesWithItem, IncludedQueryFields a_fieldsToInclude)
    {
        Inventory dto = new ();

        IncludedQueryFields includedFieldsForItem = a_fieldsToInclude.GetIncludedFieldsForNestedEntity(nameof(dto.Item));
        dto.Item = ToItemDto(a_entity.Item, includedFieldsForItem);

        // Include optional fields
        if (a_fieldsToInclude.ShouldLoad(nameof(dto.Id)))
        {
            dto.Id = a_entity.Id.Value;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.OnHandQty)))
        {
            dto.OnHandQty = a_entity.OnHandQty;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.Warehouses), true))
        {
            dto.Warehouses = new List<Warehouse>();
            IncludedQueryFields includedFieldsForWarehouses = a_fieldsToInclude.GetIncludedFieldsForNestedEntity(nameof(dto.Warehouses));

            foreach (Scheduler.Warehouse warehouse in a_warehousesWithItem)
            {
                dto.Warehouses.Add(ToWarehouseDto(warehouse, includedFieldsForWarehouses));
            }
        }

        return dto;
    }

    internal static Item ToItemDto(Scheduler.Item a_entity, IncludedQueryFields a_fieldsToInclude)
    {
        Item dto = new ()
        {
            // Dto-Required Values
            ExternalId = a_entity.ExternalId,
            Name = a_entity.Name,
            Description = a_entity.Description
        };

        // Optional Fields
        if (a_fieldsToInclude.ShouldLoad(nameof(dto.Id)))
        {
            dto.Id = a_entity.Id.Value;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.UnitVolume)))
        {
            dto.UnitVolume = a_entity.UnitVolume;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.ItemType)))
        {
            dto.ItemType = a_entity.ItemType.ToString();
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.Source)))
        {
            dto.Source = a_entity.Source.ToString();
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.MinOrderQty)))
        {
            dto.MinOrderQty = a_entity.MinOrderQty;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.MaxOrderQty)))
        {
            dto.MaxOrderQty = a_entity.MaxOrderQty;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.DefaultLeadTime)))
        {
            dto.DefaultLeadTime = a_entity.DefaultLeadTime;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.BatchSize)))
        {
            dto.BatchSize = a_entity.BatchSize;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.TransferQty)))
        {
            dto.TransferQty = a_entity.TransferQty;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.ShelfLife)))
        {
            dto.ShelfLife = a_entity.ShelfLife;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.Cost)))
        {
            dto.Cost = a_entity.Cost;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.PlanInventory)))
        {
            dto.PlanInventory = a_entity.PlanInventory;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.RollupAttributesToParent)))
        {
            dto.RollupAttributesToParent = a_entity.RollupAttributesToParent;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.ItemGroup)))
        {
            dto.ItemGroup = a_entity.ItemGroup;
        }

        if (a_fieldsToInclude.ShouldLoad(nameof(dto.Notes)))
        {
            dto.Notes = a_entity.Notes;
        }

        return dto;
    }

    internal static Warehouse ToWarehouseDto(Scheduler.Warehouse a_entity, IncludedQueryFields a_fieldsToInclude)
    {
        Warehouse dto = new ()
        {
            // Dto-Required Values
            ExternalId = a_entity.ExternalId,
            Name = a_entity.Name,
            Description = a_entity.Description
        };

        // Optional Fields
        if (a_fieldsToInclude.ShouldLoad(nameof(dto.Id)))
        {
            dto.Id = a_entity.Id.Value;
        }

        return dto;
    }
}