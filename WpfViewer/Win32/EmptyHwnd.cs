using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WpfViewer.Views;

namespace WpfViewer.Win32
{
    public enum Win32MouseEventType
    {
        LeftButtonDown,
        LeftButtonUp,
        RightButtonDown,
        RightButtonUp,
        MiddleButtonDown,
        MiddleButtonUp,
        Move,
        Wheel,
    }

    public class Win32MouseEventArgs : RoutedEventArgs
    {
        public Win32MouseEventType MouseEventType { get; set; }
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
    }

    public class EmptyHwnd : HwndHost
    {
        #region Win32MouseEvent
        public static readonly RoutedEvent Win32MouseEvent = EventManager.RegisterRoutedEvent(
            "Win32Mouse", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HwndHost));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Win32Mouse
        {
            add { AddHandler(Win32MouseEvent, value); }
            remove { RemoveHandler(Win32MouseEvent, value); }
        }
        #endregion

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

            LostMouseCapture += EmptyHwnd_LostMouseCapture;

            return new HandleRef(this, Hwnd);
        }

        private void EmptyHwnd_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }

        int Capture;

        void CaptureAndCursor(IntPtr hwnd)
        {
            //Mouse.Capture(this);
            //CaptureMouse();
            if (Capture == 0)
            {
                Win32.Import.SetCapture(hwnd);
            }
            ++Capture;
        }

        void ReleaseMouse()
        {
            --Capture;
            if (Capture < 0) {
                Capture = 0;
            }
            if (Capture == 0){
                Win32.Import.ReleaseCapture();
            }
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
#if true
#region MouseEvent
                case WM.WM_LBUTTONDOWN:
                    RaiseEvent(new Win32MouseEventArgs
                    {
                        RoutedEvent = Win32MouseEvent,
                        Source = this,
                        MouseEventType = Win32MouseEventType.LeftButtonDown,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    handled = true;
                    CaptureAndCursor(hwnd);
                    return IntPtr.Zero;

                case WM.WM_LBUTTONUP:
                    RaiseEvent(new Win32MouseEventArgs
                    {
                        RoutedEvent = Win32MouseEvent,
                        Source = this,
                        MouseEventType = Win32MouseEventType.LeftButtonUp,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    handled = true;
                    ReleaseMouse();
                    return IntPtr.Zero;

                case WM.WM_RBUTTONDOWN:
                    RaiseEvent(new Win32MouseEventArgs
                    {
                        RoutedEvent = Win32MouseEvent,
                        Source = this,
                        MouseEventType = Win32MouseEventType.RightButtonDown,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    handled = true;
                    CaptureAndCursor(hwnd);
                    return IntPtr.Zero;

                case WM.WM_RBUTTONUP:
                    RaiseEvent(new Win32MouseEventArgs
                    {
                        RoutedEvent = Win32MouseEvent,
                        Source = this,
                        MouseEventType = Win32MouseEventType.RightButtonUp,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    handled = true;
                    ReleaseMouse();
                    return IntPtr.Zero;

                case WM.WM_MBUTTONDOWN:
                    RaiseEvent(new Win32MouseEventArgs
                    {
                        RoutedEvent = Win32MouseEvent,
                        Source = this,
                        MouseEventType = Win32MouseEventType.MiddleButtonDown,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    handled = true;
                    CaptureAndCursor(hwnd);
                    return IntPtr.Zero;

                case WM.WM_MBUTTONUP:
                    RaiseEvent(new Win32MouseEventArgs
                    {
                        RoutedEvent = Win32MouseEvent,
                        Source = this,
                        MouseEventType = Win32MouseEventType.MiddleButtonUp,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    handled = true;
                    ReleaseMouse();
                    return IntPtr.Zero;

                case WM.WM_MOUSEMOVE:
                    Cursor = Cursors.Hand;
                    if (Capture>0)
                    {
                        RaiseEvent(new Win32MouseEventArgs
                        {
                            RoutedEvent = Win32MouseEvent,
                            Source = this,
                            MouseEventType = Win32MouseEventType.Move,
                            X = lParam.Lo(),
                            Y = lParam.Hi(),
                        });
                    }
                    else
                    {
                    }
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_MOUSEWHEEL:
                    RaiseEvent(new Win32MouseEventArgs
                    {
                        RoutedEvent = Win32MouseEvent,
                        Source = this,
                        MouseEventType = Win32MouseEventType.Wheel,
                        Y = (short)wParam.Hi(),
                    });
                    handled = true;
                    return IntPtr.Zero;
#endregion
#endif
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
