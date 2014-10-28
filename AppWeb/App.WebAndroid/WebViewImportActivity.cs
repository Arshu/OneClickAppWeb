using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace App.Web
{   
    [Activity(Label = "AppWeb",
            WindowSoftInputMode = SoftInput.AdjustPan,
            ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
            LaunchMode = Android.Content.PM.LaunchMode.SingleTask        
    )]
    // Double-escape the backslashes in C# so they end up
    // single-escaped in the generated manifest file
    // `obj/Debug/android/AndroidManifest.xml`
    //For file browsers and google drive
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataHost = "*", DataScheme = "file",
    //    DataMimeType = "*/*"
    //    DataPathPattern = ".*\\\\.zip"
    //    )
    //]
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataHost = "*", DataScheme = "file",
    //    DataMimeType = "*/*"
    //    DataPathPattern = ".*\\\\.pap"
    //    )
    //]
    // Example Intent from tapping attachment in Email app
    // START u0 {act=android.intent.action.VIEW dat=content://com.android.email.attachmentprovider/1/1/RAW typ=application/zip flg=0x80001}
    //
    //For email
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataMimeType = "application/zip",
    //    DataHost = "*", DataScheme = "content",
    //    DataPathPattern = ".*\\\\.zip"
    //    )
    //]
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataMimeType = "application/pap",
    //    DataHost = "*", DataScheme = "content",
    //    DataPathPattern = ".*\\\\.pap"
    //    )
    //]
    // Example intent from tapping file in Downloads app
    // START u0 {act=android.intent.action.VIEW dat=content://downloads/all_downloads/1 typ=application/octet-stream flg=0x3}
    //
    // Example Intent from tapping attachment in Gmail app
    // START {act=android.intent.action.VIEW dat=content://gmail-ls/user@gmail.com/messages/4/attachments/0.1/BEST/false typ=application/octet-stream flg=0x80001 u=0}
    //
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataMimeType = "application/octet-stream",
    //    DataHost = "*", DataScheme = "content",
    //    DataPathPattern = ".*\\\\.zip"
    //    )
    //]
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataMimeType = "application/octet-stream",
    //    DataHost = "*", DataScheme = "content",
    //    DataPathPattern = ".*\\\\.pap"
    //    )
    //]
    //
    // Example Intent for link in web browser:
    // START u0 {act=android.intent.action.VIEW dat=http://www.example.com/test.zio typ=application/octet-stream}
    //
    // Example Intent for link in e-mail:
    // START u0 {act=android.intent.action.VIEW dat=http://www.example.com/test.zip flg=0x90000}
    //
    //For http
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataHost = "*", DataScheme = "http",
    //    DataPathPattern = ".*\\\\.zip"
    //    )
    //]
    //For https
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataHost = "*", DataScheme = "https",
    //    DataPathPattern = ".*\\\\.zip"
    //    )
    //]
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataHost = "*", DataScheme = "http",
    //    DataPathPattern = ".*\\\\.pap"
    //    )
    //]
    //For https
    //[IntentFilter(new[] { Intent.ActionView },
    //    Categories = new string[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    //    DataHost = "*", DataScheme = "https",
    //    DataPathPattern = ".*\\\\.pap"
    //    )
    //]
    public class WebViewImportActivity : Activity
    {        
        #region Override OnCreate

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            WebViewActivity._appUri = this.Intent.Data;

            Intent webActivityIntent = new Intent(this, typeof(WebViewActivity));
            webActivityIntent.AddFlags(ActivityFlags.ClearTop);
            this.StartActivity(webActivityIntent);
        }

        #endregion       
    }
}