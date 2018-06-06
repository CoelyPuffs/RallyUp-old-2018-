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
using Android.Support.V7.Widget;
using Android.Graphics;

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "SelectFriendsActivity")]
    public class SelectFriendsActivity : Activity
    {
        private TcpClient socket;
        string selectedFriends = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SelectFriendsPage);
            ActionBar.Hide();

            Button doneButton = FindViewById<Button>(Resource.Id.doneButton);

            SelectFriendAdapter adapter = new SelectFriendAdapter(getLocalFriends());
            RecyclerView selectFriendsView = FindViewById<RecyclerView>(Resource.Id.selectFriendsView);
            selectFriendsView.HasFixedSize = true;
            selectFriendsView.SetLayoutManager(new LinearLayoutManager(this));
            selectFriendsView.SetAdapter(adapter);

            doneButton.Click += delegate
            {
                foreach (SelectFriendViewHolder friend in adapter.friendBoxes)
                {
                    if (friend.isSelected)
                    {
                        selectedFriends += (friend.nameBox.Text + ':');
                    }
                }

                Intent myIntent = new Intent(this, typeof(RallyActivity));
                myIntent.PutExtra("SelectedFriendsList", selectedFriends);
                SetResult(Result.Ok, myIntent);
                Finish();
            };
        }

        List<Friend> getLocalFriends()
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
                return friendDataList;
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
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.oneFriend, parent, false);
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

            itemView.SetBackgroundColor(Color.Gray);

            ItemView.Click += delegate
            {
                if (isSelected)
                {
                    itemView.SetBackgroundColor(Color.Gray);
                    isSelected = false;
                }
                else
                {
                    itemView.SetBackgroundColor(Color.Blue);
                    isSelected = true;
                }
            };
        }
    }
}