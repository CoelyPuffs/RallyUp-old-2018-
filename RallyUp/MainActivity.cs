using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "RallyUp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private TcpClient socket;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            ActionBar.Hide();
            socket = new TcpClient("10.0.0.6", 3292);
            socket.WriteString("JaneLogin");

            EditText userBox = FindViewById<EditText>(Resource.Id.userBox);
            EditText passBox = FindViewById<EditText>(Resource.Id.passBox);
            Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
            Button registerButton = FindViewById<Button>(Resource.Id.registerButton);

            loginButton.Click += delegate 
            {
                socket.WriteString("Login:" + userBox.Text + ':' + passBox.Text);
                if (socket.ReadString() == "ValidCredentials")
                {
                    StartActivity(typeof(PingActivity));
                }
            };

            registerButton.Click += delegate 
            {
                socket.Close();
                StartActivity(typeof(RegistrationActivity));
            };
        }
    }
}