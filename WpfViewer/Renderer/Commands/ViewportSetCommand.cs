﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfViewer.Renderer.Commands
{
    public class ViewportSetCommand : IRenderCommand
    {
        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.Viewport_Set; }
        }

        public override string ToString()
        {
            return String.Format("Viewport: {0}"
                , Viewport);
        }

        public SharpDX.ViewportF Viewport { get; set; }

        public static ViewportSetCommand Create(float x, float y, float w, float h, float minD = 0, float MaxD = 1.0f)
        {
            return new ViewportSetCommand
            {
                Viewport = new SharpDX.ViewportF
                {
                    X = x,
                    Y = y,
                    Width = w,
                    Height = h,
                    MinDepth = minD,
                    MaxDepth = MaxD,
                },
            };
        }
    }
}
