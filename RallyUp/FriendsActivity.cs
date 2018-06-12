using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Support.V7.Widget;
using Android.Content.PM;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "FriendsActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class FriendsActivity : Activity
    {
        private TcpClient socket;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.FriendsPage);
            ActionBar.Hide();

            Button backButton = FindViewById<Button>(Resource.Id.friendsBackButton);
            Button addFriendButton = FindViewById<Button>(Resource.Id.newFriendButton);

            backButton.Click += delegate
            {
                this.Finish();
            };

            FriendlyAdapter adapter = new FriendlyAdapter(makeFriends());
            RecyclerView friendList = FindViewById<RecyclerView>(Resource.Id.friendList);
            friendList.HasFixedSize = true;
            friendList.SetLayoutManager(new LinearLayoutManager(this));
            friendList.SetAdapter(adapter);

            addFriendButton.Click += delegate
            {
                StartActivity(typeof(AddFriendActivity));
                adapter = new FriendlyAdapter(makeFriends());
            };

            
            // var testAdapter = friendList.GetAdapter();
        }

        private IList<Friend> makeFriends()
        {
            IList<Friend> friendList = new List<Friend>();
            try
            {
                socket = new TcpClient("192.168.1.2", 3292);
                socket.ReceiveTimeout = 1000;
                socket.WriteString("GetFriends:" + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", ""));
                string friendListString = socket.ReadString();
                string[] nameLengths = friendListString.Split(':')[0].Split(',');
                string nameListString = friendListString.Substring(friendListString.Split(':')[0].Length + 1);
                List<Friend> firstList = new List<Friend>();
                int firstPoint = 0;
                int secondPoint;
                int thirdPoint;
                for (int i = 0; i < nameLengths.Length; i += 2)
                {
                    secondPoint = firstPoint + Convert.ToInt32(nameLengths[i]);
                    thirdPoint = secondPoint + Convert.ToInt32(nameLengths[i + 1]);
                    firstList.Add(new Friend(nameListString.Substring(secondPoint, Convert.ToInt32(nameLengths[i + 1])), nameListString.Substring(firstPoint, Convert.ToInt32(nameLengths[i]))));
                    firstPoint = thirdPoint;
                }
                if (firstList.Count > 0)
                {
                    ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
                    ISharedPreferencesEditor prefsEditor = userPrefs.Edit();
                    prefsEditor.Remove("FriendList");
                    prefsEditor.PutString("FriendList", friendListString);
                    prefsEditor.Commit();
                }
                friendList = firstList;
                return friendList;
            }
            catch
            {
                if (PreferenceManager.GetDefaultSharedPreferences(this).Contains("FriendList"))
                {
                    List<Friend> localFriendDataList = new List<Friend>();
                    string friendListString = PreferenceManager.GetDefaultSharedPreferences(this).GetString("FriendList", "");
                    string[] nameLengths = friendListString.Split(':')[0].Split(',');
                    string nameListString = friendListString.Substring(friendListString.Split(':')[0].Length + 1);
                    List<Friend> firstList = new List<Friend>();
                    int firstPoint = 0;
                    int secondPoint;
                    int thirdPoint;
                    for (int i = 0; i < nameLengths.Length; i += 2)
                    {
                        secondPoint = firstPoint + Convert.ToInt32(nameLengths[i]);
                        thirdPoint = secondPoint + Convert.ToInt32(nameLengths[i + 1]);
                        firstList.Add(new Friend(nameListString.Substring(secondPoint, Convert.ToInt32(nameLengths[i + 1])), nameListString.Substring(firstPoint, Convert.ToInt32(nameLengths[i]))));
                        firstPoint = thirdPoint;
                    }
                    localFriendDataList = firstList;
                    return localFriendDataList;
                }
            }
            return new List<Friend>();
        }
    }

    public class Friend
    {
        public string screenName;
        public string username;

        public Friend(string screenName, string username)
        {
            this.screenName = screenName;
            this.username = username;
        }
    }

    public class FriendlyAdapter : RecyclerView.Adapter
    {
        IList<Friend> friends;

        public FriendlyAdapter(IList<Friend> friends)
        {
            this.friends = friends;
        }

        override public RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.oneFriend, parent, false);
            return new FriendViewHolder(itemView);
        }

        override public void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            FriendViewHolder fvh = holder as FriendViewHolder;
            fvh.flagBox.SetImageResource(Resource.Drawable.placeholderProfilePhoto);
            fvh.nameBox.Text = friends[position].screenName;
        }

        override public int ItemCount
        {
            get { return friends.Count; }
        }
    }

    public class FriendViewHolder : RecyclerView.ViewHolder
    {
        public ImageView flagBox;
        public TextView nameBox;

        public FriendViewHolder(View itemView) : base(itemView)
        {
            flagBox = ItemView.FindViewById<ImageView>(Resource.Id.flagBox);
            nameBox = itemView.FindViewById<TextView>(Resource.Id.nameBox);
        }
    }
}