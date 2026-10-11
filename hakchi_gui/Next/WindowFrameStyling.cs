using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace com.clusterrr.hakchi_gui.Next
{
    internal static class WindowFrameStyling
    {
        private const int DwmwaBorderColor = 34;
        private const int HakchiBorderColor = 0x00269E18; // COLORREF for #189E26

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attribute,
            ref int value,
            int valueSize);

        public static void ApplyHakchiBorder(Form form)
        {
            if (form == null || !form.IsHandleCreated)
                return;

            try
            {
                var color = HakchiBorderColor;
                DwmSetWindowAttribute(
                    form.Handle,
                    DwmwaBorderColor,
                    ref color,
                    Marshal.SizeOf(typeof(int)));
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }
    }
}
