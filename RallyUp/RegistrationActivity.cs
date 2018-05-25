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
    [Activity(Label = "Registration")]
    public class RegistrationActivity : Activity
    {
        private TcpClient socket;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RegistrationPage);
            ActionBar.Hide();

            TextView errorBox = FindViewById<TextView>(Resource.Id.registrationErrorBox);
            EditText screenNameBox = FindViewById<EditText>(Resource.Id.screenNameBox);
            EditText newUserBox = FindViewById<EditText>(Resource.Id.newUserBox);
            EditText newPassBox = FindViewById<EditText>(Resource.Id.newPassBox);
            Button registerButton = FindViewById<Button>(Resource.Id.newRegisterButton);

            registerButton.Click += delegate
            {
                try
                {
                    socket = new TcpClient("10.0.0.6", 3292);
                    socket.WriteString("JaneRegister");
                    socket.WriteString("Register:" + newUserBox.Text + ':' + newPassBox.Text + ':' + screenNameBox.Text);
                    errorBox.Text = "";
                    if (socket.ReadString() == "RegistrationSuccessful")
                    {
                        errorBox.Text = "Registration successful. Logging you in.";
                        // TODO: save credentials, log in
                    }
                    else if (socket.ReadString() == "UsernameAlreadyRegistered")
                    {
                        errorBox.Text = "Username is already taken.";
                    }
                }
                catch
                {
                    errorBox.Text = "Server connection failed. Make sure you're online.";
                }
            };
        }

        
    }
}