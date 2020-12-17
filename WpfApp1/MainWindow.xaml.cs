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
using System.Windows.Automation.Peers;
using WpfApp1.DAL;
using System.IO.Ports;
using System.Threading;

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
        private List<ProductConfig> Gwlist;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private ConfigData config;
        private BarCodeStr codeStr;
        private ProductConfig product;
        private ProductConfig rightproduct;
        private SerialPort serialPort;
        private SerialPort serialPortLeft;
        private SerialPort serialPortRight;
        //private Plc plc;
        private SiemensS7Net splc;
        private OperateResult connect;
        private GDbStr GunStr;
        private int markN = 0;
        private int barCount = 0;
        private int rightbarCount = 0;
        private int barCount1 = 0;
        private int rightbarCount1 = 0;
        private List<GDbData> ReList = new List<GDbData>();
        private List<GDbData> rightReList = new List<GDbData>();
        private bool remark = false;
        private bool saveMark = false;
        private bool saveMark1 = false;
        private bool IsOn = false;
        //private static BitmapImage IStation = new BitmapImage(new Uri("C:\\Users\\Administrator\\Desktop\\cs.png", UriKind.Absolute));  //"C:\\Users\\Administrator\\Desktop\\cs.png", UriKind.Absolute
        private static BitmapImage ILogo = new BitmapImage(new Uri("/Images/logo.png", UriKind.Relative));
        private static BitmapImage IFalse = new BitmapImage(new Uri("/Images/01.png", UriKind.Relative));
        private static BitmapImage ITrue = new BitmapImage(new Uri("/Images/02.png", UriKind.Relative));
        private ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<GongXuModel> left = new List<GongXuModel>();
        private List<GongXuModel> right = new List<GongXuModel>();
        private List<string> barList = new List<string>();
        private List<string> rightbarList = new List<string>();
        private List<string> yzList = new List<string>();
        private List<string> rightyzList = new List<string>();
        private List<string> elist = new List<string>();
        private List<string> rightelist = new List<string>();
        private List<string> beforeLeftList = new List<string>();
        private List<string> beforeRightList = new List<string>();
        private MainDAL dal;
        private bool leftMark = false;
        private bool rightMark = false;
        private bool leftMark1 = false;
        private bool rightMark1 = false;
        private Thread thread;
        private long FIntryID = 0;
        private long FIntryIDRight = 0;
        private string DJCode = null;
        private string QGCode = null;
        private string LXCode = null;
        private string CBCode = null;

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

                connect = splc.ConnectServer();

                InitGw();

                #region PLC连接定时器
                //timer = new System.Windows.Threading.DispatcherTimer();
                //timer.Tick += new EventHandler(ThreadCheck);
                //timer.Interval = new TimeSpan(0, 0, 0, 5);
                //timer.Start();
                #endregion

                #region cancel
                //ListViewAutomationPeer lvapl = new ListViewAutomationPeer(listViewL);
                //double rowMarkl = -1;
                //var listTimerl = new DispatcherTimer();
                //listTimerl.Tick += (s, e) =>
                //{
                //    var svap = lvapl.GetPattern(PatternInterface.Scroll) as ScrollViewerAutomationPeer;
                //    var scroll = svap.Owner as ScrollViewer;
                //    if (rowMarkl == scroll.VerticalOffset)
                //    {
                //        scroll.ScrollToTop();
                //    }
                //    else
                //    {
                //        rowMarkl = scroll.VerticalOffset;
                //        scroll.ScrollToVerticalOffset(scroll.VerticalOffset + 1);
                //    }
                //};
                //listTimerl.Interval = new TimeSpan(0, 0, 0, 5);
                //listTimerl.Start();

                //ListViewAutomationPeer lvapr = new ListViewAutomationPeer(listViewR);
                //double rowMarkr = -1;
                //var listTimerr = new DispatcherTimer();
                //listTimerr.Tick += (s, e) =>
                //{
                //    var svap = lvapr.GetPattern(PatternInterface.Scroll) as ScrollViewerAutomationPeer;
                //    var scroll = svap.Owner as ScrollViewer;
                //    if (rowMarkr == scroll.VerticalOffset)
                //    {
                //        scroll.ScrollToTop();
                //    }
                //    else
                //    {
                //        rowMarkr = scroll.VerticalOffset;
                //        scroll.ScrollToVerticalOffset(scroll.VerticalOffset + 1);
                //    }
                //};
                //listTimerr.Interval = new TimeSpan(0, 0, 0, 5);
                //listTimerr.Start();
                #endregion
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

            dal = new MainDAL(config);
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
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) =>
            {
                try
                {
                    //读取在线 离线
                    string stron = "";
                    switch (config.GWNo)
                    {
                        case 04051:
                            stron = "DB19.4.0";
                            break;
                        case 04062:
                            stron = "DB25.182.6";
                            break;
                    }
                    var IsOnMark = splc.ReadBool(stron);
                    if (IsOnMark.IsSuccess)
                    {
                        IsOn = IsOnMark.Content;
                    }
                    IsOnline.Text = IsOn ? "在线" : "离线";
                    // 开始访问扫描枪
                    if (IsOn)
                    {
                        if (config.GWNo == 04062)
                        {

                            this.PortConnection();
                        }
                        else if (config.GWNo == 04051)
                        {
                            this.PortConnectionLeft();
                            this.PortConnectionRight();
                        }
                    }
                    else
                    {
                        this.PortClose();
                        this.PortClose2();
                    }

                    //读取PLC工序步骤状态
                    //左
                    //var sta = (ushort)plc.Read(service.GetStaStr(config.Station1No));
                    //ModifyStep1(sta, config.GWNo, 0);
                    var sta = splc.ReadUInt16(service.GetStaStr(config.Station1No));
                    if (sta.IsSuccess)
                    {
                        ModifyStep1(sta.Content, config.GWNo, 0);

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
                        ModifyStep1(sta1.Content, config.GWNo, 1);

                    }
                    else
                    {
                        throw new Exception("工序步骤状态读取失败");
                    }




                    #region 型号获取
                    //var type1 = (ushort)plc.Read(service.GetTypeStr(config.Product1No)); //left
                    //var type2 = (ushort)plc.Read(service.GetTypeStr(config.Product2No)); //right
                    //var type1 = splc.ReadUInt16(service.GetTypeStr(config.Product1No));
                    //if (type1.IsSuccess)
                    //{
                    //    //log.Debug(type1.Content + "   " + service.GetTypeStr(config.Product1No));
                    //    switch (type1.Content)
                    //    {
                    //        case 1:
                    //            XingHao1.Text = "正驾";
                    //            break;
                    //        case 2:
                    //            XingHao1.Text = "副驾";
                    //            break;
                    //        default: break;
                    //    }
                    //}
                    //else
                    //{
                    //    throw new Exception("型号读取失败");
                    //}
                    //var type2 = splc.ReadUInt16(service.GetTypeStr(config.Product2No));
                    //if (type2.IsSuccess)
                    //{
                    //    //log.Debug(type1.Content + " 2  " + service.GetTypeStr(config.Product2No));
                    //    switch (type2.Content)
                    //    {
                    //        case 1:
                    //            XingHao2.Text = "正驾";
                    //            break;
                    //        case 2:
                    //            XingHao2.Text = "副驾";
                    //            break;
                    //        default: break;
                    //    }
                    //}
                    //else
                    //{
                    //    throw new Exception("型号读取失败");
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
                    //// Left BarCode
                    //int lStartAddr = service.GetNewBarCodeStr(config.GWNo, 0);
                    //// Right BarCode
                    //int rStartAddr = service.GetNewBarCodeStr(config.GWNo, 1);

                    //Barcode1.Text = "";
                    //Barcode2.Text = "";
                    //Barcode3.Text = "";
                    //Barcode4.Text = "";
                    //BarYz.Text = "";
                    //for (int i = 1; i <= 4; i++)
                    //{
                    //    if (i != 1)
                    //    {
                    //        lStartAddr += 72;
                    //        rStartAddr += 72;
                    //    }
                    //    string lStr = "DB2000." + lStartAddr;
                    //    string rStr = "DB2000." + rStartAddr;

                    //    //var lCode = (string)plc.Read(DataType.DataBlock, 2000, lStartAddr, VarType.String, config.BarLengh);
                    //    //var rCode = (string)plc.Read(DataType.DataBlock, 2000, rStartAddr, VarType.String, config.BarLengh);
                    //    //lCode = lCode.Trim();
                    //    //rCode = rCode.Trim();
                    //    string lCode = null;
                    //    string rCode = null;
                    //    var lResult = splc.ReadString(lStr, config.BarLengh);
                    //    var rResult = splc.ReadString(rStr, config.BarLengh);
                    //    if (lResult.IsSuccess)
                    //    {
                    //        lCode = lResult.Content.Replace("\0", "").Trim();
                    //        if (lCode.Length > 1)
                    //        {
                    //            lCode = lCode.Substring(1, lCode.Length - 1);
                    //        }
                    //        else
                    //        {
                    //            lCode = null;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("条码读取失败！");
                    //    }

                    //    if (rResult.IsSuccess)
                    //    {
                    //        rCode = rResult.Content.Replace("\0", "").Trim();
                    //        if (rCode.Length > 1)
                    //        {
                    //            rCode = rCode.Substring(1, rCode.Length - 1);
                    //        }
                    //        else
                    //        {
                    //            rCode = null;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("条码读取失败！");
                    //    }

                    //    if (!lCode.IsNullOrEmpty())
                    //    {
                    //        switch (i)
                    //        {
                    //            case 1:
                    //                Barcode1.Text = lCode;
                    //                break;
                    //            case 2:
                    //                Barcode2.Text = lCode;
                    //                break;
                    //            case 3:
                    //                Barcode1.Text = lCode;
                    //                break;
                    //            case 4:
                    //                Barcode2.Text = lCode;
                    //                break;
                    //        }
                    //        BarYz.Text = "比对成功";
                    //    }
                    //    if (!rCode.IsNullOrEmpty())
                    //    {
                    //        switch (i)
                    //        {
                    //            case 1:
                    //                Barcode3.Text = rCode;
                    //                break;
                    //            case 2:
                    //                Barcode4.Text = rCode;
                    //                break;
                    //            case 3:
                    //                Barcode3.Text = rCode;
                    //                break;
                    //            case 4:
                    //                Barcode4.Text = rCode;
                    //                break;
                    //        }
                    //        BarYz.Text = "比对成功";
                    //    }
                    //}

                    #endregion

                    #region 拧紧枪数据获取
                    ReList.Clear();
                    rightReList.Clear();
                    DataList.ItemsSource = null;
                    DataList.Items.Refresh();
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
                            var t = splc.ReadFloat(GunStr.TorqueStr);
                            var a = splc.ReadFloat(GunStr.AngleStr);
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
                                ReList.Add(new GDbData(i, torque1, angle1, rest));
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
                                var t = splc.ReadFloat(GunStr.TorqueStr);
                                var a = splc.ReadFloat(GunStr.AngleStr);
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
                                    if (i == 1)
                                    {
                                        rightReList.Clear();
                                        markN = 4;
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
                                    rightReList.Add(new GDbData(markN, torque1, angle1, rest));
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

                    // 读取保存信号
                    var saveSingal = splc.ReadBool(service.GetReadSaveStr(config.GWNo, 0));
                    if (saveSingal.IsSuccess)
                    {
                        if (saveSingal.Content)
                        {
                            if (!saveMark)
                            {
                                //log.Debug(service.GetReadSaveStr(config.GWNo, 0));
                                bool save = false;
                                string process = "";
                                switch (config.GWNo)
                                {
                                    case 04051:
                                        process = "上部框架预装";
                                        //save = dal.SaveInfo(product.FInterID, process, barList, null);
                                        save = dal.UpdateData40512(FIntryID);
                                        //log.Debug(save);
                                        break;
                                    case 04062:
                                        process = "H型滑轨装配";
                                        //save = dal.SaveInfo(product.FInterID, process, barList, ReList);
                                        save = dal.UpdateData4062(FIntryID, ReList);
                                        break;
                                }
                                if (save)
                                {
                                    FIntryID = 0;
                                    splc.Write(service.GetWriteSaveStr(config.GWNo, 0), true);
                                    saveMark = true;
                                    barList.Clear();
                                    barCount = 0;
                                    elist.Clear();
                                    leftMark = false;
                                    leftMark1 = false;
                                    barCount1 = 0;
                                    beforeLeftList.Clear();
                                }
                            }
                        }
                        else
                        {
                            saveMark = false;
                        }
                    }

                    var saveSingal1 = splc.ReadBool(service.GetReadSaveStr(config.GWNo, 1));
                    if (saveSingal1.IsSuccess)
                    {
                        if (saveSingal1.Content)
                        {
                            if (!saveMark1)
                            {
                                //log.Debug(service.GetReadSaveStr(config.GWNo, 1));
                                bool save = false;
                                string process = "";
                                switch (config.GWNo)
                                {
                                    case 04051:
                                        process = "前管装配";
                                        //save = dal.SaveInfo(rightproduct.FInterID, process, rightbarList, ReList);
                                        save = dal.UpdateData40511(FIntryIDRight, ReList);
                                        //log.Debug(save);
                                        break;
                                    case 04062:
                                        process = "H型滑轨装配";
                                        //save = dal.SaveInfo(rightproduct.FInterID, process, rightbarList, rightReList);
                                        save = dal.UpdateData4062(FIntryIDRight, rightReList);
                                        break;
                                }
                                if (save)
                                {
                                    FIntryIDRight = 0;
                                    splc.Write(service.GetWriteSaveStr(config.GWNo, 1), true);
                                    saveMark1 = true;
                                    rightbarList.Clear();
                                    rightbarCount = 0;
                                    rightelist.Clear();
                                    rightMark = false;
                                    rightMark1 = false;
                                    rightbarCount1 = 0;
                                    beforeRightList.Clear();
                                }
                            }
                        }
                        else
                        {
                            saveMark1 = false;
                        }
                    }



                    remark = true;
                }
                catch (Exception exc)
                {
                    log.Error("------PLC访问出错------");
                    log.Error(exc.Message);
                    dispatcherTimer.Stop();
                    remark = false;
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
            if (mark == 1)
            {
                list.ForEach(f =>
                {
                    left.Add(new GongXuModel()
                    {
                        Status = IFalse,
                        Name = f.Name,
                        sType = f.Type
                    });
                });

                listViewL.ItemsSource = left;
            }
            else if (mark == 2)
            {
                list.ForEach(f =>
                {
                    right.Add(new GongXuModel()
                    {
                        Status = IFalse,
                        Name = f.Name,
                        sType = f.Type
                    });
                });

                listViewR.ItemsSource = right;
            }

            #region who care
            //if (mark == 1)
            //{
            //    switch (count)
            //    {
            //        case 1:
            //            Step1.Text = list[0].Name;
            //            StepImage1.Source = IFalse;
            //            break;
            //        case 2:
            //            Step1.Text = list[0].Name;
            //            Step2.Text = list[1].Name;
            //            StepImage1.Source = IFalse;
            //            StepImage2.Source = IFalse;
            //            break;
            //        case 3:
            //            Step1.Text = list[0].Name;
            //            Step2.Text = list[1].Name;
            //            Step3.Text = list[2].Name;
            //            StepImage1.Source = IFalse;
            //            StepImage2.Source = IFalse;
            //            StepImage3.Source = IFalse;
            //            break;
            //        case 4:
            //            Step1.Text = list[0].Name;
            //            Step2.Text = list[1].Name;
            //            Step3.Text = list[2].Name;
            //            Step4.Text = list[3].Name;
            //            StepImage1.Source = IFalse;
            //            StepImage2.Source = IFalse;
            //            StepImage3.Source = IFalse;
            //            StepImage4.Source = IFalse;
            //            break;
            //        case 5:
            //            Step1.Text = list[0].Name;
            //            Step2.Text = list[1].Name;
            //            Step3.Text = list[2].Name;
            //            Step4.Text = list[3].Name;
            //            Step5.Text = list[4].Name;
            //            StepImage1.Source = IFalse;
            //            StepImage2.Source = IFalse;
            //            StepImage3.Source = IFalse;
            //            StepImage4.Source = IFalse;
            //            StepImage5.Source = IFalse;
            //            break;
            //        case 6:
            //            Step1.Text = list[0].Name;
            //            Step2.Text = list[1].Name;
            //            Step3.Text = list[2].Name;
            //            Step4.Text = list[3].Name;
            //            Step5.Text = list[4].Name;
            //            Step6.Text = list[5].Name;
            //            StepImage1.Source = IFalse;
            //            StepImage2.Source = IFalse;
            //            StepImage3.Source = IFalse;
            //            StepImage4.Source = IFalse;
            //            StepImage5.Source = IFalse;
            //            StepImage6.Source = IFalse;
            //            break;
            //        case 7:
            //            Step1.Text = list[0].Name;
            //            Step2.Text = list[1].Name;
            //            Step3.Text = list[2].Name;
            //            Step4.Text = list[3].Name;
            //            Step5.Text = list[4].Name;
            //            Step6.Text = list[5].Name;
            //            Step7.Text = list[6].Name;
            //            StepImage1.Source = IFalse;
            //            StepImage2.Source = IFalse;
            //            StepImage3.Source = IFalse;
            //            StepImage4.Source = IFalse;
            //            StepImage5.Source = IFalse;
            //            StepImage6.Source = IFalse;
            //            StepImage7.Source = IFalse;
            //            break;

            //    }
            //}
            //else if (mark == 2)
            //{
            //    switch (count)
            //    {
            //        case 1:
            //            Step21.Text = list[0].Name;
            //            StepImage21.Source = IFalse;
            //            break;
            //        case 2:
            //            Step21.Text = list[0].Name;
            //            Step22.Text = list[1].Name;
            //            StepImage21.Source = IFalse;
            //            StepImage22.Source = IFalse;
            //            break;
            //        case 3:
            //            Step21.Text = list[0].Name;
            //            Step22.Text = list[1].Name;
            //            Step23.Text = list[2].Name;
            //            StepImage21.Source = IFalse;
            //            StepImage22.Source = IFalse;
            //            StepImage23.Source = IFalse;
            //            break;
            //        case 4:
            //            Step21.Text = list[0].Name;
            //            Step22.Text = list[1].Name;
            //            Step23.Text = list[2].Name;
            //            Step24.Text = list[3].Name;
            //            StepImage21.Source = IFalse;
            //            StepImage22.Source = IFalse;
            //            StepImage23.Source = IFalse;
            //            StepImage24.Source = IFalse;
            //            break;
            //        case 5:
            //            Step21.Text = list[0].Name;
            //            Step22.Text = list[1].Name;
            //            Step23.Text = list[2].Name;
            //            Step24.Text = list[3].Name;
            //            Step25.Text = list[4].Name;
            //            StepImage21.Source = IFalse;
            //            StepImage22.Source = IFalse;
            //            StepImage23.Source = IFalse;
            //            StepImage24.Source = IFalse;
            //            StepImage25.Source = IFalse;
            //            break;
            //        case 6:
            //            Step21.Text = list[0].Name;
            //            Step22.Text = list[1].Name;
            //            Step23.Text = list[2].Name;
            //            Step24.Text = list[3].Name;
            //            Step25.Text = list[4].Name;
            //            Step26.Text = list[5].Name;
            //            StepImage21.Source = IFalse;
            //            StepImage22.Source = IFalse;
            //            StepImage23.Source = IFalse;
            //            StepImage24.Source = IFalse;
            //            StepImage25.Source = IFalse;
            //            StepImage26.Source = IFalse;
            //            break;
            //        case 7:
            //            Step21.Text = list[0].Name;
            //            Step22.Text = list[1].Name;
            //            Step23.Text = list[2].Name;
            //            Step24.Text = list[3].Name;
            //            Step25.Text = list[4].Name;
            //            Step26.Text = list[5].Name;
            //            Step27.Text = list[6].Name;
            //            StepImage21.Source = IFalse;
            //            StepImage22.Source = IFalse;
            //            StepImage23.Source = IFalse;
            //            StepImage24.Source = IFalse;
            //            StepImage25.Source = IFalse;
            //            StepImage26.Source = IFalse;
            //            StepImage27.Source = IFalse;
            //            break;

            //    }
            //}
            #endregion
        }

        private void ModifyStep1(int type, int GWNo, int mark)
        {
            if (mark == 0) //0左边
            {
                switch (GWNo)
                {
                    case 04062: //工位
                        if (type == 0)
                        {
                            left.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                            });
                            ClearInfo();
                        }
                        else if (type == 100 || type == 110)
                        {
                            left.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                                if (f.sType == 0)
                                    f.Status = ITrue;
                                if (f.sType == 100)
                                    f.Status = ITrue;
                            });
                        }
                        else
                        {
                            left.ForEach(f =>
                            {
                                if (type >= 1200 && type <= 1290 && f.sType == 1200)
                                {
                                    f.Status = ITrue;
                                }
                                else if (type >= 1400 && type <= 1490 && f.sType == 1400)
                                {
                                    f.Status = ITrue;
                                }
                                else if (type >= 1600 && type <= 1690 && f.sType == 1600)
                                {
                                    f.Status = ITrue;
                                }
                                else if (type >= 1800 && type <= 1890 && f.sType == 1800)
                                {
                                    f.Status = ITrue;
                                }
                                else
                                {
                                    f.Status = f.Status == IFalse ? (f.sType == type ? ITrue : IFalse) : ITrue;
                                }

                            });
                        }

                        left.ForEach(f =>
                        {
                            if (f.sType <= type)
                            {
                                f.Status = ITrue;
                            }
                            else
                            {
                                f.Status = IFalse;
                            }
                        });
                        if (type == 2100)
                        {
                            ClearInfo();
                        }
                        //switch (type)
                        //{
                        //    case 100:
                        //        StepImage1.Source = IFalse;
                        //        StepImage2.Source = IFalse;
                        //        StepImage3.Source = IFalse;
                        //        StepImage4.Source = IFalse;
                        //        StepImage5.Source = IFalse;
                        //        StepImage6.Source = IFalse;
                        //        StepImage7.Source = IFalse;
                        //        break;
                        //    case 150:
                        //        StepImage1.Source = ITrue;
                        //        break;
                        //    case 500:
                        //        StepImage2.Source = ITrue;
                        //        break;
                        //    case 700:
                        //        StepImage3.Source = ITrue;
                        //        break;
                        //    case 1000:
                        //        StepImage4.Source = ITrue;
                        //        break;
                        //    case 1900:
                        //        StepImage5.Source = ITrue;
                        //        break;
                        //    case 2100:
                        //        StepImage6.Source = ITrue;
                        //        break;
                        //    case 2300:
                        //        StepImage7.Source = ITrue;
                        //        break;
                        //}
                        break;
                    case 04051: //工位
                        if (type == 0)
                        {
                            left.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                            });
                            ClearInfo();
                        }
                        else if (type == 100 || type == 110)
                        {
                            left.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                                if (f.sType == 0)
                                    f.Status = ITrue;
                                if (f.sType == 100)
                                    f.Status = ITrue;
                            });
                        }
                        else
                        {
                            left.ForEach(f =>
                            {
                                f.Status = f.Status == IFalse ? (f.sType == type ? ITrue : IFalse) : ITrue;

                            });
                        }

                        left.ForEach(f =>
                        {
                            if (f.sType <= type)
                            {
                                f.Status = ITrue;
                            }
                            else
                            {
                                f.Status = IFalse;
                            }
                        });
                        if (type == 900)
                        {
                            ClearInfo();
                        }
                        //switch (type)
                        //{
                        //    case 150:
                        //        StepImage1.Source = IFalse;
                        //        StepImage2.Source = IFalse;
                        //        StepImage3.Source = IFalse;
                        //        StepImage4.Source = IFalse;
                        //        StepImage5.Source = IFalse;
                        //        StepImage6.Source = IFalse;
                        //        StepImage7.Source = IFalse;
                        //        break;

                        //    case 200:
                        //        StepImage2.Source = ITrue;
                        //        break;
                        //    case 400:
                        //        StepImage3.Source = ITrue;
                        //        break;
                        //    case 550:
                        //        StepImage4.Source = ITrue;
                        //        break;
                        //    case 600:
                        //        StepImage5.Source = ITrue;
                        //        break;
                        //    case 800:
                        //        StepImage6.Source = ITrue;
                        //        break;
                        //    case 900:
                        //        StepImage7.Source = ITrue;
                        //        break;
                        //}
                        break;
                }
            }
            else if (mark == 1) //1 右边
            {
                switch (GWNo)
                {
                    case 04062: //工位
                        if (type == 0)
                        {
                            right.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                            });
                            ClearInforight();
                        }
                        else if (type == 100 || type == 110)
                        {
                            right.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                                if (f.sType == 0)
                                    f.Status = ITrue;
                                if (f.sType == 100)
                                    f.Status = ITrue;
                            });
                        }
                        else
                        {
                            right.ForEach(f =>
                            {
                                if (type >= 1200 && type <= 1290 && f.sType == 1200)
                                {
                                    f.Status = ITrue;
                                }
                                else if (type >= 1400 && type <= 1490 && f.sType == 1400)
                                {
                                    f.Status = ITrue;
                                }
                                else if (type >= 1600 && type <= 1690 && f.sType == 1600)
                                {
                                    f.Status = ITrue;
                                }
                                else if (type >= 1800 && type <= 1890 && f.sType == 1800)
                                {
                                    f.Status = ITrue;
                                }
                                else
                                {
                                    f.Status = f.Status == IFalse ? (f.sType == type ? ITrue : IFalse) : ITrue;
                                }

                            });
                        }

                        right.ForEach(f =>
                        {
                            if (f.sType <= type)
                            {
                                f.Status = ITrue;
                            }
                            else
                            {
                                f.Status = IFalse;
                            }
                        });
                        if (type == 2100)
                        {
                            ClearInforight();
                        }
                        //switch (type)
                        //{
                        //    case 100:
                        //        StepImage21.Source = IFalse;
                        //        StepImage22.Source = IFalse;
                        //        StepImage23.Source = IFalse;
                        //        StepImage24.Source = IFalse;
                        //        StepImage25.Source = IFalse;
                        //        StepImage26.Source = IFalse;
                        //        StepImage27.Source = IFalse;
                        //        break;
                        //    case 150:
                        //        StepImage21.Source = ITrue;
                        //        break;
                        //    case 500:
                        //        StepImage22.Source = ITrue;
                        //        break;
                        //    case 700:
                        //        StepImage23.Source = ITrue;
                        //        break;
                        //    case 1000:
                        //        StepImage24.Source = ITrue;
                        //        break;
                        //    case 1900:
                        //        StepImage25.Source = ITrue;
                        //        break;
                        //    case 2100:
                        //        StepImage26.Source = ITrue;
                        //        break;
                        //    case 2300:
                        //        StepImage27.Source = ITrue;
                        //        break;
                        //}
                        break;
                    case 04051: //工位
                        if (type == 0)
                        {
                            right.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                            });
                            ClearInforight();
                        }
                        else if (type == 100 || type == 110)
                        {
                            right.ForEach(f =>
                            {
                                f.Status = (f.sType == type ? ITrue : IFalse);
                                if (f.sType == 0)
                                    f.Status = ITrue;
                                if (f.sType == 100)
                                    f.Status = ITrue;
                            });
                        }
                        else
                        {
                            right.ForEach(f =>
                            {
                                if (type >= 600 && type <= 690 && f.sType == 600)
                                {
                                    f.Status = ITrue;
                                }
                                else if (type >= 800 && type <= 899 && f.sType == 800)
                                {
                                    f.Status = ITrue;
                                }
                                else
                                {
                                    f.Status = f.Status == IFalse ? (f.sType == type ? ITrue : IFalse) : ITrue;
                                }

                            });
                        }

                        right.ForEach(f =>
                        {
                            if (f.sType <= type)
                            {
                                f.Status = ITrue;
                            }
                            else
                            {
                                f.Status = IFalse;
                            }
                        });

                        if (type == 1100)
                        {
                            ClearInforight();
                        }
                        //switch (type)
                        //{
                        //    case 100:
                        //        StepImage21.Source = ITrue;
                        //        StepImage22.Source = IFalse;
                        //        StepImage23.Source = IFalse;
                        //        StepImage24.Source = IFalse;
                        //        StepImage25.Source = IFalse;
                        //        StepImage26.Source = IFalse;
                        //        StepImage27.Source = IFalse;
                        //        break;
                        //    case 200:
                        //        StepImage22.Source = ITrue;
                        //        break;
                        //    case 400:
                        //        StepImage23.Source = ITrue;
                        //        break;
                        //    case 700:
                        //        StepImage24.Source = ITrue;
                        //        break;
                        //    case 900:
                        //        StepImage25.Source = ITrue;
                        //        break;
                        //    case 1000:
                        //        StepImage26.Source = ITrue;
                        //        break;
                        //    case 1100:
                        //        StepImage27.Source = ITrue;
                        //        break;
                        //}
                        break;
                }
            }

            listViewL.ItemsSource = null;
            listViewL.ItemsSource = left;
            listViewL.Items.Refresh();
            listViewR.ItemsSource = null;
            listViewR.ItemsSource = right;
            listViewR.Items.Refresh();
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

        private void ReadErrorInfo()
        {
            while (true)
            {
                Thread.Sleep(500);
                bool errMark = false;
                foreach (var err in config.InfoDatas)
                {
                    var e1 = splc.ReadBool(err.Address);
                    if (e1.IsSuccess && e1.Content)
                    {
                        errMark = true;
                        Dispatcher.InvokeAsync(() =>
                        {
                            ErrorInfo.Text = err.ErrorInfo;
                        });
                        break;
                    }
                    //else
                    //{
                    //    Dispatcher.InvokeAsync(() =>
                    //    {
                    //        ErrorInfo.Text = "";
                    //    });
                    //}
                }
                if (!errMark)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        ErrorInfo.Text = "";
                    });
                }
                //foreach (var err in config.InfoDataRight)
                //{
                //    var e1 = splc.ReadBool(err.Address);
                //    if (e1.IsSuccess && e1.Content)
                //    {
                //        Dispatcher.InvokeAsync(() =>
                //        {
                //            ErrorInfoRight.Text = err.ErrorInfo;
                //        });
                //        break;
                //    }
                //}
            }
        }

        public void ThreadCheck(object sender, EventArgs e)
        {
            var check = splc.ReadUInt16("DB2000.0");
            if (check.IsSuccess)
            {
                PLCImage.Source = ITrue;
                //log.Info("PLC Connected!");

                if (!remark)
                {
                    DataReload();
                    //起个thread 报警信息
                    if (thread == null || !thread.IsAlive)
                    {
                        thread = new Thread(new ThreadStart(ReadErrorInfo));
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
            }
            else
            {
                PLCImage.Source = IFalse;
                //log.Info("PLC Not Connected!");
            }
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {

            ConfigWindow w = new ConfigWindow(config);
            w.productHandler += new ConfigWindow.ProductHandler(ChildWin_Form);
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.ShowDialog();
            w.Activate();
        }

        private void InitGw()
        {
            Gwlist = dal.QueryItem();
            string process = "";
            string processRight = "";
            int xh = 0;
            int xh1 = 0;
            switch (config.GWNo)
            {
                case 04051:
                    process = "上部框架预装";
                    processRight = "前管装配";
                    xh = 1;
                    xh1 = 1;
                    break;
                case 04062:
                    ChangeMenuItem.Visibility = Visibility.Hidden;
                    process = "H型滑轨装配";
                    processRight = "H型滑轨装配";
                    xh = 2;
                    xh1 = 1;
                    break;

            }
            ProductConfig productConfig = Gwlist.Find(f => f.FGWItem.Equals(process) && f.FXingHao == xh);
            ProductConfig productConfig1 = Gwlist.Find(f => f.FGWItem.Equals(processRight) && f.FXingHao == xh1);

            if (productConfig != null && productConfig1 != null)
            {
                Start(productConfig, productConfig1);
            }

        }

        private void Start(ProductConfig pro, ProductConfig proRight)
        {
            yzList.Clear();
            rightyzList.Clear();
            this.product = pro;
            this.rightproduct = proRight;
            ZongType.Text = pro.FZCType;
            ZongType_Right.Text = proRight.FZCType;
            switch (pro.FXingHao)
            {
                case 1:
                    XingHao1.Text = "正驾";
                    break;
                case 2:
                    XingHao1.Text = "副驾";
                    break;
            }
            switch (proRight.FXingHao)
            {
                case 1:
                    XingHao2.Text = "正驾";
                    break;
                case 2:
                    XingHao2.Text = "副驾";
                    break;
            }

            BarRule.Text = "左侧：";
            BarRule.Text += "\r\n电机:" + pro.FDianJiCodeRule;
            if (pro.FDianJiCodeRule != string.Empty)
                yzList.Add(pro.FDianJiCodeRule);
            if (pro.FStatus1 == 1)
            {
                BarRule.Text += "\r\n前管:" + pro.FQianGuanCodeRule;
                if (pro.FQianGuanCodeRule != string.Empty)
                    yzList.Add(pro.FQianGuanCodeRule);
            }
            if (pro.FStatus2 == 1)
            {
                BarRule.Text += "\r\nL型:" + pro.FLXingCodeRule;
                if (pro.FLXingCodeRule != string.Empty)
                    yzList.Add(pro.FLXingCodeRule);
            }
            if (pro.FStatus3 == 1)
            {
                BarRule.Text += "\r\n侧板:" + pro.FCeBanCodeRule;
                if (pro.FCeBanCodeRule != string.Empty)
                    yzList.Add(pro.FCeBanCodeRule);
            }

            BarRule_Right.Text = "右侧：";
            BarRule_Right.Text += "\r\n电机:" + proRight.FDianJiCodeRule;
            if (proRight.FDianJiCodeRule != string.Empty)
                rightyzList.Add(proRight.FDianJiCodeRule);
            if (proRight.FStatus1 == 1)
            {
                BarRule_Right.Text += "\r\n前管:" + proRight.FQianGuanCodeRule;
                if (proRight.FQianGuanCodeRule != string.Empty)
                    rightyzList.Add(proRight.FQianGuanCodeRule);
            }
            if (proRight.FStatus2 == 1)
            {
                BarRule_Right.Text += "\r\nL型:" + proRight.FLXingCodeRule;
                if (proRight.FLXingCodeRule != string.Empty)
                    rightyzList.Add(proRight.FLXingCodeRule);
            }
            if (proRight.FStatus3 == 1)
            {
                BarRule_Right.Text += "\r\n侧板:" + proRight.FCeBanCodeRule;
                if (proRight.FCeBanCodeRule != string.Empty)
                    rightyzList.Add(proRight.FCeBanCodeRule);
            }

            if (dispatcherTimer.IsEnabled)
                dispatcherTimer.Stop();
            remark = false;

            // write plc data
            string addressleft = "";
            string addressright = "";
            switch (config.GWNo)
            {
                case 04051:
                    addressleft = "DB2000.4270";
                    addressright = "DB2000.4270";
                    break;
                case 04062:
                    addressleft = "DB2000.4272";
                    addressright = "DB2000.4274";
                    break;
            }
            splc.Write(addressleft, (short)pro.FXingHao);
            splc.Write(addressright, (short)proRight.FXingHao);

        }

        private void ChildWin_Form(object sender, ProductConfig pro, ProductConfig proRight)
        {
            Start(pro, proRight);
        }

        private bool PortConnection()
        {
            bool mark = false;
            if (serialPort == null)
            {
                serialPort = new SerialPort(config.PortName, config.BaudRate, Parity.None, 8, StopBits.One);
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                serialPort.ReadTimeout = 100;
                serialPort.DataReceived += serialPort_DataReceived;
                mark = OpenPort();
            }
            return mark;
        }

        private bool PortConnectionLeft()
        {
            bool mark = false;
            if (serialPortLeft == null)
            {
                serialPortLeft = new SerialPort(config.PortName, config.BaudRate, Parity.None, 8, StopBits.One);
                serialPortLeft.DtrEnable = true;
                serialPortLeft.RtsEnable = true;
                serialPortLeft.ReadTimeout = 100;
                serialPortLeft.DataReceived += serialPort_DataReceivedLeft;
                mark = OpenPortLeft();
            }
            return mark;
        }

        private bool PortConnectionRight()
        {
            bool mark = false;
            if (serialPortRight == null)
            {
                serialPortRight = new SerialPort(config.PortName1, config.BaudRate1, Parity.None, 8, StopBits.One);
                serialPortRight.DtrEnable = true;
                serialPortRight.RtsEnable = true;
                serialPortRight.ReadTimeout = 100;
                serialPortRight.DataReceived += serialPort_DataReceivedRight;
                mark = OpenPortRight();
            }
            return mark;
        }



        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                //开启接收数据线程
                Thread threadReceiveSub = new Thread(new ParameterizedThreadStart(ReceiveData));
                threadReceiveSub.Start(serialPort);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void ReceiveData(object serialPortobj)
        {
            try
            {
                SerialPort serialPort = (SerialPort)serialPortobj;

                //防止数据接收不完整 线程sleep(100)
                Thread.Sleep(100);

                string str = serialPort.ReadExisting();

                if (str == string.Empty)
                {
                    return;
                }
                else
                {
                    BarCodeMatch(str);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void serialPort_DataReceivedRight(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                //开启接收数据线程
                Thread threadReceiveSub = new Thread(new ParameterizedThreadStart(ReceiveDataRight));
                threadReceiveSub.Start(serialPort);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void ReceiveDataRight(object serialPortobj)
        {
            try
            {
                SerialPort serialPort = (SerialPort)serialPortobj;

                //防止数据接收不完整 线程sleep(100)
                Thread.Sleep(100);

                string str = serialPort.ReadExisting();

                if (str == string.Empty)
                {
                    return;
                }
                else
                {
                    BarCodeMatchRight(str);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void serialPort_DataReceivedLeft(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                //开启接收数据线程
                Thread threadReceiveSub = new Thread(new ParameterizedThreadStart(ReceiveDataLeft));
                threadReceiveSub.Start(serialPort);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void ReceiveDataLeft(object serialPortobj)
        {
            try
            {
                SerialPort serialPort = (SerialPort)serialPortobj;

                //防止数据接收不完整 线程sleep(100)
                Thread.Sleep(100);

                string str = serialPort.ReadExisting();

                if (str == string.Empty)
                {
                    return;
                }
                else
                {
                    BarCodeMatchLeft(str);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        /// <summary>
        /// 04051 左侧 上部框架预装 
        /// </summary>
        /// <param name="barcode"></param>
        private void BarCodeMatchLeft(string barcode)
        {
            bool fcmark = false;
            bool lx = false;
            bool cb = false;
            bool last = false;

            if (!leftMark)
            {
                if (barcode.Contains(product.FLXingCodeRule))
                {
                    if (string.IsNullOrEmpty(LXCode))
                    {
                        barCount += 1;
                    }
                    LXCode = barcode;
                    fcmark = true;
                    lx = true;
                }
                else if (barcode.Contains(product.FCeBanCodeRule))
                {
                    if (string.IsNullOrEmpty(CBCode))
                    {
                        barCount += 1;
                    }
                    CBCode = barcode;
                    fcmark = true;
                    cb = true;
                }
                else
                {
                    fcmark = false;
                }
            }
            //上工序 判断 
            FIntryID = dal.QueryBefore4051(barcode);
            if (FIntryID > 0 && !leftMark)
            {
                barCount += 1;
                fcmark = true;
                last = true;
            }

            Dispatcher.InvokeAsync(() =>
            {
                if (!leftMark)
                {
                    if (!lx && !cb && !last)
                    {
                        if (string.IsNullOrEmpty(LXCode))
                        {
                            Barcode1.Text = barcode;
                            Barcode1.Background = Brushes.Red;
                        }
                        else if (string.IsNullOrEmpty(CBCode))
                        {
                            Barcode2.Text = barcode;
                            Barcode2.Background = Brushes.Red;
                        }
                        else
                        {
                            Barcode3.Text = barcode;
                            Barcode3.Background = Brushes.Red;
                        }
                    }
                    else if (lx)
                    {
                        Barcode1.Text = barcode;
                        Barcode1.Background = Brushes.SteelBlue;
                    }
                    else if (cb)
                    {
                        Barcode2.Text = barcode;
                        Barcode2.Background = Brushes.SteelBlue;
                    }
                    else if (last)
                    {
                        Barcode3.Text = barcode;
                        Barcode3.Background = Brushes.SteelBlue;
                    }

                    if (string.IsNullOrEmpty(LXCode) && string.IsNullOrEmpty(CBCode))
                    {
                        LeftError.Text = "条码匹配失败！";
                    }
                    else if (!string.IsNullOrEmpty(LXCode) && string.IsNullOrEmpty(CBCode))
                    {
                        LeftError.Text = "侧板条码匹配失败！";
                    }
                    else if (string.IsNullOrEmpty(LXCode) && !string.IsNullOrEmpty(CBCode))
                    {
                        LeftError.Text = "L型条码匹配失败！";
                    }
                    else if (!last)
                    {
                        LeftError.Text = "该条码上工序无数据！";
                    }
                    else
                    {
                        LeftError.Text = "";
                    }


                    BarYz.Text = fcmark ? "OK" : "NG";
                }
            });

            if (barCount == product.FCodeSum && !leftMark)
            {
                // write plc ???
                splc.Write(service.GetSaoMaStr(config.GWNo, 0), 2);
                dal.UpdateBarCode40512(FIntryID, LXCode, CBCode);
                leftMark = true;
            }

        }

        /// <summary>
        /// 04062
        /// </summary>
        /// <param name="barcode"></param>
        private void BarCodeMatch(string barcode)
        {
            bool barmark = false;
            bool fcmark = false;
            bool fcrmark = false;

            //上工序 left
            FIntryID = dal.QueryBefore4062(barcode, product.FXingHao);
            if (FIntryID > 0 && !leftMark)
            {
                barCount += 1;
                fcmark = true;
                barmark = true;
            }
            //上工序 right
            FIntryIDRight = dal.QueryBefore4062(barcode, rightproduct.FXingHao);
            if (FIntryIDRight > 0 && !rightMark)
            {
                rightbarCount += 1;
                fcrmark = true;
            }

            Dispatcher.InvokeAsync(() =>
            {
                if (barmark)
                {
                    switch (barCount % 2)
                    {
                        case 0:
                            Barcode2.Text = barcode;
                            break;
                        case 1:
                            Barcode1.Text = barcode;
                            break;
                    }

                }
                else
                {

                    switch (rightbarCount % 2)
                    {
                        case 0:
                            Barcode4.Text = barcode;
                            break;
                        case 1:
                            Barcode3.Text = barcode;
                            break;
                    }
                }

                if (!fcmark && !fcrmark)
                {
                    BarYz.Text = "NG";
                }
                else
                {
                    BarYz.Text = "OK";
                }

            });

            // 
            if (barCount == product.FCodeSum && !leftMark)
            {
                // write plc 
                splc.Write(service.GetSaoMaStr(config.GWNo, 0), 2);
                dal.UpdateBarCode4062(FIntryID);
                leftMark = true;
            }
            if (rightbarCount == rightproduct.FCodeSum && !rightMark)
            {
                // write plc ???
                splc.Write(service.GetSaoMaStr(config.GWNo, 1), 2);
                dal.UpdateBarCode4062(FIntryIDRight);
                rightMark = true;
            }
        }

        /// <summary>
        /// 04051 右侧 前管装配 
        /// </summary>
        /// <param name="barcode"></param>
        private void BarCodeMatchRight(string barcode)
        {
            bool fcmark = false;
            bool dj = false;
            bool qg = false;

            if (!rightMark)
            {
                if (barcode.Contains(product.FDianJiCodeRule))
                {
                    if (string.IsNullOrEmpty(DJCode))
                    {
                        rightbarCount += 1;
                    }
                    DJCode = barcode;
                    fcmark = true;
                    dj = true;
                }
                else if (barcode.Contains(product.FQianGuanCodeRule))
                {
                    if (string.IsNullOrEmpty(QGCode))
                    {
                        rightbarCount += 1;
                    }
                    QGCode = barcode;
                    fcmark = true;
                    qg = true;
                }
                else
                {
                    fcmark = false;
                }
            }

            Dispatcher.InvokeAsync(() =>
            {
                if (!rightMark)
                {
                    if (!dj && !qg)
                    {
                        if (string.IsNullOrEmpty(DJCode))
                        {
                            Barcode4.Text = barcode;
                            Barcode4.Background = Brushes.Red;
                        }
                        else
                        {
                            Barcode5.Text = barcode;
                            Barcode5.Background = Brushes.Red;
                        }
                    }
                    else if (dj)
                    {
                        Barcode4.Text = barcode;
                        Barcode4.Background = Brushes.SteelBlue;
                    }
                    else if (qg)
                    {
                        Barcode5.Text = barcode;
                        Barcode5.Background = Brushes.SteelBlue;
                    }

                    if (string.IsNullOrEmpty(DJCode) && string.IsNullOrEmpty(QGCode))
                    {
                        RightError.Text = "条码匹配失败！";
                    }
                    else if (!string.IsNullOrEmpty(DJCode) && string.IsNullOrEmpty(QGCode))
                    {
                        RightError.Text = "前管条码匹配失败！";
                    }
                    else if (string.IsNullOrEmpty(DJCode) && !string.IsNullOrEmpty(QGCode))
                    {
                        RightError.Text = "电机条码匹配失败！";
                    }
                    else
                    {
                        RightError.Text = "";
                    }

                    BarYz_Copy.Text = fcmark ? "OK" : "NG";
                }
            });

            if (rightbarCount == rightproduct.FCodeSum && !rightMark)
            {
                // write plc ???
                splc.Write(service.GetSaoMaStr(config.GWNo, 1), 2);
                FIntryIDRight = dal.SaveBarCode405(rightproduct.FXingHao, DJCode, QGCode);
                rightMark = true;
            }


        }

        private bool OpenPort()
        {
            string message = null;
            try//这里写成异常处理的形式以免串口打不开程序崩溃
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (serialPort.IsOpen)
            {
                log.Info("连接成功！");
                return true;
            }
            else
            {
                log.Error("打开失败!原因为： " + message);
                return false;
            }
        }

        private bool OpenPortLeft()
        {
            string message = null;
            try//这里写成异常处理的形式以免串口打不开程序崩溃
            {
                serialPortLeft.Open();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (serialPortLeft.IsOpen)
            {
                log.Info("连接成功！");
                return true;
            }
            else
            {
                log.Error("打开失败!原因为： " + message);
                return false;
            }
        }

        private bool OpenPortRight()
        {
            string message = null;
            try//这里写成异常处理的形式以免串口打不开程序崩溃
            {
                serialPortRight.Open();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (serialPortRight.IsOpen)
            {
                log.Info("连接成功！");
                return true;
            }
            else
            {
                log.Error("打开失败!原因为： " + message);
                return false;
            }
        }

        private void PortClose()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort = null;
            }
        }

        private void PortClose2()
        {
            if (serialPortLeft != null && serialPortLeft.IsOpen)
            {
                serialPortLeft.Close();
                serialPortLeft = null;
            }
            if (serialPortRight != null && serialPortRight.IsOpen)
            {
                serialPortRight.Close();
                serialPortRight = null;
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (config.GWNo == 04051)
            {
                string process = "上部框架预装";
                string processRight = "前管装配";
                int value = 0;

                switch (product.FXingHao)
                {
                    case 1:
                        value = 2;
                        break;
                    case 2:
                        value = 1;
                        break;
                }

                ProductConfig productConfig = Gwlist.Find(f => f.FGWItem.Equals(process) && f.FXingHao == value);
                ProductConfig productConfig1 = Gwlist.Find(f => f.FGWItem.Equals(processRight) && f.FXingHao == value);

                if (productConfig != null && productConfig1 != null)
                {
                    Start(productConfig, productConfig1);
                }
            }
        }

        private void ClearInfo()
        {
            FIntryID = 0;
            Barcode1.Text = "";
            Barcode2.Text = "";
            Barcode3.Text = "";
            Barcode1.Background = Brushes.SteelBlue;
            Barcode2.Background = Brushes.SteelBlue;
            Barcode3.Background = Brushes.SteelBlue;
            BarYz.Text = "";
            barList.Clear();
            barCount = 0;
            elist.Clear();
            leftMark = false;
            beforeLeftList.Clear();
        }

        private void ClearInforight()
        {
            FIntryIDRight = 0;
            Barcode5.Text = "";
            Barcode4.Text = "";
            Barcode4.Background = Brushes.SteelBlue;
            Barcode5.Background = Brushes.SteelBlue;
            BarYz_Copy.Text = "";
            rightbarList.Clear();
            rightbarCount = 0;
            rightelist.Clear();
            rightMark = false;
            beforeRightList.Clear();
        }
    }
}
