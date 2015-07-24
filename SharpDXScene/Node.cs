using Reactive.Bindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace SharpDXScene
{
    public class Node<U> : IEnumerable<Node<U>>
    {
        U m_content;
        public U Content
        {
            get { return m_content; }
        }

        ReactiveProperty<String> m_name;
        public ReactiveProperty<String> Name
        {
            get
            {
                if (m_name == null)
                {
                    m_name = new ReactiveProperty<string>();
                }
                return m_name;
            }
        }

        ReactiveProperty<Boolean> m_isSelected;
        public ReactiveProperty<Boolean> IsSelected
        {
            get
            {
                if (m_isSelected == null)
                {
                    m_isSelected = new ReactiveProperty<bool>();
                }
                return m_isSelected;
            }
        }

        public Node(String name)
        {
            Name.Value = name;
            m_content = Activator.CreateInstance<U>();
        }

        public Node(String name, U content)
        {
            Name.Value = name;
            m_content = content;
        }

        #region IEnumerable
        ObservableCollection<Node<U>> m_children = new ObservableCollection<Node<U>>();
        public ObservableCollection<Node<U>> Children
        {
            get { return m_children; }
        }

        public void Add(U value)
        {
            Children.Add(new Node<U>("", value));
        }

        public void Add(Node<U> value)
        {
            Children.Add(value);
        }

        public IEnumerator<Node<U>> GetEnumerator()
        {
            return Children.Cast<Node<U>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Children.GetEnumerator();
        }
        #endregion

        #region Traverse
        public IEnumerable<Node<U>> Traverse()
        {
            return Enumerable.Repeat(this, 1).Concat(Children.SelectMany(x => x.Traverse()));
        }

        public IEnumerable<Tuple<Node<U>, Node<U>>> TraversePair()
        {
            foreach (var child in Children)
            {
                yield return Tuple.Create(this, child);

                foreach (var x in child.TraversePair())
                {
                    yield return x;
                }
            }
        }
        #endregion

        public delegate S Pred<S>(IEnumerable<Node<U>> path, IEnumerable<S> results);

        #region ForEach
        void ForEach<S>(Pred<S> pred
            , IEnumerable<Node<U>> path, IEnumerable<S> results)
        {
            var result = pred(path, results);

            foreach (var child in Children)
            {
                child.ForEach(pred
                    , Enumerable.Repeat(child, 1).Concat(path)
                    , Enumerable.Repeat(result, 1).Concat(results));
            }
        }

        public void ForEach<S>(S seed, Pred<S> pred)
        {
            ForEach(pred, Enumerable.Repeat(this, 1), Enumerable.Repeat(seed, 1));
        }
        #endregion

        #region Map
        Node<S> Map<S>(Pred<S> pred
            , IEnumerable<Node<U>> path, IEnumerable<S> results)
        {
            var result = pred(path, results);
            var node = new Node<S>("", result);

            foreach (var child in Children)
            {
                node.Children.Add(child.Map(pred
                    , Enumerable.Repeat(child, 1).Concat(path)
                    , Enumerable.Repeat(result, 1).Concat(results)));
            }

            return node;
        }

        public Node<S> Map<S>(S seed, Pred<S> pred)
        {
            return Map(pred, Enumerable.Repeat(this, 1), Enumerable.Repeat(seed, 1));
        }
        #endregion
    }
}
