using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetsView.Util
{
    public static class PropertyGridExtensions
    {
        //https://stackoverflow.com/questions/24571817/how-to-set-selected-item-of-property-grid
        public static IEnumerable<GridItem> EnumerateAllItems(this PropertyGrid grid)
        {
            if (grid == null)
                yield break;

            // get to root item
            GridItem start = grid.SelectedGridItem;
            while (start.Parent != null)
            {
                start = start.Parent;
            }

            foreach (GridItem item in start.EnumerateAllItems())
            {
                yield return item;
            }
        }

        public static IEnumerable<GridItem> EnumerateAllItems(this GridItem item)
        {
            if (item == null)
                yield break;

            yield return item;
            foreach (GridItem child in item.GridItems)
            {
                foreach (GridItem gc in child.EnumerateAllItems())
                {
                    yield return gc;
                }
            }
        }
    }
}
