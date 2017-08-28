using System;
using Xamarin.Forms;
using Xamarians.Media;
using Xamarians.CropImage;

namespace TestApp
{
    public partial class MainPage : ContentPage
    {
        MediaResult result;
        public MainPage()
        {
            InitializeComponent();
        }
        private async void BtnClicked(object sender, EventArgs e)
        {
			result = await MediaService.Instance.OpenMediaPickerAsync(MediaType.Image);
          //  await System.Threading.Tasks.Task.Delay(1000);

             if (result.IsSuccess)
			     CropImage(result.FilePath);

			//var actions = new string[] { "Open Camera", "Open Gallery" };
			//var action = await DisplayActionSheet("Change Picture", "Cancel", null, actions);
			//if (actions[0].Equals(action))
			//{

			//            var fileName = MediaService.Instance.GenerateUniqueFileName("jpg");
			//            var filePath = System.IO.Path.Combine(MediaService.Instance.GetPublicDirectoryPath(), fileName);
			//            result = await MediaService.Instance.TakePhotoAsync(new CameraOption() { FilePath = filePath});
			//            if (result.IsSuccess)
			//                CropImage(result.FilePath);
			//else
			//await DisplayAlert("Error", result.Message, "OK");
			//        }
			//        else if (actions[1].Equals(action))
			//        {
			//            result = await MediaService.Instance.OpenMediaPickerAsync(MediaType.Image);
			//            if (result.IsSuccess)
			//                CropImage(result.FilePath);
			//else
			//await DisplayAlert("Error", result.Message, "OK");
			//}
		}

        private async void CropImage(string filePath)
        {
            var cropResult = await CropImageService.Instance.CropImage(filePath, CropRatioType.Square);
            if (cropResult.IsSuccess)
            {
                image.Source = cropResult.FilePath;
            }
        }
    }
}
