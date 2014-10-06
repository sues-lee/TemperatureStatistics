using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Temperature
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }
        string pathIn = @"D:\z.txt";
        string pathOut = @"D:\x.txt";
        readonly string pointCode = "58345";    //气象站号
        readonly string noUse1 = "32766";       //两个无数据标识
        readonly string noUse2 = "32744";
        readonly string pattern = @"\s+";                 //匹配空格
        readonly string patternYear = @"[1-2][0-9]{3}";                 //匹配年份
        Regex regYear;
        DataTable dtResult;
        DataRow drTemp ;

        private void Form1_Load(object sender, EventArgs e)
        {
            //初始化datatable
            ClearData();

            regYear = new Regex(patternYear);     //初始化年份匹配
            //初始化openFileDialog
            openFileDialog1.Filter = "txt文件(*.txt)|*.txt";
            saveFileDialog1.Filter = "txt文件(*.txt)|*.txt";
        }

        private void ClearData()
        {
            dtResult = new DataTable();
            dtResult.Columns.Add("year");
            dtResult.Columns.Add("month");
            dtResult.Columns.Add("temp");
            drTemp = dtResult.NewRow();
        }


        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            pathIn = openFileDialog1.FileName;
            FileStream fs = new FileStream(pathIn, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            
            double tempTempeature = 0;
            int tempDay = 0;  //除去无效温度剩下的天数，用来求平均温度
            int i=0;        //记录当前行数
           
            string info; 
            Regex regex = new Regex(pattern);
            try
            {
                while ((info = sr.ReadLine()) != null)
                {
                    string[] tempStr = regex.Split(info);
                    int t = 0;          //用来找当前月份
                    foreach (var item in tempStr)
                    {
                        if (item != "")     //提取每条数据
                        {
                            if (t == 2)
                            {
                                //月份的index为2
                                drTemp["month"] = item;
                            }
                            else
                                if (item == pointCode)
                                {
                                    continue;   //查到气象站号时跳过
                                }
                                else if (item == noUse1 || item == noUse2)
                                {
                                    //无数据时
                                }
                                else if (regYear.IsMatch(item))
                                {
                                    //判断是否为年份
                                    drTemp["year"] = item;
                                }
                                else
                                {
                                    //剩余情况都为有效温度数据
                                    tempDay++;
                                    tempTempeature += Convert.ToDouble(item) * 0.1;
                                }
                        }
                        t++;
                    }
                    tempTempeature = tempTempeature / tempDay;
                    drTemp["temp"] = tempTempeature;  //保存平均气温
                    dtResult.Rows.Add(drTemp);
                    i++;
                    tempDay = 0;
                    tempTempeature = 0;
                    drTemp = dtResult.NewRow();
                }
            }
            catch (Exception)
            {

                MessageBox.Show("格式错误！");

                return;
            }
            finally
            {
                dataGridView1.DataSource = dtResult;
                sr.Close();     //关闭连接
               // ClearData();    //清空数据
                drTemp = dtResult.NewRow();
            }
            label1.Text = "当前路径：" + pathIn;
            //dataGridView1.DataSource = dtResult;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null)
            {
                MessageBox.Show("请导入文件后再输出！");
                return;
            }
            if (saveFileDialog1.ShowDialog()!= System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            pathOut = saveFileDialog1.FileName;
            StringBuilder result = new StringBuilder();
            result.Append("=================统计结果==================\r\n");
            foreach (DataRow item in dtResult.Rows)
	            {
                     for (int i = 0; i < item.ItemArray.Length; i++)  
                        {  
                            result.Append(item[i].ToString() + "\t");  
                        }
                     result.Append("\r\n"); 
	            }
            WriteFile(result.ToString(),pathOut);
        }

        /// <summary>
        /// 将字符串写到指定路径
        /// </summary>
        /// <param name="result">要输出的字符串</param>
        /// <param name="path">文件路径</param>
        private void WriteFile(String result,string path)
        {
            try
            {
                using(FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                { 
                    byte[] s = System.Text.Encoding.UTF8.GetBytes(result.ToString());
                    fs.Write(s, 0, s.Length);
                    System.Diagnostics.Process.Start("notepad", path);
                 }
            }
            catch (Exception)
            {

                MessageBox.Show("输出路径错误！");
                return;
            }
            
        }

       
    }
}
