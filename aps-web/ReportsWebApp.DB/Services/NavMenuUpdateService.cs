using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Services
{
    public class NavMenuUpdateService
    {
        public event Action<bool>? OnNavMenuUpdate;

        public void TriggerUpdate(bool refreshUser = true)
        {
            OnNavMenuUpdate?.Invoke(refreshUser);
        }
    }

}
