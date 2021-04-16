using photomask.Image;
using photomask.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class FirstAction : IAction
    {
        public IAction next_action { get; set; }

        public void DoAction(Img current_img, List<Img> images)
        {
            //List<Img> masks = images.Clone() as List<Img>;
            images.RemoveAll(m => m.blend_data.mode == BlendMode.None);

            Images.ClearResult();
            if (images.Count() == 0) return;

            next_action?.DoAction(current_img, images);
        }
    }
}
