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
using Android.OS;
using System;
using System.Threading;

namespace Xamarians.CropImage.Droid
{
	public class BackgroundJob
    {
        #region Static helpers

        public static void StartBackgroundJob(
            MonitoredActivity activity, string title,
            string message, Action job, Handler handler)
		{
			// Make the progress dialog uncancelable, so that we can gurantee
			// the thread will be done before the activity getting destroyed.
			ProgressDialog dialog = ProgressDialog.Show(activity, title, message, true, false);
			ThreadPool.QueueUserWorkItem((w) => new BackgroundJob(activity, job, dialog, handler).Run());
		}

        #endregion

        #region Members

        private MonitoredActivity activity;
		private ProgressDialog progressDialog;
		private Action job;
		private Handler handler;

        #endregion

        #region Constructor

        public BackgroundJob(MonitoredActivity activity, Action job,
		                     ProgressDialog progressDialog, Handler handler)
		{
			this.activity = activity;
			this.progressDialog = progressDialog;
			this.job = job;			
			this.handler = handler;

			activity.Destroying += (sender, e) =>  {
				// We get here only when the onDestroyed being called before
				// the cleanupRunner. So, run it now and remove it from the queue
				cleanUp();
				handler.RemoveCallbacks(cleanUp);
			};

			activity.Stopping += (sender, e) =>progressDialog.Hide();
			activity.Starting += (sender, e) => progressDialog.Show();
		}

        #endregion

        #region Methods

        public void Run()
		{
			try
			{
				job();
			}
			finally
			{
				handler.Post (cleanUp);
			}
        }

        #endregion

        #region Private helpers

        private void cleanUp()
        {
            if (progressDialog.Window != null)
            {
                progressDialog.Dismiss();
            }
        }

        #endregion
    }   
}