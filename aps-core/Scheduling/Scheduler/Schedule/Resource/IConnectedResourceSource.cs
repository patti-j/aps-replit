using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.APSCommon;

namespace PT.Scheduler.Schedule.Resource;

internal interface IConnectedResourceSource
{
    BaseId ResourceId { get; }
}