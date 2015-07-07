using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using WpfViewer.Renderer;
using WpfViewer.Renderer.Commands;
using WpfViewer.Renderer.Resources;

namespace WpfViewer.Models
{
    public class Scene
    {
        RenderFrame m_currentFrame;
        public RenderFrame CurrentFrame
        {
            get { return m_currentFrame; }
        }

        public Scene()
        {
            m_currentFrame = new RenderFrame
            {
                Resources = new RenderResourceBase[] { },
                Commands = new IRenderCommand[] { BackbufferClearCommand.Create(new SharpDX.Color4(0, 0.5f, 0, 0.5f)) },
            };

            // Timer駆動でPushする
            Observable.Interval(TimeSpan.FromMilliseconds(33))
                .Subscribe(_ => {

                    m_renderFrameSubject.OnNext(CurrentFrame);
                    
                })
                ;
        }

        Subject<RenderFrame> m_renderFrameSubject = new Subject<RenderFrame>();
        public IObservable<RenderFrame> RenderFrameObservable
        {
            get
            {
                return m_renderFrameSubject;
            }
        }

        public Node LoadPmd(Uri uri)
        {
            var root = new Node
            {
                Name=uri.ToString(),
            };
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmdParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => new Node
                {
                    Name=x.Name,

                }).ToArray();

            // build tree
            model.Bones.ForEach((x, i) => {
                var node = nodes[i];
                var parent = x.Parent.HasValue ? nodes[x.Parent.Value] : root;
                parent.Children.Add(node);
            });

            return root;
        }

        public Node LoadPmx(Uri uri)
        {
            var root = new Node
            {
                Name = uri.ToString(),
            };
            var bytes = File.ReadAllBytes(uri.LocalPath);
            var model = MMIO.Mmd.PmxParse.Execute(bytes);

            var nodes = model.Bones
                .Select(x => new Node
                {
                    Name = x.Name,

                }).ToArray();

            // build tree
            model.Bones.ForEach((x, i) => {
                var node = nodes[i];
                var parent = x.ParentIndex.HasValue ? nodes[x.ParentIndex.Value] : root;
                parent.Children.Add(node);
            });

            return root;
        }
    }
}
