using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace pps
{
    //public class PPS : Form
    public class PPS : ApplicationContext
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private static Guid HIGH_PERF = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        private static Guid LOW_PERF = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");

        [DllImport("PowrProf.dll")]
        public static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey,
                                                       ref IntPtr ActivePolicyGuid);
        [DllImport("powrprof.dll")]
        static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey,
                                                ref Guid ActivePolicyGuid);

        public PPS()
        {
            Init();
        }

        ~PPS()
        {
            notifyIcon.Visible = false;
        }

        private void Init()
        {
            notifyIcon = new NotifyIcon();
            SetIconState();

            notifyIcon.Click += new EventHandler(ClickHandler);
        }

        private Guid GetCurrentGuid()
        {
            IntPtr pCurrentSchemeGuid = IntPtr.Zero;
            PowerGetActiveScheme(IntPtr.Zero, ref pCurrentSchemeGuid);
            return (Guid)Marshal.PtrToStructure(pCurrentSchemeGuid, typeof(Guid));
        }

        private bool IsCurrentHigh()
        {
            return  HIGH_PERF == GetCurrentGuid();
        }

        private bool IsCurrentLow()
        {
            return LOW_PERF == GetCurrentGuid();
        }

        private void SetIconState()
        {
            if (IsCurrentHigh()) {
                notifyIcon.Icon = Resource.high;
                notifyIcon.Text = "You are on HIGH PERFORMANCE";
            } else if (IsCurrentLow()) {
                notifyIcon.Icon = Resource.low;
                notifyIcon.Text = "You are on POWER SAVER";
            } else {
                notifyIcon.Icon = Resource.favicon;
                notifyIcon.Text = "You are on ~BALANCE";
            }

            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(2000, "PowerPlanSwitcher", notifyIcon.Text, ToolTipIcon.Info);
            notifyIcon.Text = notifyIcon.BalloonTipText =
                notifyIcon.Text + "\nLClick - change\nRClick - exit";
        }

        void ClickHandler(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right) {
                ExitThread();
                return;
            }

            if (IsCurrentHigh()) {
                PowerSetActiveScheme(IntPtr.Zero, ref LOW_PERF);
            } else {
                PowerSetActiveScheme(IntPtr.Zero, ref HIGH_PERF);
            }
            SetIconState();
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new PPS());
        }
    }
}
