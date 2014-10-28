using System;
using Mono.Data.Sqlite;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.OS;
using Android.Graphics;

using Arshu.Web.Basic.Log;
using Arshu.Web.Common;
using Arshu.AppWeb;

#if DEBUG
[assembly: Application(Debuggable = true, HardwareAccelerated = true)]
#else
[assembly: Application(Debuggable = false, HardwareAccelerated=true)]
#endif

namespace App.Secure
{
    [Activity(Label = "AppSecure",
              MainLauncher = true,
              Icon = "@drawable/icon",
              Theme = "@style/Theme.GridBackground",
              WindowSoftInputMode = SoftInput.AdjustPan,
              ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
              LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    //[IntentFilter(new[] { Intent.ActionMain },
    //    Categories = new string[] { Intent.CategoryLauncher, Intent.CategoryDefault })]
    public class WebViewActivity : Activity
    {
        #region Variable

        public static Android.Net.Uri _appUri;
        private static ArshuWebGrid _arshuWebGrid = null;

        #endregion

        #region Override OnCreate

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ArshuWebGrid.DrawablePackageName = "app.web.v1";
            // If the Android version is lower than Jellybean, use this call to hide
            // the status bar.
            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
            {
                this.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }

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
            if (_arshuWebGrid != null) _arshuWebGrid.Destroy();

            base.OnDestroy();
        }

        #endregion

        #region Override OnResume

        protected override void OnResume()
        {
            StartWebGrid();

            base.OnResume();
        }

        #endregion

        #region Override OnPause

        protected override void OnPause()
        {
            StopWebGrid();

            base.OnPause();
        }

        #endregion

        #region Override OnKeyDown

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
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
                _arshuWebGrid.InitView(rootView);

                _arshuWebGrid.CurrentPageAnimation = PageAnimation.FlipLeft;
                _arshuWebGrid.RequireWifi = false;
                _arshuWebGrid.StartAnimationTime = 2000;
                _arshuWebGrid.EndAnimationTime = 1000;
                _arshuWebGrid.ShowInstallLink = true;
                _arshuWebGrid.ShowBackLink = true;
                _arshuWebGrid.RestartOnRotate = false;

                SetContentView(rootView, webviewLayoutParams);
            }
        }

        private RelativeLayout.LayoutParams GetWebLayoutParams()
        {
            //Display display = this.WindowManager.DefaultDisplay;

            //DisplayMetrics metrics = new DisplayMetrics();
            //display.GetMetrics(metrics);

            //Point size = new Point();
            //display.GetSize(size);
            //int displayHeight = size.X;
            //int displayWidth = size.Y;
            //int statusBarHeight = 0; //GetStatusBarHeight();

            //int webViewHeight = displayHeight - statusBarHeight;
            //RelativeLayout.LayoutParams webviewLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, webViewHeight);            
            RelativeLayout.LayoutParams webviewLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
            webviewLayoutParams.AddRule(LayoutRules.AlignParentTop);
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
                ConfigureWebView();
                _arshuWebGrid.StartWebServer(false);

                ImportApp();
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
                ConfigureWebView();
                _arshuWebGrid.ReloadView();
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
                }
            }
        }

        #endregion

        #region Import App

        private void ImportApp()
        {
            if (_arshuWebGrid != null)
            {
                if (_appUri != null)
                {                    
                    string appPath = GetPathFromURI(_appUri);
                    if (string.IsNullOrEmpty(appPath) == false)
                    {
                        _arshuWebGrid.ImportApp(appPath);
                    }
                    else
                    {
                        LogManager.Log(LogType.Error, "WebViewActivity-ImportApp", "Invalid AppURI [" + _appUri.ToString() + "]");
                    }
                    _appUri = null;
                }
            }
        }

        private string GetPathFromURI(Android.Net.Uri contentUri)
        {
            string realPath = "";
            if (contentUri != null)
            {
                try
                {
                    String[] projection = new String[] { Android.Provider.MediaStore.MediaColumns.Data };
                    ContentResolver cr = this.ContentResolver;
                    Android.Database.ICursor cursor = cr.Query(contentUri, projection, null, null, null);
                    if (cursor != null && cursor.Count > 0)
                    {
                        cursor.MoveToFirst();
                        int index = cursor.GetColumnIndex(Android.Provider.MediaStore.MediaColumns.Data);
                        realPath = cursor.GetString(index);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogType.Error, "WebViewActivity-GetPathFromURI", "Error:" + ex.Message);
                }
            }
            return realPath;
        }

        #endregion

        #region Dummy Register

        //Dummy Variable to Suppress the error could not create a dempendency map for System.Xml
        //System.Xml.XmlDocument _xmlDocument = null;
        bool _isNull = false;
        private void DummyObjectRegister()
        {
            //To Remove the error could not create a complete dempendency map
            //if ((_arshuWebGrid == null) && (_isNull == true))
            //{
            //    _xmlDocument = new System.Xml.XmlDocument();
            //}

            //if ((_xmlDocument == null) && (_isNull == false))
            //{
            //    _isNull = true;
            //}

            //To force load Mono.Data.Sqlite dll
            SqliteFactory sqliteFactory = new SqliteFactory();
            if ((sqliteFactory == null) && (_isNull == false))
            {
                _isNull = true;
            }
        }

        #endregion
    }
}

