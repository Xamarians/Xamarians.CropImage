/*
 * Copyright (C) 2009 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace Xamarians.CropImage.Droid
{
    /// <summary>
    /// The activity can crop specific region of interest from an image.
    /// </summary>
    [Activity]
    public class CropImageActivity : MonitoredActivity
    {
        #region Private members

        // These are various options can be specified in the intent.
        private Bitmap.CompressFormat outputFormat = Bitmap.CompressFormat.Jpeg;
        private Android.Net.Uri saveUri = null;
        private int aspectX, aspectY;
        private Handler mHandler = new Handler();

        // These options specifiy the output image size and whether we should
        // scale the output to fit it (or just crop it).
        private int outputX, outputY;
        private bool scale;
        private bool scaleUp = true;

        private CropImageView imageView;
        private Bitmap bitmap;

        private string imagePath;

        private const int NO_STORAGE_ERROR = -1;
        private const int CANNOT_STAT_ERROR = -2;

        #endregion

        #region Properties

        public HighlightView Crop
        {
            set;
            get;
        }

        /// <summary>
        /// Whether the "save" button is already clicked.
        /// </summary>
        public bool Saving { get; set; }

        #endregion

        #region Overrides

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.cropimage);

            imageView = FindViewById<CropImageView>(Resource.Id.image);

            showStorageToast(this);

            Bundle extras = Intent.Extras;

            if (extras != null)
            {
                imagePath = extras.GetString("image-path");

                saveUri = getImageUri(imagePath);
                if (extras.GetString(MediaStore.ExtraOutput) != null)
                {
                    saveUri = getImageUri(extras.GetString(MediaStore.ExtraOutput));
                }

                bitmap = getBitmap(imagePath);

                aspectX = extras.GetInt("aspectX");
                aspectY = extras.GetInt("aspectY");
                outputX = extras.GetInt("outputX");
                outputY = extras.GetInt("outputY");
                scale = extras.GetBoolean("scale", true);
                scaleUp = extras.GetBoolean("scaleUpIfNeeded", true);

                if (extras.GetString("outputFormat") != null)
                {
                    outputFormat = Bitmap.CompressFormat.ValueOf(extras.GetString("outputFormat"));
                }
            }

            if (bitmap == null)
            {
                Finish();
                return;
            }

            Window.AddFlags(WindowManagerFlags.Fullscreen);


            FindViewById<Button>(Resource.Id.cancel).Click += (sender, e) =>
            {
                CropImageServiceAndroid.SetResult(new CropResult(false) { Message = "Cancelled" });
                Finish();
                return;
            };
            FindViewById<Button>(Resource.Id.done).Click += (sender, e) => { OnSaveClicked(); };

            FindViewById<Button>(Resource.Id.rotateLeft).Click += (o, e) =>
            {
                bitmap = Util.rotateImage(bitmap, -90);
                RotateBitmap rotateBitmap = new RotateBitmap(bitmap);
                imageView.SetImageRotateBitmapResetBase(rotateBitmap, true);
                addHighlightView();
            };

            //FindViewById<Button>(Resource.Id.rotateRight).Click += (o, e) =>
            //{
            //    bitmap = Util.rotateImage(bitmap, 90);
            //    RotateBitmap rotateBitmap = new RotateBitmap(bitmap);
            //    imageView.SetImageRotateBitmapResetBase(rotateBitmap, true);
            //    addHighlightView();
            //};

            imageView.SetImageBitmapResetBase(bitmap, true);
            addHighlightView();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (bitmap != null && bitmap.IsRecycled)
            {
                bitmap.Recycle();
            }
        }

        //protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        //{
        //    base.OnActivityResult(requestCode, resultCode, data);
        //    if (resultCode != Result.Ok)
        //    {
        //        CropImageServiceAndroid.SetResult(new CropResult(false) { Message = resultCode.ToString() });
        //        Finish();
        //        return;
        //    }

        //    switch (requestCode)
        //    {
        //        case RequestCodeCamera:
        //            if (_maxWidth > 0 && _maxHeight > 0)
        //                ImageResizer.ResizeImage(this, _filePath, _filePath, _maxWidth, _maxHeight);
        //            MediaServiceAndroid.SetResult(new MediaResult(true) { FilePath = _filePath });
        //            break;
        //        case RequestCodeGallery:
        //            MediaServiceAndroid.SetResult(new MediaResult(true) { FilePath = RealPathHelper.GetPath(this, data.Data) });
        //            break;
        //    }
        //    Finish();
        //}

        #endregion

        #region Private helpers

        private void addHighlightView()
        {
            Crop = new HighlightView(imageView);

            int width = bitmap.Width;
            int height = bitmap.Height;

            Rect imageRect = new Rect(0, 0, width, height);

            // make the default size about 4/5 of the width or height
            int cropWidth = Math.Min(width, height) * 4 / 5;
            int cropHeight = cropWidth;

            if (aspectX != 0 && aspectY != 0)
            {
                if (aspectX > aspectY)
                {
                    cropHeight = cropWidth * aspectY / aspectX;
                }
                else
                {
                    cropWidth = cropHeight * aspectX / aspectY;
                }
            }

            int x = (width - cropWidth) / 2;
            int y = (height - cropHeight) / 2;

            RectF cropRect = new RectF(x, y, x + cropWidth, y + cropHeight);
            Crop.Setup(imageView.ImageMatrix, imageRect, cropRect, aspectX != 0 && aspectY != 0);

            imageView.ClearHighlightViews();
            Crop.Focused = true;
            imageView.AddHighlightView(Crop);
        }

        private Android.Net.Uri getImageUri(String path)
        {
            return Android.Net.Uri.FromFile(new Java.IO.File(path));
        }

        private Bitmap getBitmap(String path)
        {
            var uri = getImageUri(path);
            System.IO.Stream ins = null;

            try
            {
                int IMAGE_MAX_SIZE = 1024;
                ins = ContentResolver.OpenInputStream(uri);

                // Decode image size
                BitmapFactory.Options o = new BitmapFactory.Options();
                o.InJustDecodeBounds = true;

                BitmapFactory.DecodeStream(ins, null, o);
                ins.Close();

                int scale = 1;
                if (o.OutHeight > IMAGE_MAX_SIZE || o.OutWidth > IMAGE_MAX_SIZE)
                {
                    scale = (int)Math.Pow(2, (int)Math.Round(Math.Log(IMAGE_MAX_SIZE / (double)Math.Max(o.OutHeight, o.OutWidth)) / Math.Log(0.5)));
                }

                BitmapFactory.Options o2 = new BitmapFactory.Options();
                o2.InSampleSize = scale;
                ins = ContentResolver.OpenInputStream(uri);
                Bitmap b = BitmapFactory.DecodeStream(ins, null, o2);
                ins.Close();

                return b;
            }
            catch (Exception e)
            {
                Log.Error(GetType().Name, e.Message);
            }

            return null;
        }

        private void OnSaveClicked()
        {
            // TODO this code needs to change to use the decode/crop/encode single
            // step api so that we don't require that the whole (possibly large)
            // bitmap doesn't have to be read into memory
            if (Saving)
            {
                return;
            }

            Saving = true;

            var r = Crop.CropRect;

            int width = r.Width();
            int height = r.Height();

            Bitmap croppedImage = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
            {
                Canvas canvas = new Canvas(croppedImage);
                Rect dstRect = new Rect(0, 0, width, height);
                canvas.DrawBitmap(bitmap, r, dstRect, null);
            }

            // If the output is required to a specific size then scale or fill
            if (outputX != 0 && outputY != 0)
            {
                if (scale)
                {
                    // Scale the image to the required dimensions
                    Bitmap old = croppedImage;
                    croppedImage = Util.transform(new Matrix(),
                                                  croppedImage, outputX, outputY, scaleUp);
                    if (old != croppedImage)
                    {
                        old.Recycle();
                    }
                }
                else
                {
                    // Don't scale the image crop it to the size requested.
                    // Create an new image with the cropped image in the center and
                    // the extra space filled.              
                    Bitmap b = Bitmap.CreateBitmap(outputX, outputY,
                                                   Bitmap.Config.Rgb565);
                    Canvas canvas = new Canvas(b);

                    Rect srcRect = Crop.CropRect;
                    Rect dstRect = new Rect(0, 0, outputX, outputY);

                    int dx = (srcRect.Width() - dstRect.Width()) / 2;
                    int dy = (srcRect.Height() - dstRect.Height()) / 2;

                    // If the srcRect is too big, use the center part of it.
                    srcRect.Inset(Math.Max(0, dx), Math.Max(0, dy));

                    // If the dstRect is too big, use the center part of it.
                    dstRect.Inset(Math.Max(0, -dx), Math.Max(0, -dy));

                    // Draw the cropped bitmap in the center
                    canvas.DrawBitmap(bitmap, srcRect, dstRect, null);

                    // Set the cropped bitmap as the new bitmap
                    croppedImage.Recycle();
                    croppedImage = b;
                }
            }

            // Return the cropped image directly or save it to the specified URI.
            Bundle myExtras = Intent.Extras;

            if (myExtras != null &&
                (myExtras.GetParcelable("data") != null || myExtras.GetBoolean("return-data")))
            {
                Bundle extras = new Bundle();
                extras.PutParcelable("data", croppedImage);
                SetResult(Result.Ok, (new Intent()).SetAction("inline-data").PutExtras(extras));
                CropImageServiceAndroid.SetResult(new CropResult(true) { FilePath = saveUri.Path, Message = "Image cropped successfully" });
                Finish();
            }
            else
            {
                Bitmap b = croppedImage;
                BackgroundJob.StartBackgroundJob(this, null, "Saving image", () => SaveOutput(b), mHandler);
            }
        }

        private void SaveOutput(Bitmap croppedImage)
        {
            if (saveUri != null)
            {
                try
                {
                    using (var outputStream = ContentResolver.OpenOutputStream(saveUri))
                    {
                        if (outputStream != null)
                        {
                            croppedImage.Compress(outputFormat, 75, outputStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(this.GetType().Name, ex.Message);
                }

                Bundle extras = new Bundle();
                SetResult(Result.Ok, new Intent(saveUri.ToString())
                          .PutExtras(extras));
                CropImageServiceAndroid.SetResult(new CropResult(true) { FilePath = saveUri.Path, Message = "Image cropped successfully" });

            }
            else
            {
                Log.Error(this.GetType().Name, "not defined image url");
            }
            croppedImage.Recycle();
            Finish();
        }

        private static void showStorageToast(Activity activity)
        {
            showStorageToast(activity, calculatePicturesRemaining());
        }

        private static void showStorageToast(Activity activity, int remaining)
        {
            string noStorageText = null;

            if (remaining == NO_STORAGE_ERROR)
            {
                String state = Android.OS.Environment.ExternalStorageState;
                if (state == Android.OS.Environment.MediaChecking)
                {
                    noStorageText = "Preparing card";
                }
                else
                {
                    noStorageText = "No storage card";
                }
            }
            else if (remaining < 1)
            {
                noStorageText = "Not enough space";
            }

            if (noStorageText != null)
            {
                Toast.MakeText(activity, noStorageText, ToastLength.Long).Show();
            }
        }

        private static int calculatePicturesRemaining()
        {
            try
            {
                string storageDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).ToString();
                StatFs stat = new StatFs(storageDirectory);
                float remaining = ((float)stat.AvailableBlocks
                                   * (float)stat.BlockSize) / 400000F;
                return (int)remaining;
            }
            catch (Exception)
            {
                // if we can't stat the filesystem then we don't know how many
                // pictures are remaining.  it might be zero but just leave it
                // blank since we really don't know.
                return CANNOT_STAT_ERROR;
            }
        }

        #endregion
    }
}
