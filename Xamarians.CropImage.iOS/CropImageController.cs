using System;
using System.Drawing;
using CoreGraphics;
using UIKit;

namespace Xamarians.CropImage.iOS
{
    public partial class CropImageController : UIViewController
    {
        Action<bool> _callback;

        UIImageView mainImageView;
		UIView mainContainer;
        string ImagePath;
        CropperView cropperView;
        UIViewController _parent;
        UIPanGestureRecognizer pan;
        UIPinchGestureRecognizer pinch;
        CGSize screenSize;
        double imageWidth, imageHeight;
        double xRatio, yRatio;
        UIImage image;
        public CropImageController(string imagePath) 
        {
            ImagePath = imagePath;
        }

		public void CropImageAsync(UIViewController parent, Action<bool> completed)
		{
            _parent = parent;
			_callback = completed;
		}


        public override void ViewDidLoad()
        {
			base.ViewDidLoad();
		    screenSize = UIScreen.MainScreen.Bounds.Size;
            mainContainer = new UIView();
			mainContainer.Frame = new CGRect(0, 0, screenSize.Width, screenSize.Height);
			mainContainer.BackgroundColor = UIColor.Black;
		    this.View.AddSubview(mainContainer);

			double maxWidth = screenSize.Width;  
			double maxHeight = screenSize.Height;  

			// Bottom Bar, which will contain the controlls
			UIView bottomBar = new UIView();
		    bottomBar.Frame = new CGRect(0, screenSize.Height - 70, screenSize.Width, 50);
	        bottomBar.BackgroundColor = UIColor.Black;
	        mainContainer.AddSubview(bottomBar);

			// Adding Items to bottom bar
	        var btnCancel = new UIButton(new CGRect(30, 10, 100, 30));
	        btnCancel.SetTitle("Cancel", UIControlState.Normal);
	        btnCancel.SetTitleColor(UIColor.White, UIControlState.Normal);
	        btnCancel.BackgroundColor = UIColor.Clear;
            btnCancel.TouchUpInside += async(s,e)=>
	         {
                await this.DismissViewControllerAsync(true);
		       _callback?.Invoke(false);
			 };
			 bottomBar.AddSubview(btnCancel);

			 UIButton rotationButton = new UIButton();
            image = new UIImage("rotate_button.png");
			 rotationButton.Frame = new CGRect(screenSize.Width / 2 - 40, 10, 80, 30);
			 rotationButton.SetImage(image,UIControlState.Normal);
			 rotationButton.AddTarget((s,e)=>{ RotateImageTapped();},UIControlEvent.TouchUpInside);
             bottomBar.AddSubview(rotationButton);


			 var btnSave = new UIButton(new CGRect(screenSize.Width - 120, 10, 100, 30));
			 btnSave.SetTitle("Save", UIControlState.Normal);
			 btnSave.SetTitleColor(UIColor.White, UIControlState.Normal);
			 btnSave.BackgroundColor = UIColor.Clear;
			 btnSave.TouchUpInside += Crop;
			 bottomBar.AddSubview(btnSave);

			if (ImagePath == null)
				return;
			using (var image = UIImage.FromFile(ImagePath))
			{

				imageWidth = Math.Min(image.Size.Width, maxWidth);
				imageHeight = imageWidth * image.Size.Height / image.Size.Width;
				mainImageView = new UIImageView(new CGRect(0, ((screenSize.Height - 70)-imageHeight )/2, imageWidth, imageHeight));
				mainImageView.Image = image;
				xRatio = image.Size.Width / imageWidth;
				yRatio = image.Size.Height / imageHeight;

			}
			double cropW = Math.Min(imageWidth, imageHeight) > 300 ? 300 : 200;
			cropperView = new CropperView((imageWidth - cropW) / 2, (imageHeight - cropW) / 2, cropW, cropW) { Frame = mainImageView.Frame };
			mainContainer.AddSubviews(mainImageView, cropperView);

			nfloat dx = 0;
			nfloat dy = 0;

			pan = new UIPanGestureRecognizer(() =>
			{
				if ((pan.State == UIGestureRecognizerState.Began || pan.State == UIGestureRecognizerState.Changed) && (pan.NumberOfTouches == 1))
				{

					var p0 = pan.LocationInView(View);

					if (dx == 0)
						dx = p0.X - cropperView.Origin.X;

					if (dy == 0)
						dy = p0.Y - cropperView.Origin.Y;

					var p1 = new CGPoint(p0.X - dx, p0.Y - dy);
					cropperView.Origin = p1;
				}
				else if (pan.State == UIGestureRecognizerState.Ended)
				{
					dx = 0;
					dy = 0;
				}
			});

			nfloat s0 = 1;

			pinch = new UIPinchGestureRecognizer(() =>
			{
				nfloat s = pinch.Scale;
				nfloat ds = (nfloat)Math.Abs(s - s0);
				nfloat sf = 0;
				const float rate = 0.5f;

				if (s >= s0)
				{
					sf = 1 + ds * rate;
				}
				else if (s < s0)
				{
					sf = 1 - ds * rate;
				}
				s0 = s;

				cropperView.CropSize = new CGSize(cropperView.CropSize.Width * sf, cropperView.CropSize.Height * sf);

				if (pinch.State == UIGestureRecognizerState.Ended)
				{
					s0 = 1;
				}
			});

			cropperView.AddGestureRecognizer(pan);
			cropperView.AddGestureRecognizer(pinch);
		}


		public void ReLoadView(bool roate)
		{
			
			cropperView = new CropperView((imageHeight - 200) / 2, (imageWidth-200 ) / 2, 200, 200) { Frame = mainImageView.Frame };
			mainContainer.AddSubviews(mainImageView, cropperView);
			nfloat dx = 0;
			nfloat dy = 0;
			pan = new UIPanGestureRecognizer(() =>
			{
				if ((pan.State == UIGestureRecognizerState.Began || pan.State == UIGestureRecognizerState.Changed) && (pan.NumberOfTouches == 1))
				{

					var p0 = pan.LocationInView(View);

					if (dx == 0)
						dx = p0.X - cropperView.Origin.X;

					if (dy == 0)
						dy = p0.Y - cropperView.Origin.Y;

					var p1 = new CGPoint(p0.X - dx, p0.Y - dy);
					cropperView.Origin = p1;
				}
				else if (pan.State == UIGestureRecognizerState.Ended)
				{
					dx = 0;
					dy = 0;
				}
			});

			nfloat s0 = 1;

			pinch = new UIPinchGestureRecognizer(() =>
						{
							nfloat s = pinch.Scale;
							nfloat ds = (nfloat)Math.Abs(s - s0);
							nfloat sf = 0;
							const float rate = 0.5f;

							if (s >= s0)
							{
								sf = 1 + ds * rate;
							}
							else if (s < s0)
							{
								sf = 1 - ds * rate;
							}
							s0 = s;

				cropperView.CropSize = new CGSize(cropperView.CropSize.Width * sf, cropperView.CropSize.Height * sf);

							if (pinch.State == UIGestureRecognizerState.Ended)
							{
								s0 = 1;
							}
						});

			cropperView.AddGestureRecognizer(pan);
			cropperView.AddGestureRecognizer(pinch);

		}

        private async void Crop(object sender, EventArgs e)
		{
			var img = ScaleAndRotateImage(mainImageView.Image, rotateDir == 0 ? UIImageOrientation.Up : rotateDir == 1 ? UIImageOrientation.Left : rotateDir == 2 ? UIImageOrientation.Down : UIImageOrientation.Right);

			var rect = cropperView.GetCropRect(xRatio,yRatio);
			var image = img.CGImage.WithImageInRect(rect);
			using (var croppedImage = UIImage.FromImage(image))
			{
				croppedImage.AsJPEG().Save(ImagePath, false);
                await this.DismissViewControllerAsync(true);
				_callback?.Invoke(true);
			}

		}
 

        float rotate = 0;
        int rotateDir = 0;
        private void RotateImageTapped()
        {
            rotate += (float)Math.PI / 2;
            rotateDir += 1;
            if (rotate >= 2 * Math.PI)
            {
                rotateDir = 0;
                rotate = 0;
            }

            mainImageView.Transform = CGAffineTransform.MakeRotation(-rotate);
			ReLoadView(rotateDir == 1 || rotateDir == 3);

		}

		private UIImage ScaleAndRotateImage(UIImage imageIn, UIImageOrientation orIn)
		{

			UIImage res;

			using (CGImage imageRef = imageIn.CGImage)
			{
				CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
				CGColorSpace colorSpaceInfo = CGColorSpace.CreateDeviceRGB();
				if (alphaInfo == CGImageAlphaInfo.None)
				{
					alphaInfo = CGImageAlphaInfo.NoneSkipLast;
				}

				int width, height;
				double maxSize = (double)imageIn.Size.Width;
				width = (int)imageRef.Width;
				height = (int)imageRef.Height;


				if (height >= width)
				{
                    if (orIn == UIImageOrientation.Up || orIn == UIImageOrientation.Down)
                    {
                        maxSize = (double)imageIn.Size.Height;
                    }
                    else
                    {
                        maxSize = (double)imageIn.Size.Height;
                        alphaInfo = CGImageAlphaInfo.PremultipliedLast;
                    }

                    width = (int)Math.Floor((double)width * ((double)maxSize / (double)height));
					height = (int)maxSize;
				}
				else
				{
					height = (int)Math.Floor((double)height * ((double)maxSize / (double)width));
					width = (int)maxSize;
				}


				CGBitmapContext bitmap;

				if (orIn == UIImageOrientation.Up || orIn == UIImageOrientation.Down)
				{
					bitmap = new CGBitmapContext(IntPtr.Zero, width, height, imageRef.BitsPerComponent, imageRef.BytesPerRow, colorSpaceInfo, alphaInfo);
				}
				else
				{
					bitmap = new CGBitmapContext(IntPtr.Zero, height, width, imageRef.BitsPerComponent, width * imageRef.BitsPerPixel, colorSpaceInfo, alphaInfo);
				}

				switch (orIn)
				{
					case UIImageOrientation.Left:
						bitmap.RotateCTM((float)Math.PI / 2);
						bitmap.TranslateCTM(0, -height);
						break;
					case UIImageOrientation.Right:
						bitmap.RotateCTM(-((float)Math.PI / 2));
						bitmap.TranslateCTM(-width, 0);
						break;
					case UIImageOrientation.Up:
						break;
					case UIImageOrientation.Down:
						bitmap.TranslateCTM(width, height);
						bitmap.RotateCTM(-(float)Math.PI);
						break;
				}

				bitmap.DrawImage(new Rectangle(0, 0, width, height), imageRef);


				res = UIImage.FromImage(bitmap.ToImage());
				bitmap = null;
			

				return res;
			}
		}


		//private UIImage ScaleAndRotateImage(UIImage imageIn, UIImageOrientation orIn)
		//{

		//	int kMaxResolution = 2048;

		//	CGImage imgRef = imageIn.CGImage;
		//	float width = imgRef.Width;
		//	float height = imgRef.Height;
		//	CGAffineTransform transform = CGAffineTransform.MakeIdentity();
		//	RectangleF bounds = new RectangleF(0, 0, width, height);

		//	if (width > kMaxResolution || height > kMaxResolution)
		//	{
		//		float ratio = width / height;

		//		if (ratio > 1)
		//		{
		//			bounds.Size = new SizeF(kMaxResolution, bounds.Size.Width / ratio);

		//		}
		//		else
		//		{
		//			bounds.Size = new SizeF(bounds.Size.Height * ratio, kMaxResolution);
		//		}
		//	}

		//	float scaleRatio = bounds.Size.Width / width;
		//	SizeF imageSize = new SizeF(width, height);
		//	UIImageOrientation orient = orIn;
		//	float boundHeight;

		//	switch (orient)
		//	{
		//		case UIImageOrientation.Up:                                        //EXIF = 1
		//			transform = CGAffineTransform.MakeIdentity();
		//			break;

		//		case UIImageOrientation.UpMirrored:                                //EXIF = 2
		//			transform = CGAffineTransform.MakeTranslation(imageSize.Width, 0f);
		//			transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
		//			break;

		//		case UIImageOrientation.Down:                                      //EXIF = 3
		//			transform = CGAffineTransform.MakeTranslation(imageSize.Width, imageSize.Height);
		//			transform = CGAffineTransform.Rotate(transform, (float)Math.PI);
		//			break;

		//		case UIImageOrientation.DownMirrored:                              //EXIF = 4
		//			transform = CGAffineTransform.MakeTranslation(0f, imageSize.Height);
		//			transform = CGAffineTransform.MakeScale(1.0f, -1.0f);
		//			break;

		//		case UIImageOrientation.LeftMirrored:                              //EXIF = 5
		//			boundHeight = bounds.Height;
		//			bounds.Height = bounds.Width;
		//			bounds.Width = boundHeight;
		//			transform = CGAffineTransform.MakeTranslation(imageSize.Height, imageSize.Width);
		//			transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
		//			transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
		//			break;

		//		case UIImageOrientation.Left:                                      //EXIF = 6
		//			boundHeight = bounds.Height;
		//			bounds.Height = bounds.Width;
		//			bounds.Width = boundHeight;
		//			transform = CGAffineTransform.MakeTranslation(0.0f, imageSize.Width);
		//			transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
		//			break;

		//		case UIImageOrientation.RightMirrored:                             //EXIF = 7
		//			boundHeight = bounds.Height;
		//			bounds.Height = bounds.Width;
		//			bounds.Width = boundHeight;
		//			transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
		//			transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
		//			break;

		//		case UIImageOrientation.Right:
		//			boundHeight = bounds.Size.Height;
		//			bounds.Size = new SizeF(boundHeight, bounds.Size.Width);
		//			transform = CGAffineTransform.MakeTranslation(imageSize.Height, 0);
		//			transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
		//			break;//EXIF = 8
		//			//boundHeight = bounds.Height;
		//			//bounds.Height = bounds.Width;
		//			//bounds.Width = boundHeight;
		//			//transform = CGAffineTransform.MakeTranslation(imageSize.Height, 0.0f);
		//			//transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
		//			//break;

		//		default:
		//			break;
		//	}

		//	UIGraphics.BeginImageContext(imageIn.Size);

		//	CGContext context = UIGraphics.GetCurrentContext();


		//	if (orient == UIImageOrientation.Right || orient == UIImageOrientation.Left)
		//	{
		//			context.ScaleCTM(-scaleRatio, scaleRatio);
		//			context.TranslateCTM(-height, 0);
		//	}
		//	else
		//	{
		//		context.ScaleCTM(scaleRatio, -scaleRatio);
		//		context.TranslateCTM(0, -height);
		//	}

		//	context.ConcatCTM(transform);
		//	context.DrawImage(new RectangleF(0, 0, width, height), imgRef);

		//	UIImage imageCopy = UIGraphics.GetImageFromCurrentImageContext();
		//	UIGraphics.EndImageContext();

  //          imageWidth = Math.Min(imageCopy.Size.Width, screenSize.Width);
		//	imageHeight = imageWidth * imageCopy.Size.Height / imageCopy.Size.Width;

  //          xRatio = imageCopy.Size.Width / imageWidth;
  //          yRatio = imageCopy.Size.Height / imageHeight;

		//	return imageCopy;
		//}


		 
		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
		}

	}
}