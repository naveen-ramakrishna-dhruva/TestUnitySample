/*
	@File: 			AutoBuild.cs
	@Author: 		Naveen Ramakrishna
	@Date: 			7-Aug-2014
	@Description:
					This Editor script allows you to automatically build for various platforms.
					It adds an "AutoBuild" menu. It supports the following platforms: iOS, Android and Desktop/Laptop

					--- What it does? ---
					* For iOS: It builds a Xcode project.
					* For Android  
						- Google Play: It builds either an Eclipse project or an APK file. 	
					* For Desktop / Laptop: It builds Windows, Mac or Linux executables.

					--- Where can it be used? ---
					* For building Unity projects using command line.
					* In Automated Build systems for building to various platforms.

	@Requirements: 	UNITY PRO + iOS PRO + ANDROID PRO
	@Copyright: 	©2014, Dhruva Interactive

*/

using UnityEngine;
using UnityEditor;

using System.IO ;
using System.Collections.Generic;
using System;

public class AutoBuild : MonoBehaviour	
{	
	// Checks before building	
	// * Company Name is NOT default one - DefaultCompany	
	// * Bundle identifier is not the default one - com.Company.ProductName	
	// * 	
	// * Check if "Builds" folder is available. If not, create one	
	// Build a folder containing iOS Xcode project
	
	static readonly string AB_LOG_FILE = "AutoBuildLog.txt" ;
	static string m_logInfo = "" ;
	
	public enum BuildPlatform
	{
		iOS,
		Android,
		PC_x86,
		PC_x64,		
		Mac_Intel,
		Mac_Intel_64,
		Mac_Universal,
	}
	
	public enum BuildPlatformType
	{
		ProjectFolder,
	}
	
	////////////////////////////////////////////////////////////////////////////////////
	/// iOS 
	////////////////////////////////////////////////////////////////////////////////////
	
	// Build a folder containing iOS Xcode project
	[MenuItem ("AutoBuild/iOS")]	
	static void Build_iOS()
	{	
		Build_Platform(BuildPlatform.iOS) ;
	}
	
	////////////////////////////////////////////////////////////////////////////////////
	/// Android
	////////////////////////////////////////////////////////////////////////////////////
	
	// Build a package containing APK file
	[MenuItem ("AutoBuild/Android/Google Play")]
	static void Build_Android_Google_Play()
	{
		Build_Platform(BuildPlatform.Android) ;
	}
	
	////////////////////////////////////////////////////////////////////////////////////
	/// PC - x86, x64, All
	////////////////////////////////////////////////////////////////////////////////////
	
	// Build a folder containing PC build - x86 -> 32-bit
	[MenuItem ("AutoBuild/PC/x86 (32-bit)")]
	static void Build_PC_x86()
	{
		Build_Platform(BuildPlatform.PC_x86) ;
	}
	
	// Build a folder containing PC build - x64 -> 64-bit
	[MenuItem ("AutoBuild/PC/x64 (64-bit)")]
	static void Build_PC_x64()
	{
		Build_Platform(BuildPlatform.PC_x64) ;
	}
	
	// Build a folder containing PC builds - All architectures
	[MenuItem ("AutoBuild/PC/All architectures")]
	static void Build_PC_All_Architectures()
	{
		Build_Platform(BuildPlatform.PC_x86) ;
		Build_Platform(BuildPlatform.PC_x64) ;
	}
	
	
	////////////////////////////////////////////////////////////////////////////////////
	/// Mac - Intel x86, Intel x64, Universal, All
	////////////////////////////////////////////////////////////////////////////////////
	
	// Build a folder containing Mac build - Intel -> 32-bit
	[MenuItem ("AutoBuild/Mac/Intel (32-bit)")]
	static void Build_Mac_Intel_x86()
	{
		Build_Platform(BuildPlatform.Mac_Intel) ;
	}
	
	
	// Build a folder containing Mac build - Intel -> 64-bit
	[MenuItem ("AutoBuild/Mac/Intel (64-bit)")]
	static void Build_Mac_Intel_x64()
	{
		Build_Platform(BuildPlatform.Mac_Intel_64) ;
	}
	
	// Build a folder containing Mac build - Universal
	[MenuItem ("AutoBuild/Mac/Universal")]
	static void Build_Mac_Universal()
	{
		Build_Platform(BuildPlatform.Mac_Universal) ;
	}
	
	// Build a folder containing Mac builds - All architectures
	[MenuItem ("AutoBuild/Mac/All architectures")]
	static void Build_Mac_All_Architectures()
	{
		Build_Platform(BuildPlatform.Mac_Intel) ;
		Build_Platform(BuildPlatform.Mac_Intel_64) ;
		Build_Platform(BuildPlatform.Mac_Universal) ;
	}
	
	////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////
	
	static void Build_Platform(BuildPlatform platform)
	{	
		m_logInfo = "" ;
		
		bool buildSuccessfull = false ;
		string successMessage = "" ;
		
		AppendLogInfo("AutoBuild: TASK: Starting to build for platform: " + platform) ;
		
		CheckBuildSettings() ;
		
		if(BuildPlatform.Android == platform)
		{
			//CheckAndroidSpecificSettings() ;
		}
		
		// Get current project directory
		string pwd = Directory.GetCurrentDirectory() ;
		AppendLogInfo("AutoBuild: INFO: Current Directory: " + pwd) ;
		
		// Check and/or Create "Builds" directory
		if(CheckAndCreateDirectory(pwd, "Builds"))
		{
			// Check and/or Create "iOS" directory
			string currBuildsDir = pwd + "/Builds" ;
			string getPlatformFolderToCreate = GetPlatformFolderToCreate(platform) ;
			
			if(CheckAndCreateDirectory(currBuildsDir, getPlatformFolderToCreate))
			{
				AppendLogInfo("AutoBuild: INFO: All " + platform + " build directories initialized") ;
				
				// Get all the scenes which have been selected
				List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes) ;
				
				if(0 == scenes.Count)
				{
					AppendLogInfo("AutoBuild: ERROR: No Scenes have been added for Build. Add atleast ONE scene in Build Settings.") ;
				}
				else if(scenes.Count > 0)
				{
					List<string> enabledScenes = new List<string>() ;
					
					foreach (EditorBuildSettingsScene scene in scenes)
					{
						//AppendLogInfo("Found Scene: " + scene.path + ", enabled = " + scene.enabled) ;
						
						if(scene.enabled)
						{
							enabledScenes.Add(scene.path);
						}
					}
					
					// Check if any scenes have been
					if(0 == enabledScenes.Count)
					{
						AppendLogInfo("AutoBuild: ERROR: No Scenes have been selected for Build. Select atleast ONE scene to build in Build Settings.") ;
					}
					else if(enabledScenes.Count > 0)
					{
						AppendLogInfo("AutoBuild: INFO: Total Scenes Enabled = " + enabledScenes.Count) ;
						
						string productName = PlayerSettings.productName ;
						productName = productName.Replace(" ", "") ;
						
						string buildFolder = "Builds/" + getPlatformFolderToCreate + "/" ;
						BuildTarget buildTarget = BuildTarget.iPhone ;
						BuildOptions buildOptions = BuildOptions.None ;
						
						switch(platform)
						{
							case BuildPlatform.iOS:
							{
								buildTarget = BuildTarget.iPhone ;
								successMessage = "AutoBuild: SUCCESS: Created iOS Xcode Project Folder." ;
							}
							break ;
								
							case BuildPlatform.Android:
							{
								productName = productName.ToLowerInvariant() ;								
								
								buildTarget = BuildTarget.Android ;
								buildFolder += productName + ".apk" ;
								successMessage = "AutoBuild: SUCCESS: Created Google Play - Android APK file." ;
							}
							break ;
								
							case BuildPlatform.PC_x86:
							{
								productName = productName.Replace(" ", "") ;
								
								buildTarget = BuildTarget.StandaloneWindows ;
								buildFolder += productName + ".exe" ;
								successMessage = "AutoBuild: SUCCESS: Created PC (x86 -> 32-bit) Build folder." ;
							}
							break ;
								
							case BuildPlatform.PC_x64:
							{
								productName = productName.Replace(" ", "") ;
								
								buildTarget = BuildTarget.StandaloneWindows64 ;
								buildFolder += productName  + ".exe" ;
								successMessage = "AutoBuild: SUCCESS: Created PC (x64 -> 64-bit) Build folder." ;
							}
							break ;
								
								
							case BuildPlatform.Mac_Intel:
							{
								productName = productName.Replace(" ", "") ;
								
								buildTarget = BuildTarget.StandaloneOSXIntel ;
								buildFolder += productName ;
								successMessage = "AutoBuild: SUCCESS: Created Mac (Intel -> 32-bit) Build folder." ;
							}
							break ;
								
							case BuildPlatform.Mac_Intel_64:
							{
								productName = productName.Replace(" ", "") ;
								
								buildTarget = BuildTarget.StandaloneOSXIntel64 ;
								buildFolder += productName ;
								successMessage = "AutoBuild: SUCCESS: Created Mac (Intel -> 64-bit) Build folder." ;
							}
							break ;
								
							case BuildPlatform.Mac_Universal:
							{
								productName = productName.Replace(" ", "") ;
								
								buildTarget = BuildTarget.StandaloneOSXUniversal ;
								buildFolder += productName ;
								successMessage = "AutoBuild: SUCCESS: Created Mac (Universal) Build folder." ;
							}
							break ;
						}
						
						BuildPipeline.BuildPlayer( 		                          
							                          enabledScenes.ToArray(), 		                          
							                          buildFolder,   
							                          buildTarget,
							                          buildOptions
						                          ); 
						
						buildSuccessfull = true ;
					}
				}
			}
		}
		
		if(buildSuccessfull)
		{
			AppendLogInfo(successMessage) ;
		}
		else
		{
			AppendLogInfo("AutoBuild: FAILED: Check error log file: " + AB_LOG_FILE + " for more info.") ;
		}
		
		SaveAutoBuildLogFile(m_logInfo) ;
		m_logInfo = "" ;
	}
	
	////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////
	
	
	static void CheckBuildSettings()
	{
		// Check if Company name is correct
		if(!CheckCompanyNameIsProper())
		{
			AppendLogInfo("AutoBuild: ERROR: Company Name is not specified correctly. Changing it to default company name.") ;
			
			// Set by Default: Dhruva Interactive Pvt. Ltd.
			PlayerSettings.companyName = "Dhruva Interactive Pvt. Ltd." ;
		}
		
		// Check if Bundle Identifier is correct
		if(!CheckBundleIdentifierIsProper())
		{
			AppendLogInfo("AutoBuild: ERROR: Bundle identfier is not in a PROPER format. Changing it into a VALID format.") ;
			
			// Set to Default: The 1st part for bundle identifer is: com.dhruva
			// Add the product name to the 2nd part of the bundle identifier
			
			// Bundle Identifier Rule: All Lower Case + No SPACES
			string productName = PlayerSettings.productName.ToLowerInvariant() ;
			productName = productName.Replace(" ", "") ;
			
			PlayerSettings.bundleIdentifier = "com.dhruva." + productName ;
		}
		
		AppendLogInfo("AutoBuild: INFO: Company Name: " + PlayerSettings.companyName) ;
		AppendLogInfo("AutoBuild: INFO: Bundle Identifier: " + PlayerSettings.bundleIdentifier) ;
	}
	
	static void CheckAndroidSpecificSettings()
	{
		// Check Publishing Settings
		// -> Check if KeyStore is selected and password is provided.
		// -> Check if Key Alias is selected and password is provided.
		PlayerSettings.Android.keystoreName="KeyStore/googleplay.keystore" ;
		PlayerSettings.Android.keyaliasName="release" ;
		PlayerSettings.keyaliasPass = "release";
		PlayerSettings.keystorePass = "release";
	}
	
	static bool CheckAndCreateDirectory(string parentDir, string dirName)
	{
		bool createdOrFoundDir = false ;
		string dirPath = parentDir + "/" + dirName ;
		
		if(Directory.Exists(dirPath))
		{
			AppendLogInfo("AutoBuild: INFO: " + dirName + " Directory FOUND ...") ;
			createdOrFoundDir = true ;
		}
		else
		{
			AppendLogInfo("AutoBuild: INFO: Creating " + dirName + " Directory ...") ;
			Directory.CreateDirectory(dirPath) ;
			createdOrFoundDir = true ;
		}
		
		return createdOrFoundDir ;
	}
	
	static bool CheckCompanyNameIsProper()
	{
		bool nameIsProper = true ;
		
		if(
			(0 == PlayerSettings.companyName.CompareTo("")) ||
			(0 == PlayerSettings.companyName.CompareTo("DefaultCompany"))
		  )
		{
			nameIsProper = false ;
		}
		
		return nameIsProper ;
	}
	
	static bool CheckBundleIdentifierIsProper()
	{
		bool bundleIdIsProper = true ;
		
		if(
				(0 == PlayerSettings.bundleIdentifier.CompareTo("")) ||
				(0 == PlayerSettings.bundleIdentifier.CompareTo("com.Company.ProductName")) ||
				(PlayerSettings.bundleIdentifier.StartsWith("com.gametantra.")) 
		  )
		{
			bundleIdIsProper = false ;
		}
		
		return bundleIdIsProper ;
	}
	
	static void SaveAutoBuildLogFile(string content)
	{
		//string assetsPath = Application.dataPath ;
		
		// Get current project directory
		string pwd = Directory.GetCurrentDirectory() ;
		string fileName = pwd + "/" + AB_LOG_FILE ;		
		AppendLogInfo("Saving Auto Build Log . Log file path: " + fileName) ;
		
		// Always create
		FileStream fileStream = new FileStream(fileName, FileMode.Create) ;
		
		if(null != fileStream)
		{
			// Create the writer for data.
			//BinaryWriter binaryWriter = new BinaryWriter(fileStream) ;
			StreamWriter streamWriter = new StreamWriter(fileStream) ;
			
			if(null != streamWriter)
			{
				// Write Auto Build Log Info
				streamWriter.Write(content) ;
				
				// Closes the Writer and the Stream
				streamWriter.Close() ;
			}
		}
	}
	
	static void AppendLogInfo(string info)
	{ 
		string logInfo = System.Environment.NewLine ;
		logInfo += DateTime.Now.ToString("d-MMM-yyyy, dddd, h:mm:ss.fff tt", System.Globalization.DateTimeFormatInfo.InvariantInfo) ;
		logInfo += " \t" + info ;
		Debug.Log(logInfo) ;
		
		m_logInfo += logInfo ;
	}
	
	
	static string GetPlatformFolderToCreate(BuildPlatform platform)
	{
		string folderCreated = "" ;
		switch(platform)
		{
			case BuildPlatform.iOS:
			{
				folderCreated = "iOS" ;
			}
			break ;
				
			case BuildPlatform.Android:
			{
				folderCreated = "Android" ;
			}
			break ;
				
			case BuildPlatform.PC_x86:
			{
				folderCreated = "PC/x86" ;
			}
			break ;
				
			case BuildPlatform.PC_x64:
			{
				folderCreated = "PC/x64" ;
			}
			break ;
				
				
			case BuildPlatform.Mac_Intel:
			{
				folderCreated = "Mac/Intel/x86" ;
			}
			break ;
				
			case BuildPlatform.Mac_Intel_64:
			{
				folderCreated = "Mac/Intel/x64" ;
			}
			break ;
				
			case BuildPlatform.Mac_Universal:
			{
				folderCreated = "Mac/Universal" ;
			}
			break ;
		}
		
		return folderCreated ;
	}
}


