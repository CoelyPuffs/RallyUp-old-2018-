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

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "AddFriendActivity")]
    public class AddFriendActivity : Activity
    {
        public TcpClient socket;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AddFriendPage);
            ActionBar.Hide();

            TextView addFriendErrorBox = FindViewById<TextView>(Resource.Id.addFriendErrorBox);
            EditText friendNameBox = FindViewById<EditText>(Resource.Id.friendNameBox);
            Button addFriendUsernameButton = FindViewById<Button>(Resource.Id.addFriendUsernameButton);

            addFriendUsernameButton.Click += delegate
            {
                try
                {
                    socket = new TcpClient("192.168.1.2", 3292);
                    socket.ReceiveTimeout = 1000;
                    socket.WriteString("AddFriend:" + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "") + ':' + friendNameBox.Text);
                    string replyString = socket.ReadString();
                    if (replyString == "FriendAdded")
                    {
                        addFriendErrorBox.Text = "Friend added!";
                    }
                    else
                    {
                        addFriendErrorBox.Text = "Error adding friend";
                    }
                }
                catch
                {
                    addFriendErrorBox.Text = "Error connecting to server";
                }
            };
        }
    }
}