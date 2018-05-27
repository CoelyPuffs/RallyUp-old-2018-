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

            TextView errorBox = FindViewById<TextView>(Resource.Id.loginErrorBox);
            EditText userBox = FindViewById<EditText>(Resource.Id.userBox);
            EditText passBox = FindViewById<EditText>(Resource.Id.passBox);
            Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
            Button registerButton = FindViewById<Button>(Resource.Id.registerButton);

            loginButton.Click += delegate
            {
                try
                {
                    socket = new TcpClient("10.0.0.2", 3292);
                    //socket.WriteString("JaneLogin");
                    errorBox.Text = "";
                    socket.WriteString("Login:" + userBox.Text + ':' + passBox.Text);
                    string returnString = socket.ReadString();
                    if (returnString == "ValidCredentials")
                    {
                        StartActivity(typeof(PingActivity));
                    }
                    else if (returnString == "BadPassword")
                    {
                        errorBox.Text = "Incorrect Password.";
                    }
                    else if (returnString == "noUser")
                    {
                        errorBox.Text = "Invalid Username.";
                    }
                    socket.Close();
                }
                catch
                {
                    errorBox.Text = "Server connection failed. Make sure you're online.";
                }
            };

            registerButton.Click += delegate
            {
                StartActivity(typeof(RegistrationActivity));
            };
        }
    }
}