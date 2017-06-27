# Xamarians.CropImage
   Cross platform library to rotate and crop picture taken from camera or gallery
   
First install package from nuget using following command -
## Install-Package Xamarians.CropImage

You can integrate crop tools in Xamarin Form application using following code:

Shared Code -

Crop Image:- 

```c#
using Xamarians.CropImage;

...

var cropResult = await CropImageService.Instance.CropImage(filePath, CropRatioType.None);

```

Android - in MainActivity file write below code -
```c#
 Xamarians.CropImage.Droid.CropImageServiceAndroid.Initialize(this);
```

iOS - in AppDelegate file write below code -
```c#
 Xamarians.CropImage.iOS.CropImageServiceIOS.Initialize();
```

