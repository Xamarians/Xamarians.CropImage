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

using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace Xamarians.CropImage.Droid
{
    public abstract class ImageViewTouchBase : ImageView
    {
        #region Members

        // This is the base transformation which is used to show the image
        // initially.  The current computation for this shows the image in
        // it's entirety, letterboxing as needed.  One could choose to
        // show the image as cropped instead.
        //
        // This matrix is recomputed when we go from the thumbnail image to
        // the full size image.
        protected Matrix baseMatrix = new Matrix();

        // This is the supplementary transformation which reflects what
        // the user has done in terms of zooming and panning.
        //
        // This matrix remains the same when we go from the thumbnail image
        // to the full size image.
        protected Matrix suppMatrix = new Matrix();

        // This is the final matrix which is computed as the concatentation
        // of the base matrix and the supplementary matrix.
        private Matrix displayMatrix = new Matrix();

        // Temporary buffer used for getting the values out of a matrix.
        private float[] matrixValues = new float[9];

        // The current bitmap being displayed.
        protected RotateBitmap bitmapDisplayed = new RotateBitmap(null);

        private int thisWidth = -1;
        private int thisHeight = -1;
        const float SCALE_RATE = 1.25F;

        private float maxZoom;

        private Handler handler = new Handler();

        private Action onLayoutRunnable = null;

        #endregion

        #region Constructor

        public ImageViewTouchBase(Context context)
            : base(context)
        {
            init();
        }

        public ImageViewTouchBase(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            init();
        }

        #endregion

        #region Private helpers

        private void init()
        {
            SetScaleType(ImageView.ScaleType.Matrix);
        }

        private void setImageBitmap(Bitmap bitmap, int rotation)
        {
            base.SetImageBitmap(bitmap);
            var d = Drawable;
            if (d != null)
            {
                d.SetDither(true);
            }

            Bitmap old = bitmapDisplayed.Bitmap;
            bitmapDisplayed.Bitmap = bitmap;
            bitmapDisplayed.Rotation = rotation;
        }

        #endregion

        #region Public methods

        public void Clear()
        {
            SetImageBitmapResetBase(null, true);
        }

        /// <summary>
        /// This function changes bitmap, reset base matrix according to the size
        /// of the bitmap, and optionally reset the supplementary matrix.
        /// </summary>
        public void SetImageBitmapResetBase(Bitmap bitmap, bool resetSupp)
        {
            SetImageRotateBitmapResetBase(new RotateBitmap(bitmap), resetSupp);
        }

        public void SetImageRotateBitmapResetBase(RotateBitmap bitmap, bool resetSupp)
        {
            int viewWidth = Width;

            if (viewWidth <= 0)
            {
                onLayoutRunnable = () =>
                {
                    SetImageRotateBitmapResetBase(bitmap, resetSupp);
                };

                return;
            }

            if (bitmap.Bitmap != null)
            {
                getProperBaseMatrix(bitmap, baseMatrix);
                setImageBitmap(bitmap.Bitmap, bitmap.Rotation);
            }
            else
            {
                baseMatrix.Reset();
                base.SetImageBitmap(null);
            }

            if (resetSupp)
            {
                suppMatrix.Reset();
            }
            ImageMatrix = GetImageViewMatrix();
            this.maxZoom = CalculateMaxZoom();
        }


        #endregion

        #region Overrides

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            this.IvLeft = left;
            this.IvRight = right;
            this.IvTop = top;
            this.IvBottom = bottom;

            thisWidth = right - left;
            thisHeight = bottom - top;

            var r = onLayoutRunnable;

            if (r != null)
            {
                onLayoutRunnable = null;
                r();
            }

            if (bitmapDisplayed.Bitmap != null)
            {
                getProperBaseMatrix(bitmapDisplayed, baseMatrix);
                ImageMatrix = GetImageViewMatrix();
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && GetScale() > 1.0f)
            {
                // If we're zoomed in, pressing Back jumps out to show the entire
                // image, otherwise Back returns the user to the gallery.
                ZoomTo(1.0f);
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            setImageBitmap(bm, 0);
        }

        #endregion

        #region Properties

        public int IvLeft { get; private set; }

        public int IvRight { get; private set; }

        public int IvTop { get; private set; }

        public int IvBottom { get; private set; }

        #endregion

        #region Protected methods

        protected float GetValue(Matrix matrix, int whichValue)
        {
            matrix.GetValues(matrixValues);
            return matrixValues[whichValue];
        }

        /// <summary>
        /// Get the scale factor out of the matrix.
        /// </summary>
        protected float GetScale(Matrix matrix)
        {
            return GetValue(matrix, Matrix.MscaleX);
        }

        protected float GetScale()
        {
            return GetScale(suppMatrix);
        }

        /// <summary>
        /// Setup the base matrix so that the image is centered and scaled properly.
        /// </summary>
        private void getProperBaseMatrix(RotateBitmap bitmap, Matrix matrix)
        {
            float viewWidth = Width;
            float viewHeight = Height;

            float w = bitmap.Width;
            float h = bitmap.Height;
            int rotation = bitmap.Rotation;
            matrix.Reset();

            // We limit up-scaling to 2x otherwise the result may look bad if it's
            // a small icon.
            float widthScale = Math.Min(viewWidth / w, 2.0f);
            float heightScale = Math.Min(viewHeight / h, 2.0f);
            float scale = Math.Min(widthScale, heightScale);

            matrix.PostConcat(bitmap.GetRotateMatrix());
            matrix.PostScale(scale, scale);

            matrix.PostTranslate(
                (viewWidth - w * scale) / 2F,
                (viewHeight - h * scale) / 2F);
        }

        // Combine the base matrix and the supp matrix to make the final matrix.
        protected Matrix GetImageViewMatrix()
        {
            // The final matrix is computed as the concatentation of the base matrix
            // and the supplementary matrix.
            displayMatrix.Set(baseMatrix);
            displayMatrix.PostConcat(suppMatrix);

            return displayMatrix;
        }

        // Sets the maximum zoom, which is a scale relative to the base matrix. It
        // is calculated to show the image at 400% zoom regardless of screen or
        // image orientation. If in the future we decode the full 3 megapixel image,
        // rather than the current 1024x768, this should be changed down to 200%.
        protected float CalculateMaxZoom()
        {
            if (bitmapDisplayed.Bitmap == null)
            {
                return 1F;
            }

            float fw = (float)bitmapDisplayed.Width / (float)thisWidth;
            float fh = (float)bitmapDisplayed.Height / (float)thisHeight;
            float max = Math.Max(fw, fh) * 4;

            return max;
        }

        protected virtual void ZoomTo(float scale, float centerX, float centerY)
        {
            if (scale > maxZoom)
            {
                scale = maxZoom;
            }

            float oldScale = GetScale();
            float deltaScale = scale / oldScale;

            suppMatrix.PostScale(deltaScale, deltaScale, centerX, centerY);
            ImageMatrix = GetImageViewMatrix();
            Center(true, true);
        }

        protected void ZoomTo(float scale, float centerX,
                               float centerY, float durationMs)
        {
            float incrementPerMs = (scale - GetScale()) / durationMs;
            float oldScale = GetScale();

            long startTime = System.Environment.TickCount;

            Action anim = null;

            anim = () =>
            {
                long now = System.Environment.TickCount;
                float currentMs = Math.Min(durationMs, now - startTime);
                float target = oldScale + (incrementPerMs * currentMs);
                ZoomTo(target, centerX, centerY);

                if (currentMs < durationMs)
                {
                    handler.Post(anim);
                }
            };

            handler.Post(anim);
        }

        protected void ZoomTo(float scale)
        {
            float cx = Width / 2F;
            float cy = Height / 2F;

            ZoomTo(scale, cx, cy);
        }

        protected virtual void ZoomIn()
        {
            ZoomIn(SCALE_RATE);
        }

        protected virtual void ZoomOut()
        {
            ZoomOut(SCALE_RATE);
        }

        protected virtual void ZoomIn(float rate)
        {
            if (GetScale() >= maxZoom)
            {
                // Don't let the user zoom into the molecular level.
                return;
            }

            if (bitmapDisplayed.Bitmap == null)
            {
                return;
            }

            float cx = Width / 2F;
            float cy = Height / 2F;

            suppMatrix.PostScale(rate, rate, cx, cy);
            ImageMatrix = GetImageViewMatrix();
        }

        protected void ZoomOut(float rate)
        {
            if (bitmapDisplayed.Bitmap == null)
            {
                return;
            }

            float cx = Width / 2F;
            float cy = Height / 2F;

            // Zoom out to at most 1x.
            Matrix tmp = new Matrix(suppMatrix);
            tmp.PostScale(1F / rate, 1F / rate, cx, cy);

            if (GetScale(tmp) < 1F)
            {
                suppMatrix.SetScale(1F, 1F, cx, cy);
            }
            else
            {
                suppMatrix.PostScale(1F / rate, 1F / rate, cx, cy);
            }

            ImageMatrix = GetImageViewMatrix();
            Center(true, true);
        }

        protected virtual void PostTranslate(float dx, float dy)
        {
            suppMatrix.PostTranslate(dx, dy);
        }

        protected void PanBy(float dx, float dy)
        {
            PostTranslate(dx, dy);
            ImageMatrix = GetImageViewMatrix();
        }


        /// <summary>
        /// Center as much as possible in one or both axis.  Centering is
        /// defined as follows:  if the image is scaled down below the
        /// view's dimensions then center it (literally).  If the image
        /// is scaled larger than the view and is translated out of view
        /// then translate it back into view (i.e. eliminate black bars).
        /// </summary>
        protected void Center(bool horizontal, bool vertical)
        {
            if (bitmapDisplayed.Bitmap == null)
            {
                return;
            }

            Matrix m = GetImageViewMatrix();

            RectF rect = new RectF(0, 0,
                                   bitmapDisplayed.Bitmap.Width,
                                   bitmapDisplayed.Bitmap.Height);

            m.MapRect(rect);

            float height = rect.Height();
            float width = rect.Width();

            float deltaX = 0, deltaY = 0;

            if (vertical)
            {
                int viewHeight = Height;
                if (height < viewHeight)
                {
                    deltaY = (viewHeight - height) / 2 - rect.Top;
                }
                else if (rect.Top > 0)
                {
                    deltaY = -rect.Top;
                }
                else if (rect.Bottom < viewHeight)
                {
                    deltaY = Height - rect.Bottom;
                }
            }

            if (horizontal)
            {
                int viewWidth = Width;
                if (width < viewWidth)
                {
                    deltaX = (viewWidth - width) / 2 - rect.Left;
                }
                else if (rect.Left > 0)
                {
                    deltaX = -rect.Left;
                }
                else if (rect.Right < viewWidth)
                {
                    deltaX = viewWidth - rect.Right;
                }
            }

            PostTranslate(deltaX, deltaY);
            ImageMatrix = GetImageViewMatrix();
        }


        #endregion
    }
}

