using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace calssSqlServer
{
    public class SqlServerHelper
    {
        //引用
        /*
         * -------引用----------
         * using System.Data;
         * using System.Data.SqlClient;
         * ---------------------
         */
        //例子：
        /*
         * 增：
         * insert [into] <表名> (列名) values (列值)
         * 例：insert into Strdents (姓名,性别,出生日期) values ('开心朋朋','男','1980/6/15')
         * 
         * 删：
         * 删除行：
         * delete from <表名> [where <删除条件>]
         * 例：delete from TableName where KeyID = 1
         * 删除表：
         * truncate table <表名>
         * 例：truncate table tongxunlu
         * 
         * 改：
         * update <表名> set <列名=更新值> [where <更新条件>]
         * 例：update tongxunlu set 年龄=18 where 姓名='蓝色小名'
         * 
         * 查：
         * select <列名> from <表名> [where <查询条件表达试>] [order by <排序的列名>[asc或desc]]
         * 例：select * from TableName where AID='f7326912-2a54-47ec-b1a2-06c561473aa9' order by SortiD asc
         */

        //////////////////////////成员/////////////////////////////
        //数据库连接对象
        #region SqlConnection con;
        private SqlConnection Con = null;
        /// <summary>
        /// 连接对象
        /// </summary>
        public SqlConnection con
        {
            get;
            private set;
        }
        #endregion

        //DataSet和 SQL Server之间的桥接器----数据适配器
        #region SqlDataAdapter sda;
        /// <summary>
        /// 名称：SqlDataAdapter是 DataSet和 SQL Server之间的桥接器。
        /// 作用：填充成员ds（DataSet）或dt（DataTable）
        /// 步骤：（查数据->）
        /// 1.1.填充数据：checkToDataSet();//填充数据到dataSet中  （1.1和1.2选其一）
        /// 1.2.填充数据：checkToDataTable();//填充数据到DataTable中
        /// </summary>
        public readonly SqlDataAdapter sda = new SqlDataAdapter();

        #endregion

        //本地数据集合
        #region DataSet ds = new DataSet();
        private DataSet Ds = new DataSet();
        /// <summary>
        /// 功能：DataSet是不依赖于数据库的独立数据集合
        /// 步骤：1.填充数据：checkToDataSet();//填充数据到DataSet中
        /// </summary>
        public DataSet ds
        {
            get { return Ds; }
        }
        #endregion
        #region DataTable dt = new DataTable();
        private DataTable Dt = new DataTable();
        /// <summary>
        /// 功能：将查询到的数据保存到dt表格中
        /// 步骤：1.填充数据：checkToDataTable();//填充数据到DataTable中
        /// </summary>
        public DataTable dt
        {
            get { return Dt; }
        }
        #endregion


        //////////////////////////方法/////////////////////////////
        //连接 - public int connectToSQL(string connStr)
        #region
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="connStr">例子："server=ip地址;database= ;user=ICMS;pwd=abc123*"</param>
        /// <returns>成功：0  失败：-1</returns>
        public int connectToSQL(string connStr)
        {
            try
            {
                con = new SqlConnection(connStr);
            }
            catch (Exception e)
            {
                ShowError(e.Message);
                return -1;
            }
            return 0;
        }
        #endregion

        //报错 - void ShowError(string message)
        #region
        void ShowError(string message)
        {
            Console.WriteLine(message);//控制台
            //MessageBox.Show(message);//窗口
        }
        #endregion

        //增删改 - int handleSql(string com)
        #region
        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="com">数据库操作语句</param>
        /// <returns>0：正常  -1：抛出异常   -2：未连接数据库</returns>
        public int handleSql(string com)
        {
            if (null == con)
            {
                ShowError("未初始化数据库连接");
                return -2;
            }

            try
            {
                SqlCommand commad = new SqlCommand(com, con);
                con.Open();
                commad.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                ShowError(e.Message);
                return -1;
            }
            return 0;
        }
        #endregion

        //查询1 - public int checkToDataSet(string sql)
        #region
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>0：正常  -1：抛出异常   -2：未连接数据库</returns>
        public int checkToDataSet(string sql)
        {
            if (null == con)
            {
                ShowError("未初始化数据库连接");
                return -2;
            }
            try
            {
                SqlCommand MyCommand = new SqlCommand(sql, con); //定义一个数据库操作指令
                sda.SelectCommand = MyCommand;//设置操作指令
                con.Open();//打开数据库连接
                sda.SelectCommand.ExecuteNonQuery();//执行数据库查询指令
                sda.Fill(Ds);
                con.Close();//关闭数据库连接
            }
            catch (Exception e)
            {
                ShowError(e.Message);
                return -1;
            }
            return 0;
        }
        #endregion

        //查询2 - public int checkToDataTable(string sql)
        #region
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>0：正常  -1：抛出异常   -2：未连接数据库</returns>
        public int checkToDataTable(string sql)
        {
            int re = checkToDataSet(sql);
            if (0 == re)
                Dt = Ds.Tables[0];
            return re;
        }
        #endregion
    }
}
