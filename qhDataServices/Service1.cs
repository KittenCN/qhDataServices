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
    public partial class Service1 : ServiceBase
    {
        static System.Timers.Timer oTimer_Get = new System.Timers.Timer();
        public static string strLocalAdd = ".\\Config.xml";
        public static string LinkString = "Server = 127.0.0.1;Database = SLEC_Carpark_DTXJQ;User ID = sa;Password = sa123;Trusted_Connection = False;";
        public static string RemoteInterface = "http://114.55.136.29:3000/admin/insertCar";
        public static int DBCacheRate = 1800;
        public static string BaseTable = "CP_InOutCar";
        public Service1()
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
                BaseTable = xnCon.SelectSingleNode("CP_InOutCar").InnerText;

                Thread.Sleep(30000);        //30秒等待
                MainEvent();
                SW("MainEvent Success");
            }
            catch (Exception ex)
            {
                SW(ex.Source + "。" + ex.Message);
            }
            AutoLog = false;
            oTimer_Get.Enabled = true;
            oTimer_Get.Interval = DBCacheRate;      //30分钟轮询一次
            oTimer_Get.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
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
            //string url = txtURL.Text + txtData.Text;
            //string param = "{\"access_token\":\"" + txtData.Text + "\",\"name\":\"qq\",\"parentid\":\"1\",\"order\":\"2\",\"createDeptGroup\":\"false\"}";
            //string callback = Post(url, param);
            //txtResult.Text = callback;
            if(File.Exists(strLocalAdd))
            {
                try
                {
                    calssSqlServer.SqlServerHelper ssh = new calssSqlServer.SqlServerHelper();
                    ssh.connectToSQL(LinkString);

                    string strLastDateTime = "";
                    string strSQLpara = " ";
                    string strSQL = "select * from " + BaseTable + strSQLpara;
                    int intSQLresult = ssh.checkToDataTable(strSQL);
                    if(intSQLresult == 0)
                    {
                        DataTable dtResult = ssh.dt;
                        if(dtResult.Rows.Count > 0)
                        {
                            for(int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                DataRow dr = dtResult.Rows[0];
                                string param = "{\"ID\":\"" + dr["ID"] + "\",\"CCode\":\"qq\",\"InChannelId\":\"1\",\"InDT\":\"2\",\"OutChannelId\":\"false\",\"OutDT\":\"qq\",\"CreateTime\":\"qq\",\"isOut\":\"qq\"}";
                            }
                        }
                    }
                    else
                    {
                        SW("Error::checkToDataTable::" + intSQLresult.ToString() + "::" + strSQL);
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }

        private void SW(string strT)
        {
            string str_logName = DateTime.Now.ToShortDateString().ToString() + "_log.txt";
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("C:\\" + str_logName, true))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + strT);
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
    }
}
