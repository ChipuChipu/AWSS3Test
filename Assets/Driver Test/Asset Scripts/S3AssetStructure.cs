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
		Debug.Log ("Count: " + S3FileList.Count);
	}

	void Awake()
	{
		UnityInitializer.AttachToGameObject (this.gameObject);
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

		_S3FileList = new Dictionary<string, FileEntry> ();
	}

	void Start()
	{
		OnAsyncRetrieved += new OnAsyncRetrievedEvent (OnAsyncRetrievedTest);
	}

	#endregion

	#region S3 Initialization
	public static string S3BucketName = "chipuchiputest";
	public static string IdentityPoolId = "us-east-1:e7825514-f4b7-42db-bb26-fd30c66b224";
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

	#region Getters and Setters
	public static Dictionary<string, FileEntry> GetS3FileList()
	{
		return S3FileList;
	}
	#endregion


	public static void LoadFiles()
	{
		S3FileList = S3AssetLoader.S3ListObjects (Client, S3BucketName);
	}

	public static void S3GetObjects(List<FileEntry> LocalModifiedList)
	{
		S3AssetLoader.S3GetObjects (Client, S3BucketName, LocalModifiedList);
	}

	public static void S3PostObjects()
	{
		S3AssetLoader.S3PostFiles (Client, S3BucketName);
	}
}
