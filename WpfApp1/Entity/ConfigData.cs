using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WpfApp1.Entity
{
    class ConfigData
    {


        public string IpAdress { get; set; }

        public int GWNo { get; set; }
        public int Station1No { get; set; }

        public int Station2No { get; set; }


        public List<StationData> Station1 { get; set; }

        public List<StationData> Station2 { get; set; }
        public int GunNo { get; set; }

        public int ProductNo { get; set; }

        public string ProductType { get; set; }

        public string BarRule { get; set; }

        public int BarNo { get; set; }

        public string ImageUri { get; set; }
    }
}
