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

using Android.Graphics;
using System;

namespace Xamarians.CropImage.Droid
{
    public class Util
    {
        // Rotates the bitmap by the specified degree.
        // If a new bitmap is created, the original bitmap is recycled.
        public static Bitmap rotateImage(Bitmap b, int degrees)
        {
            if (degrees != 0 && b != null)
            {
                Matrix m = new Matrix();
                m.SetRotate(degrees,
                        (float)b.Width / 2, (float)b.Height / 2);
                try
                {
                    Bitmap b2 = Bitmap.CreateBitmap(
                            b, 0, 0, b.Width, b.Height, m, true);
                    if (b != b2)
                    {
                        b.Recycle();
                        b = b2;
                    }
                }
                catch (Java.Lang.OutOfMemoryError)
                {
                    // We have no memory to rotate. Return the original bitmap.
                }
            }

            return b;
        }

        public static Bitmap transform(Matrix scaler,
                                       Bitmap source,
                                       int targetWidth,
                                       int targetHeight,
                                       bool scaleUp)
        {
            int deltaX = source.Width - targetWidth;
            int deltaY = source.Height - targetHeight;

            if (!scaleUp && (deltaX < 0 || deltaY < 0))
            {
                // In this case the bitmap is smaller, at least in one dimension,
                // than the target.  Transform it by placing as much of the image
                // as possible into the target and leaving the top/bottom or
                // left/right (or both) black.
                Bitmap b2 = Bitmap.CreateBitmap(targetWidth, targetHeight,
                                                Bitmap.Config.Argb8888);
                Canvas c = new Canvas(b2);

                int deltaXHalf = Math.Max(0, deltaX / 2);
                int deltaYHalf = Math.Max(0, deltaY / 2);

                Rect src = new Rect(
                    deltaXHalf,
                    deltaYHalf,
                    deltaXHalf + Math.Min(targetWidth, source.Width),
                    deltaYHalf + Math.Min(targetHeight, source.Height));

                int dstX = (targetWidth - src.Width()) / 2;
                int dstY = (targetHeight - src.Height()) / 2;

                Rect dst = new Rect(
                    dstX,
                    dstY,
                    targetWidth - dstX,
                    targetHeight - dstY);

                c.DrawBitmap(source, src, dst, null);
                return b2;
            }

            float bitmapWidthF = source.Width;
            float bitmapHeightF = source.Height;

            float bitmapAspect = bitmapWidthF / bitmapHeightF;
            float viewAspect = (float)targetWidth / targetHeight;

            if (bitmapAspect > viewAspect)
            {
                float scale = targetHeight / bitmapHeightF;
                if (scale < .9F || scale > 1F)
                {
                    scaler.SetScale(scale, scale);
                }
                else
                {
                    scaler = null;
                }
            }
            else
            {
                float scale = targetWidth / bitmapWidthF;

                if (scale < .9F || scale > 1F)
                {
                    scaler.SetScale(scale, scale);
                }
                else
                {
                    scaler = null;
                }
            }

            Bitmap b1;

            if (scaler != null)
            {
                // this is used for minithumb and crop, so we want to filter here.
                b1 = Bitmap.CreateBitmap(source, 0, 0,
                                         source.Width, source.Height, scaler, true);
            }
            else
            {
                b1 = source;
            }

            int dx1 = Math.Max(0, b1.Width - targetWidth);
            int dy1 = Math.Max(0, b1.Height - targetHeight);

            Bitmap b3 = Bitmap.CreateBitmap(
                b1,
                dx1 / 2,
                dy1 / 2,
                targetWidth,
                targetHeight);

            if (b1 != source)
            {
                b1.Recycle();
            }

            return b3;
        }
    }
}