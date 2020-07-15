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

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _message = "服务器连接异常！";
        private static double mark = 233.33;
        private GetInfoService service = new GetInfoService();
        private DispatcherTimer ShowTimer;
        private ConfigData config;
        private BarCodeStr codeStr;
        private Plc plc;
        private GDbStr GunStr;
        private int markN = 0;
        private List<GDbData> ReList = new List<GDbData>();
        //private static BitmapImage IStation = new BitmapImage(new Uri("C:\\Users\\Administrator\\Desktop\\cs.png", UriKind.Absolute));  //"C:\\Users\\Administrator\\Desktop\\cs.png", UriKind.Absolute
        private static BitmapImage ILogo = new BitmapImage(new Uri("/Images/logo.png", UriKind.Relative));
        private static BitmapImage IFalse = new BitmapImage(new Uri("/Images/01.png", UriKind.Relative));
        private static BitmapImage ITrue = new BitmapImage(new Uri("/Images/02.png", UriKind.Relative));

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


            //读取本地配置JSON文件
            LoadJsonData();
            Init();
            MainPageLoad();
            plc = new Plc(CpuType.S71200, config.IpAdress, 0, 1);

            var result = plc.Open();
            if (!plc.IsConnected)
            {
                PLCImage.Source = IFalse;
            }
            else
            {
                PLCImage.Source = ITrue;
                DataReload();
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
            ZongType.Text = config.ProductType;
            BarRule.Text = config.BarRule;
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
                //读取PLC工序步骤状态
                //左
                var sta = ((uint)plc.Read(service.GetStaStr(config.Station1No))).ConvertToInt();
                ModifyStep1(sta, config.GWNo, 0);
                //右
                var sta1 = ((uint)plc.Read(service.GetStaStr(config.Station2No))).ConvertToInt();
                ModifyStep1(sta, config.GWNo, 1);

                //型号获取
                var type = ((uint)plc.Read(service.GetTypeStr(config.ProductNo))).ConvertToInt();
                switch (type)
                {
                    case 1:
                        XingHao.Text = "正驾";
                        break;
                    case 2:
                        XingHao.Text = "副驾";
                        break;
                    default: break;
                }

                //BarCode Get
                codeStr = service.GetBarCodeStr(config.BarNo);
                var temp = (string)plc.Read(DataType.DataBlock, 2000, codeStr.BarStr, VarType.String, 40);
                var BarResult = (bool)plc.Read(codeStr.ResultStr);
                //var cMark = (string)plc.Read(DataType.DataBlock, 2000, codeStr.BarStr, VarType.String, 5); //mark
                if (!temp.IsNullOrEmpty())
                {
                    Barcode.Text = temp;
                    if (BarResult)
                    {
                        BarYz.Text = "比对成功";
                    }
                    else
                    {
                        BarYz.Text = "比对失败";
                    }
                }

                #region 拧紧枪数据获取
                GunStr = service.GetGunStr(config.GunNo);
                var torque = ((uint)plc.Read(GunStr.TorqueStr)).ConvertToDouble();
                var angle = ((uint)plc.Read(GunStr.AngleStr)).ConvertToDouble();
                var result = (bool)plc.Read(GunStr.ResultStr);

                if (torque != mark && torque != 0) //做标记
                {
                    if (markN == 50)
                    {
                        ReList.Clear();
                        markN = 0;
                    }
                    markN += 1;
                    ReList.Add(new GDbData(markN, torque, angle, result.ToString()));
                    ReList.Sort((x, y) => -x.Num.CompareTo(y.Num));
                    DataList.ItemsSource = null;
                    DataList.ItemsSource = ReList;
                    DataList.Items.Refresh();
                    //mark
                    plc.Write(GunStr.TorqueStr, mark.ConvertToUInt());
                }
                #endregion


            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
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
            using (var sr = File.OpenText("C:\\config\\config1.json"))
            {
                string JsonStr = sr.ReadToEnd();
                config = JsonConvert.DeserializeObject<ConfigData>(JsonStr);
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
                            case 400:
                                StepImage2.Source = ITrue;
                                break;
                            case 600:
                                StepImage3.Source = ITrue;
                                break;
                            case 800:
                                StepImage4.Source = ITrue;
                                break;
                            case 970:
                                StepImage5.Source = ITrue;
                                break;
                            case 1400:
                                StepImage6.Source = ITrue;
                                break;
                            case 1600:
                                StepImage7.Source = ITrue;
                                break;
                        }
                        break;
                    case 04051: //工位
                        switch (type)
                        {
                            case 100:
                                StepImage1.Source = IFalse;
                                StepImage2.Source = IFalse;
                                StepImage3.Source = IFalse;
                                StepImage4.Source = IFalse;
                                StepImage5.Source = IFalse;
                                break;
                            case 300:
                                StepImage1.Source = ITrue;
                                break;
                            case 400:
                                StepImage2.Source = ITrue;
                                break;
                            case 500:
                                StepImage3.Source = ITrue;
                                break;
                            case 700:
                                StepImage4.Source = ITrue;
                                break;
                            case 800:
                                StepImage5.Source = ITrue;
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
                            case 400:
                                StepImage22.Source = ITrue;
                                break;
                            case 600:
                                StepImage23.Source = ITrue;
                                break;
                            case 800:
                                StepImage24.Source = ITrue;
                                break;
                            case 970:
                                StepImage25.Source = ITrue;
                                break;
                            case 1400:
                                StepImage26.Source = ITrue;
                                break;
                            case 1600:
                                StepImage27.Source = ITrue;
                                break;
                        }
                        break;
                    case 04051: //工位
                        switch (type)
                        {
                            case 100:
                                StepImage21.Source = IFalse;
                                StepImage22.Source = IFalse;
                                StepImage23.Source = IFalse;
                                StepImage24.Source = IFalse;
                                StepImage25.Source = IFalse;
                                break;
                            case 300:
                                StepImage21.Source = ITrue;
                                break;
                            case 500:
                                StepImage22.Source = ITrue;
                                break;
                            case 700:
                                StepImage23.Source = ITrue;
                                break;
                            case 900:
                                StepImage24.Source = ITrue;
                                break;
                            case 1000:
                                StepImage25.Source = ITrue;
                                break;
                        }
                        break;
                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBoxX.Show("是否要关闭？", "确认", Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
                if (plc.IsConnected)
                {
                    plc.Close();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
