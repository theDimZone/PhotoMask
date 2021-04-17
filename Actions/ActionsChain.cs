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
        //public Bitmap result { get; set; }
        public IAction first { get; set; }

        public ActionsChain()
        {
            FilterAction filter = new FilterAction();
            CurvingAction curving = new CurvingAction();
            BlendAction blend = new BlendAction();
            WriteAction write = new WriteAction();

            //filter.next_action = curving;
            curving.next_action = filter;
            filter.next_action = blend;
            blend.next_action = write;

            first = curving;
        }


    }
}
