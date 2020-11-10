using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
using Entity;
    public class GetInfoService
    {
        public string GetStaStr(int value)
        {
            string conn = null;
            switch (value)
            {
                case 1:
                    conn = "DB2000.0";  //DBW
                    break;
                case 2:
                    conn = "DB2000.2";
                    break;
                case 3:
                    conn = "DB2000.4";
                    break;
                case 4:
                    conn = "DB2000.6";
                    break;
                case 5:
                    conn = "DB2000.8";
                    break;
            }
            return conn;
        }

        public string GetTypeStr(int value)
        {
            string conn = null;
            switch (value)
            {
                case 1:
                    conn = "DB2000.4270"; //DBW
                    break;
                case 2:
                    conn = "DB2000.4272";
                    break;
                case 3:
                    conn = "DB2000.4274";
                    break;
                case 4:
                    conn = "DB2000.4276";
                    break;
                case 5:
                    conn = "DB2000.4278";
                    break;
                case 6:
                    conn = "DB2000.4280";
                    break;
                case 7:
                    conn = "DB2000.4282";
                    break;
                case 8:
                    conn = "DB2000.4284";
                    break;
                case 9:
                    conn = "DB2000.4286";
                    break;
                case 10:
                    conn = "DB2000.4288";
                    break;
            }
            return conn;
        }

        public GDbStr GetGunStr(int value)
        {
            GDbStr str = new GDbStr();
            switch(value)
            {
                case 1:
                    str.TorqueStr = "DB2000.10"; //DBD
                    str.AngleStr = "DB2000.14"; //DBD
                    str.ResultStr = "DB2000.18"; //DBX
                    break;
                case 2:
                    str.TorqueStr = "DB2000.20";
                    str.AngleStr = "DB2000.24";
                    str.ResultStr = "DB2000.28";
                    break;
                case 3:
                    str.TorqueStr = "DB2000.30";
                    str.AngleStr = "DB2000.34";
                    str.ResultStr = "DB2000.38";
                    break;
                case 4:
                    str.TorqueStr = "DB2000.40";
                    str.AngleStr = "DB2000.44";
                    str.ResultStr = "DB2000.48";
                    break;
                case 5:
                    str.TorqueStr = "DB2000.50";
                    str.AngleStr = "DB2000.54";
                    str.ResultStr = "DB2000.58";
                    break;
                case 6:
                    str.TorqueStr = "DB2000.60";
                    str.AngleStr = "DB2000.64";
                    str.ResultStr = "DB2000.68";
                    break;
                case 7:
                    str.TorqueStr = "DB2000.70";
                    str.AngleStr = "DB2000.74";
                    str.ResultStr = "DB2000.78";
                    break;
                case 8:
                    str.TorqueStr = "DB2000.80";
                    str.AngleStr = "DB2000.84";
                    str.ResultStr = "DB2000.88";
                    break;
                case 9:
                    str.TorqueStr = "DB2000.90";
                    str.AngleStr = "DB2000.94";
                    str.ResultStr = "DB2000.98";
                    break;
                case 10:
                    str.TorqueStr = "DB2000.100";
                    str.AngleStr = "DB2000.104";
                    str.ResultStr = "DB2000.108";
                    break;
            }
            return str;
        }

        public GDbStr GetGunStr2(int value)
        {
            GDbStr str = new GDbStr();
            switch (value)
            {
                case 1:
                    str.TorqueStr = "DB2000.5442";
                    str.AngleStr =  "DB2000.5446";
                    str.ResultStr = "DB2000.5450";
                    break;
                case 2:
                    str.TorqueStr = "DB2000.5452";
                    str.AngleStr =  "DB2000.5456";
                    str.ResultStr = "DB2000.5460";
                    break;
                case 3:
                    str.TorqueStr = "DB2000.5462";
                    str.AngleStr =  "DB2000.5466";
                    str.ResultStr = "DB2000.5470";
                    break;
                case 4:
                    str.TorqueStr = "DB2000.5472";
                    str.AngleStr =  "DB2000.5476";
                    str.ResultStr = "DB2000.5480";
                    break;
            }
            return str;
        }

        public BarCodeStr GetBarCodeStr(int value)
        {
            var tmp = value+1;
            BarCodeStr str = new BarCodeStr();
            var i = value * 100 + 10;
            str.BarStr = "DB2000." + i;
            str.ResultStr = "DB2000.DBX"+tmp+"08";
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GWNo">工位序号</param>
        /// <param name="type">界面左右0左1右</param>
        /// <returns></returns>
        public int GetNewBarCodeStr(int GWNo,int type)
        {
            int firstStr = 0;
            switch (GWNo)
            {
                case 04051:
                    if (type == 0) //left
                    {
                        firstStr = 4578;
                    }
                    else if (type == 1) //right
                    {
                        firstStr = 4290;
                    }
                    break;
                case 04062:
                    if (type == 0) //left
                    {
                        firstStr = 4578;
                    }
                    else if (type == 1) //right
                    {
                        firstStr = 4866;
                    }
                    break;
            }
            return firstStr;
        }

        public string GetErrorStr(int value)
        {
            string conn = null;
            switch (value)
            {
                case 1:
                    conn = "DB2000.5542";  //DBW
                    break;
                case 2:
                    conn = "DB2000.5544";
                    break;
                case 3:
                    conn = "DB2000.5546";
                    break;
                case 4:
                    conn = "DB2000.5548";
                    break;
                case 5:
                    conn = "DB2000.5550";
                    break;
            }
            return conn;
        }

        public string GetSaoMaStr(int GWNo, int type)
        {
            string conn = null;
            switch (GWNo)
            {
                case 04051:
                    if (type == 0) //left
                    {
                        conn = "";
                    }
                    else if (type == 1) //right
                    {
                        conn = "DB4000.510";
                    }
                    break;
                case 04062:
                    if (type == 0) //left
                    {
                        conn = "DB4000.1420";
                    }
                    else if (type == 1) //right
                    {
                        conn = "DB4000.1421";
                    }
                    break;
            }
            return conn;
        }

        public string GetReadSaveStr(int GWNo, int type)
        {
            string conn = null;
            switch (GWNo)
            {
                case 04051:
                    if (type == 0) //left
                    {
                        conn = "DB4000.509";
                    }
                    else if (type == 1) //right
                    {
                        conn = "DB4000.125";
                    }
                    break;
                case 04062:
                    if (type == 0) //left
                    {
                        conn = "DB4000.1420";
                    }
                    else if (type == 1) //right
                    {
                        conn = "DB4000.1421";
                    }
                    break;
            }
            return conn;
        }

        public string GetWriteSaveStr(int GWNo, int type)
        {
            string conn = null;
            switch (GWNo)
            {
                case 04051:
                    if (type == 0) //left
                    {
                        conn = "DB4000.511";
                    }
                    else if (type == 1) //right
                    {
                        conn = "DB4000.127";
                    }
                    break;
                case 04062:
                    if (type == 0) //left
                    {
                        conn = "DB4000.1420";
                    }
                    else if (type == 1) //right
                    {
                        conn = "DB4000.1421";
                    }
                    break;
            }
            return conn;
        }

    }
}