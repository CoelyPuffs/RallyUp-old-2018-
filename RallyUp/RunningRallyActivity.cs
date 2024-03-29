﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Android.Graphics;

namespace RallyUp
{
    [Activity(Label = "RunningRallyActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class RunningRallyActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RunningRallyPage);
            ActionBar.Hide();

            TextView runningRallyTaglineBox = FindViewById<TextView>(Resource.Id.runningRallyTagline);
            TextView runningRallyTimer = FindViewById<TextView>(Resource.Id.runningRallyTimer);
            RecyclerView runningRallyFriendsList = FindViewById<RecyclerView>(Resource.Id.runningRallyFriendsList);

            string rallyInfo = Intent.GetStringExtra("RallyInfo");
            string[] prefix = { rallyInfo.Split(':')[0], rallyInfo.Split(':')[1] };
            string[] lengthsArray = prefix[1].Split(',');
            string infoString = rallyInfo.Substring(prefix[0].Length + prefix[1].Length + 2);
            string senderName = infoString.Substring(0, Convert.ToInt32(lengthsArray[0]));
            string tagline = infoString.Substring(Convert.ToInt32(lengthsArray[0]), Convert.ToInt32(lengthsArray[1]));
            List<Friend> rallyFriendsList = new List<Friend>();
            int firstPoint = Convert.ToInt32(lengthsArray[0]) + Convert.ToInt32(lengthsArray[1]);
            int secondPoint;
            int thirdPoint;
            for (int i = 2; i < lengthsArray.Length; i += 2)
            {
                secondPoint = firstPoint + Convert.ToInt32(lengthsArray[i]);
                thirdPoint = secondPoint + Convert.ToInt32(lengthsArray[i + 1]);
                rallyFriendsList.Add(new Friend(infoString.Substring(secondPoint, Convert.ToInt32(lengthsArray[i + 1])), infoString.Substring(firstPoint, Convert.ToInt32(lengthsArray[i]))));
                firstPoint = thirdPoint;
            }

            runningRallyTaglineBox.Text = tagline;

            tickTimer(runningRallyTimer);

            RunningRallyFriendAdapter adapter = new RunningRallyFriendAdapter(rallyFriendsList);
            runningRallyFriendsList.HasFixedSize = true;
            runningRallyFriendsList.SetLayoutManager(new LinearLayoutManager(this));
            runningRallyFriendsList.SetAdapter(adapter);

        }

        async void tickTimer(TextView timerBox)
        {
            int minutes = 10;
            int seconds = 0;
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
            timerBox.Text = "Time's Up!";
        }
    }

    public class RunningRallyFriendAdapter : RecyclerView.Adapter
    {
        IList<Friend> friends = new List<Friend>();
        public List<RunningRallyFriendViewHolder> friendBoxes = new List<RunningRallyFriendViewHolder>();

        public RunningRallyFriendAdapter(IList<Friend> friends)
        {
            this.friends = friends;
        }

        override public RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.oneRunningRallyFriend, parent, false);
            return new RunningRallyFriendViewHolder(itemView);
        }

        override public void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            RunningRallyFriendViewHolder sfvh = holder as RunningRallyFriendViewHolder;
            friendBoxes.Add(sfvh);
            sfvh.flagBox.SetImageResource(Resource.Drawable.icon);
            sfvh.nameBox.Text = friends[position].screenName;
        }

        override public int ItemCount
        {
            get { return friends.Count; }
        }
    }

    public class RunningRallyFriendViewHolder : RecyclerView.ViewHolder
    {
        public ImageView flagBox;
        public TextView nameBox;
        public Boolean isSelected = false;

        public RunningRallyFriendViewHolder(View itemView) : base(itemView)
        {
            flagBox = ItemView.FindViewById<ImageView>(Resource.Id.flagBox);
            nameBox = itemView.FindViewById<TextView>(Resource.Id.nameBox);

            itemView.SetBackgroundColor(Color.LightGray);

            ItemView.Click += delegate
            {
                if (isSelected)
                {
                    itemView.SetBackgroundColor(Color.LightGray);
                    isSelected = false;
                }
                else
                {
                    itemView.SetBackgroundColor(Color.LightBlue);
                    isSelected = true;
                }
            };
        }
    }
}