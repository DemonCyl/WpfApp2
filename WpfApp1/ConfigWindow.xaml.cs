﻿using log4net;
using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.DAL;
using WpfApp1.Entity;

namespace WpfApp1
{

    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private ProductConfig info;
        private List<ProductConfig> list;
        private ProductConfig leftPro;
        private ProductConfig rightPro;
        private MainDAL dal;
        private ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int mark = 0;
        public delegate void ProductHandler(object sender, ProductConfig product, ProductConfig product1);
        public event ProductHandler productHandler;
        private string process = "";
        private string processRight = "";
        private ConfigData data;

        public ConfigWindow(ConfigData cdata)
        {
            this.data = cdata;
            InitializeComponent();
            dal = new MainDAL(data);

            switch (data.GWNo)
            {
                case 04051:
                    process = "上部框架预装";
                    processRight = "前管装配";
                    break;
                case 04062:
                    process = "H型滑轨装配";
                    processRight = "H型滑轨装配";
                    break;
            }

            GWItems.Items.Add("上部框架装配");
            GWItems.Items.Add("上部框架卡圈压装");
            GWItems.Items.Add("滑轨马达组件装配");
            GWItems.Items.Add("下横梁卡圈压装");
            GWItems.Items.Add("前管装配");
            GWItems.Items.Add("上部框架预装");
            GWItems.Items.Add("H型滑轨装配");
            GWItems.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            if (mark != 1 && mark != 2)
            {
                Info.Text = "无法保存！";
                return;
            }

            try
            {
                info = new ProductConfig();
                info.FZCType = NewZCItem.Text.Trim();
                info.FCodeRule = codeRule.Text.Trim();
                info.FCodeRule1 = codeRule1.Text.Trim();
                info.FStatus1 = code1.IsChecked.Value ? 1 : 0;
                info.FCodeRule2 = codeRule2.Text.Trim();
                info.FStatus2 = code2.IsChecked.Value ? 1 : 0;
                info.FCodeRule3 = codeRule3.Text.Trim();
                info.FStatus3 = code3.IsChecked.Value ? 1 : 0;
                //if (PLCItem.Text.Equals(""))
                //{
                //    Info.Text = "PLC通讯标识值不能为空!";
                //    return;
                //}
                //info.FPLC = int.Parse(PLCItem.Text.Trim());
                info.FGWItem = GWItems.SelectedItem.ToString();
                int lrType = 0;
                if (right.IsChecked.Value)
                    lrType = 1;
                if (left.IsChecked.Value)
                    lrType = 2;
                info.FXingHao = lrType;
                if (CodeSum.Text.Equals(""))
                {
                    Info.Text = "条码总数不能为空!";
                    return;
                }
                info.FCodeSum = int.Parse(CodeSum.Text.Trim());

                if (IdText.Text.Equals(""))
                {
                    result = dal.SaveItem(info);
                }
                else
                {
                    info.FInterID = int.Parse(IdText.Text.Trim());
                    result = dal.UpdateItem(info);
                }
                if (result)
                {
                    Query();
                    ZCItems.SelectedItem = info.FZCType;
                }
                Info.Text = result ? "保存成功!" : "保存失败!";
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show("确认删除？", "确认", Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    int id = 0;
                    if (IdText.Text == null || IdText.Text.Equals(""))
                    {
                        Info.Text = "未选择项目！";
                    }
                    else
                    {
                        id = int.Parse(IdText.Text.Trim());
                    }

                    if (dal.DeleteItem(id))
                    {
                        Info.Text = "删除成功！";
                        Query();
                    }
                    else
                    {
                        Info.Text = "删除失败！";
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
            }
        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void Query()
        {
            ZCItems.Visibility = Visibility.Visible;
            NewZCItem.Visibility = Visibility.Hidden;
            Default();

            try
            {
                list = dal.QueryItem();
                var itemlist = new List<string>();
                list.ForEach(f =>
                {
                    itemlist.Add(f.FZCType);
                });
                ZCItems.ItemsSource = itemlist;
                ZCItems.SelectedIndex = 0;

                mark = 2;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ZCItems.Visibility = Visibility.Hidden;
            NewZCItem.Visibility = Visibility.Visible;
            Default();
            mark = 1;
        }

        private void Default()
        {
            IdText.Text = "";
            ZCItems.ItemsSource = null;
            NewZCItem.Text = "";
            right.IsChecked = true;
            PLCItem.Text = "";
            CodeSum.Text = "0";
            codeRule.Text = "";
            codeRule1.Text = "";
            codeRule2.Text = "";
            codeRule3.Text = "";
            code1.IsChecked = false;
            code2.IsChecked = false;
            code3.IsChecked = false;
            Info.Text = "";
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)

        {

            Regex re = new Regex("[^0-9]");

            e.Handled = re.IsMatch(e.Text);

        }

        private void code1_Checked(object sender, RoutedEventArgs e)
        {
            if (code1.IsChecked.Value)
            {
                codeRule1.IsEnabled = true;
            }
            else
            {
                codeRule1.IsEnabled = false;
            }
        }

        private void code2_Checked(object sender, RoutedEventArgs e)
        {
            if (code2.IsChecked.Value)
            {
                codeRule2.IsEnabled = true;
            }
            else
            {
                codeRule2.IsEnabled = false;
            }
        }

        private void code3_Checked(object sender, RoutedEventArgs e)
        {
            if (code3.IsChecked.Value)
            {
                codeRule3.IsEnabled = true;
            }
            else
            {
                codeRule3.IsEnabled = false;
            }
        }

        private void ZCItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ZCItems.HasItems)
            {

                var item = ZCItems.SelectedItem.ToString();
                var pro = list.Find(f => f.FZCType == item);

                NewZCItem.Text = item;
                IdText.Text = pro.FInterID.ToString();
                switch (pro.FXingHao)
                {
                    case 1:
                        right.IsChecked = true;
                        break;
                    case 2:
                        left.IsChecked = true;
                        break;
                }
                //PLCItem.Text = pro.FPLC.ToString();
                CodeSum.Text = pro.FCodeSum.ToString();
                codeRule.Text = pro.FCodeRule;
                codeRule1.Text = pro.FCodeRule1;
                codeRule2.Text = pro.FCodeRule2;
                codeRule3.Text = pro.FCodeRule3;
                GWItems.SelectedItem = pro.FGWItem;
                switch (pro.FStatus1)
                {
                    case 0:
                        code1.IsChecked = false;
                        break;
                    case 1:
                        code1.IsChecked = true;
                        break;
                }

                switch (pro.FStatus2)
                {
                    case 0:
                        code2.IsChecked = false;
                        break;
                    case 1:
                        code2.IsChecked = true;
                        break;
                }

                switch (pro.FStatus3)
                {
                    case 0:
                        code3.IsChecked = false;
                        break;
                    case 1:
                        code3.IsChecked = true;
                        break;
                }
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (leftPro == null)
            {
                Info.Text = "请选择左侧产品！";
                return;
            }
            if (rightPro == null)
            {
                Info.Text = "请选择右侧产品！";
                return;
            }
            if (data.GWNo == 04051)
            {
                if (leftPro.FXingHao != rightPro.FXingHao)
                {
                    Info.Text = "此工位产品需为同一类型！";
                    return;
                }
            }

            productHandler(this, leftPro, rightPro);

            this.Close();
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            if (!ZCItems.HasItems)
            {
                Info.Text = "请选择要切换的产品！";
                return;
            }

            var item = ZCItems.SelectedItem.ToString();
            leftPro = list.Find(f => f.FZCType == item);
            if (!leftPro.FGWItem.Equals(process))
            {
                Info.Text = $"当前工位为{process},请选择正确的产品";
                return;
            }

            Info.Text = "左侧产品确认";
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (!ZCItems.HasItems)
            {
                Info.Text = "请选择要切换的产品！";
                return;
            }

            var item = ZCItems.SelectedItem.ToString();
            rightPro = list.Find(f => f.FZCType == item);
            if (!rightPro.FGWItem.Equals(processRight))
            {
                Info.Text = $"当前工位为{processRight},请选择正确的产品";
                return;
            }

            Info.Text = "右侧产品确认";
        }
    }
}
