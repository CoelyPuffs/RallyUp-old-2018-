﻿using System;
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
    [Activity(Label = "RallyActivity")]
    public class RallyActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RallyPage);
            ActionBar.Hide();

            Button rallyBackButton = FindViewById<Button>(Resource.Id.rallyBackButton);

            RallyAdapter adapter = new RallyAdapter(populateRallyList());
            RecyclerView rallyFlagCarousel = FindViewById<RecyclerView>(Resource.Id.rallyFlagCarousel);
            rallyFlagCarousel.HasFixedSize = true;
            rallyFlagCarousel.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
            rallyFlagCarousel.SetAdapter(adapter);

            SnapHelper rallySnapper = new LinearSnapHelper();
            rallySnapper.AttachToRecyclerView(rallyFlagCarousel);

            //rallyFlagCarousel.SmoothScrollToPosition(0);

            rallyBackButton.Click += delegate
            {
                this.Finish();
            };
        }

        private IList<RallyType> populateRallyList()
        {
            IList<RallyType> returnList = new List<RallyType>();

            returnList.Add(new RallyType("defaultRally", Resource.Drawable.RallyDefaultFlag, "Up!"));
            returnList.Add(new RallyType("secondDefaultRally", Resource.Drawable.RallyDefaultFlag, "Down!"));

            return returnList;
        }
    }

    public class RallyType
    {
        private string rallyName;
        public int rallyImageLocation;
        public string rallyTagLine;

        public RallyType(string rallyName, int rallyImageLocation, string rallyTagLine)
        {
            this.rallyName = rallyName;
            this.rallyImageLocation = rallyImageLocation;
            this.rallyTagLine = rallyTagLine;
        }
    }

    public class RallyAdapter : RecyclerView.Adapter
    {
        IList<RallyType> rallyList = new List<RallyType>();

        public RallyAdapter(IList<RallyType> rallyList)
        {
            this.rallyList = rallyList;
        }

        override public RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.oneRally, parent, false);
            return new RallyViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            RallyViewHolder rvh = holder as RallyViewHolder;
            rvh.rallyFlagImage.SetImageResource(rallyList[position].rallyImageLocation);
            rvh.rallyTaglineBox.SetText(("Rally " + rallyList[position].rallyTagLine).ToCharArray(), 0, rallyList[position].rallyTagLine.Length + 6);
        }

        override public int ItemCount
        {
            get { return rallyList.Count; }
        }
    }

    public class RallyViewHolder : RecyclerView.ViewHolder
    {
        public ImageView rallyFlagImage;
        public TextView rallyTaglineBox;
        public Button rallyWhoButton;

        public RallyViewHolder(View itemView) : base(itemView)
        {
            rallyFlagImage = itemView.FindViewById<ImageView>(Resource.Id.rallyFlagImage);
            rallyTaglineBox = itemView.FindViewById<TextView>(Resource.Id.rallyTaglineBox);
            rallyWhoButton = itemView.FindViewById<Button>(Resource.Id.rallyWhatFriendsButton);

            rallyWhoButton.Click += delegate
            {
                var intent = new Intent(ItemView.Context, typeof(SelectFriendsActivity));
                intent.PutExtra("FriendsSelected", "Data from Activity1");
                itemView.Context.StartActivity(intent);
                string selectedFriendList = intent.GetStringExtra("SelectedFriendsList");
            };
        }
    }
}