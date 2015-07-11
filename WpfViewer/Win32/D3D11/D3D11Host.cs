using Reactive.Bindings.Extensions;
using RenderingPipe;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace WpfViewer.Win32.D3D11
{
    public class D3D11Host : EmptyHwnd
    {
        D3D11Renderer m_renderer = new D3D11Renderer();

        #region RenderFrame
        public IObservable<RenderFrame> RenderFrameObservable
        {
            get { return (IObservable<RenderFrame>)GetValue(RenderFrameObservableProperty); }
            set { SetValue(RenderFrameObservableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RenderFrameObservable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RenderFrameObservableProperty =
            DependencyProperty.Register("RenderFrameObservable", typeof(IObservable<RenderFrame>)
                , typeof(D3D11Host)
                , new PropertyMetadata(null, new PropertyChangedCallback(RenderFrameObservableChanged)));

        static void RenderFrameObservableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as D3D11Host).RenderFrameObservableChanged(e.NewValue as IObservable<RenderFrame>);
        }

        CompositeDisposable m_compositeDisposable;

        void RenderFrameObservableChanged(IObservable<RenderFrame> renderFrameObservable)
        {
            if (m_compositeDisposable != null)
            {
                m_compositeDisposable.Dispose();
            }
            m_compositeDisposable = new CompositeDisposable();

            renderFrameObservable
                .ObserveOnDispatcher()
                .Subscribe(frame =>
                {
                    // update & draw
                    m_renderer.Render(frame);
                })
                .AddTo(m_compositeDisposable)
                ;

        }
        #endregion

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            switch ((WM)msg)
            {
                case WM.WM_PAINT:
                    {
                        PAINTSTRUCT ps;
                        var hdc = Import.BeginPaint(hwnd, out ps);
                        Import.EndPaint(hwnd, ref ps);
                        handled = true;

                        m_renderer.OnPaint(hwnd);
                    }
                    return IntPtr.Zero;

                case WM.WM_SIZE:
                    m_renderer.ResizeSwapchain(lParam.Lo(), lParam.Hi());
                    handled = true;
                    break;

                case WM.WM_DESTROY:
                    m_renderer.Dispose();
                    break;

                case WM.WM_ERASEBKGND:
                    handled = true;
                    return IntPtr.Zero;

                case WM.WM_KEYDOWN:
                    //EmitKeyDowned(wParam.ToInt32());
                    ///handled = true;
                    return IntPtr.Zero;

            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
