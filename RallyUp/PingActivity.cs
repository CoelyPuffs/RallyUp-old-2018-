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
    [Activity(Label = "RallyUp")]
    public class PingActivity : Activity
    {
        private TcpClient socket;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PingPage);
            ActionBar.Hide();

            Button pingButton = FindViewById<Button>(Resource.Id.pingButton);

            try
            {
                socket = new TcpClient("192.168.87.44", 3292);
            }
            catch
            {
                pingButton.Text = "No connection. Try again later.";
            }

            FirebaseMessaging.Instance.SubscribeToTopic("Testing");

            pingButton.Click += delegate
            {
                socket.WriteString("Ping");
                Log.Debug("MainActivity", "InstanceID token: " + FirebaseInstanceId.Instance.Token);
            };
        }
    }
}

