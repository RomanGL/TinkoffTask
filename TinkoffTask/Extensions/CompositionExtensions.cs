using System.Numerics;
using Windows.UI.Composition;

namespace TinkoffTask.Extensions
{
    public static class CompositionExtensions
    {
        public static void SetOffsetX(this Visual visual, float x) 
            => visual.Offset = new Vector3(x, visual.Offset.Y, visual.Offset.Z);
    }
}
