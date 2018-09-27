using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileSyncManager : Singleton<FileSyncManager> {

	[RuntimeInitializeOnLoadMethodAttribute (RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeStructure()
	{
		InitializeSingleton ();
	}

	void Awake()
	{
		AWSPathStructure.OnRetrievedDirectory += new AWSPathStructure.RetrievedDirectoryEvent (startCheck);
		AWSLoader.OnDownloadFinished += new AWSLoader.FinishedDownloadEvent(S3AssetLoader.OnAsyncDownloadedFile);
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
