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
using Android.Content.PM;
using Android.Preferences;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "AcceptFriendActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AcceptFriendActivity : Activity
    {
        private TcpClient socket;
        private string friendName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            ActionBar.Hide();

            Button yesButton = FindViewById<Button>(Resource.Id.yesFriendButton);
            Button noButton = FindViewById<Button>(Resource.Id.noFriendButton);
            TextView friendRequestView = FindViewById<TextView>(Resource.Id.friendRequestView);
            TextView acceptFriendErrorBox = FindViewById<TextView>(Resource.Id.acceptFriendErrorBox);

            if (Intent.Extras != null)
            {
                friendName = Intent.Extras.GetString("friendScreenName");
                friendRequestView.Text = friendName + "wants to be your friend";
            }

            yesButton.Click += delegate
            {
                try
                {
                    socket = new TcpClient("192.168.1.2", 3292);
                    socket.ReceiveTimeout = 1000;
                    string myUsername = PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "");
                    string acceptFriendString = "AcceptFriend:" + myUsername.Length + ',' + friendName.Length + ':' + myUsername + friendName;
                    socket.WriteString(acceptFriendString);
                    StartActivity(typeof(FriendsActivity));
                    this.Finish();
                }
                catch
                {
                    acceptFriendErrorBox.Text = "Server connection failed. Make sure you're online.";
                }
            };

            noButton.Click += delegate
            {
                try
                {
                    socket = new TcpClient("192.168.1.2", 3292);
                    socket.ReceiveTimeout = 1000;
                    string myUsername = PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "");
                    string acceptFriendString = "DeclineFriend:" + myUsername.Length + ',' + friendName.Length + ':' + myUsername + friendName;
                    socket.WriteString(acceptFriendString);
                    StartActivity(typeof(FriendsActivity));
                    this.Finish();
                }
                catch
                {
                    acceptFriendErrorBox.Text = "Server connection failed. Make sure you're online.";
                }
            };
        }
    }
}