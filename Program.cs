using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace COBOL.NET
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var app = new MyApplication();
            app.Run(Environment.GetCommandLineArgs());
        }
    }
}
public class MyApplication : WindowsFormsApplicationBase
{
    public MyApplication()
    {
        this.ShutdownStyle = ShutdownMode.AfterAllFormsClose;
    }
    protected override void OnCreateMainForm()
    {
        MainForm = new COBOL.NET.CobolMainView();
    }
    protected override void OnCreateSplashScreen()
    {
        SplashScreen = new COBOL.NET.Form1();
    }
}
