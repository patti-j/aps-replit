using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Cell objects.
/// </summary>
[Serializable]
public class CellManager : ScenarioBaseObjectManager<Cell>, IPTSerializable
{
    #region IPTSerializable Members
    public CellManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Cell cell = new (a_reader);
                Add(cell);
            }
        }
    }

    public new const int UNIQUE_ID = 12;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    internal class CellManagerException : PTException
    {
        public CellManagerException(string message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    internal CellManager(BaseIdGenerator a_idGen)
        : base(a_idGen) { }
    #endregion

    #region Cell Edit Functions
    private Cell AddDefault(CellDefaultT t)
    {
        Cell c = new (NextID());
        ValidateAdd(c);
        return Add(c);
    }

    private Cell AddCopy(CellCopyT t)
    {
        ValidateCopy(t);
        Cell c = GetById(t.originalId);
        return AddCopy(c, c.Clone(), NextID());
    }

    private Cell Delete(CellDeleteT t)
    {
        ValidateDelete(t);
        Cell c = GetById(t.cellId);
        Delete(c);
        return c;
    }

    private Cell Delete(BaseId a_cellId)
    {
        ValidateExistence(a_cellId);
        Cell c = GetById(a_cellId);
        Delete(c);
        return c;
    }

    private void Delete(Cell c)
    {
        c.Deleting();
        Remove(c.Id); //Now remove it from the Manager.
    }

    /// <summary>
    /// Deletes all Cells.
    /// </summary>
    internal void Clear(IScenarioDataChanges a_dataChanges)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            a_dataChanges.CellChanges.DeletedObject(this[i].Id);
            Delete(this[i]);
        }
    }

    public new Cell this[int index] => GetByIndex(index);
    #endregion

    #region Transmissions
    private void ValidateAdd(Cell c)
    {
        if (Contains(c.Id))
        {
            throw new CellManagerException("2749", new object[] { c.Id.ToString() });
        }
    }

    private void ValidateCopy(CellCopyT t)
    {
        ValidateExistence(t.originalId);
    }

    private void ValidateDelete(CellDeleteT t)
    {
        ValidateExistence(t.cellId);
    }

    public void Receive(CellBaseT t, IScenarioDataChanges a_dataChanges)
    {
        Cell c;
        ScenarioEvents se;
        if (t is CellDefaultT)
        {
            c = AddDefault((CellDefaultT)t);
            a_dataChanges.CellChanges.AddedObject(c.Id);
        }
        else if (t is CellCopyT)
        {
            c = AddCopy((CellCopyT)t);
            a_dataChanges.CellChanges.AddedObject(c.Id);
        }
        else if (t is CellDeleteT cellDeleteT)
        {
            if (cellDeleteT.IsMultiDelete)
            {
                foreach (BaseId cellId in cellDeleteT.CellsToDeleteIds)
                {
                    Delete(cellId);
                    a_dataChanges.CellChanges.DeletedObject(cellId);
                }
            }
            else
            {
                c = Delete(cellDeleteT);
                a_dataChanges.CellChanges.DeletedObject(c.Id);
            }
        }
        else if (t is CellDeleteAllT)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                c = Delete(GetByIndex(i).Id);
                a_dataChanges.CellChanges.DeletedObject(c.Id);
            }
        }
    }
    #endregion

    #region ERP Transmissions
    public void Receive(UserFieldDefinitionManager a_udfManager, CellT a_t, IScenarioDataChanges a_dataChanges)
    {
        HashSet<BaseId> affectedCells = new ();

        for (int i = 0; i < a_t.Count; ++i)
        {
            CellT.Cell cellNode = a_t[i];

            Cell cell;
            if (cellNode.IdSet)
            {
                cell = GetById(cellNode.Id);
                if (cell == null)
                {
                    throw new ValidationException("2275", new object[] { cellNode.Id });
                }
            }
            else
            {
                cell = GetByExternalId(cellNode.ExternalId);
            }

            if (cell == null)
            {
                cell = new Cell(NextID(), cellNode, a_udfManager);
                Add(cell);
                a_dataChanges.CellChanges.AddedObject(cell.Id);
            }
            else
            {
                cell.Update(a_udfManager, cellNode, a_t);
                a_dataChanges.CellChanges.UpdatedObject(cell.Id);
            }


            affectedCells.Add(cell.Id);
        }

        if (a_t.AutoDeleteMode)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                BaseId id = GetByIndex(i).Id;
                if (!affectedCells.Contains(id))
                {
                    Remove(id);
                    a_dataChanges.CellChanges.DeletedObject(id);
                }
            }
        }
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(Cell);
    #endregion

    #region Restore References
    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Cell cell in this)
        {
            a_udfManager.RestoreReferences(cell, UserField.EUDFObjectType.Cells);
        }
    }
    #endregion
}