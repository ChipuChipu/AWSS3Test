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

		// Initialize S3's FileList
		S3AssetStructure.LoadFiles();
		// Initialize Local FileList
		LocalAssetStructure.LoadInitialFiles();
	}

	void Update()
	{
		if (Input.GetKey ("up")) {
			LocalAssetStructure.CompareDictionaries (S3AssetStructure.GetS3FileList ());
		}
			
		else if (Input.GetKey("down"))
			// Download Necessary Files from S3
			S3AssetStructure.S3GetObjects(LocalAssetStructure.GetModifiedFileList());
	}
}
