using System.Collections.Concurrent;
using System.IO.Hashing;

using Microsoft.JSInterop;

using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;

namespace ReportsWebApp.Data;

public class GanttNoteInteropContext : IDisposable
{
    private ConcurrentDictionary<long, GanttNoteSaveObj> m_noteObjs = new ();
    private List<GanttNote> m_notes = new List<GanttNote>();
    private List<Event> m_events = new List<Event>();
    private string m_scenarioName;
    private IGanttDataService m_ganttDataService;
    private EventBusListener m_listener;
    private int m_companyId;
    public GanttNoteInteropContext(int companyId, string a_scenarioName, IGanttDataService a_ganttDataService, List<Event> a_events)
    {
        m_scenarioName = a_scenarioName;
        m_ganttDataService = a_ganttDataService;
        m_events = a_events;
        m_companyId = companyId;
        
        m_listener = new EventBusListener((ev) =>
        {
            if (ev is GanttNoteUpdatedEvent updated)
            {
                if (m_notes.Replace(updated.Note, (old, @new) => old.Id == @new.Id) == false)
                {
                    m_notes.Add(updated.Note);
                }
                AssociateNotesWithEvents();
            }
        }, typeof(GanttNoteUpdatedEvent));
    }

    public async Task Init()
    {
        m_notes = await m_ganttDataService.GetNotesForScenarioAsync(m_companyId, m_scenarioName);
        AssociateNotesWithEvents();
    }
    
    private void AssociateNotesWithEvents()
    {
        Dictionary<ulong, Event> lookup = new ();
        try
        {
            foreach (Event ev in m_events)
            {
                lookup.Add(GanttNote.GetHashForEvent(ev), ev);
            }
        }
        catch (Exception ex)
        {
            throw;
        }

        foreach (GanttNote note in m_notes)
        {
            if (lookup.ContainsKey(note.BlockHash))
            {
                m_noteObjs.AddOrUpdate(lookup[note.BlockHash].Id, new GanttNoteSaveObj()
                {
                    id = lookup[note.BlockHash].Id,
                    text = note.Text,
                }, (long a_s, GanttNoteSaveObj a_obj) => new GanttNoteSaveObj()
                {
                    id = lookup[note.BlockHash].Id,
                    text = note.Text,
                });
            }
        }
    }
    
    [JSInvokable]
    public string GetNoteForEvent(long eventId)
    {
        var note = m_noteObjs.FirstOrDefault(x => x.Key == eventId);
        if (note.Value == null)
        {
            return string.Empty;
        }
        return note.Value.text;
    }

    [JSInvokable]
    public void SaveNote(GanttNoteSaveObj obj)
    { 
        Event? ev = m_events.FirstOrDefault(e => e.Id == obj.id);
        if (ev != null)
        {
            ulong hash = GanttNote.GetHashForEvent(ev);
            GanttNote? note = m_notes.FirstOrDefault(n => n.BlockHash == hash);
            if (note != null)
            {
                note.Text = obj.text;
                m_ganttDataService.SaveNote(note);
            }
            else
            {
                GanttNote newNote = new GanttNote();
                newNote.Id = 0;
                newNote.Text = obj.text;
                newNote.BlockHash = hash;
                newNote.CompanyId = m_companyId;
                newNote.NewScenarioId = m_scenarioName;
                m_ganttDataService.SaveNote(newNote);
            }
        }
    }

    public void Dispose()
    {
        m_listener.Dispose();
    }
}

public class GanttNoteSaveObj
{
    public long id { get; set; }
    public string text { get; set; }
}