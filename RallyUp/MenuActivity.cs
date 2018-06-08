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
using Android.Content.PM;

namespace RallyUp
{
    [Activity(Label = "MenuActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MenuActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Menu);
            ActionBar.Hide();

            Button friendButton = FindViewById<Button>(Resource.Id.friendButton);
            Button calendarButton = FindViewById<Button>(Resource.Id.calendarButton);
            Button rallyButton = FindViewById<Button>(Resource.Id.rallyButton);
            Button settingsButton = FindViewById<Button>(Resource.Id.settingsButton);

            friendButton.Click += delegate
            {
                StartActivity(typeof(FriendsActivity));
            };

            calendarButton.Click += delegate
            {
                StartActivity(typeof(CalendarActivity));
            };

            rallyButton.Click += delegate
            {
                StartActivity(typeof(RallyActivity));
            };

            settingsButton.Click += delegate
            {
                StartActivity(typeof(SettingsActivity));
            };
        }
    }
}