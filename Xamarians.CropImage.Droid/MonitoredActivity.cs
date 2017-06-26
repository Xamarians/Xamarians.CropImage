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
using System;

namespace Xamarians.CropImage.Droid
{
    public class MonitoredActivity : Activity
    {
        #region IMonitoredActivity implementation

        public event EventHandler Destroying;
        public event EventHandler Stopping;
        public event EventHandler Starting;

        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Destroying != null)
            {
                Destroying(this, EventArgs.Empty);
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (Stopping != null)
            {
                Stopping(this, EventArgs.Empty);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            if(Starting != null)
            {
                Starting(this, EventArgs.Empty);
            }
        }
    }
}

