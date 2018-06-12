using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Content.PM;

using Firebase.Messaging;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "RallyUp", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        private TcpClient socket;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            ActionBar.Hide();

            ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
            if (userPrefs.GetBoolean("isAuthenticated", false) == true)
            {
                StartActivity(typeof (RallyActivity));
                this.Finish();
            }

            TextView errorBox = FindViewById<TextView>(Resource.Id.loginErrorBox);
            EditText userBox = FindViewById<EditText>(Resource.Id.userBox);
            EditText passBox = FindViewById<EditText>(Resource.Id.passBox);
            Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
            Button registerButton = FindViewById<Button>(Resource.Id.registerButton);

            loginButton.Click += delegate
            {
                if (userBox.Text == "boop")
                {
                    StartActivity(typeof(PingActivity));
                    this.Finish();
                }
                else
                {
                    try
                    {
                        socket = new TcpClient("192.168.1.2", 3292);
                        socket.ReceiveTimeout = 1000;
                        errorBox.Text = "";
                        socket.WriteString("Login:" + userBox.Text.Length + ',' + passBox.Text.Length + ':' + userBox.Text + passBox.Text);
                        string returnString = socket.ReadString();
                        errorBox.Text = returnString;
                        if (returnString == "ValidCredentials")
                        {
                            ISharedPreferencesEditor prefsEditor = userPrefs.Edit();
                            prefsEditor.PutString("currentUsername", userBox.Text);
                            prefsEditor.PutString("currentPassword", passBox.Text);
                            prefsEditor.PutBoolean("isAuthenticated", true);
                            prefsEditor.Commit();
                            FirebaseMessaging.Instance.SubscribeToTopic(userBox.Text);
                            StartActivity(typeof(RallyActivity));
                            this.Finish();
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
                }
            };

            registerButton.Click += delegate
            {
                StartActivity(typeof(RegistrationActivity));
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
            if (userPrefs.GetBoolean("isAuthenticated", false) == true)
            {
                StartActivity(typeof(RallyActivity));
                this.Finish();
            }
        }
    }
}