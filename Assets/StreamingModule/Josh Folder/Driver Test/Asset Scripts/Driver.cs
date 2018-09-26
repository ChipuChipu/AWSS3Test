﻿using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using AssetStruct;

/*
 	Note: 
 */


public class Driver : MonoBehaviour 
{
	public bool downloadFile;
	public string path;

	void Start()
	{
		//UnityInitializer.AttachToGameObject (this.gameObject);
		//AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

		//ThreadedLocalS3.Start ();
		//StartCoroutine(startCheck());

		AWSPathStructure.OnRetrievedDirectory += new AWSPathStructure.RetrievedDirectoryEvent (startCheck);
		AWSLoader.OnDownloadFinished += new AWSLoader.FinishedDownloadEvent(S3AssetLoader.OnAsyncDownloadedFile);

	}

	void Update() {
		if (downloadFile) {
			//Downlaod it and put the file on sync on the cache path...
			AWSLoader.S3GetObjects (S3AssetStructure.GetS3FileList()[path], S3AssetLoader.CachePath);
			downloadFile = false;
		}
	}

	void startCheck(AWSPathStructure.FileSystem files)
	{
		// Update Local FileList to most Recent
		LocalAssetLoader.InitializeFiles();
		// Update S3 FileList to most Recent

		// This will be synched with the update of its database 
		S3AssetLoader.LoadAWSAssetPathsIntoAWSPathStructure();

		// Generate a manifest of all Modified Files in Local
		LocalAssetLoader.UpdateModifiedFilesToLocalAssetStructure();

		// Perform Updates to LocalFileList
		LocalAssetLoader.UpdateFileList();
		// Compare Dictionaries with Local and S3 File Lists
		LocalAssetStructure.GetDictionaryDifferenceList (S3AssetStructure.GetS3FileList());
		// Perform Updates to LocalFileList by Downloading, Uploading, or Deleting
		S3AssetLoader.S3UpdateLocalDirectory(LocalAssetStructure.GetModifiedFileList());
	}
}



/*
	void Start()
	{
		UnityInitializer.AttachToGameObject (this.gameObject);
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

		TimerCallback tmCallback = CheckEffectExpiry; 
		Timer timer = new Timer (tmCallback, "test", 5000, 5000);
	}

	static void CheckEffectExpiry(object objectInfo)
	{
		// Update Local FileList to most Recent
		LocalAssetStructure.InitializeFiles();
		// Update S3 FileList to most Recent
		S3AssetStructure.LoadObjects();
		// Generate a manifest of all Modified Files in Local
		LocalAssetStructure.LoadFiles();
		// Perform Updates to LocalFileList
		LocalAssetStructure.UpdateFileList();
		// Compare Dictionaries with Local and S3 File Lists
		LocalAssetStructure.CompareDictionaries(S3AssetStructure.GetS3FileList());
		// Perform Updates to LocalFileList by Downloading, Uploading, or Deleting
		S3AssetStructure.S3UpdateLocalDirectory(LocalAssetStructure.GetModifiedFileList());
	}
 */