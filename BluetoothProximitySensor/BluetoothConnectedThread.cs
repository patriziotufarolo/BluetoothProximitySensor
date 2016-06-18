using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Bluetooth;
using System.IO;
using Android.Media;
using Android.Graphics;

namespace BluetoothProximitySensor
{
    class BluetoothConnectedThread : BaseThread
    {
        private readonly BluetoothSocket mSocket;
        private readonly System.IO.Stream mInStream;
        private readonly String TAG = "BluetoothProximitySensorConnectedThread";
        private readonly Context context;
        private readonly NotificationManager nMgr;
        private bool thread_running = false;
        public BluetoothConnectedThread(BluetoothSocket socket, NotificationManager nMgr, Context context)
        {
            mSocket = socket;
            System.IO.Stream tmpIn = null;
            try
            {
                tmpIn = socket.InputStream;
            }
            catch (IOException)
            {
                Log.Error(TAG, "Unable to create socket stream");
            }

            mInStream = tmpIn;
            this.context = context;
            this.nMgr = nMgr;
        }

        public override void RunThread()
        {
            Log.Info(TAG, "Connected thread running");
            Log.Info(TAG, "Socket connected: " + mSocket.IsConnected);
            thread_running = true;
            while (thread_running && mSocket.IsConnected)
            {
                try
                {
                        byte[] buffer = new byte[1];
                        mInStream.Read(buffer, 0, 1);

                        if (buffer[0] == 49)
                        {
                        Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
                        Java.Text.SimpleDateFormat simpleDateFormat = new Java.Text.SimpleDateFormat("HH:mm");

                        

                        var pendingIntent = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)), 0);
                        Android.Net.Uri alarmSound = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                        var notification = new Notification.Builder(context)
                        .SetContentTitle(context.Resources.GetString(Resource.String.motion_detected))
                        .SetContentText(simpleDateFormat.Format(calendar.Time))
                        .SetSmallIcon(Resource.Drawable.Icon)
                        .SetLargeIcon(BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.Icon))
                        .SetContentIntent(pendingIntent)
                        .SetAutoCancel(true)
                        .SetSound(alarmSound) //boolean customizable
                        .SetVibrate(new long[] { 1000, 500, 1000, 500, 1000 }) //three patterns
                        .SetLights(Android.Graphics.Color.Yellow, 1000, 500) //colors and duration
                        .Build();
                        nMgr.Notify(0, notification);
                        }
                }
                catch (Exception e)
                {
                    Log.Error(TAG, e.ToString());
                }
            }
        }

        public void cancel()
        {
            thread_running = false;
            mSocket.Close();
        }
    }
}