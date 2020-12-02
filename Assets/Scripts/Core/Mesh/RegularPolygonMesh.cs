using UnityEngine;

namespace FairyGUI
{
    public class RegularPolygonMesh : IMeshFactory, IHitTest
    {
        public Rect? drawRect;

        public int sides;

        public float lineWidth;

        public Color32 lineColor;

        public Color32? centerColor;

        public Color32? fillColor;

        public float[] distances;

        public float rotation;

        public RegularPolygonMesh()
        {
            sides = 3;
            lineColor = Color.black;
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            if (distances != null && distances.Length < sides)
            {
                Debug.LogError("distances.Length<sides");
                return;
            }

            var rect = drawRect != null ? (Rect) drawRect : vb.contentRect;
            var color = fillColor != null ? (Color32) fillColor : vb.vertexColor;

            var angleDelta = 2 * Mathf.PI / sides;
            var angle = rotation * Mathf.Deg2Rad;
            var radius = Mathf.Min(rect.width / 2, rect.height / 2);

            var centerX = radius + rect.x;
            var centerY = radius + rect.y;
            vb.AddVert(new Vector3(centerX, centerY, 0), centerColor == null ? color : (Color32) centerColor);
            for (var i = 0; i < sides; i++)
            {
                var r = radius;
                if (distances != null)
                    r *= distances[i];
                var xv = Mathf.Cos(angle) * (r - lineWidth);
                var yv = Mathf.Sin(angle) * (r - lineWidth);
                var vec = new Vector3(xv + centerX, yv + centerY, 0);
                vb.AddVert(vec, color);
                if (lineWidth > 0)
                {
                    vb.AddVert(vec, lineColor);

                    xv = Mathf.Cos(angle) * r + centerX;
                    yv = Mathf.Sin(angle) * r + centerY;
                    vb.AddVert(new Vector3(xv, yv, 0), lineColor);
                }

                angle += angleDelta;
            }

            if (lineWidth > 0)
            {
                var tmp = sides * 3;
                for (var i = 0; i < tmp; i += 3)
                    if (i != tmp - 3)
                    {
                        vb.AddTriangle(0, i + 1, i + 4);
                        vb.AddTriangle(i + 5, i + 2, i + 3);
                        vb.AddTriangle(i + 3, i + 6, i + 5);
                    }
                    else
                    {
                        vb.AddTriangle(0, i + 1, 1);
                        vb.AddTriangle(2, i + 2, i + 3);
                        vb.AddTriangle(i + 3, 3, 2);
                    }
            }
            else
            {
                for (var i = 0; i < sides; i++)
                    vb.AddTriangle(0, i + 1, i == sides - 1 ? 1 : i + 2);
            }
        }

        public bool HitTest(Rect contentRect, Vector2 point)
        {
            if (drawRect != null)
                return ((Rect) drawRect).Contains(point);
            else
                return contentRect.Contains(point);
        }
    }
}