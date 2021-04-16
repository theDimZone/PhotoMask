using photomask.Actions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask
{
    public class ActionsChain
    {
        //public Bitmap result { get; set; }
        public IAction action { get; set; }

        public ActionsChain()
        {
            FirstAction first = new FirstAction();
            CurvingAction curving = new CurvingAction();
            BlendAction blend = new BlendAction();
            WriteAction write = new WriteAction();

            first.next_action = curving;
            curving.next_action = blend;
            blend.next_action = write;

            action = first;
        }


    }
}
