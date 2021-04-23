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

        public ActionsChain()
        {
            CurvingAction curving = new CurvingAction();
            BinarizationAction binarization = new BinarizationAction();
            BlendAction blend = new BlendAction();
            WriteAction write = new WriteAction();

            curving.next_action = binarization;
            binarization.next_action = blend;
            blend.next_action = write;

            first = curving;
        }


    }
}
