using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISGuard.Services
{
    public static class AppPoolChecker
    {
        public static void CheckAppPools()
        {
            Console.WriteLine("[AppPoolChecker] Checking for bad recycle settings and timeouts...");
            // Placeholder: Use Microsoft.Web.Administration for real checks
            ServerManager serverManager = new ServerManager();
            ApplicationPoolCollection applicationPools = serverManager.ApplicationPools;

            foreach (ApplicationPool appPool in applicationPools)
            {
                Console.WriteLine($"Checking Application Pool: {appPool.Name}");

                // Check recycling settings
                ApplicationPoolRecycling recyclingSettings = appPool.Recycling;
                if (recyclingSettings.PeriodicRestart.Time.TotalMinutes > 0)
                {
                    Console.WriteLine($"  Periodic Recycling Time: {recyclingSettings.PeriodicRestart.Time}");
                }
                // You can add checks for other recycling events here

                // Check timeout settings
                var processModel = appPool.ProcessModel;
                if (processModel.IdleTimeout.TotalMinutes > 0)
                {
                    Console.WriteLine($"  Idle Timeout: {processModel.IdleTimeout}");
                }
                // You can add checks for other timeout settings here
            }
        }
    }
}