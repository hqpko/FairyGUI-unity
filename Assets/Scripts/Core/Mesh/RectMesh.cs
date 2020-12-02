﻿using UnityEngine;

namespace FairyGUI
{
    public class RectMesh : IMeshFactory, IHitTest
    {
        public Rect? drawRect;

        public float lineWidth;

        public Color32 lineColor;

        public Color32? fillColor;

        public Color32[] colors;

        public RectMesh()
        {
            lineColor = Color.black;
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            var rect = drawRect != null ? (Rect) drawRect : vb.contentRect;
            var color = fillColor != null ? (Color32) fillColor : vb.vertexColor;
            if (lineWidth == 0)
            {
                if (color.a != 0) //optimized
                    vb.AddQuad(rect, color);
            }
            else
            {
                Rect part;

                //left,right
                part = new Rect(rect.x, rect.y, lineWidth, rect.height);
                vb.AddQuad(part, lineColor);
                part = new Rect(rect.xMax - lineWidth, rect.y, lineWidth, rect.height);
                vb.AddQuad(part, lineColor);

                //top, bottom
                part = new Rect(rect.x + lineWidth, rect.y, rect.width - lineWidth * 2, lineWidth);
                vb.AddQuad(part, lineColor);
                part = new Rect(rect.x + lineWidth, rect.yMax - lineWidth, rect.width - lineWidth * 2, lineWidth);
                vb.AddQuad(part, lineColor);

                //middle
                if (color.a != 0) //optimized
                {
                    part = Rect.MinMaxRect(rect.x + lineWidth, rect.y + lineWidth, rect.xMax - lineWidth,
                        rect.yMax - lineWidth);
                    if (part.width > 0 && part.height > 0)
                        vb.AddQuad(part, color);
                }
            }

            if (colors != null)
                vb.RepeatColors(colors, 0, vb.currentVertCount);

            vb.AddTriangles();
        }

        public bool HitTest(Rect contentRect, Vector2 point)
        {
            return contentRect.Contains(point);
        }
    }
}