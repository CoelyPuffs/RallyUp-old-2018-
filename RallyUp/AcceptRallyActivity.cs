using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Support.V7.Widget;
using Android.Preferences;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "AcceptRallyActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AcceptRallyActivity : Activity
    {
        private TcpClient socket;
        string senderName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RegistrationPage);
            ActionBar.Hide();

            TextView acceptRallyTagline = FindViewById<TextView>(Resource.Id.acceptRallyTagline);
            TextView acceptRallyErrorBox = FindViewById<TextView>(Resource.Id.acceptRallyErrorBox);
            TextView acceptRallyTimer = FindViewById<TextView>(Resource.Id.acceptRallyTimer);
            RecyclerView acceptRallyFriendList = FindViewById<RecyclerView>(Resource.Id.acceptRallyFriendsList);
            Button acceptRallyButton = FindViewById<Button>(Resource.Id.acceptRallyButton);
            Button declineRallyButton = FindViewById<Button>(Resource.Id.declineRallyButton);

            try
            {
                socket = new TcpClient("192.168.1.2", 3292);
                socket.ReceiveTimeout = 1000;
                string senderName = Intent.GetStringExtra("rallyStarter");
                string tagline = Intent.GetStringExtra("rallyTagline");

                string getRallyString = "GetRally:" + senderName;
                socket.WriteString(getRallyString);
                string dataString = socket.ReadString();
                if (dataString == "RallyNotFound")
                {
                    acceptRallyErrorBox.Text = "Rally is over.";
                }
                else
                {
                    string[] lengthsString = dataString.Split(':')[0].Split(',');
                    string infoString = dataString.Substring(dataString.Split(':')[0].Length);

                    tickTimer(acceptRallyTimer, Convert.ToInt32(dataString.Substring(Convert.ToInt32(lengthsString[0]), Convert.ToInt32(lengthsString[1]))), acceptRallyButton, declineRallyButton);

                    List<Friend> rallyFriendsList = new List<Friend>();
                    int firstPoint = Convert.ToInt32(lengthsString[0]) + Convert.ToInt32(lengthsString[1]);
                    int secondPoint;
                    int thirdPoint;
                    for (int i = 2; i < lengthsString.Length; i += 2)
                    {
                        secondPoint = firstPoint + Convert.ToInt32(lengthsString[i]);
                        thirdPoint = secondPoint + Convert.ToInt32(lengthsString[i + 1]);
                        rallyFriendsList.Add(new Friend(infoString.Substring(secondPoint, Convert.ToInt32(lengthsString[i + 1])), infoString.Substring(firstPoint, Convert.ToInt32(lengthsString[i]))));
                        firstPoint = thirdPoint;
                    }

                    acceptRallyTagline.Text = tagline;

                    RunningRallyFriendAdapter adapter = new RunningRallyFriendAdapter(rallyFriendsList);
                    acceptRallyFriendList.HasFixedSize = true;
                    acceptRallyFriendList.SetLayoutManager(new LinearLayoutManager(this));
                    acceptRallyFriendList.SetAdapter(adapter);
                }
            }
            catch
            {
                acceptRallyErrorBox.Text = "Server connection failed. Make sure you're online.";
            }

            acceptRallyButton.Click += delegate
            {
                try
                {
                    socket = new TcpClient("192.168.1.2", 3292);
                    socket.ReceiveTimeout = 1000;
                    string acceptRallyString = "AcceptRally:" + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "").Length + ',' + senderName.Length + ':' + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "") + senderName;
                    socket.WriteString(acceptRallyString);
                    acceptRallyButton.Visibility = ViewStates.Gone;
                    declineRallyButton.Visibility = ViewStates.Gone;
                    acceptRallyErrorBox.Text = "";
                }
                catch
                {
                    acceptRallyErrorBox.Text = "Error. Could not contact server.";
                }
            };

            declineRallyButton.Click += delegate
            {
                try
                {
                    socket = new TcpClient("192.168.1.2", 3292);
                    socket.ReceiveTimeout = 1000;
                    string acceptRallyString = "DeclineRally:" + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "").Length + ',' + senderName.Length + ':' + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "") + senderName;
                    socket.WriteString(acceptRallyString);
                    acceptRallyButton.Visibility = ViewStates.Gone;
                    declineRallyButton.Visibility = ViewStates.Gone;
                    acceptRallyErrorBox.Text = "";
                }
                catch
                {
                    acceptRallyErrorBox.Text = "Error. Could not contact server.";
                }
            };

        }

        async void tickTimer(TextView timerBox, int timeLeft, Button acceptButton, Button declineButton)
        {
            if (timeLeft > 0)
            {
                int minutes = timeLeft / 60;
                int seconds = timeLeft % 60;
                while (minutes > 0 || seconds > 0)
                {
                    await Task.Delay(1000);
                    if (seconds == 0)
                    {
                        minutes--;
                        seconds = 59;
                    }
                    else
                    {
                        seconds--;
                    }
                    timerBox.Text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }
            }
            timerBox.Text = "Time's Up!";
            acceptButton.Visibility = ViewStates.Gone;
            declineButton.Visibility = ViewStates.Gone;
        }
    }
}