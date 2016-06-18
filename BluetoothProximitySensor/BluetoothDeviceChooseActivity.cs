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
using Android.Bluetooth;

namespace BluetoothProximitySensor
{
    [Activity(Label = "Choose a device")]
    public class BluetoothDeviceChooseActivity : Activity
    {

        private BluetoothAdapter btAdapter;
        public BluetoothDeviceAdapter devicesAdapter;
        public BluetoothDeviceAdapter pairedDevicesAdapter;
        private Receiver receiver;
        private IntentFilter filter = new IntentFilter();

        ISharedPreferences prefs;




        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BluetoothDeviceChoose);

            prefs = GetSharedPreferences("BluetoothProximitySensor", FileCreationMode.Private);

            btAdapter = BluetoothAdapter.DefaultAdapter;
            devicesAdapter = new BluetoothDeviceAdapter(this, Resource.Layout.DevicesListItem);
            pairedDevicesAdapter = new BluetoothDeviceAdapter(this, Resource.Layout.DevicesListItem);

            var buttonScan = FindViewById<Button>(Resource.Id.button_scan);
            buttonScan.Click += (sender, e) =>
            {
                DoDiscovery();
            };

            var devicesListView = FindViewById<ListView>(Resource.Id.devices);
            devicesListView.Adapter = devicesAdapter;
            devicesListView.ItemClick += DeviceListClick;

            var pairedDevices = btAdapter.BondedDevices;

            if (pairedDevices.Count > 0)
            {
                FindViewById<TextView>(Resource.Id.title_paired_devices).Text = GetString(Resource.String.title_paired_devices);
                foreach (var device in pairedDevices)
                {
                    pairedDevicesAdapter.devices.Add(device);
                }
            }
            else
            {
                FindViewById<TextView>(Resource.Id.title_paired_devices).Text = GetString(Resource.String.title_no_paired_devices);
            }

            var pairedDevicesListView = FindViewById<ListView>(Resource.Id.paired_devices);
            pairedDevicesListView.Adapter = pairedDevicesAdapter;
            pairedDevicesListView.ItemClick += PairedDeviceListClick;


            receiver = new Receiver(this);
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            RegisterReceiver(receiver, filter);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btAdapter != null)
            {
                btAdapter.CancelDiscovery();
            }

            try
            {
                UnregisterReceiver(receiver);
            }
            catch (Java.Lang.IllegalArgumentException)
            {

            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(receiver, filter);
        }

        protected override void OnPause()
        {
            base.OnPause();
            try
            {
                UnregisterReceiver(receiver);
            }
            catch (Java.Lang.IllegalArgumentException)
            {

            }
        }

        private void DoDiscovery()
        {
            devicesAdapter.devices.Clear();
            devicesAdapter.NotifyDataSetChanged();
            FindViewById<TextView>(Resource.Id.title_devices).Text = GetString(Resource.String.title_devices);
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
            Toast deviceClickToast = Toast.MakeText(this, info, ToastLength.Short);
            deviceClickToast.Show();
            BluetoothDevice currentDevice = devicesAdapter.devices[((int)(e.Id & 0xFFFFFFFF))];

            var methods = currentDevice.GetType().GetMethod("CreateBond");
            methods.Invoke(currentDevice, null);
        }

        private void PairedDeviceListClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            btAdapter.CancelDiscovery();
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Do you want to use this device?");
            alert.SetPositiveButton("Yes", (dialog, args) => {
                ISharedPreferencesEditor mPrefEditor = prefs.Edit();
                mPrefEditor.PutString("device_address", pairedDevicesAdapter.devices[((int)(e.Id & 0xFFFFFFFF))].Address);
                Toast.MakeText(this, "Device set", ToastLength.Short).Show();
                mPrefEditor.Commit();
            });
            alert.SetNegativeButton("No", (dialog, args) => { ((AlertDialog)dialog).Dismiss(); });
            RunOnUiThread(() => alert.Show());
        }

        public class Receiver : BroadcastReceiver
        {
            BluetoothDeviceChooseActivity _activity;
            public Receiver(BluetoothDeviceChooseActivity act)
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
                        Toast foundDeviceToast = Toast.MakeText(_activity, _activity.GetString(Resource.String.device_found) + "\n " + device.Name + "\n" + device.Address, ToastLength.Short);
                        foundDeviceToast.Show();
                        break;
                    case (BluetoothAdapter.ActionDiscoveryFinished):
                        if (_activity.devicesAdapter.Count == 0)
                        {
                            _activity.FindViewById<TextView>(Resource.Id.title_devices).Text = _activity.GetString(Resource.String.none_found);
                            Toast noDeviceToast = Toast.MakeText(_activity, _activity.GetString(Resource.String.none_found), ToastLength.Short);
                            noDeviceToast.Show();
                        }
                        break;
                    case (BluetoothDevice.ActionBondStateChanged):
                        BluetoothDevice paired_device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        if (paired_device.BondState == Bond.Bonded)
                        {
                            _activity.pairedDevicesAdapter.devices.Add(paired_device);
                            Toast.MakeText(_activity, _activity.GetString(Resource.String.successfully_paired), ToastLength.Short).Show();
                            _activity.pairedDevicesAdapter.NotifyDataSetChanged();
                            if (BluetoothAdapter.DefaultAdapter.BondedDevices.Count > 0)
                            {
                                _activity.FindViewById<TextView>(Resource.Id.title_paired_devices).Text = _activity.GetString(Resource.String.title_paired_devices);
                            }
                        }
                        break;
                }
            }
        }

    }
}