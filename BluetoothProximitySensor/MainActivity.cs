using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Util;
using System.Linq;
using Android.Preferences;

namespace BluetoothProximitySensor
{
    [Activity(Label = "@string/title_main", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private IntentFilter filter = new IntentFilter();
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);


            var bluetoothChooseDeviceButton = FindViewById<Button>(Resource.Id.button_bluetooth_choose_device);
            bluetoothChooseDeviceButton.Click += (sender, e) => {
                Intent chooseBluetoothDevice = new Intent(this, typeof(BluetoothDeviceChooseActivity));
                StartActivity(chooseBluetoothDevice);
            };

            var startServiceButton = FindViewById<Button>(Resource.Id.button_start);
            startServiceButton.Click += delegate {
                StartService(new Intent(this, typeof(ProximityService)));
            };
            var stopServiceButton = FindViewById<Button>(Resource.Id.button_stop);
            stopServiceButton.Click += delegate
            {
                StopService(new Intent(this, typeof(ProximityService)));
            };

        }

    }
}

