using System.Text;

using PT.APSCommon;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.APIs;

internal class ImportExportActionBase : IDisposable
{
    internal ImportExportActionBase()
    {
        m_scenarioId = BaseId.NULL_ID;
        m_dataLock = new object();
    }

    protected BaseId m_scenarioId;
    protected readonly object m_dataLock;
    protected bool m_started;
    protected bool m_finished;
    protected ApplicationExceptionList m_exceptions;

    protected virtual void DeInitializeListeners() { }

    public void Dispose()
    {
        DeInitializeListeners();
    }

    internal bool Finished
    {
        get
        {
            lock (m_dataLock)
            {
                return m_finished;
            }
        }
    }

    internal string Errors
    {
        get
        {
            lock (m_dataLock)
            {
                if (m_exceptions == null)
                {
                    return string.Empty;
                }

                StringBuilder s = new ();
                ApplicationExceptionList.Node node = m_exceptions.First;
                while (node != null)
                {
                    s.AppendLine(node.Data.Message);
                    node = node.Next;
                }

                return s.ToString();
            }
        }
    }

    internal async Task AwaitResult(TimeSpan a_timeOut)
    {
        await Task.Run(async () =>
        {
            TimeSpan waitDuration = TimeSpan.Zero;
            const int c_sleepMs = 500;
            while (!m_finished)
            {
                await Task.Delay(c_sleepMs);
                waitDuration = waitDuration.Add(TimeSpan.FromMilliseconds(c_sleepMs));
                lock (m_dataLock)
                {
                    if (!m_started)
                    {
                        if (waitDuration > a_timeOut)
                        {
                            return;
                        }
                    }

                    if (!m_finished && waitDuration > a_timeOut)
                    {
                        return;
                    }
                }
            }
        });
    }
}