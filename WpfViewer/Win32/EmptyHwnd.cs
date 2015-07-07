using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WpfViewer.Win32
{
    public class EmptyHwnd: HwndHost
    {
        #region Class
        public static readonly String ClassName = "EmtpyHWnd";
        static UInt16 m_classAtom;
        static Import.WndProc WndProcDelegate = CustomWndProc;
        static IntPtr WndProcPtr = Marshal.GetFunctionPointerForDelegate(WndProcDelegate);
        static private IntPtr CustomWndProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            return Import.DefWindowProcW(hWnd, msg, wParam, lParam);
        }
        #endregion

        IntPtr m_hwnd;
        public IntPtr Hwnd
        {
            get { return m_hwnd; }
            private set
            {
                if (m_hwnd == value)
                {
                    return;
                }
                m_hwnd = value;
            }
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            if (m_classAtom == 0)
            {
                WNDCLASS wind_class = new WNDCLASS
                {
                    lpszClassName = ClassName,
                    lpfnWndProc = WndProcPtr,
                };

                m_classAtom = Import.RegisterClassW(ref wind_class);
            }

            // create window
            Hwnd = Import.CreateWindowExW(
                0, // ex flag
                ClassName,
                "EmptyHwndTitle",
                WS.WS_CHILD,
                Import.CW_USEDEFAULT,
                Import.CW_USEDEFAULT,
                Import.CW_USEDEFAULT,
                Import.CW_USEDEFAULT,
                hwndParent.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero
            );

            return new HandleRef(this, Hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }
    }
}
