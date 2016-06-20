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
    public class BluetoothDeviceAdapter : BaseAdapter<BluetoothDevice>
    {
        Activity context;
        public List<BluetoothDevice> devices;

        private int mResource;
        public BluetoothDeviceAdapter(Activity act, int resource) : base()
        {
            context = act;
            mResource = resource;
            this.devices = new List<BluetoothDevice>();
        }

        public override int Count
        {
            get { return devices.Count; }
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
            text.Text = device.Name + "\n" + device.Address;
            return view;
        }
    }
}