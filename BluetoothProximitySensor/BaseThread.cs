using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BluetoothProximitySensor
{
    abstract class BaseThread
    {
        private Thread _thread;
        protected BaseThread()
        {
            _thread = new Thread(new ThreadStart(this.RunThread));
        }

        public void Start() { _thread.Start(); }
        public void Join() { _thread.Join(); }
        public bool IsAlive
        {
            get
            {
                return _thread.IsAlive;
            }
        }
        
        public abstract void RunThread();
    }
}