using photomask.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Actions
{
    public class FilteringAction : IAction
    {
        public IAction next_action { get; set; }

        private delegate void FilteringMethod(Img editing_image);
        private Dictionary<FilteringMode, FilteringMethod> filtering_methods = new Dictionary<FilteringMode, FilteringMethod>();
        public FilteringAction()
        {
            filtering_methods[FilteringMode.Linear] = Linear;
        }
        public void DoAction(Img current_img, List<Img> images)
        {
            Parallel.For(0, images.Count, i =>
            {
                if (images[i].filtering_data.mode == FilteringMode.None) return;

                filtering_methods[images[i].filtering_data.mode](images[i]);
            });

            next_action?.DoAction(current_img, images);
        }

        private void Linear(Img image)
        {

            Parallel.For(0, image.width, x =>
            {
                for (int y = 0; y < image.height; y++)
                {
                    
                }
            });
        }
    }
}
