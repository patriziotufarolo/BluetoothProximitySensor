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

namespace BluetoothProximitySensor
{
    class ProximityServiceBinder: Binder
    {
        public ProximityService Service
        {
            get { return this.service; }
        }
        protected ProximityService service;
        public bool isBound { get; set; }
        public ProximityServiceBinder (ProximityService service)
        {
            this.service = service;
        }
    }
}