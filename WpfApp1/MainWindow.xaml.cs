using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Panuon.UI.Silver;
//using OpenApi;
using Panuon.UI.Silver.Core;
using System.Windows.Threading;
using S7.Net;
using WpfApp1.Entity;
using System.IO;
using Newtonsoft.Json;
using WpfApp1.Services;
using log4net;
using HslCommunication;
using HslCommunication.Profinet.Siemens;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private GetInfoService service = new GetInfoService();
        private DispatcherTimer ShowTimer;
        private DispatcherTimer timer;
        private ConfigData config;
        private BarCodeStr codeStr;
        //private Plc plc;
        private SiemensS7Net splc;
        private OperateResult connect;
        private GDbStr GunStr;
        private int markN = 0;
        private List<GDbData> ReList = new List<GDbData>();
        private bool remark = false;
        //private static BitmapImage IStation = new BitmapImage(new Uri("C:\\Users\\Administrator\\Desktop\\cs.png", UriKind.Absolute));  //"C:\\Users\\Administrator\\Desktop\\cs.png", UriKind.Absolute
        private static BitmapImage ILogo = new BitmapImage(new Uri("/Images/logo.png", UriKind.Relative));
        private static BitmapImage IFalse = new BitmapImage(new Uri("/Images/01.png", UriKind.Relative));
        private static BitmapImage ITrue = new BitmapImage(new Uri("/Images/02.png", UriKind.Relative));
        private ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            #region 启动时串口最大化显示
            this.WindowState = WindowState.Maximized;
            Rect rc = SystemParameters.WorkArea; //获取工作区大小
            //this.Topmost = true;
            this.Left = 0; //设置位置
            this.Top = 0;
            this.Width = rc.Width;
            this.Height = rc.Height;
            #endregion

            try
            {
                //读取本地配置JSON文件
                LoadJsonData();
                Init();
                //MainPageLoad();
                //plc = new Plc(CpuType.S71200, config.IpAdress, 0, 1);
                splc = new SiemensS7Net(SiemensPLCS.S1200, config.IpAdress)
                {
                    ConnectTimeOut = 5000
                };
                switch (config.GWNo)
                {

                    case 04051:
                        TM_Copy.Text = "CC特性：\r\n \r\n       24 + 4 Nm\r\n       20 + 2 Nm ";
                        break;
                    case 04062:
                        TM_Copy.Text = "CC特性：\r\n \r\n       24 + 4 Nm\r\n       ";
                        break;
                }

                #region PLC连接定时器
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += new EventHandler(ThreadCheck);
                timer.Interval = new TimeSpan(0, 0, 0, 5);
                timer.Start();
                #endregion
                //if (plc.IsAvailable)
                //{

                //    var result = plc.Open();
                //    if (!plc.IsConnected)
                //    {
                //        PLCImage.Source = IFalse;
                //        log.Info("PLC Not Connected!");
                //    }
                //    else
                //    {
                //        PLCImage.Source = ITrue;
                //        log.Info("PLC Connected!");
                //        DataReload();
                //    }
                //}
                //else
                //{
                //    PLCImage.Source = IFalse;
                //    log.Info("PLC Not Connected!");
                //}
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void Init()
        {
            #region 时间定时器
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowTimer1);
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1);
            ShowTimer.Start();
            #endregion

            var count1 = config.Station1.Count; //left
            var count2 = config.Station2.Count; //right
            SetStepData(count1, config.Station1, 1);
            SetStepData(count2, config.Station2, 2);

            ReList.Clear();
            pImage.Source = new BitmapImage(new Uri(config.ImageUri, UriKind.Absolute)); ;
            Logo.Source = ILogo;

        }

        private void MainPageLoad()
        {
            //定时查询-定时器
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) =>
            {
                if (markN == 50)
                {
                    ReList.Clear();
                    markN = 0;
                }
                markN += 1;
                ReList.Add(new GDbData(markN, 20.3, 65.5, "True"));
                ReList.Sort((x, y) => -x.Num.CompareTo(y.Num));
                DataList.ItemsSource = null;
                DataList.ItemsSource = ReList;
                DataList.Items.Refresh();
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);
            dispatcherTimer.Start();
        }

        private void DataReload()
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) =>
            {
                try
                {
                    //读取PLC工序步骤状态
                    //左
                    //var sta = (ushort)plc.Read(service.GetStaStr(config.Station1No));
                    //ModifyStep1(sta, config.GWNo, 0);
                    var sta = splc.ReadUInt16(service.GetStaStr(config.Station1No));
                    if (sta.IsSuccess)
                    {
                        ModifyStep1(sta.Content, config.GWNo,0);
                    }
                    else
                    {
                        throw new Exception("工序步骤状态读取失败");
                    }
                    //右
                    //var sta1 = (ushort)plc.Read(service.GetStaStr(config.Station2No));
                    //ModifyStep1(sta, config.GWNo, 1);
                    var sta1 = splc.ReadUInt16(service.GetStaStr(config.Station2No));
                    if (sta1.IsSuccess)
                    {
                        ModifyStep1(sta1.Content, config.GWNo,1);
                    }
                    else
                    {
                        throw new Exception("工序步骤状态读取失败");
                    }

                    #region 型号获取
                    //var type1 = (ushort)plc.Read(service.GetTypeStr(config.Product1No)); //left
                    //var type2 = (ushort)plc.Read(service.GetTypeStr(config.Product2No)); //right
                    var type1 = splc.ReadUInt16(service.GetTypeStr(config.Product1No));
                    if (type1.IsSuccess)
                    {
                        switch (type1.Content)
                        {
                            case 1:
                                XingHao1.Text = "正驾";
                                break;
                            case 2:
                                XingHao1.Text = "副驾";
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        throw new Exception("型号读取失败");
                    }
                    var type2 = splc.ReadUInt16(service.GetTypeStr(config.Product2No));
                    if (type2.IsSuccess)
                    {
                        switch (type2.Content)
                        {
                            case 1:
                                XingHao2.Text = "正驾";
                                break;
                            case 2:
                                XingHao2.Text = "副驾";
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        throw new Exception("型号读取失败");
                    }
                    //switch (type1)
                    //{
                    //    case 1:
                    //        XingHao1.Text = "正驾";
                    //        break;
                    //    case 2:
                    //        XingHao1.Text = "副驾";
                    //        break;
                    //    default: break;
                    //}
                    //switch (type2)
                    //{
                    //    case 1:
                    //        XingHao2.Text = "正驾";
                    //        break;
                    //    case 2:
                    //        XingHao2.Text = "副驾";
                    //        break;
                    //    default: break;
                    //}
                    #endregion

                    #region Old BarCode Get Cancel
                    //if (config.BarCount > 0)
                    //{
                    //    int k = config.BarNo;  //adress get;
                    //    Barcode1.Text = "";
                    //    Barcode2.Text = "";
                    //    Barcode3.Text = "";
                    //    Barcode4.Text = "";
                    //    BarYz.Text = "";
                    //    for (int i = 1; i <= config.BarCount; i++)
                    //    {
                    //        var i1 = i + k - 1;
                    //        if (i1 == k)
                    //        {
                    //            codeStr = service.GetBarCodeStr(k);
                    //            var temp = (string)plc.Read(DataType.DataBlock, 2000, codeStr.BarStr, VarType.String, 40);
                    //            temp = temp.Trim();
                    //            var BarResult = (bool)plc.Read(codeStr.ResultStr);
                    //            if (!temp.IsNullOrEmpty())
                    //            {
                    //                switch (i)
                    //                {
                    //                    case 1:
                    //                        Barcode1.Text = temp;
                    //                        break;
                    //                    case 2:
                    //                        Barcode2.Text = temp;
                    //                        break;
                    //                    case 3:
                    //                        Barcode3.Text = temp;
                    //                        break;
                    //                    case 4:
                    //                        Barcode4.Text = temp;
                    //                        break;
                    //                }
                    //                if (BarResult)
                    //                {
                    //                    BarYz.Text = "比对成功";
                    //                }
                    //                else
                    //                {
                    //                    BarYz.Text = "比对失败";
                    //                }
                    //                k += 1;
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion

                    #region New BarcodeGet
                    // Left BarCode
                    int lStartAddr = service.GetNewBarCodeStr(config.GWNo, 0);
                    // Right BarCode
                    int rStartAddr = service.GetNewBarCodeStr(config.GWNo, 1);

                    Barcode1.Text = "";
                    Barcode2.Text = "";
                    Barcode3.Text = "";
                    Barcode4.Text = "";
                    BarYz.Text = "";
                    for (int i = 1; i <= 4; i++)
                    {
                        if (i != 1)
                        {
                            lStartAddr += 72;
                            rStartAddr += 72;
                        }
                        string lStr = "DB2000." + lStartAddr;
                        string rStr = "DB2000." + rStartAddr;

                        //var lCode = (string)plc.Read(DataType.DataBlock, 2000, lStartAddr, VarType.String, config.BarLengh);
                        //var rCode = (string)plc.Read(DataType.DataBlock, 2000, rStartAddr, VarType.String, config.BarLengh);
                        //lCode = lCode.Trim();
                        //rCode = rCode.Trim();
                        string lCode = null;
                        string rCode = null;
                        var lResult = splc.ReadString(lStr, config.BarLengh);
                        var rResult = splc.ReadString(rStr, config.BarLengh);
                        if (lResult.IsSuccess)
                        {
                            lCode = lResult.Content.Trim();
                        }
                        else
                        {
                            throw new Exception("条码读取失败！");
                        }

                        if (rResult.IsSuccess)
                        {
                            rCode = rResult.Content.Trim();
                        }
                        else
                        {
                            throw new Exception("条码读取失败！");
                        }

                        if (!lCode.IsNullOrEmpty())
                        {
                            switch (i)
                            {
                                case 1:
                                    Barcode1.Text = lCode;
                                    break;
                                case 2:
                                    Barcode2.Text = lCode;
                                    break;
                                case 3:
                                    Barcode1.Text = lCode;
                                    break;
                                case 4:
                                    Barcode2.Text = lCode;
                                    break;
                            }
                            BarYz.Text = "比对成功";
                        }
                        if (!rCode.IsNullOrEmpty())
                        {
                            switch (i)
                            {
                                case 1:
                                    Barcode3.Text = rCode;
                                    break;
                                case 2:
                                    Barcode4.Text = rCode;
                                    break;
                                case 3:
                                    Barcode3.Text = rCode;
                                    break;
                                case 4:
                                    Barcode4.Text = rCode;
                                    break;
                            }
                            BarYz.Text = "比对成功";
                        }
                    }

                    #endregion

                    #region 拧紧枪数据获取
                    ReList.Clear();

                    if (config.GunCount > 0)
                    {
                        for (int i = 1; i <= config.GunCount; i++)
                        {
                            var i1 = i + config.GunNo - 1;
                            GunStr = service.GetGunStr(i1);
                            //var torque1 = ((uint)plc.Read(GunStr.TorqueStr)).ConvertToDouble();
                            //torque1 = double.Parse(torque1.ToString("F2"));
                            //var angle1 = ((uint)plc.Read(GunStr.AngleStr)).ConvertToDouble();
                            //angle1 = double.Parse(angle1.ToString("F2"));
                            //var result1 = (bool)plc.Read(GunStr.ResultStr);
                            double torque1 = 0;
                            double angle1 = 0;
                            bool result1 = false;
                            var t = splc.ReadDouble(GunStr.TorqueStr);
                            var a = splc.ReadDouble(GunStr.AngleStr);
                            var r = splc.ReadBool(GunStr.ResultStr);
                            if (t.IsSuccess)
                            {
                                torque1 = double.Parse(t.Content.ToString("F2"));
                            }
                            else
                            {
                                throw new Exception("扭矩读取失败！");
                            }

                            if (a.IsSuccess)
                            {
                                angle1 = double.Parse(a.Content.ToString("F2"));
                            }
                            else
                            {
                                throw new Exception("角度读取失败！");
                            }

                            if (r.IsSuccess)
                            {
                                result1 = r.Content;
                            }
                            else
                            {
                                throw new Exception("结果读取失败！");
                            }

                            string rest;
                            if (torque1 != 0)
                            {
                                if (i == 1)
                                {
                                    ReList.Clear();
                                    markN = 0;
                                }
                                if (result1)
                                {
                                    rest = "OK";
                                }
                                else
                                {
                                    rest = "NG";
                                }
                                markN += 1;
                                ReList.Add(new GDbData(markN, torque1, angle1, rest));
                                ReList.Sort((x, y) => -x.Num.CompareTo(y.Num));
                                DataList.ItemsSource = null;
                                DataList.ItemsSource = ReList;
                                DataList.Items.Refresh();
                            }
                        }
                        if (config.GWNo == 04062)
                        {
                            for (int i = 1; i <= config.GunCount; i++)
                            {
                                GunStr = service.GetGunStr2(i);
                                //var torque1 = ((uint)plc.Read(GunStr.TorqueStr)).ConvertToDouble();
                                //torque1 = double.Parse(torque1.ToString("F2"));
                                //var angle1 = ((uint)plc.Read(GunStr.AngleStr)).ConvertToDouble();
                                //angle1 = double.Parse(angle1.ToString("F2"));
                                //var result1 = (bool)plc.Read(GunStr.ResultStr);

                                double torque1 = 0;
                                double angle1 = 0;
                                bool result1 = false;
                                var t = splc.ReadDouble(GunStr.TorqueStr);
                                var a = splc.ReadDouble(GunStr.AngleStr);
                                var r = splc.ReadBool(GunStr.ResultStr);
                                if (t.IsSuccess)
                                {
                                    torque1 = double.Parse(t.Content.ToString("F2"));
                                }
                                else
                                {
                                    throw new Exception("扭矩读取失败！");
                                }

                                if (a.IsSuccess)
                                {
                                    angle1 = double.Parse(a.Content.ToString("F2"));
                                }
                                else
                                {
                                    throw new Exception("角度读取失败！");
                                }

                                if (r.IsSuccess)
                                {
                                    result1 = r.Content;
                                }
                                else
                                {
                                    throw new Exception("角度读取失败！");
                                }

                                string rest;
                                if (torque1 != 0)
                                {
                                    if (result1)
                                    {
                                        rest = "OK";
                                    }
                                    else
                                    {
                                        rest = "NG";
                                    }
                                    markN += 1;
                                    ReList.Add(new GDbData(markN, torque1, angle1, rest));
                                    ReList.Sort((x, y) => -x.Num.CompareTo(y.Num));
                                    DataList.ItemsSource = null;
                                    DataList.ItemsSource = ReList;
                                    DataList.Items.Refresh();
                                }
                            }
                        }

                    }
                    #endregion

                } catch (Exception exc)
                {
                    log.Error("------PLC访问出错------");
                    log.Error(exc.Message);
                    dispatcherTimer.Stop();
                }
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(2);
            dispatcherTimer.Start();
        }

        public void ShowTimer1(object sender, EventArgs e)
        {
            this.TM.Text = " ";
            //获得年月日 
            this.TM.Text += DateTime.Now.ToString("yyyy年MM月dd日");   //yyyy年MM月dd日 
            this.TM.Text += "\r\n       ";
            //获得时分秒 
            this.TM.Text += DateTime.Now.ToString("HH:mm:ss");
            this.TM.Text += "              ";
            this.TM.Text += DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("zh-cn"));
            this.TM.Text += " ";
        }

        private void LoadJsonData()
        {
            try
            {
                using (var sr = File.OpenText("C:\\config\\config.json"))
                {
                    string JsonStr = sr.ReadToEnd();
                    config = JsonConvert.DeserializeObject<ConfigData>(JsonStr);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private void SetStepData(int count, List<StationData> list, int mark)
        {
            #region who care
            if (mark == 1)
            {
                switch (count)
                {
                    case 1:
                        Step1.Text = list[0].Name;
                        StepImage1.Source = IFalse;
                        break;
                    case 2:
                        Step1.Text = list[0].Name;
                        Step2.Text = list[1].Name;
                        StepImage1.Source = IFalse;
                        StepImage2.Source = IFalse;
                        break;
                    case 3:
                        Step1.Text = list[0].Name;
                        Step2.Text = list[1].Name;
                        Step3.Text = list[2].Name;
                        StepImage1.Source = IFalse;
                        StepImage2.Source = IFalse;
                        StepImage3.Source = IFalse;
                        break;
                    case 4:
                        Step1.Text = list[0].Name;
                        Step2.Text = list[1].Name;
                        Step3.Text = list[2].Name;
                        Step4.Text = list[3].Name;
                        StepImage1.Source = IFalse;
                        StepImage2.Source = IFalse;
                        StepImage3.Source = IFalse;
                        StepImage4.Source = IFalse;
                        break;
                    case 5:
                        Step1.Text = list[0].Name;
                        Step2.Text = list[1].Name;
                        Step3.Text = list[2].Name;
                        Step4.Text = list[3].Name;
                        Step5.Text = list[4].Name;
                        StepImage1.Source = IFalse;
                        StepImage2.Source = IFalse;
                        StepImage3.Source = IFalse;
                        StepImage4.Source = IFalse;
                        StepImage5.Source = IFalse;
                        break;
                    case 6:
                        Step1.Text = list[0].Name;
                        Step2.Text = list[1].Name;
                        Step3.Text = list[2].Name;
                        Step4.Text = list[3].Name;
                        Step5.Text = list[4].Name;
                        Step6.Text = list[5].Name;
                        StepImage1.Source = IFalse;
                        StepImage2.Source = IFalse;
                        StepImage3.Source = IFalse;
                        StepImage4.Source = IFalse;
                        StepImage5.Source = IFalse;
                        StepImage6.Source = IFalse;
                        break;
                    case 7:
                        Step1.Text = list[0].Name;
                        Step2.Text = list[1].Name;
                        Step3.Text = list[2].Name;
                        Step4.Text = list[3].Name;
                        Step5.Text = list[4].Name;
                        Step6.Text = list[5].Name;
                        Step7.Text = list[6].Name;
                        StepImage1.Source = IFalse;
                        StepImage2.Source = IFalse;
                        StepImage3.Source = IFalse;
                        StepImage4.Source = IFalse;
                        StepImage5.Source = IFalse;
                        StepImage6.Source = IFalse;
                        StepImage7.Source = IFalse;
                        break;

                }
            }
            else if (mark == 2)
            {
                switch (count)
                {
                    case 1:
                        Step21.Text = list[0].Name;
                        StepImage21.Source = IFalse;
                        break;
                    case 2:
                        Step21.Text = list[0].Name;
                        Step22.Text = list[1].Name;
                        StepImage21.Source = IFalse;
                        StepImage22.Source = IFalse;
                        break;
                    case 3:
                        Step21.Text = list[0].Name;
                        Step22.Text = list[1].Name;
                        Step23.Text = list[2].Name;
                        StepImage21.Source = IFalse;
                        StepImage22.Source = IFalse;
                        StepImage23.Source = IFalse;
                        break;
                    case 4:
                        Step21.Text = list[0].Name;
                        Step22.Text = list[1].Name;
                        Step23.Text = list[2].Name;
                        Step24.Text = list[3].Name;
                        StepImage21.Source = IFalse;
                        StepImage22.Source = IFalse;
                        StepImage23.Source = IFalse;
                        StepImage24.Source = IFalse;
                        break;
                    case 5:
                        Step21.Text = list[0].Name;
                        Step22.Text = list[1].Name;
                        Step23.Text = list[2].Name;
                        Step24.Text = list[3].Name;
                        Step25.Text = list[4].Name;
                        StepImage21.Source = IFalse;
                        StepImage22.Source = IFalse;
                        StepImage23.Source = IFalse;
                        StepImage24.Source = IFalse;
                        StepImage25.Source = IFalse;
                        break;
                    case 6:
                        Step21.Text = list[0].Name;
                        Step22.Text = list[1].Name;
                        Step23.Text = list[2].Name;
                        Step24.Text = list[3].Name;
                        Step25.Text = list[4].Name;
                        Step26.Text = list[5].Name;
                        StepImage21.Source = IFalse;
                        StepImage22.Source = IFalse;
                        StepImage23.Source = IFalse;
                        StepImage24.Source = IFalse;
                        StepImage25.Source = IFalse;
                        StepImage26.Source = IFalse;
                        break;
                    case 7:
                        Step21.Text = list[0].Name;
                        Step22.Text = list[1].Name;
                        Step23.Text = list[2].Name;
                        Step24.Text = list[3].Name;
                        Step25.Text = list[4].Name;
                        Step26.Text = list[5].Name;
                        Step27.Text = list[6].Name;
                        StepImage21.Source = IFalse;
                        StepImage22.Source = IFalse;
                        StepImage23.Source = IFalse;
                        StepImage24.Source = IFalse;
                        StepImage25.Source = IFalse;
                        StepImage26.Source = IFalse;
                        StepImage27.Source = IFalse;
                        break;

                }
            }
            #endregion
        }

        private void ModifyStep1(int type, int GWNo, int mark)
        {
            if (mark == 0) //0左边
            {
                switch (GWNo)
                {
                    case 04062: //工位
                        switch (type)
                        {
                            case 100:
                                StepImage1.Source = IFalse;
                                StepImage2.Source = IFalse;
                                StepImage3.Source = IFalse;
                                StepImage4.Source = IFalse;
                                StepImage5.Source = IFalse;
                                StepImage6.Source = IFalse;
                                StepImage7.Source = IFalse;
                                break;
                            case 150:
                                StepImage1.Source = ITrue;
                                break;
                            case 500:
                                StepImage2.Source = ITrue;
                                break;
                            case 700:
                                StepImage3.Source = ITrue;
                                break;
                            case 1000:
                                StepImage4.Source = ITrue;
                                break;
                            case 1900:
                                StepImage5.Source = ITrue;
                                break;
                            case 2100:
                                StepImage6.Source = ITrue;
                                break;
                            case 2300:
                                StepImage7.Source = ITrue;
                                break;
                        }
                        break;
                    case 04051: //工位
                        switch (type)
                        {
                            case 150:
                                StepImage1.Source = IFalse;
                                StepImage2.Source = IFalse;
                                StepImage3.Source = IFalse;
                                StepImage4.Source = IFalse;
                                StepImage5.Source = IFalse;
                                StepImage6.Source = IFalse;
                                StepImage7.Source = IFalse;
                                break;

                            case 200:
                                StepImage2.Source = ITrue;
                                break;
                            case 400:
                                StepImage3.Source = ITrue;
                                break;
                            case 550:
                                StepImage4.Source = ITrue;
                                break;
                            case 600:
                                StepImage5.Source = ITrue;
                                break;
                            case 800:
                                StepImage6.Source = ITrue;
                                break;
                            case 900:
                                StepImage7.Source = ITrue;
                                break;
                        }
                        break;
                }
            }
            else if (mark == 1) //1 右边
            {
                switch (GWNo)
                {
                    case 04062: //工位
                        switch (type)
                        {
                            case 100:
                                StepImage21.Source = IFalse;
                                StepImage22.Source = IFalse;
                                StepImage23.Source = IFalse;
                                StepImage24.Source = IFalse;
                                StepImage25.Source = IFalse;
                                StepImage26.Source = IFalse;
                                StepImage27.Source = IFalse;
                                break;
                            case 150:
                                StepImage21.Source = ITrue;
                                break;
                            case 500:
                                StepImage22.Source = ITrue;
                                break;
                            case 700:
                                StepImage23.Source = ITrue;
                                break;
                            case 1000:
                                StepImage24.Source = ITrue;
                                break;
                            case 1900:
                                StepImage25.Source = ITrue;
                                break;
                            case 2100:
                                StepImage26.Source = ITrue;
                                break;
                            case 2300:
                                StepImage27.Source = ITrue;
                                break;
                        }
                        break;
                    case 04051: //工位
                        switch (type)
                        {
                            case 100:
                                StepImage21.Source = ITrue;
                                StepImage22.Source = IFalse;
                                StepImage23.Source = IFalse;
                                StepImage24.Source = IFalse;
                                StepImage25.Source = IFalse;
                                StepImage26.Source = IFalse;
                                StepImage27.Source = IFalse;
                                break;
                            case 200:
                                StepImage22.Source = ITrue;
                                break;
                            case 400:
                                StepImage23.Source = ITrue;
                                break;
                            case 700:
                                StepImage24.Source = ITrue;
                                break;
                            case 900:
                                StepImage25.Source = ITrue;
                                break;
                            case 1000:
                                StepImage26.Source = ITrue;
                                break;
                            case 1100:
                                StepImage27.Source = ITrue;
                                break;
                        }
                        break;
                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (MessageBoxX.Show("是否要关闭？", "确认", Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //    e.Cancel = false;
            //    //if (plc.IsConnected)
            //    //{
            //    //    plc.Close();
            //    //    log.Info("PLC Disconnected!");
            //    //}
            //    if (connect.IsSuccess)
            //    {
            //        splc.ConnectClose();
            //    }
            //}
            //else
            //{
            //    e.Cancel = true;
            //}
            if (connect.IsSuccess)
            {
                splc.ConnectClose();
            }
            log.Info("PLC Disconnected!");
        }

        public void ThreadCheck(object sender, EventArgs e)
        {
            OperateResult<short> connect = splc.ReadInt16("DB2000.0");
            if (connect.IsSuccess)
            {
                PLCImage.Source = ITrue;
                log.Info("PLC Connected!");

                if (!remark)
                {
                    DataReload();
                }
            }
            else
            {
                PLCImage.Source = IFalse;
                log.Info("PLC Not Connected!");
            }
        }
    }
}
