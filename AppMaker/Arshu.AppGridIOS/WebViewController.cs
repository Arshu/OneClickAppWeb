using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

using Mono.Data.Sqlite;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;
using MonoTouch.MessageUI;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.AssetsLibrary;
using MonoTouch.MediaPlayer;
using MonoTouch.AVFoundation;
using MonoTouch.CoreMedia;

using Arshu.Web.Basic.Log;
using Arshu.Web.IO;
using Arshu.Web.Common;
using Arshu.Web.Json;

using Arshu.AppWeb;

namespace Arshu.AppGrid
{
    [Register("WebViewController")]
    public class WebViewController : UIViewController
    {
        #region Variable

        private static ArshuWebGrid _arshuWebGrid = null;
        private static WebViewController _gridViewController = null;

        private float heightOffset = 25.0f;

        #endregion

        #region Constructor

        public WebViewController()
        {
            _arshuWebGrid = new ArshuWebGrid(this);

            DummyObjectRegister();
        }

        #endregion

        #region Override DidReceiveMemoryWarning

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Override ViewDidLoad

        /// <summary>
        /// Views the did load.
        /// </summary>
        public override void ViewDidLoad()
        {
            _gridViewController = this;

            InitWebGrid();

            base.ViewDidLoad();
        }

        #endregion

        #region Override WillRotate

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            Console.WriteLine("WillRotate! " + UIDevice.CurrentDevice.Orientation);

            ReloadWebGrid();

            base.WillRotate(toInterfaceOrientation, duration);
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
                Size screenSize = _arshuWebGrid.GetScreenSize();
                _arshuWebGrid.InitView(rootView, screenSize.Width, screenSize.Height, heightOffset);

                _arshuWebGrid.CurrentPageAnimation = PageAnimation.FlipRight;
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
            if ((_arshuWebGrid == null) && (_isNull == true))
            {
                _xmlDocument = new System.Xml.XmlDocument();
            }

            if ((_xmlDocument == null) && (_isNull == false))
            {
                _isNull = true;
            }

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
                if (_videoAsset != null) _videoAsset.Dispose();
                _videoAsset = null;
                if (_videoExporter != null) _videoExporter.Dispose();
                _videoExporter = null;
                if (_audioAsset != null) _audioAsset.Dispose();
                _audioAsset = null;
                if (_audioExporter != null) _audioExporter.Dispose();
                _audioExporter = null;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "WebViewController_CleanUp", "Error:" + ex.Message);
            }
        }

        #endregion



        #region Refresh WebView

        private static void RefreshWebView(string source)
        {
            if (_gridViewController != null)
            {
                if (_arshuWebGrid != null)
                {
                    _gridViewController.InvokeOnMainThread(delegate()
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

        private static CLLocationManager _locationManager;
        private static CLLocation _lastKnownLocation = null;

        public static bool DisableLocationTracking()
        {
            bool ret = true;

            try
            {
                if (_gridViewController != null)
                {
                    _gridViewController.BeginInvokeOnMainThread(delegate
                    {
                        if (_locationManager != null)
                        {
                            _locationManager.StopUpdatingLocation();
                            _locationManager.Delegate = null;
                            // handle the updated location method and update the UI
                            _locationManager.LocationsUpdated -= (object sender, CLLocationsUpdatedEventArgs e) =>
                            {
                            };
                            _locationManager = null;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "GridViewController-DisableLocationTracking", ex.Message);
                ret = false;
            }
            return ret;
        }

        public static bool EnableLocationTracking(out string retMessage)
        {
            bool ret = true;
            string message = "";

            try
            {
                if (_gridViewController != null)
                {
                    _gridViewController.BeginInvokeOnMainThread(delegate
                    {
                        if (_locationManager == null)
                        {
                            _locationManager = new CLLocationManager();
                            _locationManager.Delegate = new LocationManagerDelegate(_gridViewController);

                            // handle the updated location method and update the UI
                            _locationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) =>
                            {
                                LocationManagerDelegate.UpdateLocation(e.Locations[e.Locations.Length - 1]);
                            };

                            // start updating our location, et. al.
                            if (CLLocationManager.LocationServicesEnabled)
                                _locationManager.StartUpdatingLocation();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "GridViewController-EnableLocationTracking", ex.Message);
                message = "EnableLocationTracking Error:" + ex.Message;
                ret = false;
            }

            _enableLocationTracking = true;
            retMessage = message;
            return ret;

        }

        private class LocationManagerDelegate : CLLocationManagerDelegate
        {
            #region Variables

            public enum UnitsOfLength
            {
                Miles,
                Kilometer,
                NauticalMiles
            }

            private static double _MilesToKilometers = 1.609344;
            private static double _MilesToNautical = 0.868976242;
            //private GridViewController _gridViewController;
            private static CLGeocoder _geocoder = new CLGeocoder();

            #endregion

            #region Constructor

            public LocationManagerDelegate(WebViewController gridViewController)
                : base()
            {
                _gridViewController = gridViewController;

                Console.WriteLine("Delegate created");
            }

            #endregion

            #region Overrides

            // called for iOS5.x and earlier
            [Obsolete]
            public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
            {
                UpdateLocation(newLocation);
            }

            public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
            {
                if (locations.Length > 0)
                {
                    CLLocation newLocation = locations[locations.Length - 1];
                    UpdateLocation(newLocation);
                }
            }

            public override void Failed(CLLocationManager manager, NSError error)
            {
                //_appd.labelInfo.Text = "Failed to find location";
                Console.WriteLine("Failed to find location");
                base.Failed(manager, error);
            }

            #endregion

            #region Update Location

            public static void UpdateLocation(CLLocation location)
            {
                _lastKnownLocation = location; ;
                if (_lastKnownLocation != null)
                {
                    _lastLocation = new JsonObject();
                    _lastLocation.Add("latitude", _lastKnownLocation.Coordinate.Latitude.ToString());
                    _lastLocation.Add("longitude", _lastKnownLocation.Coordinate.Longitude.ToString());

                    //Console.WriteLine("Location updated");
                    //double distanceToConference = Distance(new Coordinate(orgLocation), new Coordinate(newLocation.Coordinate), UnitsOfLength.Miles);
                    //Console.WriteLine("Distance: {0}", distanceToConference);

                    if (_geocoder != null)
                    {
                        _geocoder.ReverseGeocodeLocation(_lastKnownLocation, HandleCLGeocodeCompletionHandler);
                    }
                }
            }

            private static void HandleCLGeocodeCompletionHandler(CLPlacemark[] placemarks, NSError error)
            {
                if ((placemarks != null) && (placemarks.Length > 0))
                {
                    //var loc = placemarks[placemarks.Length -1].Location.Coordinate;
                    //oal.Add(new ObjAnnotation(new CLLocationCoordinate2D(loc.Latitude, loc.Longitude), placemarks[placemarks.Length -1].Name, string.Empty));
                    _lastAddress = new JsonObject();
                    _lastAddress.Add("address", placemarks[placemarks.Length - 1].SubThoroughfare + "," + placemarks[placemarks.Length - 1].Thoroughfare);
                    _lastAddress.Add("locality", placemarks[placemarks.Length - 1].Locality);
                    _lastAddress.Add("country", placemarks[placemarks.Length - 1].Country);
                }
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Calculates the distance between two points of latitude and longitude.
            /// Great Link - http://www.movable-type.co.uk/scripts/latlong.html
            /// </summary>
            /// <param name="coordinate1">First coordinate.</param>
            /// <param name="coordinate2">Second coordinate.</param>
            /// <param name="unitsOfLength">Sets the return value unit of length.</param>
            public static Double Distance(Coordinate coordinate1, Coordinate coordinate2, UnitsOfLength unitsOfLength)
            {
                var theta = coordinate1.Longitude - coordinate2.Longitude;
                var distance = Math.Sin(Coordinate.ToRadian(coordinate1.Latitude)) * Math.Sin(Coordinate.ToRadian(coordinate2.Latitude)) +
                               Math.Cos(Coordinate.ToRadian(coordinate1.Latitude)) * Math.Cos(Coordinate.ToRadian(coordinate2.Latitude)) *
                               Math.Cos(Coordinate.ToRadian(theta));

                distance = Math.Acos(distance);
                distance = Coordinate.ToDegree(distance);
                distance = distance * 60 * 1.1515;

                if (unitsOfLength == UnitsOfLength.Kilometer)
                    distance = distance * _MilesToKilometers;
                else if (unitsOfLength == UnitsOfLength.NauticalMiles)
                    distance = distance * _MilesToNautical;

                return (distance);
            }

            public class Coordinate
            {
                private double latitude, longitude;

                public Coordinate() { }

                public Coordinate(CLLocationCoordinate2D mapCoord)
                {
                    latitude = mapCoord.Latitude;
                    longitude = mapCoord.Longitude;
                }

                public static double ToRadian(double d)
                {
                    return d * (Math.PI / 180);
                }
                public static double ToDegree(double r)
                {
                    return r * (180 / Math.PI);
                }

                /// <summary>
                /// Latitude in degrees. -90 to 90
                /// </summary>
                public Double Latitude
                {
                    get { return latitude; }
                    set
                    {
                        if (value > 90) throw new ArgumentOutOfRangeException("value", "Latitude value cannot be greater than 90.");
                        if (value < -90) throw new ArgumentOutOfRangeException("value", "Latitude value cannot be less than -90.");
                        latitude = value;
                    }
                }

                /// <summary>
                /// Longitude in degree. -180 to 180
                /// </summary>
                public Double Longitude
                {
                    get { return longitude; }
                    set
                    {
                        if (value > 180) throw new ArgumentOutOfRangeException("value", "Longitude value cannot be greater than 180.");
                        if (value < -180) throw new ArgumentOutOfRangeException("value", "Longitude value cannot be less than -180.");
                        longitude = value;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Pick Images

        public static string _imageError = "";
        public static int _maxImageSize = 100;
        public static string _imageName = "";

        public static NSUrl _cameraURI;
        private static UIImagePickerController _cameraPicker;
        public static void PickPhotoFromCamera(int maxImageSize, string imageName)
        {
            _maxImageSize = maxImageSize;
            _imageName = imageName;
            _imageError = "";

            if (_gridViewController != null)
            {
                _gridViewController.BeginInvokeOnMainThread(delegate
                {
                    _cameraPicker = new UIImagePickerController();

                    _cameraPicker.SourceType = UIImagePickerControllerSourceType.Camera;
                    _cameraPicker.MediaTypes = new string[] { "public.image" }; //UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera);
                    _cameraPicker.AllowsEditing = false;

                    _cameraPicker.FinishedPickingMedia += Handle_FinishedPickingImage;
                    _cameraPicker.Canceled += Handle_PickingImageCanceled;

                    if (_gridViewController.NavigationController == null)
                    {
                        _gridViewController.PresentViewController(_cameraPicker, true, null);
                    }
                    else
                    {
                        _gridViewController.NavigationController.PresentViewController(_cameraPicker, true, null);
                    }
                });
            }
        }

        public static NSUrl _photoURI;
        private static UIImagePickerController _imagePicker;
        public static void PickPhotoFromGallery(int maxImageSize, string imageName)
        {
            _maxImageSize = maxImageSize;
            _imageName = imageName;
            _imageError = "";

            if (_gridViewController != null)
            {
                _gridViewController.BeginInvokeOnMainThread(delegate
                {
                    _imagePicker = new UIImagePickerController();

                    _imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                    _imagePicker.MediaTypes = new string[] { "public.image" }; //UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
                    _imagePicker.AllowsEditing = false;

                    _imagePicker.FinishedPickingMedia += Handle_FinishedPickingImage;
                    _imagePicker.Canceled += Handle_PickingImageCanceled;

                    if (_gridViewController.NavigationController == null)
                    {
                        _gridViewController.PresentViewController(_imagePicker, true, null);
                    }
                    else
                    {
                        _gridViewController.NavigationController.PresentViewController(_imagePicker, true, null);
                    }
                });
            }
        }

        private static void Handle_FinishedPickingImage(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            try
            {
                #region Check If Image

                bool isImage = false;
                switch (e.Info[UIImagePickerController.MediaType].ToString())
                {
                    case "public.image":
                        isImage = true;
                        break;
                    case "public.video":
                        break;
                }

                #endregion

                #region Get Reference Url

                // get common info (shared between images and video)
                //NSUrl referenceURL = e.Info[UIImagePickerController.ReferenceUrl] as NSUrl;
                ////referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceUrl")] as NSUrl;
                //if (referenceURL != null)
                //{
                //    Console.WriteLine(referenceURL.ToString());
                //    if (isImage)
                //    {
                //        if (_imagePicker != null)
                //        {
                //            _photoURI = referenceURL;
                //        }
                //        else if (_cameraPicker != null)
                //        {
                //            _cameraURI = referenceURL;
                //        }
                //    }
                //    Console.WriteLine("Url:" + referenceURL.ToString());
                //}

                #endregion

                if (isImage)
                {
                    #region Get Selected Image

                    UIImage imageSelected = null;
                    UIImage editedImage = e.Info[UIImagePickerController.EditedImage] as UIImage;
                    if (editedImage != null)
                    {
                        imageSelected = editedImage;
                    }
                    else
                    {
                        UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                        if (originalImage != null)
                        {
                            imageSelected = originalImage;
                        }
                    }

                    #endregion

                    #region Resize, Rotate and Save Image

                    if (imageSelected != null)
                    {
                        #region Resize and Rotate

                        UIImage imageToSave = ScaleImage(imageSelected, _maxImageSize);

                        #endregion

                        if (imageToSave != null)
                        {
                            #region Save to Temp Location

                            string tempFilePath = "";
                            if (_imagePicker != null)
                            {
                                if (string.IsNullOrEmpty(_imageName) == true) _imageName = "GalleryImage.png";
                                tempFilePath = IOManager.GetTempFilePath(_imageName, true);
                            }
                            else if (_cameraPicker != null)
                            {
                                if (string.IsNullOrEmpty(_imageName) == true) _imageName = "CameraImage.png";
                                tempFilePath = IOManager.GetTempFilePath("CameraImage.png", true);
                            }
                            if (tempFilePath != null)
                            {
                                if (File.Exists(tempFilePath) == true) File.Delete(tempFilePath);

                                NSData imageData = imageToSave.AsPNG();
                                if (imageData != null)
                                {
                                    File.WriteAllBytes(tempFilePath, NSDataToByte(imageData));
                                }
                            }

                            #endregion

                            #region Set Image Url

                            if (IOManager.CachedFileExists(tempFilePath, false, false) == true)
                            {
                                if (_imagePicker != null)
                                {
                                    _photoURI = new NSUrl(tempFilePath.Replace(IOManager.RootDirectory, ""));
                                }
                                else if (_cameraPicker != null)
                                {
                                    _cameraURI = new NSUrl(tempFilePath.Replace(IOManager.RootDirectory, ""));
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion
                }
                else
                {
                    #region Get Media Url

                    NSUrl mediaURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
                    if (mediaURL != null)
                    {
                        Console.WriteLine(mediaURL.ToString());

                        if (_imagePicker != null)
                        {
                            _photoURI = mediaURL;
                        }
                        else if (_cameraPicker != null)
                        {
                            _cameraURI = mediaURL;
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.Write("Handle_FinishedPickingImage Error: " + ex.Message);
            }

            DismissImagePickerViews();
        }

        private static void Handle_PickingImageCanceled(object sender, EventArgs e)
        {
            DismissImagePickerViews();
        }

        public static UIImage ScaleImage(UIImage image, int maxSize)
        {
            UIImage res;
            if (maxSize == 0) maxSize = 1000;

            using (CGImage imageRef = image.CGImage)
            {
                CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
                CGColorSpace colorSpaceInfo = CGColorSpace.CreateDeviceRGB();
                if (alphaInfo == CGImageAlphaInfo.None)
                {
                    alphaInfo = CGImageAlphaInfo.NoneSkipLast;
                }

                int width, height;

                width = imageRef.Width;
                height = imageRef.Height;


                if (height >= width)
                {
                    width = (int)Math.Floor((double)width * ((double)maxSize / (double)height));
                    height = maxSize;
                }
                else
                {
                    height = (int)Math.Floor((double)height * ((double)maxSize / (double)width));
                    width = maxSize;
                }


                CGBitmapContext bitmap;

                if (image.Orientation == UIImageOrientation.Up || image.Orientation == UIImageOrientation.Down)
                {
                    bitmap = new CGBitmapContext(IntPtr.Zero, width, height, imageRef.BitsPerComponent, imageRef.BytesPerRow, colorSpaceInfo, alphaInfo);
                }
                else
                {
                    bitmap = new CGBitmapContext(IntPtr.Zero, height, width, imageRef.BitsPerComponent, imageRef.BytesPerRow, colorSpaceInfo, alphaInfo);
                }

                switch (image.Orientation)
                {
                    case UIImageOrientation.Left:
                        bitmap.RotateCTM((float)Math.PI / 2);
                        bitmap.TranslateCTM(0, -height);
                        break;
                    case UIImageOrientation.Right:
                        bitmap.RotateCTM(-((float)Math.PI / 2));
                        bitmap.TranslateCTM(-width, 0);
                        break;
                    case UIImageOrientation.Up:
                        break;
                    case UIImageOrientation.Down:
                        bitmap.TranslateCTM(width, height);
                        bitmap.RotateCTM(-(float)Math.PI);
                        break;
                }

                bitmap.DrawImage(new Rectangle(0, 0, width, height), imageRef);


                res = UIImage.FromImage(bitmap.ToImage());
                bitmap = null;

            }

            return res;
        }

        private static void DismissImagePickerViews()
        {
            // dismiss the picker
            if (_cameraPicker != null)
            {
                _cameraPicker.DismissViewController(true, null);
                RefreshWebView("FinishedPickingCamera");
            }
            if (_imagePicker != null)
            {
                _imagePicker.DismissViewController(true, null);
                RefreshWebView("FinishedPickingImage");
            }
            _imagePicker = null;
            _cameraPicker = null;

        }

        private static byte[] NSDataToByte(NSData data)
        {
            byte[] result = { };
            try
            {
                result = new byte[data.Length];
                Marshal.Copy(data.Bytes, result, 0, (int)data.Length);
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "NSDataToByte", "Error:" + ex.Message);
            }
            return result;
        }

        #endregion

        #region Pick Video

        public static string _videoError = "";
        public static string _videoName = "";
        public static NSUrl _videoURI;

        private static UIImagePickerController _videoPicker;
        public static void PickVideoFromGallery(string videoName)
        {
            _videoError = "";
            _videoName = videoName;

            if (_gridViewController != null)
            {
                _gridViewController.BeginInvokeOnMainThread(delegate
                {
                    _videoPicker = new UIImagePickerController();

                    _videoPicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                    _videoPicker.MediaTypes = new string[] { "public.movie" }; //UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
                    _videoPicker.AllowsEditing = false;

                    _videoPicker.FinishedPickingMedia += Handle_FinishedPickingVideo;
                    _videoPicker.Canceled += Handle_PickingVideoCanceled;

                    if (_gridViewController.NavigationController == null)
                    {
                        _gridViewController.PresentViewController(_videoPicker, true, null);
                    }
                    else
                    {
                        _gridViewController.NavigationController.PresentViewController(_videoPicker, true, null);
                    }
                });
            }
        }

        private static AVAssetExportSession _videoExporter = null;
        private static AVUrlAsset _videoAsset = null;
        private static void Handle_FinishedPickingVideo(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            try
            {
                #region Check If Video

                bool isVideo = true;
                switch (e.Info[UIImagePickerController.MediaType].ToString())
                {
                    case "public.image":
                        isVideo = false;
                        break;
                    case "public.movie":
                        isVideo = true;
                        break;
                }

                #endregion

                if (isVideo)
                {
                    #region Get Video Url

                    NSUrl videoURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
                    if (videoURL != null)
                    {
                        if (_videoPicker != null)
                        {
                            if (string.IsNullOrEmpty(_videoName) == true) _videoName = "GalleryVideo.mp4";
                            string tempFilePath = IOManager.GetTempFilePath(_videoName, true);
                            if (tempFilePath != null)
                            {
                                if (File.Exists(tempFilePath) == true) File.Delete(tempFilePath);

                                if (_videoAsset != null) _videoAsset.Dispose();
                                if (_videoExporter != null) _videoExporter.Dispose();

                                _videoAsset = AVUrlAsset.FromUrl(videoURL, null);
                                _videoExporter = AVAssetExportSession.FromAsset(_videoAsset, AVAssetExportSession.PresetPassthrough); //AVAssetExportSession.PresetMediumQuality
                                _videoExporter.OutputFileType = AVFileType.Mpeg4;
                                _videoExporter.OutputUrl = NSUrl.FromFilename(tempFilePath);

                                // Set range
                                //CMTimeRange range = new CMTimeRange();
                                //range.Start = CMTime.FromSeconds (0, songAsset.Duration.TimeScale);
                                //range.Duration = CMTime.FromSeconds (stopTime, songAsset.Duration.TimeScale);
                                //exporter.TimeRange = range;

                                _videoExporter.ExportAsynchronously(new AVCompletionHandler(delegate
                                {
                                    switch (_videoExporter.Status)
                                    {
                                        case AVAssetExportSessionStatus.Failed:
                                            _videoError = _videoExporter.Error.Description;
                                            Console.WriteLine("Export failed: {0}", _videoExporter.Error.Description);
                                            break;
                                        case AVAssetExportSessionStatus.Cancelled:
                                            Console.WriteLine("Export cancelled");
                                            break;
                                        case AVAssetExportSessionStatus.Completed:
                                            _videoURI = new NSUrl(tempFilePath.Replace(IOManager.RootDirectory, ""));
                                            RefreshWebView("FinishedPickingVideo");
                                            Console.WriteLine("Export Completed");
                                            break;
                                        default:
                                            Console.WriteLine("Export {0}", _videoExporter.Status);
                                            break;
                                    }

                                }));

                                //NSData videoData = NSData.FromUrl(videoURL);
                                //if (videoData != null)
                                //{
                                //    File.WriteAllBytes(tempFilePath, NSDataToByte(videoData));
                                //    if (_videoPicker != null)
                                //    {
                                //        _videoURI = new NSUrl(tempFilePath.Replace(IOManager.RootDirectory, ""));
                                //    }
                                //}
                            }
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.Write("Handle_FinishedPickingVideo Error: " + ex.Message);

            }

            DismissVideoPickerViews();
        }

        private static void Handle_PickingVideoCanceled(object sender, EventArgs e)
        {
            DismissVideoPickerViews();
        }

        private static void DismissVideoPickerViews()
        {
            // dismiss the picker
            if (_videoPicker != null)
            {
                _videoPicker.DismissViewController(true, null);
            }
            _videoPicker = null;

        }

        #endregion

        #region Pick Audio

        public static string _audioError = "";
        public static string _audioName = "";
        public static NSUrl _audioURI;

        private static MPMediaPickerController _audioPicker;
        public static void PickAudioFromGallery(string audioName)
        {
            _audioError = "";
            _audioName = audioName;
            if (_gridViewController != null)
            {
                _gridViewController.BeginInvokeOnMainThread(delegate
                {
                    _audioPicker = new MPMediaPickerController(MPMediaType.Music);
                    _audioPicker.AllowsPickingMultipleItems = false;

                    _audioPicker.ItemsPicked += _mediaPicker_ItemsPicked;
                    _audioPicker.DidCancel += _mediaPicker_DidCancel;

                    if (_gridViewController.NavigationController == null)
                    {
                        _gridViewController.PresentViewController(_audioPicker, true, null);
                    }
                    else
                    {
                        _gridViewController.NavigationController.PresentViewController(_audioPicker, true, null);
                    }

                });
            }
        }

        private static void _mediaPicker_DidCancel(object sender, EventArgs e)
        {
            DismissMediaPickerView();
        }

        private static AVAssetExportSession _audioExporter = null;
        private static AVUrlAsset _audioAsset = null;
        private static void _mediaPicker_ItemsPicked(object sender, ItemsPickedEventArgs e)
        {
            if (e.MediaItemCollection.Items != null)
            {
                if (e.MediaItemCollection.Items.Length > 0)
                {
                    NSUrl audioURI = e.MediaItemCollection.Items[0].AssetURL;

                    if (string.IsNullOrEmpty(_audioName) == true) _audioName = "GalleryAudio.m4a";
                    string tempFilePath = IOManager.GetTempFilePath(_audioName, true);
                    if (tempFilePath != null)
                    {
                        if (File.Exists(tempFilePath) == true) File.Delete(tempFilePath);

                        if (_audioAsset != null) _audioAsset.Dispose();
                        if (_audioExporter != null) _audioExporter.Dispose();

                        _audioAsset = AVUrlAsset.FromUrl(audioURI, null);
                        _audioExporter = AVAssetExportSession.FromAsset(_audioAsset, AVAssetExportSession.PresetPassthrough);
                        _audioExporter.OutputFileType = AVFileType.AppleM4A;
                        _audioExporter.OutputUrl = NSUrl.FromFilename(tempFilePath);
                        // Set range
                        //CMTimeRange range = new CMTimeRange();
                        //range.Start = CMTime.FromSeconds (0, songAsset.Duration.TimeScale);
                        //range.Duration = CMTime.FromSeconds (stopTime, songAsset.Duration.TimeScale);
                        //exporter.TimeRange = range;

                        _audioExporter.ExportAsynchronously(new AVCompletionHandler(delegate
                        {
                            switch (_audioExporter.Status)
                            {
                                case AVAssetExportSessionStatus.Failed:
                                    _audioError = _audioExporter.Error.Description;
                                    Console.WriteLine("Export failed: {0}", _audioError);
                                    break;
                                case AVAssetExportSessionStatus.Cancelled:
                                    Console.WriteLine("Export cancelled");
                                    break;
                                case AVAssetExportSessionStatus.Completed:
                                    _audioURI = new NSUrl(tempFilePath.Replace(IOManager.RootDirectory, ""));
                                    RefreshWebView("FinishedPickingAudio");
                                    Console.WriteLine("Export Completed");
                                    break;
                                default:
                                    Console.WriteLine("Export {0}", _audioExporter.Status);
                                    break;
                            }

                        }));
                    }
                }
            }

            DismissMediaPickerView();
        }

        private static void DismissMediaPickerView()
        {
            // dismiss the picker
            if (_audioPicker != null)
            {
                _audioPicker.DismissViewController(true, null);
            }
            _audioPicker = null;
        }

        #endregion


    }
}