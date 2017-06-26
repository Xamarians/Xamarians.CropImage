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

using System;
using Android.Graphics;
using Android.Util;

namespace Xamarians.CropImage.Droid
{
    public class RotateBitmap
    {
        public const string TAG = "RotateBitmap";

        public RotateBitmap(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public RotateBitmap(Bitmap bitmap, int rotation)
        {
            Bitmap = bitmap;
            Rotation = rotation % 360;
        }

        public int Rotation
        {
            get;
            set;
        }

        public Bitmap Bitmap
        {
            get;
            set;
        }

        public Matrix GetRotateMatrix()
        {
            // By default this is an identity matrix.
            Matrix matrix = new Matrix();

            if (Rotation != 0)
            {
                // We want to do the rotation at origin, but since the bounding
                // rectangle will be changed after rotation, so the delta values
                // are based on old & new width/height respectively.
                int cx = Bitmap.Width / 2;
                int cy = Bitmap.Height / 2;
                matrix.PreTranslate(-cx, -cy);
                matrix.PostRotate(Rotation);
                matrix.PostTranslate(Width / 2, Height / 2);
            }

            return matrix;
        }

        public bool IsOrientationChanged
        {
            get
            {
                return (Rotation / 90) % 2 != 0;
            }
        }

        public int Height
        {
            get
            {
                if (IsOrientationChanged)
                {
                    return Bitmap.Width;
                }
                else
                {
                    return Bitmap.Height;
                }
            }
        }

        public int Width
        {
            get
            {
                if (IsOrientationChanged)
                {
                    return Bitmap.Height;
                }
                else
                {
                    return Bitmap.Width;
                }
            }
        }

        // TOOD: Recyle
    }
}