using System.Threading;
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
	void Start()
	{
		UnityInitializer.AttachToGameObject (this.gameObject);
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

		ThreadedLocalS3.Start ();
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