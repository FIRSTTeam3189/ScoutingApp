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
			Team testTeam = new Team (){ TeamNumber = 3189 };
			// The root page of your application
			MainPage = new NavigationPage(new PerformancePage(testTeam, 69));

		}

		protected override void OnStart ()
		{
			Test.TestCreate ();

			// Test Register
			Task.Factory.StartNew(async () => {
				logger.Info("Logging in...");
				var loginState = await UserManager.Login("jameswomack", "1234qwery");

				if (loginState != LoginState.Success){
					logger.Info("Failed to login: " + loginState);

					var registerState = await UserManager.CustomRegister("jameswomack", "1234qwery", "James Womack");

					if (registerState == RegisterState.Success){
						loginState = await UserManager.Login("jameswomack", "1234qwery");
						if (loginState == LoginState.Success)
							logger.Info("Logged in!");
						else 
							logger.Error("Failed to login: " + loginState);
					} else {
						logger.Error("Failed to register: " + loginState);
					}
				} else {
					logger.Info("Logged in!");
					logger.Info("Getting events!");

					var teams = await EventManager.MatchesForEvent("cada", 2015);

					if (teams != null)
						foreach (var team in teams){
							
						}
					else 
						logger.Info("Failed to get the team");
				}
			});
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

