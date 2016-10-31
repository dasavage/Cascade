using System;
using System.Diagnostics;
using Cascade.Core.Cascade;
using Cascade.Core.Config;
using Cascade.Core.Database;
using Cascade.Core.Logging;

namespace Cascade
{
    public static class Cascade
    {
        private static CascadeProcess _process;
        private static LogManager _logManager;
        private static ConfigManager _configManager;
        private static DatabaseManager _databaseManager;

        private static readonly string _projectName = "Cascade Notification";
        private static readonly string _projectVersion = "3.0 BETA DEV";
        private static readonly string _projectDevs = "DaSavage, JynX";

        public static DateTime StarteDateTime { get; private set; }

        public static void Initialize()
        {
            string[] consoleLogo;

            consoleLogo = new[] {
                @"  _____                        _      ",
                @" / ____|                      | |     ",
                @"| |     __ _ ___  ___ __ _  __| | ___     " + _projectName + "",
                @"| |    / _` / __|/ __/ _` |/ _` |/ _ \    " + _projectVersion + "",
                @"| |___| (_| \__ \ (_| (_| | (_| |  __/",
                @" \_____\__,_|___/\___\__,_|\__,_|\___|    Developers: " + _projectDevs + "",
                @"",
                @""};

            Console.ForegroundColor = ConsoleColor.Magenta;

            foreach (var consoleLogoString in consoleLogo)
            {
                Console.WriteLine(" " + consoleLogoString);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Black;

            var stopwatch = Stopwatch.StartNew();

            _process = new CascadeProcess();
            _logManager = new LogManager();
            _configManager = new ConfigManager("config.ini");

            var databaseStopwatch = Stopwatch.StartNew();

            _databaseManager = new DatabaseManager(new DatabaseSettings(
                _configManager.GetConfigElement("database.host"),
                _configManager.GetConfigElement("database.username"),
                _configManager.GetConfigElement("database.password"),
                _configManager.GetConfigElement("database.name"),
                int.Parse(_configManager.GetConfigElement("database.port")),
                int.Parse(_configManager.GetConfigElement("database.max_connections"))));

            if (!_databaseManager.WorkingConnection())
            {
                _logManager.Log("Unable to connect to the MySQL server.", LogType.Error);
                return;
            }

            databaseStopwatch.Stop();
            _logManager.Log("Loaded MySQL [" + databaseStopwatch.ElapsedMilliseconds + "ms]", LogType.Information);

            stopwatch.Stop();
            _logManager.Log("Cascade has loaded! [" + stopwatch.ElapsedMilliseconds + "ms]", LogType.Information);

            Load();

            StarteDateTime = DateTime.Now;

            Console.Title = (System.Diagnostics.Debugger.IsAttached ? "[DEBUG] " : "") + "Cascade " + _projectVersion;
            Console.CursorVisible = false;
        }

        private static void Load()
        {
            _process.SetupProcess();
        }

        public static LogManager GetLogManager()
        {
            return _logManager;
        }

        public static ConfigManager GetConfigManager()
        {
            return _configManager;
        }

        public static DatabaseManager GetDatabaseManager()
        {
            return _databaseManager;
        }
    }
}

