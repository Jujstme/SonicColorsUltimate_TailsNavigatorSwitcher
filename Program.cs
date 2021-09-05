using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using LiveSplit.ComponentUtil;

namespace SonicColorsUltimate_TailsNavigatorSwitcher
{
    static class Program
    {
        static readonly string appName = "Sonic Colors Ultimate - Tails Navigator Switcher";
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Process[] gameList = Process.GetProcessesByName("Sonic Colors - Ultimate");
            if (gameList.Length == 0) {
                MessageBox.Show("Game not detected!\nPlease ensure you are running the game!", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            Process game = gameList.First();
            var scanner = new SignatureScanner(game, game.MainModule.BaseAddress, game.MainModule.ModuleMemorySize);

            IntPtr ptr = scanner.Scan(new SigScanTarget(5,
                "74 2B",               // je "Sonic colors - Ultimate.exe"+16F3948
                "48 8B 0D ????????")); // mov rcx,["Sonic colors - Ultimate.exe"+52462A8]
            if (ptr == IntPtr.Zero)
            {
                MessageBox.Show("Could not detect target address!", "Sonic Colors Ultimate - Tails Navigator Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            var pointerAddr = new DeepPointer(ptr + 4 + game.ReadValue<int>(ptr), 0x60).Deref<IntPtr>(game);
            int status = game.ReadValue<byte>(pointerAddr + 7); 
            
            if (status == 1)
            {
                var question = MessageBox.Show("Tails Navigator is currently ENABLED\n" +
                                               "Do you want to disable it?", "Sonic Colors Ultimate - Tails Navigator Switcher", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (question == DialogResult.Yes)
                {
                    game.WriteValue<byte>(pointerAddr + 7, 0x0);
                    MessageBox.Show("Tails Navigator disabled.", "Sonic Colors Ultimate - Tails Navigator Switcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Tails Navigator stays enabled.", "Sonic Colors Ultimate - Tails Navigator Switcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            } else if (status == 0)
            {
                var question = MessageBox.Show("Tails Navigator is currently DISABLED\n" +
                               "Do you want to enable it?", appName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (question == DialogResult.Yes)
                {
                    game.WriteValue<byte>(pointerAddr + 7, 0x1);
                    MessageBox.Show("Tails Navigator enabled.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Tails Navigator stays disabled.", appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            Environment.Exit(0);
        }
    }
}