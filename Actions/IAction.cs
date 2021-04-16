using photomask.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public interface IAction
    {
        public void DoAction(Img current_img, List<Img> images);

        public IAction next_action { get; set; }
    }
}
