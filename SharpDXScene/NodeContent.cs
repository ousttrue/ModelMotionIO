using Reactive.Bindings;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXScene
{
    public enum NodeType
    {
        None,
        Skeleton,
        Mesh,
    }

    public class NodeValue
    {
        public static Node<NodeValue> CreateNode(String name
            , SharpDX.Vector3 worldPosition
            , SharpDX.Vector3 localPosition
            , NodeType nodeType = NodeType.None
            )
        {
            var node = new Node<NodeValue>(name, new NodeValue());
            node.Content.WorldPosition.Value = worldPosition;
            node.Content.LocalPosition.Value = localPosition;
            node.Content.AttributeType = nodeType;
            return node;
        }
        public static Node<NodeValue> CreateNode(String name
            , SharpDX.Vector3 worldPosition
            )
        {
            return CreateNode(name, worldPosition, SharpDX.Vector3.Zero);
        }
        public static Node<NodeValue> CreateNode(String name)
        {
            return CreateNode(name, SharpDX.Vector3.Zero);
        }

        ReactiveProperty<SharpDX.Vector3> m_localPosition;
        public ReactiveProperty<SharpDX.Vector3> LocalPosition
        {
            get
            {
                if (m_localPosition == null)
                {
                    m_localPosition = new ReactiveProperty<Vector3>();
                }
                return m_localPosition;
            }
        }

        ReactiveProperty<SharpDX.Vector3> m_worldPosition;
        public ReactiveProperty<SharpDX.Vector3> WorldPosition
        {
            get
            {
                if (m_worldPosition == null)
                {
                    m_worldPosition = new ReactiveProperty<Vector3>();
                }
                return m_worldPosition;
            }
        }

        public NodeType AttributeType
        {
            get;
            set;
        }

        public Uri IconUri
        {
            get
            {
                switch (AttributeType)
                {
                    case NodeType.Skeleton:
                        return new Uri("/Images/skeleton.png", UriKind.Relative);

                    case NodeType.Mesh:
                        return new Uri("/Images/mesh.png", UriKind.Relative);
                }
                return new Uri("/Images/null.png", UriKind.Relative);
            }
        }

        public Motion Motion
        {
            get;
            set;
        }

        ReactiveProperty<Transform> m_keyframe;
        public ReactiveProperty<Transform> KeyFrame
        {
            get
            {
                if (m_keyframe == null)
                {
                    m_keyframe = new ReactiveProperty<Transform>();
                }
                return m_keyframe;
            }
        }

        public Transform WorldTransform;

        public event EventHandler PoseSet;
    }


}
