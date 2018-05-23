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

            EditText newUserBox = FindViewById<EditText>(Resource.Id.newUserBox);
            EditText newPassBox = FindViewById<EditText>(Resource.Id.newPassBox);
            Button registerButton = FindViewById<Button>(Resource.Id.newRegisterButton);

            registerButton.Click += delegate
            {
                socket = new TcpClient("10.0.0.6", 3292);
                socket.WriteString("Jane");
                socket.WriteString("Register:" + newUserBox.Text + ':' + newPassBox.Text);
            };
        }

        
    }
}