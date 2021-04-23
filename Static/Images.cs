using photomask.Actions;
using photomask.Image;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace photomask.Static
{
    public static class Images
    {
        public static ObservableCollection<Img> imgs { get; set; } = new ObservableCollection<Img>();

        // WIP
        //public static List<Img> prev_imgs { get; set; } = new List<Img>();

        public static Bitmap result_bitmap { get; set; }

        private static ActionsChain chain = new ActionsChain();

        public static BitmapSource ResultImageSource
        {
            get => Util.GetImageSourceResultFormat(result_bitmap);
        }

        public delegate void ChangeHandler(double elapsed_time);

        public static event ChangeHandler ResultChanged;

        public static void Reset()
        {
            ClearResult();
            //prev_imgs = new List<Img>();
            ResultChanged(0);
        }

        public static void Calculate(Img current_img)
        {
            var time1 = DateTime.Now;

            List<Img> images = imgs.Clone() as List<Img>;
            chain.first?.DoAction(current_img, images);

            var time2 = DateTime.Now;
            
            ResultChanged(Math.Round((time2 - time1).TotalMilliseconds));
        }

        public static void SaveResult(string path)
        {
            if (result_bitmap == null) throw new Exception("Result image is null. You need to call Blend(...)");
            result_bitmap.Save(path);
        }

        public static void ClearResult()
        {
            result_bitmap?.Dispose();
            result_bitmap = null;
        }
    }
}
