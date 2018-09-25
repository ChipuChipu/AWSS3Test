using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using AssetStruct;

public class S3AssetStructure : Singleton<S3AssetStructure>
{
	#region Initialization
	[RuntimeInitializeOnLoadMethodAttribute (RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeStructure()
	{
		InitializeSingleton ();
	}

	[ReadOnly]
	public int S3FileListCount = 0;	

	// Should add some checks to see if these locations actually exist
	public static string DirectoryPath = "C:\\Users\\Joshu\\Desktop\\S3LocalTest";	// Local Directory used for checks. (Ignores all files not in the directory)
	public static string CachePath = "C:\\Users\\Joshu\\Desktop\\S3LocalCache";		// Files currently being downloaded are stored in a temporary file called the Cache

	private Dictionary<string, FileEntry> _S3FileList;
	static Dictionary<string, FileEntry> S3FileList
	{
		get { return Instance._S3FileList; }
		set
		{
			Instance.S3FileListCount = value.Count;
			Instance._S3FileList = value;	
		}
	}

	public delegate void OnAsyncRetrievedEvent(Dictionary<string, FileEntry> fileEntryDictionary);
	public static OnAsyncRetrievedEvent OnAsyncRetrieved;

	public static void OnAsyncRetrievedTest(Dictionary<string, FileEntry> fileEntryDictionary)
	{
		S3FileList = fileEntryDictionary;
	}

	// Note: Both cachePath and destinationPath have the requirement of the Path also including the filename of the downloaded file
	public delegate void OnAsyncDownloadedEvent(string fileName);
	public static OnAsyncDownloadedEvent OnAsyncDownloaded;

	// In Theory <Needs Testing>: On a completed S3 Download of a file, the file is moved from one folder to another
	/*
        How do I preserve the unique/specific filename on the delegate call from S3GetObject

     */

	public static void OnAsyncDownloadedFile(string fileName)
	{
		if (File.Exists(CachePath + fileName))
		{
			File.Copy(CachePath, DirectoryPath, true);
			File.Delete(CachePath + fileName);	
		}
	}

	void Awake()
	{
		//UnityInitializer.AttachToGameObject (this.gameObject);
		//AWSConfigs.Http = AWSConfigs.HttpClientOption.UnityWebRequest;

		_S3FileList = new Dictionary<string, FileEntry> ();
	}

	void Start()
	{
		OnAsyncRetrieved += new OnAsyncRetrievedEvent (OnAsyncRetrievedTest);
	}

	#endregion

	#region S3 Initialization
	public static string S3BucketName = "chipuchiputest";
	public static string IdentityPoolId = "us-east-1:e7825514-f4b7-42db-bb26-fd30c66b2245";
	public static string CognitoIdentityRegion = "us-east-1";
	private static RegionEndpoint _CognitoIdentityRegion
	{
		get { return RegionEndpoint.GetBySystemName (CognitoIdentityRegion); }
	}
	public static string S3Region = "us-east-1";
	private static RegionEndpoint _S3Region
	{
		get { return RegionEndpoint.GetBySystemName (S3Region); }
	}

	private static AWSCredentials _credentials;
	private static AWSCredentials Credentials
	{
		get 
		{
			if (_credentials == null)
				_credentials = new CognitoAWSCredentials (IdentityPoolId, _CognitoIdentityRegion);
			return _credentials;
		}
	}

	private static IAmazonS3 _s3Client;
	private static IAmazonS3 Client
	{
		get
		{
			if (_s3Client == null)
				_s3Client = new AmazonS3Client (Credentials, _S3Region);
			return _s3Client;
		}
	}
	#endregion

	#region Core Methods
	// Populates S3AssetStructure's FileList will all existing objects on the S3 Bucket
	public static void LoadObjects()
	{
		//S3AssetLoader.S3LoadObjects (Client, S3BucketName);
	}

	// Downloads and Uploads all files marked respectively in the LocalModifiedList from LocalAssetStructure
	public static void S3UpdateLocalDirectory(List<FileEntry> LocalModifiedList)
	{

		if (LocalModifiedList.Count == 0 || LocalModifiedList == null)
			return;

		foreach (FileEntry entry in LocalModifiedList) 
		{
			if (entry.State == FileEntry.Status.Download) 
			{
				Debug.Log ("Downloading: " + entry.FileName + " || File State: " + entry.State + " || File Path: " + entry.Path);
				S3GetObject (entry.Path, entry.FileName);		
			}

			/*
			else if (entry.State == FileEntry.Status.Upload)
			{
				Debug.Log ("Uploading: " + entry.FileName + " || File State: " + entry.State);
				S3PostFile (entry.Path, entry.FileName);
			}
		*/
		}
	}

	// Uploads a single file from Local Directory onto the S3 Cloud
	public static void S3PostFile(string path, string fileName)
	{
		S3AssetLoader.PostFile (Client, S3BucketName, path, fileName);
	}
	#endregion

	#region Helper Methods
	public static void UnloadS3FileList()
	{
		S3FileList.Clear ();
	}

	// Returns a Dictionary S3FileList
	public static Dictionary<string, FileEntry> GetS3FileList()
	{
		return S3FileList;
	}
	#endregion

	#region Extra Methods (Not Part of Core)
	// Uploads every file in the specified location
	public static void S3PostAllFiles()
	{
		S3AssetLoader.S3PostAllFiles (Client, S3BucketName);
	}
	#endregion
}
