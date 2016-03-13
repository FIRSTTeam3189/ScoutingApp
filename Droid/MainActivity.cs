using System;
using System.IO;
using Scouty.Database;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Scouty.Droid
{
	[Activity (Label = "Scouty.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var docPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var dbPath = Path.Combine (docPath, "db.sqlite");
			Console.WriteLine ("DATABASE PATH: " + dbPath);
			LocalDatabase.Initialize (dbPath, new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid ());

			Scouty.Azure.AzureManager.Init ("https://team3189scouting.azurewebsites.net");

			global::Xamarin.Forms.Forms.Init (this, bundle);

			LoadApplication (new App ());
		}
	}
}

