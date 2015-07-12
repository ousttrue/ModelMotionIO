using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


namespace Win32
{
    public class Win32EventArgs : RoutedEventArgs
    {
        public Win32.WM EventType { get; set; }
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
    }

    public class EmptyHwnd : HwndHost
    {
        #region Win32Event
        public static readonly RoutedEvent Win32Event = EventManager.RegisterRoutedEvent(
            "Win32", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HwndHost));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Win32
        {
            add { AddHandler(Win32Event, value); }
            remove { RemoveHandler(Win32Event, value); }
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
            //Keyboard.Focus(this);
            Import.SetFocus(hwnd);

            //Mouse.Capture(this);
            //CaptureMouse();
            if (Capture == 0)
            {
                Import.SetCapture(hwnd);
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
                Import.ReleaseCapture();
            }
        }

        protected override IntPtr WndProc(IntPtr hwnd, int _msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var msg = (WM)_msg;
            switch (msg)
            {
                case WM.WM_PAINT:
                    {
                        PAINTSTRUCT ps;
                        var hdc = Import.BeginPaint(hwnd, out ps);
                        Import.EndPaint(hwnd, ref ps);
                    }
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                    });
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_SIZE:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_DESTROY:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                    });
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_ERASEBKGND:
                    handled = true;
                    return IntPtr.Zero;

                #region MouseEvent
                case WM.WM_LBUTTONDOWN:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    CaptureAndCursor(hwnd);
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_LBUTTONUP:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    ReleaseMouse();
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_RBUTTONDOWN:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    CaptureAndCursor(hwnd);
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_RBUTTONUP:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    ReleaseMouse();
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_MBUTTONDOWN:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    CaptureAndCursor(hwnd);
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_MBUTTONUP:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        X = lParam.Lo(),
                        Y = lParam.Hi(),
                    });
                    ReleaseMouse();
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_MOUSEMOVE:
                    if (Capture>0)
                    {
                        RaiseEvent(new Win32EventArgs
                        {
                            RoutedEvent = Win32Event,
                            Source = this,
                            EventType = msg,
                            X = lParam.Lo(),
                            Y = lParam.Hi(),
                        });
                    }
                    else
                    {
                    }
                    Cursor = Cursors.Hand;
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_MOUSEWHEEL:
                    RaiseEvent(new Win32EventArgs
                    {
                        RoutedEvent = Win32Event,
                        Source = this,
                        EventType = msg,
                        Y = (short)wParam.Hi(),
                    });
                    handled = true;
                    return IntPtr.Zero;
#endregion
            }

            return base.WndProc(hwnd, _msg, wParam, lParam, ref handled);
        }
    }
}
