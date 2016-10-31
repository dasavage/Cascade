using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascade.Core.Database
{
    public class DatabaseSettings
    {
        public DatabaseSettings(string host, string userName, string password, string name, int port, int maxConnections)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentException(nameof(host));
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException(nameof(userName));
            if (string.IsNullOrEmpty(password)) throw new ArgumentException(nameof(password));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));
            if (port <= 0) throw new ArgumentException(nameof(port));
            if (maxConnections <= 0) throw new ArgumentException(nameof(maxConnections));

            DatabaseHost = host;
            DatabaseUsername = userName;
            DatabasePassword = password;
            DatabaseName = name;
            DatabasePort = port;
            MaximumConnections = maxConnections;
        }

        public string DatabaseHost { get; }
        public string DatabaseUsername { get; }
        public string DatabasePassword { get; }
        public string DatabaseName { get; }
        public int DatabasePort { get; }
        public int MaximumConnections { get; }
    }
}
