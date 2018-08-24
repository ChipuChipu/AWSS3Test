using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ThreadedLocalS3
{

	public static S3AssetStructure.DictionaryAssignedEvent CallUpdateEvent;

	public static void Start()
	{
		Thread checkThread = new Thread (CheckList);
		checkThread.Start ();
		checkThread.IsBackground = true;					// Upon the Main Thread being terminated, this thread will also be terminated.
	}

	static void CheckList()
	{
		for (;;)
		{
			// Update S3 FileList to most Recent
			S3AssetStructure.LoadObjects();
			// Update Local FileList to most Recent
			LocalAssetStructure.InitializeFiles();
			// Generate a manifest of all Modified Files in Local
			LocalAssetStructure.LoadFiles();
			// Perform Updates to LocalFileList
			LocalAssetStructure.UpdateFileList();

			CallUpdateEvent = new DictionaryAssignedEvent (UpdateDictionary);
			S3AssetStructure.OnDicitionaryAssigned += S3AssetStructure.CallUpdateEvent;
			// Make the current thread sleep for 5000 milliseconds
			Thread.Sleep (5000);
		}
	}

	static void UpdateDictionary()
	{
		// Compare Dictionaries with Local and S3 File Lists
		LocalAssetStructure.CompareDictionaries(S3AssetStructure.GetS3FileList());
		// Perform Updates to LocalFileList by Downloading, Uploading, or Deleting
		S3AssetStructure.S3UpdateLocalDirectory(LocalAssetStructure.GetModifiedFileList());

		S3AssetStructure.OnDictionaryAssgined -= S3AssetStructure.CallUpdateEvent;
	}
}

