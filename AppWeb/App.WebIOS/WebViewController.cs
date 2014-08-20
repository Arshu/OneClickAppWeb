using System;
using System.Drawing;
//using Mono.Data.Sqlite;

using MonoTouch.CoreFoundation;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

using Arshu.Web.Common;
using Arshu.AppWeb;

namespace App.Web
{
    [Register("WebViewController")]
    public class WebViewController : UIViewController
    {
        #region Variable

        ArshuWebGrid _arshuWebGrid = null;        
        private float heightOffset = 25.0f;

        #endregion

		#region Constructor

        public WebViewController()
        {
            _arshuWebGrid = new ArshuWebGrid(this);

			DummyObjectRegister ();
        }

		#endregion

		#region Basic Overrides

        public override void DidReceiveMemoryWarning()
        {
            Console.WriteLine("DidReceiveMemoryWarning");
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
        }

		/// <summary>
		/// Views the did load.
		/// </summary>
        public override void ViewDidLoad()
        {
            Console.WriteLine("ViewDidLoad");
            InitWebGrid();

            base.ViewDidLoad();
        }

		#endregion

		#region Rotation Overrides

		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
            Console.WriteLine("WillRotate - Reload Web Grid [" + UIDevice.CurrentDevice.Orientation + "]");
            ReloadWebGrid();

			base.WillRotate (toInterfaceOrientation, duration);
		}

		#endregion

        #region Get Screen Rectangle

        public Size GetScreenSize()
        {
            int deviceWidth = (int) UIScreen.MainScreen.Bounds.Width;
            int deviceHeight = (int) UIScreen.MainScreen.Bounds.Height;

            int screenWidth = deviceWidth;
            int screenHeight = deviceHeight;

            UIDeviceOrientation currentOrientation = UIDevice.CurrentDevice.Orientation;

			if ((currentOrientation != UIDeviceOrientation.LandscapeLeft)
				&& (currentOrientation != UIDeviceOrientation.LandscapeRight))
            {
                // portrait
                screenWidth = deviceWidth;
                screenHeight = deviceHeight;
            }
            else
            {
                // landsacpe
                screenWidth = deviceHeight;
                screenHeight = deviceWidth;
            }
            return new Size(screenWidth, screenHeight);
        }

        #endregion



        #region Init WebGrid

        /// <summary>
		/// Inits the web grid.
		/// </summary>
		/// <param name="rootView">Root view.</param>
		private void InitWebGrid()
		{
            DummyObjectRegister();
            UIView rootView = this.View;
			if (_arshuWebGrid != null)
			{				
                Size screenSize = GetScreenSize();
                _arshuWebGrid.InitView(rootView, screenSize.Width, screenSize.Height, heightOffset); 

				_arshuWebGrid.CurrentPageAnimation = PageAnimation.FlipRight;
                _arshuWebGrid.RequireWifi = false;
				_arshuWebGrid.StartAnimationTime = 2000;
				_arshuWebGrid.EndAnimationTime = 1000;
                _arshuWebGrid.ShowInstallLink = true;
                _arshuWebGrid.ShowBackLink = true;
                _arshuWebGrid.RestartOnRotate = true;
                _arshuWebGrid.UseDocumentFolder = true;
                _arshuWebGrid.SnapShotCount = 10;
			}
		}

		#endregion

        #region Start WebGrid

        public void StartWebGrid()
        {
            if (_arshuWebGrid != null)
            {
                _arshuWebGrid.ConfigView();
                _arshuWebGrid.StartWebServer(false);
                ConfigureWebView();
            }
        }

        #endregion

        #region Stop WebGrid

        public void StopWebGrid()
        {
            if (_arshuWebGrid != null)
            {
                _arshuWebGrid.StopWebServer();
            }
        }

        #endregion

        #region Reload WebGrid

        public void ReloadWebGrid()
        {
            InitWebGrid();

            if (_arshuWebGrid != null)
            {
                _arshuWebGrid.ConfigView();
                _arshuWebGrid.ReloadView();
                ConfigureWebView();
            }
        }

        #endregion

        #region Configure WebView

        private void ConfigureWebView()
        {
            if (_arshuWebGrid != null)
            {
                if (_arshuWebGrid.MainWebView != null)
                {
                    // if this is false, page will be 'zoomed in' to normal size
                    _arshuWebGrid.MainWebView.ScalesPageToFit = true;

                    //_arshuWebGrid.MainWebView.UserInteractionEnabled = false;
                    //_arshuWebGrid.MainWebView.ScrollView.ScrollEnabled = false;
                    _arshuWebGrid.MainWebView.ScrollView.BouncesZoom = false;
                    _arshuWebGrid.MainWebView.ScrollView.Bounces = false;
                }
            }
        }

        #endregion

        #region Dummy Register

        //Dummy Variable to Suppress the error could not create a dempendency map for System.Xml
		System.Xml.XmlDocument _xmlDocument = null;
		bool _isNull = false;
		private void DummyObjectRegister()
		{
			//To Remove the error could not create a complete dempendency map
			if ((_arshuWebGrid == null) && (_isNull==true)) {
				_xmlDocument = new System.Xml.XmlDocument ();
			}

			if ((_xmlDocument == null) && (_isNull ==false)) {
				_isNull = true;
			}

            //SqliteFactory sqliteFactory = new SqliteFactory();
            //if ((sqliteFactory == null) && (_isNull == false))
            //{
            //    _isNull = true;
            //}
		}

		#endregion
    }
}