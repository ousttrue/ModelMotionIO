using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using WpfViewer.Views;

namespace WpfViewer.Win32
{
    class MouseEventBehavior : Behavior<EmptyHwnd>
    //class MouseEventBehavior : Behavior<UIElement>
    {
        #region MouseEvent
        public IObserver<Win32MouseEventArgs> MouseEventObserver
        {
            get { return (IObserver<Win32MouseEventArgs>)GetValue(MouseEventObserverProperty); }
            set { SetValue(MouseEventObserverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseEventObserver.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseEventObserverProperty =
            DependencyProperty.Register("MouseEventObserver"
                , typeof(IObserver<Win32MouseEventArgs>), typeof(MouseEventBehavior)
                , new PropertyMetadata(null, new PropertyChangedCallback(MouseEvnetObserverChanged)));

        static void MouseEvnetObserverChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as MouseEventBehavior).MouseEventObserverChanged(e.NewValue as IObserver<Win32MouseEventArgs>);
        }

        Subject<Win32MouseEventArgs> m_subject = new Subject<Win32MouseEventArgs>();
        //Subject<MouseEventArgs> m_subject = new Subject<MouseEventArgs>();

        IDisposable m_subscription;
        void MouseEventObserverChanged(IObserver<Win32MouseEventArgs> o)
        {
            if (m_subscription != null)
            {
                m_subscription.Dispose();
            }
            m_subscription = m_subject.Subscribe(o);
        }
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            var element = this.AssociatedObject;

#if true
            element.Win32Mouse += (o, e) =>
            {
                var m = e as Win32MouseEventArgs;
                if (m != null)
                {
                    m_subject.OnNext(m);
                }
            };
#else
            element.MouseMove += (o, e) =>
            {
                m_subject.OnNext(e);
            };
#endif
        }

        protected override void OnDetaching()
        {
            m_subscription.Dispose();
            m_subscription = null;

            m_subject.Dispose();
            m_subject = null;

            var element = this.AssociatedObject;

            base.OnDetaching();
        }
    }
}
