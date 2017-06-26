using UIKit;
using CoreGraphics;

namespace Xamarians.CropImage.iOS
{
    public class CropperView : UIView
    {
        CGPoint origin;
        CGSize cropSize;

        public CropperView()
        {
            origin = new CGPoint(100, 100);
            cropSize = new CGSize(200, 200);

            BackgroundColor = UIColor.Clear;
            Opaque = false;

            Alpha = 0.4f;
        }

        public CropperView(double x, double y, double w, double h)
        {
            origin = new CGPoint(x, y);
            cropSize = new CGSize(w, h);
            BackgroundColor = UIColor.Clear;
            Opaque = false;

            Alpha = 0.4f;
        }


        public CGPoint Origin
        {
            get
            {
                return origin;
            }

            set
            {
                origin = value;
                SetNeedsDisplay();
            }
        }

        public CGSize CropSize
        {
            get
            {
                return cropSize;
            }
            set
            {
                cropSize = value;
                SetNeedsDisplay();
            }
        }

        public CGRect CropRect
        {
            get
            {
                return new CGRect(Origin, CropSize);
            }
        }

		public CGRect GetCropRect(double xratio, double yratio)
        {


			return new CGRect(Origin.X* xratio,Origin.Y * yratio, CropSize.Width * xratio, CropSize.Height * yratio);
        }


        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            using (var g = UIGraphics.GetCurrentContext())
            {

                g.SetFillColor(UIColor.Black.CGColor);
                g.FillRect(rect);

                g.SetBlendMode(CGBlendMode.Clear);
                UIColor.Clear.SetColor();

                var path = new CGPath();
                path.AddRect(new CGRect(origin, cropSize));

                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.Fill);
            }
        }
    }
}