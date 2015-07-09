using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using WpfViewer.Models;

namespace WpfViewer.Views
{
    class MouseEventBehavior : Behavior<UIElement>
    {
        #region MouseEvent
        public IMouseEventObserver MouseEventObserver
        {
            get { return (IMouseEventObserver)GetValue(MouseEventObserverProperty); }
            set { SetValue(MouseEventObserverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseEventObserver.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseEventObserverProperty =
            DependencyProperty.Register("MouseEventObserver"
                , typeof(IMouseEventObserver), typeof(MouseEventBehavior)
                , new PropertyMetadata(null, new PropertyChangedCallback(MouseEvnetObserverChanged)));

        static void MouseEvnetObserverChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as MouseEventBehavior).MouseEventObserverChanged(e.NewValue as IMouseEventObserver);
        }

        IDisposable m_subscription;
        void MouseEventObserverChanged(IMouseEventObserver o)
        {
            if (m_subscription != null)
            {
                m_subscription.Dispose();
            }
            m_subscription = m_subject
                .Select(x =>
                {
                    Console.WriteLine(x.GetPosition(this.AssociatedObject));
                    return x;
                })
                .Subscribe(o);
        }
        #endregion

        Subject<MouseEventArgs> m_subject = new Subject<MouseEventArgs>();
        protected override void OnAttached()
        {
            base.OnAttached();

            var element = this.AssociatedObject;

            element.MouseDown += (o, e) => m_subject.OnNext(e);
            element.MouseUp += (o, e) => m_subject.OnNext(e);
            element.MouseMove += (o, e) => m_subject.OnNext(e);
            element.MouseWheel += (o, e) => m_subject.OnNext(e);
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
