using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Mono.Data.Sqlite;

using Android.OS;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.PM;
using Android.Webkit;
using Android.Locations;
using Android.Hardware;
using Android.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Bluetooth;
using Android.Provider;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Telephony;
using Android.Database;
using Android.Net;
using Android.Net.Wifi;
using Android.Media;
using Android.Service;
using Android.Speech;
using Android.Speech.Tts;
using Android.Util;
using Java.Util;
using Java.Security;
using Java.Net;

using Arshu.Web.Basic.Log;
using Arshu.Web.Common;
using Arshu.Web.IO;
using Arshu.Web.Json;
using Arshu.AppWeb;

//using ZXing;
//using ZXing.Client;

#if RELEASE 
[assembly: Application(Debuggable=false)] 
#else
[assembly: Application(Debuggable = true)]
#endif

namespace Arshu.AppGrid
{
    //android:theme="@android:style/Theme.NoTitleBar.Fullscreen"
    //android:windowSoftInputMode="adjustPan"
    //@style/Theme.GridBackground
    //touchscreen|keyboard|keyboardHidden|navigation|orientation
    //[IntentFilter(new[] { Intent.ActionMain, "android.nfc.action.NDEF_DISCOVERED" }, DataMimeType = GridActivity.GridNfcMimeType, Categories = new string[] { Intent.CategoryLauncher, "android.intent.category.DEFAULT" })]
    //[IntentFilter(new[] { Intent.ActionMain }, Categories = new string[] { Intent.CategoryLauncher })]

    [Activity(Label = "AppMaker",
                MainLauncher = true,
#if __ANDROID_11__
                HardwareAccelerated=false,
#endif
 Icon = "@drawable/icon",
                Theme = "@style/Theme.GridBackground",
                WindowSoftInputMode = SoftInput.AdjustPan,
                ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
                LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new string[] { Intent.CategoryLauncher, Intent.CategoryBrowsable })]
    public class WebActivity : Activity, ILocationListener
    {
        #region Variable

        private static ArshuWebGrid _arshuWebGrid = null;
        private static WebActivity _gridActivity;
        private static bool _skipStopping = false;

        #endregion


        #region Override OnCreate

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ArshuWebGrid.DrawablePackageName = "arshu.appgrid.v1";

            _arshuWebGrid = new ArshuWebGrid(this, bundle);
            if (_arshuWebGrid != null)
            {
                InitWebGrid();
            }

            _gridActivity = this;
            _onActivityResultDone = false;
        }

        #endregion

        #region Override OnDestroy

        protected override void OnDestroy()
        {
            if (_arshuWebGrid != null) _arshuWebGrid.Destroy();

            DisableLocationTracking();

            base.OnDestroy();
        }

        #endregion

        #region Override OnResume

        protected override void OnResume()
        {
            if (_onActivityResultDone == false)
            {
                StartWebGrid();
            }

            base.OnResume();
        }

        #endregion

        #region Override OnPause

        protected override void OnPause()
        {
            if (_skipStopping == false)
            {
                StopWebGrid();
            }
            else
            {
                _skipStopping = false;
            }

            CleanUp();

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
                    return true;
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

        #region Override OnActivityResult

        private const int PICK_PHOTO_FROM_CAMERA = 2;
        private const int PICK_PHOTO_FROM_FILE = 3;
        private const int PICK_VIDEO_FROM_FILE = 4;
        private const int PICK_AUDIO_FROM_FILE = 5;

        private static bool _onActivityResultDone = false;

        /// <summary>
        /// Used by Bluetooth Module
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>        
        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            LogManager.Log(LogType.Info, "GridActivity_onActivityResult", resultCode.ToString());
            //Toast.MakeText(this, "OnActivityResult", ToastLength.Short).Show();

            switch (requestCode)
            {
                case PICK_PHOTO_FROM_CAMERA:
                    if (resultCode == Android.App.Result.Ok)
                    {
                        if ((data != null) && (data.Data != null))
                        {
                            try
                            {
                                // make it available in the gallery
                                //var contentUri = data.Data;
                                //var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                                //mediaScanIntent.SetData(contentUri);
                                //this.SendBroadcast(mediaScanIntent);

                                using (Bitmap imageBitmap = RotateScaleImage(this, data.Data, _maxImageSize))
                                {
                                    if (imageBitmap != null)
                                    {
                                        string tempFilePath = "";
                                        if (string.IsNullOrEmpty(_imageName) == true) _imageName = "CameraImage.png";
                                        tempFilePath = IOManager.GetTempFilePath("CameraImage.png", true);
                                        if (tempFilePath != null)
                                        {
                                            tempFilePath = tempFilePath.Replace(System.IO.Path.GetExtension(tempFilePath), ".png");
                                            if (File.Exists(tempFilePath) == true) File.Delete(tempFilePath);

                                            using (FileStream outFile = new FileStream(tempFilePath, FileMode.CreateNew))
                                            {
                                                imageBitmap.Compress(Bitmap.CompressFormat.Png, 90, outFile);
                                            }

                                            //_cameraURI = Android.Net.Uri.FromFile(new Java.IO.File(tempFilePath));
                                            _cameraURI = GetContentUriFromPath(this, tempFilePath);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _cameraURI = data.Data;
                                _imageError = "Camera Error: " + ex.Message;
                            }
                        }
                        else
                        {
                            _imageError = "Camera data not found";
                        }

                        RefreshWebView("FinishedPickingCamera");
                        LogManager.Log(LogType.Info, "OnActivityResult-OK", "Cameara File [" + _cameraURI + "]");
                    }
                    else
                    {
                        LogManager.Log(LogType.Info, "OnActivityResult-NotOK", "Camera File Not Found");
                    }
                    break;
                case PICK_PHOTO_FROM_FILE:
                    if (resultCode == Android.App.Result.Ok)
                    {
                        if ((data != null) && (data.Data != null))
                        {
                            try
                            {
                                using (Bitmap imageBitmap = RotateScaleImage(this, data.Data, _maxImageSize))
                                {
                                    if (imageBitmap != null)
                                    {
                                        string tempFilePath = "";
                                        if (string.IsNullOrEmpty(_imageName) == true) _imageName = "PhotoImage.png";
                                        tempFilePath = IOManager.GetTempFilePath("PhotoImage.png", true);
                                        if (tempFilePath != null)
                                        {
                                            tempFilePath = tempFilePath.Replace(System.IO.Path.GetExtension(tempFilePath), ".png");
                                            if (File.Exists(tempFilePath) == true) File.Delete(tempFilePath);

                                            using (FileStream outFile = new FileStream(tempFilePath, FileMode.CreateNew))
                                            {
                                                imageBitmap.Compress(Bitmap.CompressFormat.Png, 90, outFile);
                                            }
                                            //_photoURI = Android.Net.Uri.FromFile(new Java.IO.File(tempFilePath));
                                            _photoURI = GetContentUriFromPath(this, tempFilePath);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _photoURI = data.Data;
                                _imageError = "Image Error: " + ex.Message;
                            }
                        }
                        else
                        {
                            _imageError = "Image Data not found";
                        }

                        RefreshWebView("FinishedPickingImage");
                        LogManager.Log(LogType.Info, "OnActivityResult-OK", "Photo File [" + _photoURI + "]");
                    }
                    else
                    {
                        LogManager.Log(LogType.Info, "OnActivityResult-NotOK", "Photo File Not Found");
                    }
                    break;
                case PICK_VIDEO_FROM_FILE:
                    if (resultCode == Android.App.Result.Ok)
                    {
                        if ((data != null) && (data.Data != null))
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(_videoName) == true) _videoName = "GalleryVideo.mp4";
                                string tempFilePath = IOManager.GetTempFilePath(_videoName, true);
                                if (tempFilePath != null)
                                {
                                    string filePath = GetPathFromURI(data.Data);
                                    string fileExtension = System.IO.Path.GetExtension(filePath);
                                    tempFilePath = tempFilePath.Replace(System.IO.Path.GetExtension(tempFilePath), fileExtension);
                                    if (File.Exists(tempFilePath) == true) File.Delete(tempFilePath);

                                    File.Copy(filePath, tempFilePath, true);
                                    _videoURI = GetContentUriFromPath(this, tempFilePath);
                                }

                            }
                            catch (Exception ex)
                            {
                                _videoURI = data.Data;
                                _videoError = "Video Error: " + ex.Message;
                            }
                        }
                        else
                        {
                            _videoError = "Video Data not found";
                        }

                        RefreshWebView("FinishedPickingVideo");
                        LogManager.Log(LogType.Info, "OnActivityResult-OK", "Video File [" + _videoURI + "]");
                    }
                    else
                    {
                        LogManager.Log(LogType.Info, "OnActivityResult-NotOK", "Video File Not Found");
                    }
                    break;
                case PICK_AUDIO_FROM_FILE:
                    if (resultCode == Android.App.Result.Ok)
                    {

                        if ((data != null) && (data.Data != null))
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(_audioName) == true) _audioName = "GalleryAudio.m4a";
                                string tempFilePath = IOManager.GetTempFilePath(_audioName, true);
                                if (tempFilePath != null)
                                {
                                    string filePath = GetPathFromURI(data.Data);
                                    string fileExtension = System.IO.Path.GetExtension(filePath);
                                    tempFilePath = tempFilePath.Replace(System.IO.Path.GetExtension(tempFilePath), fileExtension);
                                    if (File.Exists(tempFilePath) == true) File.Delete(tempFilePath);

                                    File.Copy(filePath, tempFilePath);
                                    _audioURI = GetContentUriFromPath(this, tempFilePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                _audioURI = data.Data;
                                _audioError = "Audio Error: " + ex.Message;
                            }
                        }
                        else
                        {
                            _audioError = "Audio Data not found";
                        }

                        RefreshWebView("FinishedPickingAudio");
                        LogManager.Log(LogType.Info, "OnActivityResult-OK", "Audio File [" + _audioURI + "]");
                    }
                    else
                    {
                        LogManager.Log(LogType.Info, "OnActivityResult-NotOK", "Audio File Not Found");
                    }
                    break;
            }

            _onActivityResultDone = true;
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

        #region Start WebGrid

        public void StartWebGrid()
        {
            if (_arshuWebGrid != null)
            {
                _arshuWebGrid.ConfigView();
                _arshuWebGrid.StartWebServer(false);
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
            }
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

            SqliteFactory sqliteFactory = new SqliteFactory();
            if ((sqliteFactory == null) && (_isNull == false))
            {
                _isNull = true;
            }
        }

        #endregion

        #region Clean Up

        public void CleanUp()
        {
            try
            {

            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "GridActviity_CleanUp", "Error:" + ex.Message);
            }
        }

        #endregion



        #region Refresh WebView

        private static void RefreshWebView(string source)
        {
            if (_gridActivity != null)
            {
                if (_arshuWebGrid != null)
                {
                    _gridActivity.RunOnUiThread(delegate()
                    {
                        _arshuWebGrid.RefreshWebView(source);
                    });
                }
            }
        }

        #endregion

        #region Manage Animation

        public static PageAnimation GetCurrentPageAnimation()
        {
            PageAnimation currentPageAnimation = PageAnimation.Fade;
            if (_arshuWebGrid != null)
            {
                currentPageAnimation = _arshuWebGrid.CurrentPageAnimation;
            }
            return currentPageAnimation;
        }

        #endregion

        #region Manage Location

        public static bool _enableLocationTracking = false;
        public static JsonObject _lastLocation = null;
        public static JsonObject _lastAddress = null;
        public static string _locationError = "Location Tracking Not Enabled";

        private static LocationManager _locationManager;
        private static Geocoder _geocoder = null;

        private static Location _lastKnownLocation = null;
        private static Address _lastKnownAddress = null;

        public static bool DisableLocationTracking()
        {
            bool ret = false;
            if (_gridActivity != null)
            {
                if (_locationManager != null)
                {
                    _gridActivity.RunOnUiThread(delegate()
                    {
                        _locationManager.RemoveUpdates(_gridActivity);
                    });
                    _enableLocationTracking = false;
                    ret = true;
                }
            }
            return ret;
        }

        public static bool EnableLocationTracking(long minTimeInMs, float minDistance, out string retMessage)
        {
            bool ret = false;
            string message = "";
            try
            {
                if (_gridActivity != null)
                {
                    if ((_geocoder == null) || (_locationManager == null))
                    {
                        _geocoder = new Geocoder(_gridActivity);
                        _locationManager = _gridActivity.GetSystemService(Context.LocationService) as LocationManager;
                    }

                    if ((_locationManager != null) && (_enableLocationTracking == false))
                    {
                        var locationCriteria = new Criteria();
                        locationCriteria.Accuracy = Accuracy.NoRequirement;
                        locationCriteria.CostAllowed = true;
                        locationCriteria.PowerRequirement = Power.NoRequirement;

                        _gridActivity.RunOnUiThread(delegate()
                        {
                            try
                            {
                                string bestProvider = _locationManager.GetBestProvider(locationCriteria, true);
                                if (bestProvider != null)
                                {
                                    _locationManager.RequestLocationUpdates(bestProvider, minTimeInMs, minDistance, _gridActivity);
                                    _lastKnownLocation = _locationManager.GetLastKnownLocation(bestProvider);
                                }
                                else
                                {
                                    _locationManager.RequestLocationUpdates("passive", minTimeInMs, minDistance, _gridActivity);
                                    _lastKnownLocation = _locationManager.GetLastKnownLocation("passive");
                                }
                                if (_lastKnownLocation == null)
                                {
                                    var providers = _locationManager.GetProviders(false);
                                    foreach (var provider in providers)
                                    {
                                        _locationManager.RequestLocationUpdates(provider, minTimeInMs, minDistance, _gridActivity);
                                        _lastKnownLocation = _locationManager.GetLastKnownLocation(provider);
                                        if (_lastKnownLocation != null) { break; }
                                    }
                                }
                                if (_lastKnownLocation != null)
                                {
                                    _lastLocation = new JsonObject();
                                    _lastLocation.Add("latitude", _lastKnownLocation.Latitude.ToString());
                                    _lastLocation.Add("longitude", _lastKnownLocation.Longitude.ToString());
                                    SetAddress();
                                }
                            }
                            catch (Java.Lang.Exception ex)
                            {
                                _locationError = "Error in Getting Location [" + ex.Message + "]";
                                LogManager.Log(LogType.Error, "GridActivity-EnableLocationTracking", _locationError);
                            }
                            catch (Exception ex)
                            {
                                _locationError = "Error in Getting Location [" + ex.Message + "]";
                                LogManager.Log(LogType.Error, "GridActivity-EnableLocationTracking", ex.Message);
                            }
                        });

                        ret = true;
                    }
                    else if (_locationManager != null)
                    {
                        ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "GridActivity-EnableLocationTracking", ex.Message);
                message = "EnableLocationTracking Error:" + ex.Message;
            }

            _enableLocationTracking = ret;
            retMessage = message;
            return ret;
        }

        private static bool SetAddress()
        {
            bool ret = false;
            if (_gridActivity != null)
            {
                if ((_geocoder != null) && (_lastKnownLocation != null))
                {
                    _gridActivity.RunOnUiThread(delegate()
                    {
                        try
                        {
                            IList<Address> addressList = _geocoder.GetFromLocation(_lastKnownLocation.Latitude, _lastKnownLocation.Longitude, 5);
                            foreach (Address itemAddress in addressList)
                            {
                                _lastKnownAddress = itemAddress;
                                if (_lastKnownAddress != null)
                                {
                                    _lastAddress = new JsonObject();
                                    _lastAddress.Add("address", _lastKnownAddress.GetAddressLine(0));
                                    _lastAddress.Add("locality", _lastKnownAddress.Locality);
                                    _lastAddress.Add("country", _lastKnownAddress.CountryName);
                                    _locationError = "";
                                }
                            }
                            ret = true;
                        }
                        catch (Java.Lang.Exception ex)
                        {
                            _locationError = "Error in Getting Address. [" + ex.Message + "]";
                            LogManager.Log(LogType.Error, "GridActivity-SetAddress", _locationError);

                            //string message = "";
                            //JsonRpcClient jsonRpcClient = new JsonRpcClient();
                            //string geoUrl = "http://maps.googleapis.com/maps/api/geocode/json?latlng=" + _lastKnownLocation.Latitude.ToString() + "," + _lastKnownLocation.Longitude + "&sensor=true";
                            //JsonObject jsonReturn = jsonRpcClient.Invoke(geoUrl, out message) as JsonObject;

                        }
                        catch (Exception ex)
                        {
                            _locationError = "Error in Getting Address. Internet(Wifi) Not Enabled? [" + ex.Message + "]";
                            LogManager.Log(LogType.Error, "GridActivity-SetAddress", _locationError);
                        }
                    });
                }
                else
                {
                    ret = true;
                }
            }
            return ret;
        }

        public void OnLocationChanged(Location location)
        {
            _lastKnownLocation = location;
            SetAddress();
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

        #endregion

        #region Pick Images

        public static string _imageError = "";
        public static int _maxImageSize = 100;
        public static string _imageName = "";

        private static string _imageUriString = "";
        public static Android.Net.Uri _cameraURI;
        public static void PickPhotoFromCamera(int maxImageSize, string imageName)
        {
            _maxImageSize = maxImageSize;
            _imageName = imageName;
            _imageError = "";

            if (_gridActivity != null)
            {
                _cameraURI = null;
                _onActivityResultDone = false;
                Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                bool isMounted = Android.OS.Environment.ExternalStorageState.Equals(Android.OS.Environment.MediaMounted);

                var imageUri = _gridActivity.ContentResolver.Insert(isMounted ? MediaStore.Images.Media.ExternalContentUri : MediaStore.Images.Media.InternalContentUri, new ContentValues());
                _imageUriString = imageUri.ToString();

                intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, _imageUriString);
                intent.PutExtra("outputFormat", Bitmap.CompressFormat.Jpeg.ToString());
                intent.PutExtra("return-data", true);

                _gridActivity.StartActivityForResult(intent, PICK_PHOTO_FROM_CAMERA);
                _skipStopping = true;

            }
        }

        public static Android.Net.Uri _photoURI;
        public static void PickPhotoFromGallery(int maxImageSize, string imageName)
        {
            _maxImageSize = maxImageSize;
            _imageName = imageName;
            _imageError = "";

            if (_gridActivity != null)
            {
                _photoURI = null;
                _onActivityResultDone = false;
                Intent pickIntent = new Intent();
                pickIntent.SetType("image/*");
                pickIntent.SetAction(Intent.ActionGetContent);
                pickIntent.AddCategory(Intent.CategoryOpenable);

                var pkManager = _gridActivity.PackageManager;
                IList<Android.Content.PM.ResolveInfo> activities = pkManager.QueryIntentActivities(pickIntent, Android.Content.PM.PackageInfoFlags.Activities);
                if (activities.Count > 1)
                {
                    _gridActivity.StartActivityForResult(Intent.CreateChooser(pickIntent, "Select Photo"), PICK_PHOTO_FROM_FILE);
                    _skipStopping = true;
                }
                else
                {
                    _gridActivity.StartActivityForResult(pickIntent, PICK_PHOTO_FROM_FILE);
                    _skipStopping = true;
                }
            }
        }

        public static int GetOrientation(Android.Net.Uri photoUri)
        {
            int orientation = -1;
            if ((_gridActivity != null) && (photoUri != null))
            {
                try
                {
                    ContentResolver cr = _gridActivity.ContentResolver;
                    Android.Database.ICursor cursor = cr.Query(photoUri, new String[] { MediaStore.Images.ImageColumns.Orientation }, null, null, null);
                    if (cursor != null && cursor.Count > 0)
                    {
                        cursor.MoveToFirst();
                        orientation = cursor.GetInt(0);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogType.Error, "GridActivity_GetOrientation", "Error:" + ex.Message);
                }
            }
            return orientation;
        }

        private static Bitmap RotateScaleImage(Context context, Android.Net.Uri photoUri, long maxImageDimension)
        {
            BitmapFactory.Options dbo = new BitmapFactory.Options();
            dbo.InJustDecodeBounds = true;

            using (System.IO.Stream inputStream = context.ContentResolver.OpenInputStream(photoUri))
            {
                BitmapFactory.DecodeStream(inputStream, null, dbo);
                inputStream.Close();
            }

            int rotatedWidth, rotatedHeight;
            int orientation = GetOrientation(photoUri);

            if (orientation == 90 || orientation == 270)
            {
                rotatedWidth = dbo.OutHeight;
                rotatedHeight = dbo.OutWidth;
            }
            else
            {
                rotatedWidth = dbo.OutWidth;
                rotatedHeight = dbo.OutHeight;
            }

            Bitmap srcBitmap;
            using (System.IO.Stream inputStream = context.ContentResolver.OpenInputStream(photoUri))
            {
                if (rotatedWidth > maxImageDimension || rotatedHeight > maxImageDimension)
                {
                    float widthRatio = ((float)rotatedWidth) / ((float)maxImageDimension);
                    float heightRatio = ((float)rotatedHeight) / ((float)maxImageDimension);
                    float maxRatio = Math.Max(widthRatio, heightRatio);

                    // Create the bitmap from file
                    BitmapFactory.Options options = new BitmapFactory.Options();
                    options.InSampleSize = (int)maxRatio;
                    srcBitmap = BitmapFactory.DecodeStream(inputStream, null, options);
                }
                else
                {
                    srcBitmap = BitmapFactory.DecodeStream(inputStream);
                }
                inputStream.Close();
            }

            /*
             * if the orientation is not 0 (or -1, which means we don't know), we
             * have to do a rotation.
             */
            if (orientation > 0)
            {
                Matrix matrix = new Matrix();
                matrix.PostRotate(orientation);
                srcBitmap = Bitmap.CreateBitmap(srcBitmap, 0, 0, srcBitmap.Width, srcBitmap.Height, matrix, true);
            }

            byte[] bMapArray = { };
            string type = context.ContentResolver.GetType(photoUri);
            using (MemoryStream outputStream = new MemoryStream())
            {
                if (type.Equals("image/png"))
                {
                    srcBitmap.Compress(Bitmap.CompressFormat.Png, 100, outputStream);
                }
                else if (type.Equals("image/jpg") || type.Equals("image/jpeg"))
                {
                    srcBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, outputStream);
                }
                bMapArray = outputStream.ToArray();
                outputStream.Close();
            }

            Bitmap outputBitmap = null;
            if ((bMapArray != null) && (bMapArray.Length > 0))
            {
                outputBitmap = BitmapFactory.DecodeByteArray(bMapArray, 0, bMapArray.Length);
            }

            return outputBitmap;
        }

        //private static const int ORIENTATION_NORMAL = 1;
        //private static const int ORIENTATION_ROTATE_180 = 3;
        //private static const int ORIENTATION_ROTATE_270 = 8;
        //private static const int ORIENTATION_ROTATE_90 = 6;

        //private static void RotateImage(string filePath)
        //{
        //    ExifInterface exif = new ExifInterface(filePath);
        //    int exifOrientation = exif.GetAttributeInt(ExifInterface.TagOrientation, ORIENTATION_NORMAL);

        //    int rotate = 0;

        //    switch (exifOrientation)
        //    {
        //        case ORIENTATION_ROTATE_90:
        //            rotate = 90;
        //            break;
        //        case ORIENTATION_ROTATE_180:
        //            rotate = 180;
        //            break;
        //        case ORIENTATION_ROTATE_270:
        //            rotate = 270;
        //            break;
        //    }

        //    if (rotate != 0)
        //    {
        //        int w = bitmap.getWidth();
        //        int h = bitmap.getHeight();

        //        //// Setting pre rotate
        //        //Matrix mtx = new Matrix();
        //        //mtx.preRotate(rotate);

        //        //// Rotating Bitmap & convert to ARGB_8888, required by tess
        //        //bitmap = Bitmap.createBitmap(bitmap, 0, 0, w, h, mtx, false);
        //        //bitmap = bitmap.copy(Bitmap.Config.ARGB_8888, true);
        //    }
        //}

        //private static void SetCameraDisplayOrientation(int cameraId, Android.Hardware.Camera camera)
        //{
        //    if (_gridActivity != null)
        //    {
        //        Android.Hardware.Camera.CameraInfo info = new Android.Hardware.Camera.CameraInfo();
        //        Android.Hardware.Camera.GetCameraInfo(cameraId, info);
        //        SurfaceOrientation rotation = _gridActivity.WindowManager.DefaultDisplay.Rotation;
        //        int degrees = 0;
        //        switch (rotation)
        //        {
        //            case SurfaceOrientation.Rotation0: degrees = 0; break;
        //            case SurfaceOrientation.Rotation90: degrees = 90; break;
        //            case SurfaceOrientation.Rotation180: degrees = 180; break;
        //            case SurfaceOrientation.Rotation270: degrees = 270; break;
        //        }

        //        int result;
        //        if (info.Facing == Android.Hardware.CameraFacing.Front)
        //        {
        //            result = (info.Orientation + degrees) % 360;
        //            result = (360 - result) % 360;  // compensate the mirror
        //        }
        //        else
        //        {  // back-facing
        //            result = (info.Orientation - degrees + 360) % 360;
        //        }
        //        camera.SetDisplayOrientation(result);
        //    }
        //}

        #endregion

        #region Pick Video
        public static string _videoError = "";
        public static string _videoName = "";
        public static Android.Net.Uri _videoURI;

        public static void PickVideoFromGallery(string videoName)
        {
            if (_gridActivity != null)
            {
                _videoError = "";
                _videoName = videoName;
                _videoURI = null;

                _onActivityResultDone = false;

                Intent pickIntent = new Intent();
                pickIntent.SetType("video/*");
                pickIntent.SetAction(Intent.ActionGetContent);
                pickIntent.AddCategory(Intent.CategoryOpenable);

                var pkManager = _gridActivity.PackageManager;
                IList<Android.Content.PM.ResolveInfo> activities = pkManager.QueryIntentActivities(pickIntent, Android.Content.PM.PackageInfoFlags.Activities);
                if (activities.Count > 1)
                {
                    _gridActivity.StartActivityForResult(Intent.CreateChooser(pickIntent, "Select Video"), PICK_VIDEO_FROM_FILE);
                    _skipStopping = true;
                }
                else
                {
                    _gridActivity.StartActivityForResult(pickIntent, PICK_VIDEO_FROM_FILE);
                    _skipStopping = true;
                }

            }
        }

        #endregion

        #region Pick Audio

        public static string _audioError = "";
        public static string _audioName = "";
        public static Android.Net.Uri _audioURI;

        public static void PickAudioFromGallery(string audioName)
        {
            _audioError = "";
            _audioName = audioName;

            if (_gridActivity != null)
            {
                _audioURI = null;
                _onActivityResultDone = false;

                Intent pickIntent = new Intent();
                pickIntent.SetType("audio/*");
                pickIntent.SetAction(Intent.ActionGetContent);
                pickIntent.AddCategory(Intent.CategoryOpenable);

                var pkManager = _gridActivity.PackageManager;
                IList<Android.Content.PM.ResolveInfo> activities = pkManager.QueryIntentActivities(pickIntent, Android.Content.PM.PackageInfoFlags.Activities);
                if (activities.Count > 1)
                {
                    _gridActivity.StartActivityForResult(Intent.CreateChooser(pickIntent, "Select Audio"), PICK_AUDIO_FROM_FILE);
                    _skipStopping = true;
                }
                else
                {
                    _gridActivity.StartActivityForResult(pickIntent, PICK_AUDIO_FROM_FILE);
                    _skipStopping = true;
                }

            }
        }

        #endregion

        #region Pick Utilities

        public static string GetPathFromURI(Android.Net.Uri contentUri)
        {
            string realPath = "";
            if ((_gridActivity != null) && (contentUri != null))
            {
                try
                {
                    String[] projection = new String[] { Android.Provider.MediaStore.MediaColumns.Data };
                    ContentResolver cr = _gridActivity.ContentResolver;
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
                    LogManager.Log(LogType.Error, "GridActivity_GetRealPathFromURI", "Error:" + ex.Message);
                }
            }
            return realPath;
        }

        private static Android.Net.Uri GetContentUriFromPath(Context context, string filePath)
        {
            //Android.Database.ICursor cursor = context.ContentResolver.Query(
            //        MediaStore.Images.Media.ExternalContentUri,
            //        new String[] { MediaStore.Images.Media.InterfaceConsts.Id },
            //        MediaStore.Images.Media.InterfaceConsts.Data + "=? ",
            //        new String[] { filePath }, null);
            //if (cursor != null && cursor.MoveToFirst())
            //{
            //    int id = cursor.GetInt(cursor.GetColumnIndex(MediaStore.MediaColumns.Id));
            //    Android.Net.Uri baseUri = Android.Net.Uri.Parse("content://media/external/images/media");
            //    return Android.Net.Uri.WithAppendedPath(baseUri, "" + id);
            //}
            //else
            //{
            if (File.Exists(filePath))
            {
                ContentValues values = new ContentValues();
                values.Put(MediaStore.Images.Media.InterfaceConsts.Data, filePath);
                return context.ContentResolver.Insert(MediaStore.Images.Media.ExternalContentUri, values);
            }
            else
            {
                return null;
            }
            //}
        }

        #endregion


    }
}