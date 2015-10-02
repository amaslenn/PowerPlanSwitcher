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

        private bool IsCurrentHigh()
        {
            IntPtr pCurrentSchemeGuid = IntPtr.Zero;
            PowerGetActiveScheme(IntPtr.Zero, ref pCurrentSchemeGuid);
            var currentSchemeGuid = (Guid)Marshal.PtrToStructure(pCurrentSchemeGuid, typeof(Guid));

            return currentSchemeGuid == HIGH_PERF;
        }

        private void SetIconState()
        {
            if (IsCurrentHigh()) {
                notifyIcon.Icon = Resource.high;
                notifyIcon.Text = "Current scheme is HIGH PERFORMANCE";
            } else {
                notifyIcon.Icon = Resource.low;
                notifyIcon.Text = "Current scheme is POWER SAVER";
            }

            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(2000, "PowerPlanSwitcher", notifyIcon.Text, ToolTipIcon.Info);
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
