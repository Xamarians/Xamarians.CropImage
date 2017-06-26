using System;
using System.Threading.Tasks;
using UIKit;
using static CoreText.CTFontFeatureAllTypographicFeatures;

namespace Xamarians.CropImage.iOS
{
    public class CropImageServiceIOS : ICropImageService
    {
        public CropImageServiceIOS()
        {

        }

        public static void Initialize()
        {
            CropImageService.Init(new CropImageServiceIOS());
        }

        #region Private method

        private static UIViewController GetController()
        {
			var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (vc.PresentedViewController != null  && vc.PresentedViewController.ToString().Contains("Xamarin_Forms_Platform_iOS_ModalWrapper"))
				vc = vc.PresentedViewController;
            return vc;
        }

        #endregion

        #region ICropImageService

        public  Task<CropResult> CropImage(string imagePath, CropRatioType ratioType)
        {
            var task = new TaskCompletionSource<CropResult>();
            try
            {
                var controllar = new CropImageController(imagePath);
                GetController().PresentModalViewController(controllar, false);
				controllar.CropImageAsync(GetController(),(isImageCropDone) =>
				{
				    if (!isImageCropDone)
				    {
				        task.SetResult(new CropResult(false) { Message = "Cancelled" });
						return;
				    }
				    task.SetResult(new CropResult(true) { FilePath= imagePath, Message = "Image cropped successfully" });
				});

			}
            catch (Exception ex)
            {
                task.SetResult(new CropResult(false) { Message = ex.Message });

			}
            return  task.Task;
        }

        #endregion
    }
}