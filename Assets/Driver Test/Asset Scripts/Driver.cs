using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using AssetStruct;

public class Driver : MonoBehaviour 
{
	void Start()
	{
		UnityInitializer.AttachToGameObject (this.gameObject);
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
		// Update Local FileList to most Recent
		LocalAssetStructure.InitializeFiles();
		// Update S3 FileList to most Recent
		S3AssetStructure.LoadObjects();
		// Start Coroutine for 5 Second Delayed Sync Updates
		StartCoroutine (S3LocalTest());
	}

	void Update()
	{
		
	}



	IEnumerator S3LocalTest()
	{
        yield return test();
	}

    IEnumerator test()
	{
		foreach (KeyValuePair<string, FileEntry> entry in LocalAssetStructure.GetLocalFileList()) {
			Debug.Log ("entry.Key: " + entry.Key + " || File Name: " + entry.Value.FileName + " || File Status: " + entry.Value.State + " || " + entry.Value.FileSize);
		}

		// Generate a manifest of all Modified Files in Local
		LocalAssetStructure.LoadFiles();
		// Perform Updates to LocalFileList
		LocalAssetStructure.UpdateFileList();

        yield return new WaitForSeconds(5f);

		// Compare Dictionaries with Local and S3 File Lists
		LocalAssetStructure.CompareDictionaries(S3AssetStructure.GetS3FileList());
		// Perform Updates to LocalFileList by Downloading, Uploading, or Deleting
		S3AssetStructure.S3UpdateLocalDirectory(LocalAssetStructure.GetModifiedFileList());
		// Update S3 FileList to most Recent
		S3AssetStructure.LoadObjects();

        yield return new WaitForSeconds(10f);
	}
}
