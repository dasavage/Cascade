using Cascade.Core.Logging;
using System.Timers;

namespace Cascade.Core.Cascade
{
    internal class CascadeUpdater
    {
        // ahhhh System.Timers.Timer...

        private readonly LogManager _logManager;

        public CascadeUpdater()
        {
            _logManager = global::Cascade.Cascade.GetLogManager();

            var configManager = global::Cascade.Cascade.GetConfigManager();

            if (!configManager.ConfigElementExists("cascade.check_for_updates") || configManager.GetConfigElement("cascade.check_for_updates") != "yes")
            {
                return;
            }

            var myTimer = new Timer();
            myTimer.Elapsed += CheckForUpdate;
            myTimer.Interval = 10 * 60000;

            if (configManager.ConfigElementExists("cascade.check_for_updates_every_x_minutes"))
            {
                myTimer.Interval = int.Parse(configManager.GetConfigElement("cascade.check_for_updates_every_x_minutes")) * 60000;
            }

            myTimer.Enabled = true;
        }


        private void CheckForUpdate(object source, ElapsedEventArgs e)
        {
            var updateString = Utility.UpdateAvalible();

            if (!string.IsNullOrEmpty(updateString) && Utility.ValidVersionString(updateString))
            {
                _logManager.Log("Update Avalible: v" + updateString + " is now avalible.", LogType.Warning);
            }
        }
    }
}
