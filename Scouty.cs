using System;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Scouty.Azure;
using Scouty.Utility;
using Scouty.Database;
using Scouty.UI;
using Scouty.Models.Local;

namespace Scouty
{
	public class App : Application
	{
		static readonly Logger logger = new Logger (typeof(App));

		public App ()
		{
			// The root page of your application
			MainPage = new NavigationPage(new MatchesPage("cama", 2016));

		}

		protected override void OnStart ()
		{
			
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

