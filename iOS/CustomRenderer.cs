using System;
using Xamarin.Forms.Platform.iOS;
using Scouty.UI;
using Xamarin.Forms;

namespace Scouty.iOS
{
	[assembly: global::Xamarin.Forms.ExportRenderer(typeof(global::Scouty.UI.PerformancePage), typeof(global::Scouty.iOS.CustomRenderer))]
	public class CustomRenderer : PageRenderer
	{
		public override void ViewWillAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			Console.WriteLine ("Penis Penis Penis");

			var nav = NavigationController;
			if (nav != null && nav.InteractivePopGestureRecognizer != null)
				nav.InteractivePopGestureRecognizer.Enabled = false;
		}

		protected override void OnElementChanged (VisualElementChangedEventArgs e)
		{
			base.OnElementChanged (e);

			var nav = NavigationController;
			if (nav != null && nav.InteractivePopGestureRecognizer != null)
				nav.InteractivePopGestureRecognizer.Enabled = false;
		}
	}
}

