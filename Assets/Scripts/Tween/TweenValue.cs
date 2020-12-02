using UnityEngine;

namespace FairyGUI
{
    public class TweenValue
    {
        public float x;

        public float y;

        public float z;

        public float w;

        public double d;

        public TweenValue()
        {
        }

        public Vector2 vec2
        {
            get => new Vector2(x, y);
            set
            {
                x = value.x;
                y = value.y;
            }
        }

        public Vector3 vec3
        {
            get => new Vector3(x, y, z);
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }

        public Vector4 vec4
        {
            get => new Vector4(x, y, z, w);
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
                w = value.w;
            }
        }

        public Color color
        {
            get => new Color(x, y, z, w);
            set
            {
                x = value.r;
                y = value.g;
                z = value.b;
                w = value.a;
            }
        }

        /// <param name="index"></param>
        /// <returns></returns>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                    default:
                        throw new System.Exception("Index out of bounds: " + index);
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    case 3:
                        w = value;
                        break;
                    default:
                        throw new System.Exception("Index out of bounds: " + index);
                }
            }
        }

        public void SetZero()
        {
            x = y = z = w = 0;
            d = 0;
        }
    }
}