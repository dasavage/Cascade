using Cascade.Core.Cascade.User;
using Cascade.Core.Database;
using Cascade.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Cascade.Core.Cascade
{
    internal class CascadeProcess
    {
        private AutoResetEvent _resetEvent;
        private int _timerInterval;
        private LogManager _logManager;
        private bool _timerEnabled;
        private Timer _timer;
        private bool _timerLagging;
        private bool _timerProcessing;

        private int lastUserRegistrationId;
        private int lastBanRecordId;
        private Dictionary<int, UserInformation> _playersCached;

        public void SetupProcess()
        {
            _resetEvent = new AutoResetEvent(true);
            _timerInterval = 1;
            _logManager = global::Cascade.Cascade.GetLogManager();
            _timerEnabled = true;

            _playersCached = new Dictionary<int, UserInformation>();

            var configManager = global::Cascade.Cascade.GetConfigManager();

            if (configManager.ConfigElementExists("cascade.timer.interval"))
            {
                _timerInterval = int.Parse(configManager.GetConfigElement("cascade.timer.interval"));
            }

            _timer = new Timer(OnProcess, null, _timerInterval, _timerInterval);

            _timerLagging = false;
            _timerProcessing = false;
        }

        private void OnProcess(object timerObj)
        {
            if (!_timerEnabled)
            {
                return;
            }

            try
            {
                if (_timerProcessing)
                {
                    _logManager.Log("Cascade timer is lagging", LogType.Information);
                    _timerLagging = true;
                }

                _timerProcessing = true;
                _resetEvent.Reset();

                RunChecks();

                UpdateConsoleTitle(); //TODO: Move this somewhere else
            }
            catch (Exception exception)
            {
                var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
                _logManager.Log($"Error in {method}: {exception.Message}", LogType.Error);
                _logManager.Log(exception.StackTrace, LogType.Error);
            }
            finally
            {
                _timerProcessing = false;
                _timerLagging = false;
                _resetEvent.Set();
            }
        }

        private static void UpdateConsoleTitle()
        {
            var configManager = global::Cascade.Cascade.GetConfigManager();

            if (!configManager.ConfigElementExists("cascade.update_title") || configManager.GetConfigElement("cascade.update_title") != "yes")
            {
                return;
            }

            var serverUptime = DateTime.Now - global::Cascade.Cascade.StarteDateTime;

            var days = serverUptime.Days + " day" + (serverUptime.Days != 1 ? "s" : "") + ", ";
            var hours = serverUptime.Hours + " hour" + (serverUptime.Hours != 1 ? "s" : "") + ", and ";
            var minutes = serverUptime.Minutes + " minute" + (serverUptime.Minutes != 1 ? "s" : "");
            var uptimeString = days + hours + minutes;

            Console.Title = "Cascade - Uptime: " + uptimeString;
        }

        private void RunChecks()
        {
            using (var mysqlConnection = global::Cascade.Cascade.GetDatabaseManager().GetConnection())
            {
                mysqlConnection.OpenConnection();
                
                CheckForNewUsers(mysqlConnection);
                CheckForNewBan(mysqlConnection);
                CheckForCacheChanges(mysqlConnection);

                mysqlConnection.CloseConnection();
            }
        }

        private void CheckForNewUsers(DatabaseConnection mysqlConnection)
        {
            var configManager = global::Cascade.Cascade.GetConfigManager();

            if (!configManager.ConfigElementExists("cascade.check_new_registrations") || configManager.GetConfigElement("cascade.check_new_registrations") != "yes")
            {
                return;
            }

            mysqlConnection.SetQuery("SELECT `id`, `username` FROM `users` ORDER BY `id` DESC LIMIT 1");
            var latestRegistrationRow = mysqlConnection.GetRow();

            if (latestRegistrationRow == null)
            {
                return;
            }

            if (Convert.ToInt32(latestRegistrationRow["id"]) == lastUserRegistrationId || Convert.ToInt32(latestRegistrationRow["id"]) < lastUserRegistrationId)
            {
                return;
            }

            if (lastUserRegistrationId == 0)
            {
                lastUserRegistrationId = Convert.ToInt32(latestRegistrationRow["id"]);
                return;
            }

            _logManager.Log("New Registration: " + Convert.ToString(latestRegistrationRow["username"]) + " on IP " + Convert.ToInt32(latestRegistrationRow["id"]) + ", ID " + Convert.ToInt32(latestRegistrationRow["id"]) + ".", LogType.Information);
            lastUserRegistrationId = Convert.ToInt32(latestRegistrationRow["id"]);
        }

        private void CheckForNewBan(DatabaseConnection mysqlConnection)
        {
            var configManager = global::Cascade.Cascade.GetConfigManager();

            if (!configManager.ConfigElementExists("cascade.check_new_bans") || configManager.GetConfigElement("cascade.check_new_bans") != "yes")
            {
                return;
            }

            mysqlConnection.SetQuery("SELECT * FROM `bans` ORDER BY `id` DESC LIMIT 1");
            var latestBanRow = mysqlConnection.GetRow();

            if (latestBanRow == null)
            {
                return;
            }

            if (Convert.ToInt32(latestBanRow["id"]) == lastBanRecordId || Convert.ToInt32(latestBanRow["id"]) < lastBanRecordId)
            {
                return;
            }

            if (lastBanRecordId == 0)
            {
                lastBanRecordId = Convert.ToInt32(latestBanRow["id"]);
                return;
            }

            _logManager.Log("New ban: " + Convert.ToString(latestBanRow["username"]) + " has been " + Convert.ToString(latestBanRow["bantype"]) + " banned.", LogType.Information);

            lastBanRecordId = Convert.ToInt32(latestBanRow["id"]);
        }

        private void CheckForCacheChanges(DatabaseConnection mysqlConnection)
        {
            var configManager = global::Cascade.Cascade.GetConfigManager();

            if (!configManager.ConfigElementExists("cascade.disable_all_cache_checks") || configManager.GetConfigElement("cascade.disable_all_cache_checks") != "no")
            {
                return;
            }

            mysqlConnection.SetQuery("SELECT `id`, `username`, `ip_last`, `auth_ticket`, `last_online`, `look`, `rank`, `credits`, `activity_points`, `vip_points` FROM `users` ORDER BY `id`");
            var allUsersTable = mysqlConnection.GetTable();

            foreach (DataRow userRow in allUsersTable.Rows)
            {
                if (_playersCached.ContainsKey(Convert.ToInt32(userRow["id"])))
                {
                    UserInformation userInformation;

                    if (_playersCached.TryGetValue(Convert.ToInt32(userRow["id"]), out userInformation))
                    {
                        if (userInformation.AuthTicket != Convert.ToString(userRow["auth_ticket"]) && !string.IsNullOrEmpty(Convert.ToString(userRow["auth_ticket"])))
                        {
                            mysqlConnection.SetQuery("SELECT * FROM `bans` WHERE `value` = @value1 OR `value` = @value2 OR `value` = @value3");
                            mysqlConnection.AddParameter("value1", Convert.ToString(userRow["username"]));
                            mysqlConnection.AddParameter("value2", Convert.ToString(userRow["ip_last"]));
                            mysqlConnection.AddParameter("value3", Convert.ToString(userRow["machine_id"]));

                            var banCount = mysqlConnection.GetInteger();

                            if (banCount > 0)
                            {
                                _logManager.Log("Ban Alert: " + Convert.ToString(userRow["username"]) + " is trying to load the client, but is currently banned.", LogType.Information);
                            }
                            else
                            {
                                _logManager.Log("New client entry: " + Convert.ToString(userRow["username"]) + " is loading the client.", LogType.Information);
                            }

                            userInformation.AuthTicket = Convert.ToString(userRow["auth_ticket"]);
                        }

                        if (userInformation.LastOnline != Convert.ToInt32(userRow["last_online"]))
                        {
                            _logManager.Log("User now online: " + Convert.ToString(userRow["username"]) + " has logged in.", LogType.Information);

                            userInformation.LastOnline = Convert.ToInt32(userRow["last_online"]);
                        }

                        if (userInformation.Look != Convert.ToString(userRow["look"]))
                        {
                            _logManager.Log("User changed clothes: " + Convert.ToString(userRow["username"]) + " has changed their clothes.", LogType.Information);
                            userInformation.Look = Convert.ToString(userRow["look"]);
                        }

                        if (userInformation.Rank != Convert.ToInt32(userRow["rank"]))
                        {
                            _logManager.Log("User Rank Change: " + Convert.ToString(userRow["username"]) + " has been " + (Convert.ToInt32(userRow["rank"]) > userInformation.Rank ? "promoted" : "demoted") + " to rank " + Convert.ToInt32(userRow["rank"]) + "!", LogType.Information);
                            userInformation.Rank = Convert.ToInt32(userRow["rank"]);
                        }

                        if (userInformation.Credits != Convert.ToInt32(userRow["credits"]))
                        {
                            if (userInformation.Credits < Convert.ToInt32(userRow["credits"]))
                            {
                                _logManager.Log("User credits change: " + Convert.ToString(userRow["username"]) + " has gained " + (Convert.ToInt32(userRow["credits"]) - userInformation.Credits) + " credits.", LogType.Information);
                            }
                            else
                            {
                                _logManager.Log("User credits change: " + Convert.ToString(userRow["username"]) + " has lost " + (userInformation.Credits - Convert.ToInt32(userRow["credits"])) + " credits.", LogType.Information);
                            }
                            
                            userInformation.Credits = Convert.ToInt32(userRow["credits"]);
                        }

                        if (userInformation.Pixels != Convert.ToInt32(userRow["activity_points"]))
                        {
                            if (userInformation.Pixels < Convert.ToInt32(userRow["activity_points"]))
                            {
                                _logManager.Log("User pixels change: " + Convert.ToString(userRow["username"]) + " has gained " + (Convert.ToInt32(userRow["activity_points"]) - userInformation.Pixels) + " pixels.", LogType.Information);
                            }
                            else
                            {
                                _logManager.Log("User pixels change: " + Convert.ToString(userRow["username"]) + " has lost " + (userInformation.Pixels - Convert.ToInt32(userRow["activity_points"])) + " pixels.", LogType.Information);
                            }

                            userInformation.Pixels = Convert.ToInt32(userRow["pixels"]);
                        }

                        if (userInformation.VipPoints != Convert.ToInt32(userRow["vip_points"]))
                        {
                            if (userInformation.VipPoints < Convert.ToInt32(userRow["vip_points"]))
                            {
                                _logManager.Log("User vip points change: " + Convert.ToString(userRow["username"]) + " has gained " + (Convert.ToInt32(userRow["vip_points"]) - userInformation.VipPoints) + " vip points.", LogType.Information);
                            }
                            else
                            {
                                _logManager.Log("User vip points change: " + Convert.ToString(userRow["username"]) + " has lost " + (userInformation.VipPoints - Convert.ToInt32(userRow["vip_points"])) + " vip points.", LogType.Information);
                            }

                            userInformation.VipPoints = Convert.ToInt32(userRow["vip_points"]);
                        }
                    }

                    continue;
                }

                _playersCached.Add(Convert.ToInt32(userRow["id"]), new UserInformation(
                    Convert.ToInt32(userRow["id"]),
                    Convert.ToString(userRow["auth_ticket"]),
                    Convert.ToInt32(userRow["last_online"]),
                    Convert.ToString(userRow["look"]),
                    Convert.ToInt32(userRow["rank"]),
                    Convert.ToInt32(userRow["credits"]),
                    Convert.ToInt32(userRow["activity_points"]),
                    Convert.ToInt32(userRow["vip_points"])));
            }
        }
    }
}







