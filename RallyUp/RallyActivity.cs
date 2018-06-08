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
using Android.Support.V7.Widget;
using Android.Preferences;
using Android.Graphics;
using Android.Content.PM;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "RallyActivity", WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class RallyActivity : Activity
    {
        private TcpClient socket;
        string selectedFriends = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RallyPage);
            ActionBar.Hide();

            Button rallyMenuButton = FindViewById<Button>(Resource.Id.rallyMenuButton);
            EditText rallyTaglineBox = FindViewById<EditText>(Resource.Id.rallyTaglineBox);
            Button rallyUpButton = FindViewById<Button>(Resource.Id.rallyUpButton);

            SelectFriendAdapter adapter = new SelectFriendAdapter(getRallyFriends());
            RecyclerView addRallyFriendsList = FindViewById<RecyclerView>(Resource.Id.addRallyFriendsList);
            addRallyFriendsList.HasFixedSize = true;
            addRallyFriendsList.SetLayoutManager(new LinearLayoutManager(this));
            addRallyFriendsList.SetAdapter(adapter);

            rallyMenuButton.Click += delegate
            {
                StartActivity(typeof(MenuActivity));
                Finish();
            };

            rallyUpButton.Click += delegate
            {
                foreach (SelectFriendViewHolder friend in adapter.friendBoxes)
                {
                    if (friend.isSelected)
                    {
                        selectedFriends += (friend.nameBox.Text + ':');
                    }
                }
                if (selectedFriends != "")
                {
                    try
                    {
                        TcpClient socket = new TcpClient("192.168.1.2", 3292);
                        socket.ReceiveTimeout = 1000;
                        string tagline;
                        if (rallyTaglineBox.Text != "")
                        {
                            tagline = rallyTaglineBox.Text;
                        }
                        else
                        {
                            tagline = "Rally Up!";
                        }
                        //Change this to send the screen name
                        socket.WriteString("Rally:" + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "") + ':' + tagline + ':' + selectedFriends);
                        selectedFriends = "";
                    }
                    catch
                    {

                    }
                }
            };
        }

        private IList<Friend> getRallyFriends()
        {
            IList<Friend> friendList = new List<Friend>();
            try
            {
                socket = new TcpClient("192.168.1.2", 3292);
                socket.ReceiveTimeout = 1000;
                socket.WriteString("GetFriends:" + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", ""));
                string friendListString = socket.ReadString();
                string[] firstList = friendListString.Split('/');
                List<Friend> friendDataList = new List<Friend>();
                if (firstList.Length > 0)
                {
                    firstList = firstList.Take(firstList.Count() - 1).ToArray();

                    ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
                    ISharedPreferencesEditor prefsEditor = userPrefs.Edit();
                    prefsEditor.Remove("FriendList");
                    prefsEditor.PutStringSet("FriendList", firstList);
                    prefsEditor.Commit();

                    foreach (string friend in firstList)
                    {
                        friendDataList.Add(new Friend(friend.Split(':')[1], friend.Split(':')[0]));
                    }
                }
                friendList = friendDataList;
            }
            catch
            {
                if (PreferenceManager.GetDefaultSharedPreferences(this).Contains("FriendList"))
                {
                    List<Friend> localFriendDataList = new List<Friend>();
                    foreach (string friend in PreferenceManager.GetDefaultSharedPreferences(this).GetStringSet("FriendList", new List<string>()))
                    {
                        localFriendDataList.Add(new Friend(friend.Split(':')[1], friend.Split(':')[0]));
                    }
                    return localFriendDataList;
                }
                else
                {
                    return new List<Friend>();
                }
            }
            return friendList;
        }
    }
    public class SelectFriendAdapter : RecyclerView.Adapter
    {
        IList<Friend> friends = new List<Friend>();
        public List<SelectFriendViewHolder> friendBoxes = new List<SelectFriendViewHolder>();

        public SelectFriendAdapter(IList<Friend> friends)
        {
            this.friends = friends;
        }

        override public RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.oneRallyFriend, parent, false);
            return new SelectFriendViewHolder(itemView);
        }

        override public void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            SelectFriendViewHolder sfvh = holder as SelectFriendViewHolder;
            friendBoxes.Add(sfvh);
            sfvh.flagBox.SetImageResource(Resource.Drawable.icon);
            sfvh.nameBox.Text = friends[position].screenName;
        }

        override public int ItemCount
        {
            get { return friends.Count; }
        }
    }

    public class SelectFriendViewHolder : RecyclerView.ViewHolder
    {
        public ImageView flagBox;
        public TextView nameBox;
        public Boolean isSelected = false;

        public SelectFriendViewHolder(View itemView) : base(itemView)
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