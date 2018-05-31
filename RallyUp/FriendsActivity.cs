using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace RallyUp
{
    [Activity(Label = "FriendsActivity")]
    public class FriendsActivity : Activity
    {
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
            // var testAdapter = friendList.GetAdapter();
        }

        private IList<Friend> makeFriends()
        {
            IList<Friend> friendList = new List<Friend>();
            friendList.Add(new Friend("April", "May", true));
            friendList.Add(new Friend("Davy Jones", "Poseidon", false));
            friendList.Add(new Friend("Chilly Girl", "Alysia", false));
            friendList.Add(new Friend("NomNom", "Chomper", true));
            friendList.Add(new Friend("Tuxy", "Tusky", true));
            friendList.Add(new Friend("Daisy Bennet", "Skye", true));
            friendList.Add(new Friend("Asami Whats Her Last Name", "FutureIndustries", false));
            friendList.Add(new Friend("Willow You Hold Me", "LetMeGo", true));
            friendList.Add(new Friend("Coely Puffs", "isLost", false));
            return friendList;
        }
    }

    public class Friend
    {
        public string screenName;
        private string username;
        public bool isOnline;

        public Friend(string screenName, string username, bool isOnline)
        {
            this.screenName = screenName;
            this.username = username;
            this.isOnline = isOnline;
        }
    }

    public class FriendViewModel
    {
        private string friendName;

        public FriendViewModel(string friendName)
        {
            this.friendName = friendName;
        }

        public string getFriendName()
        {
            return this.friendName;
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
            fvh.onlineBox.Text = "Online: " + friends[position].isOnline.ToString();
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
        public TextView onlineBox { get; private set; }

        public FriendViewHolder(View itemView) : base(itemView)
        {
            flagBox = ItemView.FindViewById<ImageView>(Resource.Id.flagBox);
            nameBox = itemView.FindViewById<TextView>(Resource.Id.nameBox);
            onlineBox = itemView.FindViewById<TextView>(Resource.Id.onlineBox);
        }
    }
}