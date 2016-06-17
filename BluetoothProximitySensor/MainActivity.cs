using System;
using System.Collections;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Util;

namespace BluetoothProximitySensor
{

    public class BluetoothDeviceAdapter : BaseAdapter<BluetoothDevice>
    {
        Activity context;
        bool NoDevices = false;
        public List<BluetoothDevice> devices = new List<BluetoothDevice>();
        private int mResource;
        public BluetoothDeviceAdapter(Activity act, int resource): base()
        {
            context = act;
            mResource = resource;
        }

        public override int Count
        {
            get { return devices.Count;  }
        }

        public override BluetoothDevice this[int position]
        {
            get
            {
                return devices[position];
            }
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

        public void setNoDevices()
        {
            NoDevices = true;
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
            text.Text = device.Name + "\n" +device.Address + ((device.BondState == Bond.Bonded) ? "\nAlready paired" : "");
            return view;
        }
    }

    [Activity(Label = "@string/title_main", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private BluetoothAdapter btAdapter;
        public BluetoothDeviceAdapter devicesAdapter;
        private ArrayAdapter<String> mNewDevicesArrayAdapter;
        private Receiver receiver;
        private IntentFilter filter = new IntentFilter();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //RequestWindowFeature(WindowFeatures.IndeterminateProgress);

            SetContentView(Resource.Layout.Main);
            btAdapter = BluetoothAdapter.DefaultAdapter;
            devicesAdapter = new BluetoothDeviceAdapter(this, Resource.Layout.DevicesListItem);

            mNewDevicesArrayAdapter = new ArrayAdapter<String>(this, Resource.Layout.DevicesListItem);
            

            var buttonScan = FindViewById<Button>(Resource.Id.button_scan);
            buttonScan.Click += (sender, e) =>
            {
                DoDiscovery();
            };
            
            var devicesListView = FindViewById<ListView>(Resource.Id.devices);
            devicesListView.Adapter = devicesAdapter;
            devicesListView.ItemClick += DeviceListClick;

            receiver = new Receiver(this);
           
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver(receiver, filter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btAdapter != null)
            {
                btAdapter.CancelDiscovery();
            }
            UnregisterReceiver(receiver);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(receiver, filter);
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(receiver);
        }

        private void DoDiscovery()
        {
            FindViewById<View>(Resource.Id.title_devices).Visibility = ViewStates.Visible;
            if (btAdapter.IsDiscovering)
            {
                btAdapter.CancelDiscovery();
            }
            btAdapter.StartDiscovery();
        }

        private void DeviceListClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            btAdapter.CancelDiscovery();
            var info = (e.View as TextView).Text.ToString();
        }

        public class Receiver : BroadcastReceiver
        {
            MainActivity _activity;
            public Receiver(MainActivity act)
            {
                _activity = act;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                switch (action)
                {
                    case (BluetoothDevice.ActionFound):
                        BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        _activity.devicesAdapter.devices.Add(device);
                        _activity.devicesAdapter.NotifyDataSetChanged();
                        Toast foundDeviceToast = Toast.MakeText(_activity, "Device found\n " + device.Name + "\n" + device.Address , ToastLength.Short);
                        foundDeviceToast.Show();
                        _activity.mNewDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
                        break;
                    case (BluetoothAdapter.ActionDiscoveryFinished):
                        if (_activity.devicesAdapter.Count == 0)
                        {
                            _activity.devicesAdapter.setNoDevices();
                            Toast noDeviceToast = Toast.MakeText(_activity, "No device found", ToastLength.Short);
                            var noDevices = "No devices";
                            _activity.mNewDevicesArrayAdapter.Add(noDevices);
                            noDeviceToast.Show();
                        }
                        break;
                }
            }
        }
    }
}

