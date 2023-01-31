using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Easing 
{
    public enum Type
    {
        Linear, Jump, Sine, Quad, Cubic, Expo, Circ, Bounce
    }
    public enum InOut
    {
        In, Out, InOut
    }
    public Type type;
    public InOut io;

    public float Get(float x) => Get(x, type, io);
    
    public static float Get(float x, Type type, InOut io)
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;
        const float c3 = c1 + 1;
        const float c4 = 2f * Mathf.PI / 3f;
        const float c5 = 2f * Mathf.PI / 4.5f;
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        switch (type)
        {
            case Type.Linear:
                return x;
            case Type.Jump:
                switch (io)
                {
                    case InOut.In:
                        return 1;
                    case InOut.Out:
                        return x >= 1 ? 1 : 0;
                    default:
                        return x > 0.5f ? 1 : 0;
                }
            case Type.Sine:
                switch (io)
                {
                    case InOut.In:
                        return 1 - Mathf.Cos(x * Mathf.PI / 2f);
                    case InOut.Out:
                        return Mathf.Sin(x * Mathf.PI / 2f);
                    default:
                        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2f;
                }
            case Type.Quad:
                switch (io)
                {
                    case InOut.In:
                        return x * x;
                    case InOut.Out:
                        return 1 - (1 - x) * (1 - x);
                    default:
                        return x < 0.5f ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2f;
                }
            case Type.Cubic:
                switch (io)
                {
                    case InOut.In:
                        return x * x * x;
                    case InOut.Out:
                        return 1 - Mathf.Pow(1 - x, 3);
                    default:
                        return x < 0.5f ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2f;
                }
            case Type.Expo:
                switch (io)
                {
                    case InOut.In:
                        return x == 0 ? 0 : Mathf.Pow(2f, 10f * x - 10);
                    case InOut.Out:
                        return x == 1 ? 1 : 1 - Mathf.Pow(2f, -10f * x);
                    default:
                        return x <= 0 ? 
                            0 : x >= 1 ? 
                            1 : x < 0.5f ? 
                            Mathf.Pow(2f, 20f * x - 10) / 2f : (2 - Mathf.Pow(2, -20 * x + 10)) / 2f;
                }
            case Type.Circ:
                switch (io)
                {
                    case InOut.In:
                        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
                    case InOut.Out:
                        return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
                    default:
                        return x < 0.5f ? 
                            (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2f;
                }
            case Type.Bounce:
                switch (io)
                {
                    case InOut.In:
                        return 1 - Get(1 - x, Type.Bounce, InOut.Out);
                    case InOut.Out:
                        if (x < 1f / d1)
                            return n1 * x * x;
                        if (x < 2f / d1)
                            return n1 * (x -= 1.5f / d1) * x + 0.75f;
                        if (x < 2.5f / d1)
                            return n1 * (x -= 2.25f / d1) * x + 0.9375f;

                        return n1 * (x -= 2.625f / d1) * x + 0.984375f;
                    default:
                        return x < 0.5f ? (1 - Get(1 - 2 * x, Type.Bounce, InOut.Out)) / 2f : (1 + Get(2 * x - 1, Type.Bounce, InOut.Out) / 2f);
                }
        }
        return x;
    }
}
