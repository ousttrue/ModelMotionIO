using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.Collections;

namespace SharpDXScene
{
    public class NodeBase<U> : IEnumerable<NodeBase<U>>
    {
        U m_content;
        public U Content
        {
            get { return m_content; }
        }

        public NodeBase(U content)
        {
            m_content = content;
        }
        ObservableCollection<NodeBase<U>> m_children = new ObservableCollection<NodeBase<U>>();
        public ObservableCollection<NodeBase<U>> Children
        {
            get { return m_children; }
        }

        #region IEnumerable
        public void Add(U value)
        {
            Children.Add(new NodeBase<U>(value));
        }

        public void Add(NodeBase<U> value)
        {
            Children.Add(value);
        }

        public IEnumerable<NodeBase<U>> Traverse()
        {
            return Enumerable.Repeat(this, 1).Concat(Children.SelectMany(x => x.Traverse()));
        }

        public IEnumerable<Tuple<NodeBase<U>, NodeBase<U>>> TraversePair()
        {
            foreach (var child in Children)
            {
                yield return Tuple.Create(this, child);

                foreach(var x in child.TraversePair())
                {
                    yield return x;
                }
            }
        }

        public IEnumerator<NodeBase<U>> GetEnumerator()
        {
            return Traverse().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Traverse().GetEnumerator();
        }
        #endregion

        public delegate S Pred<S>(IEnumerable<NodeBase<U>> path, IEnumerable<S> results);

        #region ForEach
        void ForEach<S>(Pred<S> pred
            , IEnumerable<NodeBase<U>> path, IEnumerable<S> results)
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
        NodeBase<S> Map<S>(Pred<S> pred
            , IEnumerable<NodeBase<U>> path, IEnumerable<S> results)
        {
            var result = pred(path, results);
            var node = new NodeBase<S>(result);

            foreach (var child in Children)
            {
                node.Children.Add(child.Map(pred
                    , Enumerable.Repeat(child, 1).Concat(path)
                    , Enumerable.Repeat(result, 1).Concat(results)));
            }

            return node;
        }

        public NodeBase<S> Map<S>(S seed, Pred<S> pred)
        {
            return Map(pred, Enumerable.Repeat(this, 1), Enumerable.Repeat(seed, 1));
        }
        #endregion
    }
}
