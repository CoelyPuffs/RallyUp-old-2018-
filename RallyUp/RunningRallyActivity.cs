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

            string rallyInfo = Intent.GetStringExtra("RallyInfo");
            string[] splitRallyInfo = rallyInfo.Split('≡');
            string rallyTagline = splitRallyInfo[0];
            IList<Friend> rallyFriendsList = new List<Friend>();

            for (int i = 1; i < splitRallyInfo.Length; i++)
            {
                rallyFriendsList.Add(new Friend(splitRallyInfo[i].Split('‗')[0], splitRallyInfo[i].Split('‗')[1]));
            }

            TextView runningRallyTaglineBox = FindViewById<TextView>(Resource.Id.runningRallyTagline);
            TextView runningRallyTimer = FindViewById<TextView>(Resource.Id.runningRallyTimer);
            RecyclerView runningRallyFriendsList = FindViewById<RecyclerView>(Resource.Id.runningRallyFriendsList);

            RunningRallyFriendAdapter adapter = new RunningRallyFriendAdapter(rallyFriendsList);
            runningRallyFriendsList.HasFixedSize = true;
            runningRallyFriendsList.SetLayoutManager(new LinearLayoutManager(this));
            runningRallyFriendsList.SetAdapter(adapter);

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