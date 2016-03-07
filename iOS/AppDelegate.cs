using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.IO;
using Scouty.Database;

namespace Scouty.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Scouty.Azure.AzureManager.Init ("https://team3189scouting.azurewebsites.net");

			// Get Library directory
			var libDir = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].Path;
			var dbPath = Path.Combine (libDir, "db.sqlite");

			var platform = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS ();
			LocalDatabase.Initialize (dbPath, platform);

			Console.WriteLine (libDir);

			global::Xamarin.Forms.Forms.Init ();

			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}
	}
}

