using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstRevitTool.ChatRevit.Interfaces
{
    public interface IExternalEventService : IExternalEventHandler
    {
        Task<ExternalEventRequest> Raise(Action<UIApplication> action);
    }
}
