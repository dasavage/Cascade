using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascade.Core.Cascade.User
{
    internal class UserInformation
    {
        private readonly int _userId;
        private string _authTicket;
        private int _lastOnline;
        private string _look;
        private int _rank;
        private int _credits;
        private int _pixels;
        private int _vipPoints;

        public UserInformation(int userId, string authTicket, int lastOnline, string look, int rank, int credits, int pixels, int vipPoints)
        {
            _userId = userId;
            _authTicket = authTicket;
            _lastOnline = lastOnline;
            _look = look;
            _rank = rank;
            _credits = credits;
            _pixels = pixels;
            _vipPoints = vipPoints;
        }

        public int UserId
        {
            get { return _userId; }
        }

        public string AuthTicket
        {
            get { return _authTicket; }
            set { _authTicket = value; }
        }

        public int LastOnline
        {
            get { return _lastOnline; }
            set { _lastOnline = value; }
        }

        public string Look
        {
            get { return _look; }
            set { _look = value; }
        }

        public int Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        public int Credits
        {
            get { return _credits; }
            set { _credits = value; }
        }

        public int Pixels
        {
            get { return _pixels; }
            set { _pixels = value; }
        }

        public int VipPoints
        {
            get { return _vipPoints; }
            set { _vipPoints = value; }
        }
    }
}
