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
        /// <summary>
        /// 拧紧枪数据起始位置
        /// </summary>
        public int GunNo { get; set; }

        /// <summary>
        /// 拧紧枪数据总数
        /// </summary>
        public int GunCount { get; set; }

        public int Product1No { get; set; }

        public int Product2No { get; set; }



        /// <summary>
        /// 条码起始位置
        /// </summary>
        public int BarNo { get; set; }

        /// <summary>
        /// 条码总数
        /// </summary>
        public int BarCount { get; set; }

        public string ImageUri { get; set; }

        public ushort BarLengh { get; set; }
    }
}
