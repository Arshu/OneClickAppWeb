using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.OS;

using Arshu.Web.Common;
using Arshu.AppWeb;

#if RELEASE 
[assembly: Application(Debuggable = false)] 
#else
[assembly: Application(Debuggable = true)]
#endif

namespace App.Web
{
    //Main Launcher App
    //[Activity(Label = "AppName", Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true)]
    //[IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { Intent.CategoryHome, Intent.CategoryDefault })]

    [Activity(Label = "AppWeb", 
	          MainLauncher = true, 
	          Icon = "@drawable/icon", 
	          Theme = "@style/Theme.GridBackground", 
	          WindowSoftInputMode = SoftInput.AdjustPan,
              ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
	          LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]             
    [IntentFilter(new[] { Intent.ActionMain },
        Categories = new string[] { Intent.CategoryLauncher, Intent.CategoryDefault })]
    public class MainActivity : Activity
    {
        #region Variable
        
        ArshuWebGrid _arshuWebGrid = null;
        
        #endregion

        #region Override OnCreate

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ArshuWebGrid.DrawablePackageName = "app.web.v1";

            _arshuWebGrid = new ArshuWebGrid(this, bundle);
            if (_arshuWebGrid != null)
            {                
                InitWebGrid();
            }
        }
        
        #endregion

		#region Override OnDestroy

		protected override void OnDestroy()
		{
			if (_arshuWebGrid !=null) _arshuWebGrid.Destroy();

			base.OnDestroy ();
		}

		#endregion
		
		#region Override OnResume

		protected override void OnResume()
		{
            StartWebGrid();

			base.OnResume ();
		}

		#endregion

		#region Override OnPause

		protected override void OnPause()
		{
            StopWebGrid();

			base.OnPause ();
		}

		#endregion

        #region Override OnKeyDown

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode ==  Keycode.Back)
            {
                if (_arshuWebGrid != null)
                {
                    _arshuWebGrid.BackView();
                }
            }
            return base.OnKeyDown(keyCode, e);
        }

        #endregion

        #region Rotation Overrides

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            ReloadWebGrid();

            base.OnConfigurationChanged(newConfig);
        }

        #endregion




        #region Init Web Grid

        /// <summary>
        /// Inits the web grid.
        /// </summary>
        /// <param name="rootView">Root view.</param>
        private void InitWebGrid()
        {
            DummyObjectRegister();
            RelativeLayout rootView = new RelativeLayout(this);
            if (_arshuWebGrid != null)
            {
                RelativeLayout.LayoutParams webviewLayoutParams = GetWebLayoutParams();
                _arshuWebGrid.InitView(rootView, webviewLayoutParams);

                _arshuWebGrid.CurrentPageAnimation = PageAnimation.FlipLeft;
				_arshuWebGrid.StartAnimationTime = 2000;
                _arshuWebGrid.EndAnimationTime = 500;
                _arshuWebGrid.ShowInstallLink = true;
                _arshuWebGrid.RestartOnRotate = true;

                SetContentView(rootView, webviewLayoutParams);
            }
        }

        private RelativeLayout.LayoutParams GetWebLayoutParams()
        {
            DisplayMetrics metrics = new DisplayMetrics();
            Display display = this.WindowManager.DefaultDisplay;
            display.GetMetrics(metrics);
            int displayHeight = display.Height;
            int statusBarHeight = GetStatusBarHeight();

            int webViewHeight = displayHeight - statusBarHeight;
            RelativeLayout.LayoutParams webviewLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, webViewHeight);
            //RelativeLayout.LayoutParams webviewLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.FillParent);
            //webviewLayoutParams.AddRule(LayoutRules.AlignParentTop);
            webviewLayoutParams.AddRule(LayoutRules.AlignParentBottom);
            webviewLayoutParams.AddRule(LayoutRules.AlignParentLeft);
            webviewLayoutParams.AddRule(LayoutRules.AlignParentRight);

            return webviewLayoutParams;

        }

        private int GetStatusBarHeight()
        {
            int result = 0;
            int resourceId = this.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                result = this.Resources.GetDimensionPixelSize(resourceId);
            }
            return result;
        }

        #endregion

        #region Start Web Grid

        public void StartWebGrid()
        {
            if (_arshuWebGrid != null)
            {
                _arshuWebGrid.ConfigView();
                _arshuWebGrid.StartWebServer(false);
            }
        }

        #endregion

        #region Stop Web Grid

        public void StopWebGrid()
        {
            if (_arshuWebGrid != null)
            {
                _arshuWebGrid.StopWebServer();
            }
        }

        #endregion

        #region Reload Web Grid

        public void ReloadWebGrid()
        {
            InitWebGrid();

            if (_arshuWebGrid != null)
            {
                _arshuWebGrid.ConfigView();
                _arshuWebGrid.ReloadView();
            }
        }

        #endregion

        #region Dummy Register

        private void DummyObjectRegister()
        {
            //To force load Mono.Data.Sqlite dll
            //SqliteFactory sqliteFactory = new SqliteFactory();
            //if ((sqliteFactory == null) && (_isNull == false))
            //{
            //    _isNull = true;
            //}
        }

        #endregion

    }
}