using Reactive.Bindings.Extensions;
using RenderingPipe;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Runtime.InteropServices;

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

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var result=base.BuildWindowCore(hwndParent);

            Win32 += (o, e) =>
              {
                  var win32 = e as Win32EventArgs;
                  if (e != null)
                  {
                      switch (win32.EventType)
                      {
                          case WM.WM_SIZE:
                              m_renderer.ResizeSwapchain(win32.X, win32.Y);
                              break;
                      }
                  }
              };

            m_renderer.OnPaint(Hwnd);

            return result;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            m_renderer.Dispose();

            base.DestroyWindowCore(hwnd);
        }
    }
}
