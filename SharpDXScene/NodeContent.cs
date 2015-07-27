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

    public class NodeContent
    {
        public static MMIO.Node<NodeContent> CreateNode(String name
            , SharpDX.Vector3 worldPosition
            , SharpDX.Vector3 localPosition
            , NodeType nodeType = NodeType.None
            )
        {
            var node = new MMIO.Node<NodeContent>();
            node.Content.Name.Value = name;
            node.Content.WorldPosition.Value = worldPosition;
            node.Content.LocalPosition.Value = localPosition;
            node.Content.AttributeType = nodeType;
            return node;
        }

        public static MMIO.Node<NodeContent> CreateNode(String name
            , SharpDX.Vector3 worldPosition
            )
        {
            return CreateNode(name, worldPosition, SharpDX.Vector3.Zero);
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
                    m_keyframe = new ReactiveProperty<Transform>(Transform.Identity);
                }
                return m_keyframe;
            }
        }

        public Transform WorldTransform
        {
            get;
            set;
        }

        public Transform LocalTransform
        {
            get
            {
                return new Transform(LocalPosition.Value + KeyFrame.Value.Translation
                    , KeyFrame.Value.Rotation);
            }
        }

        public event EventHandler PoseSet;
        public void RaisePoseSet()
        {
            var tmp = PoseSet;
            if (tmp == null) return;
            tmp(this, EventArgs.Empty);
        }
    }
}
