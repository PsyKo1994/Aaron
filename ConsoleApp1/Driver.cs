using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ConsoleApp1
{
    class Driver
    {
        //Driver values
        public ulong driverID { get; set; }
        public string driver { get; set; }
        public int team { get; set; }
        public string tier { get; set; }
        public string attendanceReaction { get; set; }

        public Driver(DataRow row)
        {
            this.driverID = Convert.ToUInt64(row["driverID"]);
            this.driver = row["driver"].ToString();
            this.team = Convert.ToInt32(row["team"]);
            this.tier = row["tier"].ToString();
            this.attendanceReaction = row["attendanceReaction"].ToString();
        }
    }
}
