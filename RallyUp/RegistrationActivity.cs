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
using Android.Preferences;
using Android.Content.PM;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "Registration", ScreenOrientation = ScreenOrientation.Portrait)]
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
                if (newUserBox.Text.Length < 5 || newUserBox.Text.Length > 20)
                {
                    errorBox.Text = "Username must be between 5 and 20 characters";
                }
                else if (newPassBox.Text.Length < 5 || newPassBox.Text.Length > 30)
                {
                    errorBox.Text = "Password must be at least 5 characters";
                }
                else
                {
                    try
                    {
                        socket = new TcpClient("192.168.1.2", 3292);
                        socket.WriteString("Register:" + newUserBox.Text.Length + ',' + newPassBox.Text.Length + ',' + screenNameBox.Text.Length + ':' + newUserBox.Text + newPassBox.Text + screenNameBox.Text);
                        errorBox.Text = "";
                        if (socket.ReadString() == "RegistrationSuccessful")
                        {
                            socket.Close();
                            errorBox.Text = "Registration successful. Logging you in.";
                            ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
                            ISharedPreferencesEditor prefsEditor = userPrefs.Edit();
                            prefsEditor.PutString("currentUsername", newUserBox.Text);
                            prefsEditor.PutString("currentPassword", newPassBox.Text);
                            prefsEditor.PutBoolean("isAuthenticated", true);
                            prefsEditor.Commit();
                            this.Finish();
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
                }
            };
        }

        
    }
}