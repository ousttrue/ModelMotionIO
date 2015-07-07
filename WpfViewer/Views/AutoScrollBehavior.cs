using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace WpfViewer.Views
{
    class AutoScrollBehavior : Behavior<ListBox>
    {
        bool m_initialized;

        protected override void OnAttached()
        {
            base.OnAttached();

            var listbox = this.AssociatedObject;
            listbox.LayoutUpdated += OnLayoutUpdated;
        }

        void OnLayoutUpdated(Object o, EventArgs e)
        {
            var listbox = this.AssociatedObject;
            var collection = listbox.ItemsSource as INotifyCollectionChanged;
            if (collection != null)
            {
                listbox.LayoutUpdated -= OnLayoutUpdated;
                collection.CollectionChanged += OnCollectionChanged;
            }
        }

        protected override void OnDetaching()
        {
            var listbox = this.AssociatedObject;
            var collection = listbox.ItemsSource as INotifyCollectionChanged;
            collection.CollectionChanged -= OnCollectionChanged;

            base.OnDetaching();
        }

        private void OnCollectionChanged(Object o, NotifyCollectionChangedEventArgs e)
        {
            var listbox = this.AssociatedObject;
            if(e.Action==NotifyCollectionChangedAction.Add)
            {
                listbox.ScrollIntoView(e.NewItems[0]);
            }
        }
    }
}
