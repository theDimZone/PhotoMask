using photomask.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class CurvingAction : IAction
    {
        public IAction next_action { get; set; }

        public void DoAction(Img current_img, List<Img> images)
        {


            next_action?.DoAction(current_img, images);
        }
    }
}
