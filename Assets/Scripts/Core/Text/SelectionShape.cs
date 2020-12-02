using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    public class SelectionShape : DisplayObject, IMeshFactory
    {
        public readonly List<Rect> rects;

        public SelectionShape()
        {
            CreateGameObject("SelectionShape");
            graphics = new NGraphics(gameObject);
            graphics.texture = NTexture.Empty;
            graphics.meshFactory = this;

            rects = new List<Rect>();
        }

        public Color color
        {
            get => graphics.color;
            set
            {
                graphics.color = value;
                graphics.Tint();
            }
        }

        public void Refresh()
        {
            var count = rects.Count;
            if (count > 0)
            {
                var rect = new Rect();
                rect = rects[0];
                Rect tmp;
                for (var i = 1; i < count; i++)
                {
                    tmp = rects[i];
                    rect = ToolSet.Union(ref rect, ref tmp);
                }

                SetSize(rect.xMax, rect.yMax);
            }
            else
            {
                SetSize(0, 0);
            }

            graphics.SetMeshDirty();
        }

        public void Clear()
        {
            rects.Clear();
            graphics.SetMeshDirty();
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            var count = rects.Count;
            if (count == 0 || color == Color.clear)
                return;

            for (var i = 0; i < count; i++)
                vb.AddQuad(rects[i]);
            vb.AddTriangles();
        }

        protected override DisplayObject HitTest()
        {
            Vector2 localPoint = WorldToLocal(HitTestContext.worldPoint, HitTestContext.direction);

            if (_contentRect.Contains(localPoint))
            {
                var count = rects.Count;
                for (var i = 0; i < count; i++)
                    if (rects[i].Contains(localPoint))
                        return this;
            }

            return null;
        }
    }
}