## Prerequisites

<ol>
    <li>
		Xamarin.IOS and Xamarin.Android License to build your own AppWeb Runtime(s)
	</li>    
	<li>
		Visual Studio, Mono Develop or Sharp Develop to build your own AppWebs
	</li>
	<li>
		One Click Mobile Packer from htmltoapp.com web site.
	</li>    
</ol>

## Installation

<ol>
    <li>
		Install your prefered IDE (Visual Studio, Mono Develop or Sharp Develop)  
	</li>    
	<li>
		Use Git to checkout the OneClickAppWeb Repository or download the AppWeb Component from Xamarin Component Store
    </li>    
	<li>
		Open the OneClickAppWeb.sln solution file or the respective Android/IOS/AppWeb project file using your IDE to compile and create the sample Native Mobile App or AppWebs.
    </li>
</ol>

## Using AppWeb Runtime to pack Html AppWeb

<ol>
	<li>
		Clone the existing AppWeb Runtime Repository or download from Xamaring Component store and open the solution file or respective project file.
	</li>  	  
	<li>
		Zip your AppWeb with a index.html file at the root.
	</li>
	<li>
		Add the Zip File to the Asset/Apps Directory of the respective IOS/Android Project
	</li>
	<li>
		Set the Zip File as a AndroidAsset for Android Projects and BundleResource for IOS Projects
	</li>
	<li>
		Modify the Project Properties like app name, signing identity, provisioning profile etc
	</li>
	<li>
		Compile and create a APK or IPA with the AppWeb embedded. 
	</li>
	<li>
		Test and deploy the APK or IPA to device
	</li>
</ol>

## Creating a custom AppWeb Runtime 


