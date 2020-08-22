using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Sections
{
    public interface IAnimationController : ISection, IHashNamed
    {
        uint Flags { get; set; }
        float KeyframeLength { get; set; }
    }

    public interface IAnimationController<TValue> : IAnimationController
    {
        IList<Keyframe<TValue>> Keyframes { get; set; }
    }

    public class Keyframe<T>
    {
        public float Timestamp { get; set; }
        public T Value { get; set; }

        public Keyframe(float ts, T v)
        {
            Timestamp = ts;
            Value = v;
        }

        public override string ToString() => $"Timestamp={Timestamp} Value={Value}";
    }
}
