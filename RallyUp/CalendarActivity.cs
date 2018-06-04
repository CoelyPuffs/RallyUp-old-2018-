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

namespace RallyUp
{
    [Activity(Label = "CalendarActivity")]
    public class CalendarActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CalendarPage);
            ActionBar.Hide();

            //Button personalEventButton = FindViewById<Button>(Resource.Id.personalEventButton);
            //Button groupEventButton = FindViewById<Button>(Resource.Id.groupEventButton);
        }
    }
}