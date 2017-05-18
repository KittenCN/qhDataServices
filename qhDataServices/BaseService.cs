using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;

namespace qhDataServices
{
    public partial class BaseService : ServiceBase
    {
        static System.Timers.Timer oTimer_Get = new System.Timers.Timer();
        public static string strLocalAdd = AppDomain.CurrentDomain.BaseDirectory + "Config.xml";
        public static string LinkString = "Server = 127.0.0.1;Database = SLEC_Carpark_DTXJQ;User ID = sa;Password = sa123;Trusted_Connection = False;";
        public static string RemoteInterface = "http://114.55.136.29:3000/admin/insertCar";
        public static int DBCacheRate = 1800;
        public static string BaseTable = "CP_InOutCar";
        public static Boolean boolRunFlag = false;
        public static string strIDRecord = AppDomain.CurrentDomain.BaseDirectory + "LastID.txt";
        public BaseService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SW("Service Start.");
            try
            {
                

                XmlDocument xmlCon = new XmlDocument();
                xmlCon.Load(strLocalAdd);
                XmlNode xnCon = xmlCon.SelectSingleNode("Config");
                LinkString = xnCon.SelectSingleNode("LinkString").InnerText;
                RemoteInterface = xnCon.SelectSingleNode("RemoteInterface").InnerText;
                DBCacheRate = int.Parse(xnCon.SelectSingleNode("DBCacheRate").InnerText);
                BaseTable = xnCon.SelectSingleNode("BaseTable").InnerText;
                int intDebugMode = int.Parse(xnCon.SelectSingleNode("DebugMode").InnerText);
                
                if(intDebugMode == 1)
                {
                    SW("Debug Mode!");
                    Thread.Sleep(30000);
                }                

                MainEvent();

                AutoLog = false;
                oTimer_Get.Enabled = true;
                oTimer_Get.Interval = DBCacheRate * 1000 * 60;      
                oTimer_Get.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                SW("MainEvent Success");
            }
            catch (Exception ex)
            {
                SW(ex.Source + "。" + ex.Message);
            }
        }

        protected override void OnStop()
        {
            SW("Service Stop.");
        }

        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            SW("Main Start.");
            oTimer_Get.Enabled = false;
            try
            {
                MainEvent();
                SW("MainEvent Success.");
            }
            catch (Exception ex)
            {
                SW(ex.Source + "。" + ex.Message);
            }
            oTimer_Get.Enabled = true;
            SW("Main End.");
        }

        private void MainEvent()
        {
            if (File.Exists(strLocalAdd) && boolRunFlag == false)
            {
                try
                {

                    boolRunFlag = true;
                    int intLastID = -1;
                    int intCurrentID = -1;
                    if (File.Exists(strIDRecord))
                    {
                        string strReadst = ReadTXT(strIDRecord);
                        if (strReadst.Length > 0)
                        {
                            intLastID = int.Parse(strReadst);
                        }
                    }
                    else
                    {
                        WriteTXT(strIDRecord, "-1");
                        SW("create new id record file");
                    }
                    string strSQLpara = " where id > " + intLastID;
                    string strSQL = "select * from " + BaseTable + strSQLpara;
                    calssSqlServer.SqlServerHelper ssh = new calssSqlServer.SqlServerHelper();
                    ssh.connectToSQL(LinkString);
                    int intSQLresult = ssh.checkToDataTable(strSQL);
                    if (intSQLresult == 0)
                    {
                        DataTable dtResult = ssh.dt;
                        if (dtResult.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                DataRow dr = dtResult.Rows[0];
                                string param = "{\"ID\":\"" + dr["ID"] + "\",\"CCode\":\"" + dr["CCode"] + "\",\"InChannelId\":\"" + dr["InChannelId"] + "\",\"InDT\":\"" + dr["InDT"] + "\",\"OutChannelId\":\"" + dr["OutChannelId"] + "\",\"OutDT\":\"" + dr["OutDT"] + "\",\"CreateTime\":\"" + dr["CreateTime"] + "\",\"isOut\":\"" + dr["isOut"] + "\"}";
                                string strCallBask = Post(RemoteInterface, param);
                                intCurrentID = int.Parse(dr["ID"].ToString());
                                SW("Complete::POST::" + RemoteInterface + "::" + param + "::" + strCallBask);
                            }
                            WriteTXT(strIDRecord, intCurrentID.ToString());
                        }
                        else
                        {
                            SW("No Record after ID:" + intLastID);
                        }
                    }
                    else
                    {
                        SW("Error::checkToDataTable::" + intSQLresult.ToString() + "::" + strSQL);
                    }
                }
                catch (Exception ex)
                {
                    SW("Error::" + ex.Message);
                }
                boolRunFlag = false;
            }
            else if (!File.Exists(strLocalAdd))
            {
                SW("config file lost!");
            }
            else if (boolRunFlag == true)
            {
                SW("service is running!");
            }
        }

        private void SW(string strT)
        {
            try
            {
                string str_logName = DateTime.Now.ToString("yyyyMMdd") + "_log.txt";
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Log\\" + str_logName, true))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + strT);
                }
            }
            catch(Exception ex)
            {
                
            }
        }

        public static string Post(string url, string param)
        {
            string strURL = url;
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            string paraUrlCoded = param;
            byte[] payload;
            payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }
        public string ReadTXT(string path)
        {
            string strResult = "";
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                strResult = line.ToString();
            }
            sr.Close();
            return strResult;
        }
        public void WriteTXT(string path, string data)
        {
            FileStream fs_txt = new FileStream(path, FileMode.Create);
            StreamWriter sw_txt = new StreamWriter(fs_txt);
            //开始写入
            sw_txt.Write(data);
            //清空缓冲区
            sw_txt.Flush();
            //关闭流
            sw_txt.Close();
            fs_txt.Close();
        }
    }
}
