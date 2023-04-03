using System;
using System.Threading;
using System.Windows.Forms;

using EXGuard.Forms;

namespace EXGuard
{
    [System.Reflection.Obfuscation(Feature = "Apply to member * when method or constructor: virtualization", Exclude = false)]
    static class Program
    {
        public static Main GetMain
        {
            get;
            set;
        }

        [STAThread]
        static void Main()
        {
            using (var mutex = new Mutex(true, "64f5e6a3cd1e4ba296cbce7fa4c93c25"))
            {
                if (mutex.WaitOne(TimeSpan.Zero))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Login());
                }
                else
                {
                    MessageBox.Show("Hey, your app is working right now!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Environment.Exit(0);
                }
            }
        }
    }
}
