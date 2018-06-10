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
        List<List<string>> selectedFriends = new List<List<string>>();

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
                string tagline = "Rally Up!";
                for(int i = 0; i < adapter.ItemCount; i++)
                {
                    if (adapter.friendBoxes[i].isSelected)
                    {
                        List<string> thisFriend = new List<string>();
                        thisFriend.Add(adapter.friends[i].screenName);
                        thisFriend.Add(adapter.friends[i].screenName.Length.ToString());
                        thisFriend.Add(adapter.friends[i].username);
                        thisFriend.Add(adapter.friends[i].username.Length.ToString());
                        selectedFriends.Add(thisFriend);
                    }
                }
                if (selectedFriends.Count > 0)
                {
                    try
                    {
                        TcpClient socket = new TcpClient("192.168.1.2", 3292);
                        socket.ReceiveTimeout = 1000;
                        if (rallyTaglineBox.Text != "")
                        {
                            tagline = rallyTaglineBox.Text;
                        }
                        
                        string friendNamesString = "";
                        foreach (List<string> friend in selectedFriends)
                        {
                            friendNamesString += friend[0] + friend[2];
                        }

                        string friendLengthsString = "";
                        foreach (List<string> friend in selectedFriends)
                        {
                            friendLengthsString += friend[1] + ',' + friend[3] + ',';
                        }
                        friendLengthsString = friendLengthsString.TrimEnd(',');

                        socket.WriteString("Rally:" + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "").Length + ',' + tagline.Length + ',' + friendLengthsString + ':' + PreferenceManager.GetDefaultSharedPreferences(this).GetString("currentUsername", "") + tagline + friendNamesString);
                        Intent myIntent = new Intent(this, typeof(RunningRallyActivity));
                        myIntent.PutExtra("RallyInfo", tagline + '≡' + selectedFriends);
                        StartActivity(myIntent);
                        Finish();
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
    public class SelectFriendAdapter : RecyclerView.Adapter
    {
        public IList<Friend> friends = new List<Friend>();
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