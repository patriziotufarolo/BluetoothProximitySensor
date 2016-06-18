using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.IO;
using Java.Util;

namespace BluetoothProximitySensor
{
    class BluetoothConnectThread : BaseThread
    {
        private ProximityService serviceObject;

        private BluetoothSocket _bluetoothRfcomm;
        private readonly BluetoothDevice device;
        private readonly BluetoothAdapter adapter;
        private readonly string TAG = "BluetoothProximitySensorConnectThread";
        public readonly static String SPP_UUID = "00001101-0000-1000-8000-00805f9b34fb";


        public BluetoothConnectThread(BluetoothAdapter adapter, BluetoothDevice device, ProximityService serviceObject)
        {
            this.device = device;
            this.adapter = adapter;
            this.serviceObject = serviceObject;
        }

        public override void RunThread()
        {
            adapter.CancelDiscovery();
            try
            {
                _bluetoothRfcomm = device.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(SPP_UUID));
                _bluetoothRfcomm.Connect();
            }
            catch (Java.IO.IOException)
            {

                try
                {
                    Java.Lang.Reflect.Method m = device.Class.GetMethod("createRfcommSocket", new Java.Lang.Class[] { Java.Lang.Integer.Type });
                    _bluetoothRfcomm = (BluetoothSocket)m.Invoke(device, 1);
                    _bluetoothRfcomm.Connect();
                }
                catch (Exception)
                {
                    Log.Error(TAG, "Connection failed");
                }
            }
            Log.Info(TAG, "Is connected: " + _bluetoothRfcomm.IsConnected.ToString());
            serviceObject.btConnectThread = null;
            serviceObject.connected(_bluetoothRfcomm, device);
        }

        public void cancel()
        {
            try
            {
                _bluetoothRfcomm.Close();
            }
            catch (IOException e)
            {
                Log.Error(TAG, "close() failed");
            }
        }
    }
}