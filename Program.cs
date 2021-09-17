using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.ComponentUtil;

namespace SonicColorsUltimate_TailsNavigatorSwitcher
{
    class Program
    {
        static string exeName = "Sonic Colors - Ultimate";
        static MainForm Form1 = new MainForm();
        static Process game = null;
        static SignatureScanner scanner;
        static IntPtr ptr;
        static SigScanTarget scanTarget = new SigScanTarget(5, "74 2B 48 8B 0D ????????");
        static void Main()
        {
            Application.EnableVisualStyles();
            Task task = new Task(() => FormTask());
            task.Start();
            Application.Run(Form1);
        }


        public static void FormTask()
        {
            while (true)
            {
                if (!processIsRunning())
                {
                    Form1.BeginInvoke((MethodInvoker)delegate () { Form1.label2.Text = "GAME NOT RUNNING"; });
                    game = null;
                    Thread.Sleep(1000);
                    continue;
                }
                if (game == null)
                { 
                    game = Process.GetProcessesByName(exeName).FirstOrDefault();
                    scanner = new SignatureScanner(game, game.MainModule.BaseAddress, game.MainModule.ModuleMemorySize);
                    ptr = scanner.Scan(scanTarget);
                    Form1.BeginInvoke((MethodInvoker)delegate () { Form1.button1.Enabled = true; });
                    Form1.BeginInvoke((MethodInvoker)delegate () { Form1.button2.Enabled = true; });
                }
                if (game.HasExited)
                {
                    game = null;
                    Form1.BeginInvoke((MethodInvoker)delegate () { Form1.button1.Enabled = false; });
                    Form1.BeginInvoke((MethodInvoker)delegate () { Form1.button2.Enabled = false; });
                    Thread.Sleep(1000);
                    continue;
                }


                if (ptr != IntPtr.Zero)
                {
                    int status = game.ReadValue<byte>(new DeepPointer(ptr + 4 + game.ReadValue<int>(ptr), 0x60).Deref<IntPtr>(game) + 7);
                    if (status == 1)
                    {
                        Form1.BeginInvoke((MethodInvoker)delegate () { Form1.label2.Text = "ENABLED"; });
                    }
                    else if (status == 0)
                    {
                        Form1.BeginInvoke((MethodInvoker)delegate () { Form1.label2.Text = "DISABLED"; });
                    }
                }
                Thread.Sleep(300);
            }
        }

        private static bool processIsRunning()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.Contains(exeName)) return true;
            }
            return false;
        }

        public static void but1_Click(object sender, EventArgs e)
        {
            if (game != null)
            {
                game.WriteValue<byte>(new DeepPointer(ptr + 4 + game.ReadValue<int>(ptr), 0x60).Deref<IntPtr>(game) + 7, 0x1);
            }
        }
        public static void but2_Click(object sender, EventArgs e)
        {
            if (game != null)
            {
                game.WriteValue<byte>(new DeepPointer(ptr + 4 + game.ReadValue<int>(ptr), 0x60).Deref<IntPtr>(game) + 7, 0x0);
            }
        }
    }
}