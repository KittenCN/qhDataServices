﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace qhDSconsle
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            //如果传递了"s"参数就启动服务
            if (args.Length > 0 && args[0] == "s")
            {
                //启动服务的代码，可以从其它地方拷贝
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new BaseService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                Console.WriteLine("这是Windows应用程序");
                Console.WriteLine("请选择，[1]安装服务 [2]卸载服务 [3]退出");
                var rs = int.Parse(Console.ReadLine());
                switch (rs)
                {
                    case 1:
                        try
                        {
                            //取当前可执行文件路径，加上"s"参数，证明是从windows服务启动该程序
                            var path = Process.GetCurrentProcess().MainModule.FileName + " s";
                            Process.Start("sc", "create LongintService binpath= \"" + path + "\" displayName= LongintService start= auto");
                            Process.Start("sc", "start LongintService");
                            Console.WriteLine("安装运行成功");
                            Console.Read();                            
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("安装错误::" + ex.Message.ToString());
                            Console.Read();
                        }
                        break;
                    case 2:
                        try
                        {
                            Process.Start("sc", "stop LongintService");
                            Process.Start("sc", "delete LongintService");
                            Console.WriteLine("卸载成功");
                            Console.Read();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("卸载失败::" + ex.Message.ToString());
                            Console.Read();
                        }
                        break;
                    case 3: break;
                }
            }
        }
    }
}
