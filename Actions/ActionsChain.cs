using photomask.Actions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class ActionsChain
    {
        public IAction first { get; set; }

        // in future mb dynamically create chain for each Img via factory or smth
        public ActionsChain()
        {
            CurvingAction curving = new CurvingAction();
            RemoveInvisibleAction remove = new RemoveInvisibleAction();
            SpatialFilteringAction spatial_filtering = new SpatialFilteringAction();
            BinarizationAction binarization = new BinarizationAction();
            BlendAction blend = new BlendAction();
            WriteAction write = new WriteAction();

            curving.next_action = remove;
            remove.next_action = spatial_filtering;
            spatial_filtering.next_action = binarization;
            binarization.next_action = blend;
            blend.next_action = write;

            first = curving;
        }


    }
}
