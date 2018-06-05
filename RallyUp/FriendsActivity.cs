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

using RallyUpLibrary;

namespace RallyUp
{
    [Activity(Label = "FriendsActivity")]
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
                string[] firstList = friendListString.Split('/');
                List<Friend> friendDataList = new List<Friend>();
                if (firstList.Length > 0)
                { 
                    firstList  = firstList.Take(firstList.Count() - 1).ToArray();
                    foreach (string friend in firstList)
                    {
                        friendDataList.Add(new Friend(friend.Split(':')[1], friend.Split(':')[0]));
                    }
                }
                friendList = friendDataList;
            }
            catch
            {

            }
            /*
            friendList.Add(new Friend("April", "May"));
            friendList.Add(new Friend("Davy Jones", "Poseidon"));
            friendList.Add(new Friend("Chilly Girl", "Alysia"));
            friendList.Add(new Friend("NomNom", "Chomper"));
            friendList.Add(new Friend("Tuxy", "Tusky"));
            friendList.Add(new Friend("Daisy Bennet", "Skye"));
            friendList.Add(new Friend("Asami Whats Her Last Name", "FutureIndustries"));
            friendList.Add(new Friend("Willow You Hold Me", "LetMeGo"));
            friendList.Add(new Friend("Coely Puffs", "isLost"));
            */
            return friendList;
        }

        private void addFriend()
        {

        }
    }

    public class Friend
    {
        public string screenName;
        private string username;

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
            fvh.flagBox.SetImageResource(Resource.Drawable.icon);
            fvh.nameBox.Text = friends[position].screenName;
        }

        override public int ItemCount
        {
            get { return friends.Count; }
        }
    }

    public class FriendViewHolder : RecyclerView.ViewHolder
    {
        public ImageView flagBox { get; private set; }
        public TextView nameBox { get; private set; }

        public FriendViewHolder(View itemView) : base(itemView)
        {
            flagBox = ItemView.FindViewById<ImageView>(Resource.Id.flagBox);
            nameBox = itemView.FindViewById<TextView>(Resource.Id.nameBox);
        }
    }
}