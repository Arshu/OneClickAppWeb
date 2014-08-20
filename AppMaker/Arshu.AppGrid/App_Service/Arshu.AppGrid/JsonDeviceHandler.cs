using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Globalization;

using Arshu.Web.Basic.Log;
using Arshu.Web.Common;
using Arshu.Web.Json;
using Arshu.Web.IO;
using Arshu.Web.Http;

using Arshu.Web;

namespace Arshu.AppGrid
{
    /// <summary>
    /// A http Native handler to serve Grid Functionality
    /// </summary>
    [JsonRpcHelp("Json Device Handler")]
    public class JsonDeviceHandler : JsonRpcHandler
    {
        private static JsonDeviceService service = new JsonDeviceService();
        public JsonDeviceHandler()
        {
            if (service == null) service = new JsonDeviceService();
        }
    }

    [JsonRpcHelp("Json Device Service")]
    public class JsonDeviceService : JsonRpcService
    {
        #region Animation

        [JsonRpcMethod("GetCurrentPageAnimation", Idempotent = false)]
        [JsonRpcHelp("Get the Current Page Animation. Returns JsonObject(animation)")]
        public JsonObject GetCurrentPageAnimation()
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            PageAnimation pageAnimation = WebActivity.GetCurrentPageAnimation();
            retMessage.Add("animation", pageAnimation.ToString());
#elif __IOS__
            PageAnimation pageAnimation = WebViewController.GetCurrentPageAnimation();
            retMessage.Add("animation", pageAnimation.ToString());
#else
            retMessage.Add("error", "API not available");
#endif
            return retMessage;
        }

        #endregion
     
        #region Network

        [JsonRpcMethod("IsNetworkAvailable", Idempotent = false)]
        [JsonRpcHelp("Check if the Network is Available. Returns JsonObject(status)")]
        public JsonObject IsNetworkAvailable(bool wifiOnly)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            bool networkAvailable = false;
            networkAvailable = Arshu.AppWeb.Utility.IsNetworkAvailable(wifiOnly);
            retMessage.Add("status", networkAvailable.ToString());
#elif __IOS__
            bool networkAvailable = false;
            //MonoTouch.ObjCRuntime.Runtime.StartWWAN (new Uri("www.google.com"));
            NetworkStatus networkStatus = Arshu.AppWeb.Reachability.LocalWifiConnectionStatus();
            if ((networkStatus == NetworkStatus.ReachableViaWiFiNetwork) && (wifiOnly ==true))
            {
                networkAvailable = true;
            }
            else if ((networkStatus == NetworkStatus.ReachableViaCarrierDataNetwork) && (wifiOnly == false))
            {
                networkAvailable = true;
            }
            retMessage.Add("status", networkAvailable.ToString());
#else
            retMessage.Add("status", "true");
#endif

            return retMessage;
        }

        #endregion

        #region Location

#if !__ANDROID__ && !__IOS__
        private bool _enableLocationTracking = false;
#endif

        [JsonRpcMethod("EnableLocationTracking", Idempotent = true)]
        [JsonRpcHelp("Enable Location Tracking. Will Auto Disable On Close. Returns JsonObject(status, error, message)")]
        public JsonObject EnableLocationTracking(long minTimeInMs, float minDistance)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            string message = "";
            bool status = false;
            status = WebActivity.EnableLocationTracking(minTimeInMs, minDistance, out message);
            if (status == true)
            {
                retMessage.Add("status", true);
                retMessage.Add("message", "Location Tracking Enabled");
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "Error in Enabling Location Tracking");
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }
#elif __IOS__
            string message = "";
            bool status = false;
            status = WebViewController.EnableLocationTracking(out message);
            if (status == true)
            {
                retMessage.Add("status", true);
                retMessage.Add("message", "Location Tracking Enabled");
            }
            else
            {
                if (message.Trim().Length == 0)
                {
                    retMessage.Add("error", "Error in Enabling Location Tracking");
                }
                else
                {
                    retMessage.Add("error", message);
                }
            }
#else
            _enableLocationTracking = true;
            retMessage.Add("status", true);
            retMessage.Add("message", "Location Tracking Enabled");
#endif
            return retMessage;

        }

        [JsonRpcMethod("DisableLocationTracking", Idempotent = true)]
        [JsonRpcHelp("Disable Location Tracking. Returns JsonObject(status, error, message)")]
        public JsonObject DisableLocationTracking()
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            bool status = WebActivity.DisableLocationTracking();
            if (status == true)
            {
                retMessage.Add("status", true);
                retMessage.Add("message", "Location Tracking Disabled");
            }
            else
            {
                retMessage.Add("error", "Error in Disabling Location Tracking");
            }
#elif __IOS__
            bool status = WebViewController.DisableLocationTracking();
            if (status == true)
            {
                retMessage.Add("status", true);
                retMessage.Add("message", "Location Tracking Disabled");
            }
            else
            {
                retMessage.Add("error", "Error in Disabling Location Tracking");
            }
#else
            _enableLocationTracking = false;
            retMessage.Add("status", true);
            retMessage.Add("message", "Location Tracking Disabled");
#endif
            return retMessage;

        }

        [JsonRpcMethod("GetLocation", Idempotent = true)]
        [JsonRpcHelp("Get the Current Location. Returns JsonObject(latitude, longitude, address, locality, country and locationInfo)")]
        public JsonObject GetLocation(bool locationOnly)
        {
            var retMessage = new JsonObject();

#if __ANDROID__
            bool locationTrackingEnabled = false;
            JsonObject lastLocation = null;
            JsonObject lastAddress = null;
            string locationError = "";

            locationTrackingEnabled = WebActivity._enableLocationTracking;
            lastLocation = WebActivity._lastLocation;
            lastAddress = WebActivity._lastAddress;
            locationError = WebActivity._locationError;
            string locationInfo = "";
            if (locationTrackingEnabled == true)
            {
                if (lastLocation != null)
                {
                    string latitude = "";
                    if (lastLocation.ContainsKey("latitude") == true)
                    {
                        if (lastLocation["latitude"] != null)
                        {
                            latitude = lastLocation["latitude"].ToString();
                        }
                    }
                    string longitude = "";
                    if (lastLocation.ContainsKey("longitude") == true)
                    {
                        if (lastLocation["longitude"] != null)
                        {
                            longitude = lastLocation["longitude"].ToString();
                        }
                    }

                    retMessage.Add("latitude", latitude);
                    retMessage.Add("longitude", longitude);

                    locationInfo += " latitude=" + latitude + " longitude=" + longitude + " ";
                }
                else
                {
                    if (string.IsNullOrEmpty(locationError) == true)
                    {
                        retMessage.Add("error", "Location Info Not Available or Not Enabled");
                    }
                    else
                    {
                        retMessage.Add("error", locationError);
                    }
                }

                if (locationOnly == false)
                {
                    if (lastAddress != null)
                    {
                        string address = "";
                        if (lastAddress.ContainsKey("address") == true)
                        {
                            if (lastAddress["address"] != null)
                            {
                                address = lastAddress["address"].ToString();
                            }
                        }
                        string locality = "";
                        if (lastAddress.ContainsKey("locality") == true)
                        {
                            if (lastAddress["locality"] != null)
                            {
                                locality = lastAddress["locality"].ToString();
                            }
                        }
                        string country = "";
                        if (lastAddress.ContainsKey("country") == true)
                        {
                            if (lastAddress["country"] != null)
                            {
                                country = lastAddress["country"].ToString();
                            }
                        }
                        retMessage.Add("address", address);
                        retMessage.Add("locality", locality);
                        retMessage.Add("country", country);

                        locationInfo += " address=" + address + " locality=" + locality + " country=" + country;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(locationError) == true)
                        {
                            retMessage.Add("error", "Address Info Not Available or Not Enabled");
                        }
                        else
                        {
                            retMessage.Add("error", locationError);
                        }
                    }
                }
                if (locationInfo.Trim().Length > 0)
                {
                    retMessage.Add("locationInfo", locationInfo);
                }
            }
            else
            {
                retMessage.Add("error", "Location Tracking not enabled");
            }
#elif __IOS__
            bool locationTrackingEnabled = false;
            JsonObject lastLocation = null;
            JsonObject lastAddress = null;
            string locationError = "";

            locationTrackingEnabled = WebViewController._enableLocationTracking;
            lastLocation = WebViewController._lastLocation;
            lastAddress = WebViewController._lastAddress;
            string locationInfo = "";
            if (locationTrackingEnabled == true)
            {
                if (lastLocation != null)
                {
                    string latitude = "";
                    if (lastLocation.ContainsKey("latitude") == true)
                    {
                        if (lastLocation["latitude"] != null)
                        {
                            latitude = lastLocation["latitude"].ToString();
                        }
                    }
                    string longitude = "";
                    if (lastLocation.ContainsKey("longitude") == true)
                    {
                        if (lastLocation["longitude"] != null)
                        {
                            longitude = lastLocation["longitude"].ToString();
                        }
                    }

                    retMessage.Add("latitude", latitude);
                    retMessage.Add("longitude", longitude);

                    locationInfo += " latitude=" + latitude + " longitude=" + longitude + " ";
                }
                else
                {
                    if (string.IsNullOrEmpty(locationError) == true)
                    {
                        retMessage.Add("error", "Location Info Not Available or Not Enabled");
                    }
                    else
                    {
                        retMessage.Add("error", locationError);
                    }
                }

                if (locationOnly == false)
                {
                    if (lastAddress != null)
                    {
                        string address = "";
                        if (lastAddress.ContainsKey("address") == true)
                        {
                            if (lastAddress["address"] != null)
                            {
                                address = lastAddress["address"].ToString();
                            }
                        }
                        string locality = "";
                        if (lastAddress.ContainsKey("locality") == true)
                        {
                            if (lastAddress["locality"] != null)
                            {
                                locality = lastAddress["locality"].ToString();
                            }
                        }
                        string country = "";
                        if (lastAddress.ContainsKey("country") == true)
                        {
                            if (lastAddress["country"] != null)
                            {
                                country = lastAddress["country"].ToString();
                            }
                        }
                        retMessage.Add("address", address);
                        retMessage.Add("locality", locality);
                        retMessage.Add("country", country);

                        locationInfo += " address=" + address + " locality=" + locality + " country=" + country;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(locationError) == true)
                        {
                            retMessage.Add("error", "Address Info Not Available or Not Enabled");
                        }
                        else
                        {
                            retMessage.Add("error", locationError);
                        }
                    }
                }
                if (locationInfo.Trim().Length > 0)
                {
                    retMessage.Add("locationInfo", locationInfo);
                }
            }
            else
            {
                retMessage.Add("error", "Location Tracking not enabled");
            }

#else
            if (_enableLocationTracking == true)
            {
                string locationInfo = "";

                string latitude = "1.298721200000000000";
                string longitude = "103.847431000000030000";

                retMessage.Add("latitude", latitude);
                retMessage.Add("longitude", longitude);
                locationInfo += " latitude=" + latitude + " longitude=" + longitude + " ";

                string address = "Orchard Rd, Singapore";
                string locality = "Singapore";
                string country = "Singapore";

                retMessage.Add("address", address);
                retMessage.Add("locality", locality);
                retMessage.Add("country", country);

                locationInfo += " address=" + address + " locality=" + locality + " country=" + country;

                if (locationInfo.Trim().Length > 0)
                {
                    retMessage.Add("locationInfo", locationInfo);
                }
            }
            else
            {
                retMessage.Add("error", "Location Tracking Not Enabled");
            }
#endif
            return retMessage;
        }

        #endregion

        #region Media - Image

#if !__ANDROID__ && !__IOS__
        private static string _cameraImageDefault = "/App_Resource/Media/camera.png";
        private static string _cameraImage = "";
#endif

        [JsonRpcMethod("LaunchCamera", Idempotent = true)]
        [JsonRpcHelp("Launch Camera. Returns JsonObject(message) and Launches Camera Intent")]
        public JsonObject LaunchCamera(int maxImageSize, string imageName)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            WebActivity.PickPhotoFromCamera(maxImageSize, imageName);
            retMessage.Add("message", "Successfully Launched to Pick Photo from Camera");
#elif __IOS__
            WebViewController.PickPhotoFromCamera(maxImageSize,  imageName);
            retMessage.Add("message", "Successfully Launched to Pick Photo from Camera");
#else
            _cameraImage = _cameraImageDefault;
            retMessage.Add("message", "Successfully Launched Intent to Pick Photo from Camera");
            retMessage.Add("reload", true);
#endif
            return retMessage;
        }

        [JsonRpcMethod("PickPhotoFromCamera", Idempotent = true)]
        [JsonRpcHelp("Pick Photo from Camera. Returns JsonObject(message, itempath, orientation)")]
        public JsonObject PickPhotoFromCamera(bool launchCameraIfNotAvailable, int maxImageSize, string imageName)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            string itemPath = WebActivity.GetPathFromURI(WebActivity._cameraURI);
            string error = WebActivity._imageError;
            if (itemPath.Trim().Length == 0)
            {
                if (launchCameraIfNotAvailable == true)
                {
                    LaunchCamera(maxImageSize, imageName);
                    retMessage.Add("message", "Camera Launched for Capturing Image");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Camera Image Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }                    
                }
            }
            if (itemPath.Length > 0)
            {
                if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                retMessage.Add("orientation", WebActivity.GetOrientation(WebActivity._cameraURI));
                retMessage.Add("message", "Successfully Retrieved Camera Photo");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickPhotoFromCamera", "ItemPath[" + itemPath + "]");
            }
#elif __IOS__
            string itemPath = "";
            string error = WebViewController._imageError;
            if (WebViewController._cameraURI != null)
            {
                itemPath = WebViewController._cameraURI.AbsoluteString;
            }
            if (itemPath.Trim().Length == 0)
            {
                if (launchCameraIfNotAvailable == true)
                {
                    LaunchCamera(maxImageSize, imageName);
                    retMessage.Add("message", "Camera Launched for Capturing Image");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Camera Image Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }  
                }
            }
            if (itemPath.Length > 0)
            {
                if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                //retMessage.Add("orientation", GridActivity.GetOrientation(GridActivity._cameraURI));
                retMessage.Add("message", "Successfully Retrieved Camera Photo");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickPhotoFromCamera", "ItemPath[" + itemPath + "]");
            }
#else
            if ((string.IsNullOrEmpty(_cameraImage) ==false) && (_cameraImage.Trim().StartsWith("/") == false)) _cameraImage = "/" + _cameraImage;
            retMessage.Add("itempath", _cameraImage);
            retMessage.Add("message", "Successfully Retrieved Camera Photo");
            _cameraImage = "";
#endif
            return retMessage;
        }

        [JsonRpcMethod("ClearCamera", Idempotent = true)]
        [JsonRpcHelp("Clear Camera Image File. Returns JsonObject(message)")]
        public JsonObject ClearCamera()
        {
            var retMessage = new JsonObject();

#if __ANDROID__
            WebActivity._cameraURI = null;
#elif __IOS__
            WebViewController._cameraURI = null;
#else
            _cameraImage = "";
#endif
            retMessage.Add("message", "Successfully Cleared the Camera Image");
            retMessage.Add("reload", true);

            return retMessage;
        }

#if !__ANDROID__ && !__IOS__
        private static string _galleryImageDefault = "/App_Resource/Media/gallery.png";
        private static string _galleryImage = "";
#endif

        [JsonRpcMethod("LaunchPhotoPicker", Idempotent = true)]
        [JsonRpcHelp("Launch Photo Picker. Returns JsonObject(message) and Launches Photo Gallery Picker Intent")]
        public JsonObject LaunchPhotoPicker(int maxImageSize, string imageName)
        {
            var retMessage = new JsonObject();

#if __ANDROID__
            WebActivity.PickPhotoFromGallery(maxImageSize, imageName);
            retMessage.Add("message", "Successfully Launched to Pick Photo from Gallery");
#elif __IOS__
            WebViewController.PickPhotoFromGallery(maxImageSize, imageName);
            retMessage.Add("message", "Successfully Launched to Pick Photo from Gallery");
#else
            _galleryImage = _galleryImageDefault;
            retMessage.Add("message", "Successfully Launched Intent to Pick Photo from Gallery");
            retMessage.Add("reload", true);
#endif
            return retMessage;
        }

        [JsonRpcMethod("PickPhotoFromGallery", Idempotent = true)]
        [JsonRpcHelp("Pick Photo from Gallery. Returns JsonObject(message, error, itempath, orientation) or Launches Intent if requested and if Item Path is Empty")]
        public JsonObject PickPhotoFromGallery(bool launchPhotoPickerIfNotAvailable, int maxImageSize, string imageName)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            string error = WebActivity._imageError;
            string itemPath = WebActivity.GetPathFromURI(WebActivity._photoURI);
            if ((itemPath != null) && (itemPath.Trim().Length > 0))
            {
                if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                retMessage.Add("orientation", WebActivity.GetOrientation(WebActivity._photoURI));
                retMessage.Add("message", "Successfully Retrieved Gallery Photo");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickPhotoFromGallery", "ItemPath[" + itemPath + "]");
            }
            else
            {
                if (launchPhotoPickerIfNotAvailable == true)
                {
                    LaunchPhotoPicker(maxImageSize, imageName);
                    retMessage.Add("message", "Photo Picker Intent Launched for Picking Image");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Photo Image Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }                      
                }
            }
#elif __IOS__
            string itemPath = "";
            string error = WebViewController._imageError;
            if (WebViewController._photoURI != null)
            {
                itemPath = WebViewController._photoURI.AbsoluteString;
            }
            if (itemPath.Trim().Length > 0)
            {
                if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                //retMessage.Add("orientation", GridActivity.GetOrientation(GridActivity._photoURI));
                retMessage.Add("message", "Successfully Retrieved Gallery Photo");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickPhotoFromGallery", "ItemPath[" + itemPath + "]");
            }
            else
            {
                if (launchPhotoPickerIfNotAvailable == true)
                {
                    LaunchPhotoPicker(maxImageSize, imageName);
                    retMessage.Add("message", "Photo Picker Launched for Picking Image");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Photo Image Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }  
                }
            }
#else
            if ((string.IsNullOrEmpty(_galleryImage) == false) && (_galleryImage.Trim().StartsWith("/") == false)) _galleryImage = "/" + _galleryImage;
            retMessage.Add("itempath", _galleryImage);
            retMessage.Add("message", "Successfully Retrieved Camera Photo");
            _galleryImage = "";
#endif
            return retMessage;
        }

        [JsonRpcMethod("ClearPhotoPicker", Idempotent = true)]
        [JsonRpcHelp("Clear Photo Picker Image File. Returns JsonObject(message)")]
        public JsonObject ClearPhotoPicker()
        {
            var retMessage = new JsonObject();

#if __ANDROID__
            WebActivity._photoURI = null;;
#elif __IOS__
            WebViewController._photoURI = null;
#else
            _galleryImage = "";
#endif
            retMessage.Add("message", "Successfully Cleared the Photo Picker");
            retMessage.Add("reload", true);

            return retMessage;
        }

        #endregion

        #region Media - Video

#if !__ANDROID__ && !__IOS__
        private static string _galleryVideoDefault = "/App_Resource/Media/video.mp4";
        private static string _galleryVideo = "";
#endif

        [JsonRpcMethod("LaunchVideoPicker", Idempotent = true)]
        [JsonRpcHelp("Launch Video Picker. Returns JsonObject(message) and Launches Video Gallery Picker Intent")]
        public JsonObject LaunchVideoPicker(string videoName)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            WebActivity.PickVideoFromGallery(videoName);
            retMessage.Add("message", "Successfully Launched to Pick Video from Gallery");
#elif __IOS__
            WebViewController.PickVideoFromGallery(videoName);
            retMessage.Add("message", "Successfully Launched to Pick Video from Gallery");
#else
            _galleryVideo = _galleryVideoDefault;
            retMessage.Add("message", "Successfully Launched Intent to Pick Video from Gallery");
            retMessage.Add("reload", true);
#endif
            return retMessage;
        }

        [JsonRpcMethod("PickVideoFromGallery", Idempotent = true)]
        [JsonRpcHelp("Pick Video from Gallery. Returns JsonObject(message, itempath) or Launches Intent if requested and if Item Path is Empty")]
        public JsonObject PickVideoFromGallery(bool launchVideoPickerIfNotAvailable, string videoName)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            string error = WebActivity._videoError;
            string itemPath = WebActivity.GetPathFromURI(WebActivity._videoURI);
            if ((itemPath != null) && (itemPath.Trim().Length > 0))
            {
                if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                retMessage.Add("message", "Successfully Retrieved Gallery Video");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickVideoFromGallery", "ItemPath[" + itemPath + "]");
            }
            else
            {
                if (launchVideoPickerIfNotAvailable == true)
                {
                    LaunchVideoPicker(videoName);
                    retMessage.Add("message", "Video Picker Intent Launched for Picking Video");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Video Url Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }
                }
            }
#elif __IOS__
            string itemPath = "";
            string error = WebViewController._videoError;
            if (WebViewController._videoURI != null)
            {
                itemPath = WebViewController._videoURI.AbsoluteString;
            }
            if (itemPath.Trim().Length == 0)
            {
                if (launchVideoPickerIfNotAvailable == true)
                {
                    LaunchVideoPicker(videoName);
                    retMessage.Add("message", "Successfully Launched for Picking Video");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Video Url Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }
                }
            }
            if (itemPath.Length > 0)
            {
                if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                retMessage.Add("message", "Successfully Retrieved Video");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickVideoFromCamera", "ItemPath[" + itemPath + "]");
            }
#else
            if ((string.IsNullOrEmpty(_galleryVideo) == false) && (_galleryVideo.Trim().StartsWith("/") == false)) _galleryVideo = "/" + _galleryVideo;
            retMessage.Add("itempath", _galleryVideo);
            retMessage.Add("message", "Successfully Retrieved Gallery Video");
            retMessage.Add("show", true);
            _galleryVideo = "";
#endif
            return retMessage;
        }

        [JsonRpcMethod("ClearVideoPicker", Idempotent = true)]
        [JsonRpcHelp("Clear Video Picker Video File. Returns JsonObject(message)")]
        public JsonObject ClearVideoPicker()
        {
            var retMessage = new JsonObject();

#if __ANDROID__
            WebActivity._videoURI = null;;
#elif __IOS__
            WebViewController._videoURI = null;
#else
            _galleryVideo = "";
#endif
            retMessage.Add("message", "Successfully Cleared the Video Picker");
            retMessage.Add("reload", true);

            return retMessage;
        }

        #endregion 

        #region Media - Audio

#if !__ANDROID__ && !__IOS__
        private static string _galleryAudioDefault = "/App_Resource/Media/audio.mp3";
        private static string _galleryAudio = "";
#endif

        [JsonRpcMethod("LaunchAudioPicker", Idempotent = true)]
        [JsonRpcHelp("Launch Audio Picker. Returns JsonObject(message) and Launches Audio Gallery Picker Intent")]
        public JsonObject LaunchAudioPicker(string audioName)
        {
            var retMessage = new JsonObject();

#if __ANDROID__
            WebActivity.PickAudioFromGallery(audioName);
            retMessage.Add("message", "Successfully Launched Intent to Pick Audio from Gallery");
#elif __IOS__
            WebViewController.PickAudioFromGallery(audioName);
            retMessage.Add("message", "Successfully Launched Intent to Pick Audio from Gallery");
#else
            _galleryAudio = _galleryAudioDefault;
            retMessage.Add("message", "Successfully Launched Intent to Pick Audio from Gallery");
            retMessage.Add("reload", true);
#endif
            return retMessage;
        }

        [JsonRpcMethod("PickAudioFromGallery", Idempotent = true)]
        [JsonRpcHelp("Pick Audio from Gallery. Returns JsonObject(message, itempath) or Launches Intent if requested and if Item Path is Empty")]
        public JsonObject PickAudioFromGallery(bool launchAudioPickerIfNotAvailable, string audioName)
        {
            var retMessage = new JsonObject();
#if __ANDROID__
            string error = WebActivity._audioError;
            string itemPath = WebActivity.GetPathFromURI(WebActivity._audioURI);
            if ((itemPath != null) && (itemPath.Trim().Length > 0))
            {
                 if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                retMessage.Add("message", "Successfully Retrieved Gallery Audio");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickAudioFromGallery", "ItemPath[" + itemPath + "]");
            }
            else
            {
                if (launchAudioPickerIfNotAvailable == true)
                {
                    LaunchAudioPicker(audioName);
                    retMessage.Add("message", "Audio Picker Intent Launched for Picking Audio");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Audio Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }
                    
                }
            }
#elif __IOS__
            string itemPath = "";
            string error = WebViewController._audioError;
            if (WebViewController._audioURI != null)
            {
                itemPath = WebViewController._audioURI.AbsoluteString;
            }
            if (itemPath.Trim().Length == 0)
            {
                if (launchAudioPickerIfNotAvailable == true)
                {
                    LaunchAudioPicker(audioName);
                    retMessage.Add("message", "Successfully Launched for Picking Audio");
                }
                else
                {
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        retMessage.Add("error", "Audio Url Not Found");
                    }
                    else
                    {
                        retMessage.Add("error", error);
                    }

                }
            }
            if (itemPath.Length > 0)
            {
                if (itemPath.Trim().StartsWith("/") == false) itemPath = "/" + itemPath;
                retMessage.Add("itempath", itemPath);
                retMessage.Add("message", "Successfully Retrieved Audio");
                LogManager.Log(LogType.Info, "JsonDeviceHandler_PickVideoFromCamera", "ItemPath[" + itemPath + "]");
            }
#else
            if ((string.IsNullOrEmpty(_galleryAudio) == false) && (_galleryAudio.Trim().StartsWith("/") == false)) _galleryAudio = "/" + _galleryAudio;
            retMessage.Add("itempath", _galleryAudio);
            retMessage.Add("message", "Successfully Retrieved Audio");
            retMessage.Add("show", true);
            _galleryAudio = "";
#endif
            return retMessage;
        }

        [JsonRpcMethod("ClearAudioPicker", Idempotent = true)]
        [JsonRpcHelp("Clear Video Picker Video File. Returns JsonObject(message)")]
        public JsonObject ClearAudioPicker()
        {
            var retMessage = new JsonObject();

#if __ANDROID__
           WebActivity._audioURI = null;;
#elif __IOS__
            WebViewController._audioURI = null;
#else
            _galleryAudio = "";
#endif
            retMessage.Add("message", "Successfully Cleared the Audio Picker");
            retMessage.Add("reload", true);

            return retMessage;
        }

        #endregion    
    }
}