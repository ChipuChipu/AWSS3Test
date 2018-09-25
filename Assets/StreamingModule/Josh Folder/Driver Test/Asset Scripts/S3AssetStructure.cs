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
	}

	// Note: Both cachePath and destinationPath have the requirement of the Path also including the filename of the downloaded file
	public delegate void OnAsyncDownloadedEvent(string fileName);
	public static OnAsyncDownloadedEvent OnAsyncDownloaded;

	void Awake()
	{
		_S3FileList = new Dictionary<string, FileEntry> ();
	}

	void Start()
	{
		OnAsyncRetrieved += new OnAsyncRetrievedEvent (OnAsyncRetrievedTest);
	}

	#endregion

	#region Core Methods
	// Populates S3AssetStructure's FileList will all existing objects on the S3 Bucket
	public static void SetS3AssetDictionary (Dictionary<string, FileEntry> dictionary)
	{
		S3FileList = dictionary;
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

}
