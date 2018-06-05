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
using Android.Preferences;

namespace RallyUp
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SettingsPage);
            ActionBar.Hide();

            Button logoutButton = FindViewById<Button>(Resource.Id.logoutButton);

            logoutButton.Click += delegate
            {
                ISharedPreferences userPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
                ISharedPreferencesEditor prefsEditor = userPrefs.Edit();
                prefsEditor.Remove("currentUsername");
                prefsEditor.Remove("currentPassword");
                prefsEditor.PutBoolean("isAuthenticated", false);
                prefsEditor.Commit();
                StartActivity(typeof(MainActivity));
                this.Finish();
            };
        }
    }
}