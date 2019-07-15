using System;

namespace TinkoffTask.Controls
{
    public sealed class CarouselViewItemClickEventArgs : EventArgs
    {
        public CarouselViewItemClickEventArgs(object clickedItem)
        {
            this.ClickedItem = clickedItem ?? throw new ArgumentNullException(nameof(clickedItem));
        }

        public object ClickedItem { get; }
    }
}
