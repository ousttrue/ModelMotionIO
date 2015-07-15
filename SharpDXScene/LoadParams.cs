using Reactive.Bindings;
using System;

namespace SharpDXScene
{
    public class LoadParams
    {
        ReactiveProperty<Single> m_scale;
        public ReactiveProperty<Single> Scaling
        {
            get {
                if (m_scale == null)
                {
                    m_scale = new ReactiveProperty<float>(1.0f);
                }
                return m_scale;
            }
        }

        ReactiveProperty<Axis> m_flipAxis;
        public ReactiveProperty<Axis> FlipAxis
        {
            get {
                if (m_flipAxis == null)
                {
                    m_flipAxis = new ReactiveProperty<Axis>(Axis.None);
                }
                return m_flipAxis;
            }
        }

        ReactiveProperty<bool> m_yRotate;
        public ReactiveProperty<Boolean> YRotate
        {
            get {
                if (m_yRotate == null)
                {
                    m_yRotate = new ReactiveProperty<bool>(false);
                }
                return m_yRotate;
            }
        }

        public LoadParams(Single scale)
        {
            Scaling.Value=scale;
        }
    }
}
