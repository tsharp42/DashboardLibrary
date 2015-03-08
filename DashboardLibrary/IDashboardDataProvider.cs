using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardLibrary
{
    public delegate void DashUpdateEventHandler(DashboardData Data);

    public interface IDashboardDataProvider
    {
        event DashUpdateEventHandler OnDashUpdate;
        bool Start();
        bool Stop();
    }
}
