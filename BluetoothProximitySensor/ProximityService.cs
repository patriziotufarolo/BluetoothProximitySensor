using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Android.Util;
using Java.Util;
using Android.Preferences;
using System.Runtime.CompilerServices;
using Android.Media;

namespace BluetoothProximitySensor
{
    [Service]
    class ProximityService: Service
    {
        private int mState;
        readonly string TAG = "ProximityService";

        public readonly int STATE_NONE = 1;
        public readonly int STATE_LISTEN = 2;
        public readonly int STATE_CONNECTING = 3;
        public readonly int STATE_CONNECTED = 4;

        public readonly int MESSAGE_STATE_CHANGED = 1;

        private BluetoothAdapter adapter;
        private BluetoothSocket _bluetoothRfcomm;
        public BluetoothConnectThread btConnectThread;
        private BluetoothConnectedThread btConnectedThread;

        private static Handler mHandler = null;
        IBinder binder;

        public ProximityService() { }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void setState(int state)
        {
            mState = state;
            if (mHandler != null)
            {
                mHandler.ObtainMessage(MESSAGE_STATE_CHANGED, state, -1).SendToTarget();
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new ProximityServiceBinder(this);
            return binder;
        }
  
        public override void OnCreate()
        {
            base.OnCreate();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            String macAddress;
            ISharedPreferences prefs = GetSharedPreferences("BluetoothProximitySensor", FileCreationMode.Private);
            macAddress = prefs.GetString("device_address", null);
            adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter != null && adapter.IsEnabled)
            {
                if (macAddress != null && macAddress.Length > 0)
                {
                    connectToDevice(macAddress);
                }
            }
            return StartCommandResult.Sticky;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void connectToDevice(String address)
        {
            BluetoothDevice device = adapter.GetRemoteDevice(address);
            if (mState == STATE_CONNECTING)
            {
                if (btConnectThread != null) { 
                    btConnectThread.cancel();
                    btConnectThread = null;
                }
            }
            if (btConnectedThread != null)
            {
                btConnectedThread.cancel();
                btConnectedThread = null;
            }
            if (adapter != null) adapter.CancelDiscovery();
            btConnectThread = new BluetoothConnectThread(adapter, device, this);
            btConnectThread.Start();
            setState(STATE_CONNECTING);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void stop()
        {
            setState(STATE_NONE);
            if (btConnectThread != null)
            {
                btConnectThread.cancel();
                btConnectThread = null;
            }
            if (btConnectedThread != null)
            {
                btConnectedThread.cancel();
                btConnectedThread = null;
            }
            if (adapter != null) adapter.CancelDiscovery();
            StopSelf();
        }

        public override bool StopService(Intent name)
        {
            setState(STATE_NONE);
            if (btConnectThread != null)
            {
                btConnectThread.cancel();
                btConnectThread = null;
            }
            if (btConnectedThread != null)
            {
                btConnectedThread.cancel();
                btConnectedThread = null;
            }
            if (adapter != null) adapter.CancelDiscovery();
            return base.StopService(name);
        }

        public override void OnDestroy()
        {
            stop();
            base.OnDestroy();
        }

        private void connectionFailed()
        {
            stop();
            Toast.MakeText(this, "Unable to connect to device", ToastLength.Short).Show();
        }

        private void connectionLost()
        {
            stop();
            Toast.MakeText(this, "Connection to device lost", ToastLength.Short).Show();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void connected(BluetoothSocket socket, BluetoothDevice mDevice)
        {
            if (btConnectThread != null)
            {
                btConnectThread.cancel();
                btConnectThread = null;
            }
            if (btConnectedThread != null)
            {
                btConnectedThread.cancel();
                btConnectedThread = null;
            }

            var nMgr = (NotificationManager)GetSystemService(NotificationService);

            btConnectedThread = new BluetoothConnectedThread(socket, nMgr, this);
            btConnectedThread.Start();
            setState(STATE_CONNECTED);
        }
    }
}