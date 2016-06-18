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

    public class BluetoothDeviceAdapter : BaseAdapter<BluetoothDevice>
    {
        Activity context;
        public List<BluetoothDevice> devices;

        private int mResource;
        public BluetoothDeviceAdapter(Activity act, int resource): base()
        {
            context = act;
            mResource = resource;
            this.devices = new List<BluetoothDevice>();
        }

        public override int Count
        {
            get { return devices.Count;  }
        }

        public override BluetoothDevice this[int position]
        {
            get { return devices[position]; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return devices[position];
        }
        public override long GetItemId(int position)
        {
            return position;
        }

        public void Add(BluetoothDevice device)
        {
            devices.Add(device);
            NotifyDataSetChanged();
        }
        
        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
        }


        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view;
            TextView text;

            if (convertView == null)
            {
                view = context.LayoutInflater.Inflate(mResource, parent, false); 
            }
            else
            {
                view = convertView;
            }

            text = (TextView)view;
            BluetoothDevice item = (BluetoothDevice)GetItem(position);
  

            var device = devices[position];
            text.Text = device.Name + "\n" +device.Address;
            return view;
        }
    }

    [Activity(Label = "@string/title_main", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private BluetoothAdapter btAdapter;
        public BluetoothDeviceAdapter devicesAdapter;
        public BluetoothDeviceAdapter pairedDevicesAdapter;
        
        private IntentFilter filter = new IntentFilter();

        ISharedPreferences prefs;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //RequestWindowFeature(WindowFeatures.IndeterminateProgress);

            SetContentView(Resource.Layout.Main);


            var bluetoothChooseDeviceButton = FindViewById<Button>(Resource.Id.button_bluetooth_choose_device);
            bluetoothChooseDeviceButton.Click += (sender, e) => {
                Intent chooseBluetoothDevice = new Intent(this, typeof(BluetoothDeviceChooseActivity));
                StartActivity(chooseBluetoothDevice);
            };

            var openSettingsButton = FindViewById<Button>(Resource.Id.button_open_settings);

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

