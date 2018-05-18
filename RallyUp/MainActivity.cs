using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using RallyUpLibrary;
using System;
using Android.Gms.Common;
using Firebase.Messaging;
using Firebase.Iid;
using Android.Util;

namespace RallyUp
{
    [Activity(Label = "RallyUp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private TcpClient socket;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Locate ping button
            Button pingButton = FindViewById<Button>(Resource.Id.pingButton);

            // Connect to Rally Up Server
            socket = new TcpClient("10.0.0.6", 3292);
            socket.WriteString("Jane");

            FirebaseMessaging.Instance.SubscribeToTopic("Testing");

            pingButton.Click += delegate
            {
                socket.WriteString("Ping");
                Log.Debug("MainActivity", "InstanceID token: " + FirebaseInstanceId.Instance.Token);
            };
        }
    }
}

