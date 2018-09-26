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

public static class S3AssetLoader 
{

	// Should add some checks to see if these locations actually exist
	public static string CachePath = Application.persistentDataPath + "/S3LocalCache/";		// Files currently being downloaded are stored in a temporary file called the Cache

    public static string[] GetAllFilePaths(string path)
	{
		Directory.CreateDirectory (path);
		return Directory.GetFiles (path, "*.*");
	}

	public static void OnAsyncDownloadedFile(FileEntry file) {
		OnAsyncDownloadedFile (file.FileName);
	}

	public static void OnAsyncDownloadedFile(string fileName)
	{
		if (File.Exists(CachePath + fileName))
		{
			File.Copy(CachePath, LocalAssetLoader.DirectoryPath, true);
			File.Delete(CachePath + fileName);
		}
	}

	public static void LoadAWSAssetPathsIntoAWSPathStructure() {

		AWSPathStructure.PathEntry s3AssetPaths = AWSPathStructure.AWSDirectory.GetDirectoryInFileSystem ("Assets");

		Dictionary<string, FileEntry> s3AssetFileEntries = new Dictionary<string, FileEntry> ();

		foreach (var assetPath in s3AssetPaths.nextPath) {
			s3AssetFileEntries.Add (assetPath.Value.fileData.FileName, assetPath.Value.fileData);
		}

		S3AssetStructure.SetS3AssetDictionary (s3AssetFileEntries);

	}

	// Downloads and Uploads all files marked respectively in the LocalModifiedList from LocalAssetStructure
	public static void S3UpdateLocalDirectory(List<FileEntry> LocalModifiedList)
	{

		Directory.CreateDirectory (CachePath);

		if (LocalModifiedList.Count == 0 || LocalModifiedList == null)
			return;

		foreach (FileEntry entry in LocalModifiedList) 
		{
			if (entry.State == FileEntry.Status.Download) 
			{
				Debug.Log ("Downloading: " + entry.FileName + " || File State: " + entry.State + " || File Path: " + entry.Path);
				AWSLoader.S3GetObjects (entry, CachePath);		
			}
		}

	}

	//=================================================================================== Uploading files =======================================

	// Uploads a single file from Local Directory onto the S3 Cloud
	/*
	public static void S3PostFile(string path, string fileName)
	{
		S3AssetLoader.PostFile (Client, S3BucketName, path, fileName);
	}
	#endregion

	#region Extra Methods (Not Part of Core)
	// Uploads every file in the specified location
	public static void S3PostAllFiles()
	{
		S3AssetLoader.S3PostAllFiles (Client, S3BucketName);
	}
	#endregion
	*/

	//Moving Uploading into AWSLoader
	#region S3 PostFiles
	/*
	public static void S3PostAllFiles(IAmazonS3 Client, string S3BucketName)
	{
		foreach (string path in GetAllFilePaths())
			PostFile (Client, S3BucketName, path, Path.GetFileName (path));
	}
	*/

	public static void PostFile(IAmazonS3 Client, string S3BucketName, string filePath, string fileName)
	{

		try
		{
			var stream = new FileStream (filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var request = new PostObjectRequest () 
			{
				Bucket = S3BucketName,
				Key = fileName,
				InputStream = stream,
				CannedACL = S3CannedACL.PublicReadWrite		// Reference: https://docs.aws.amazon.com/sdkfornet1/latest/apidocs/html/T_Amazon_S3_Model_S3CannedACL.htm
			};

			Client.PostObjectAsync (request, (requestObject) => 
				{
					if (requestObject.Exception != null)
						throw requestObject.Exception;
				});

		}

		catch (Exception e)
		{
			Debug.Log ("Exception in PostFile: " + e.Message);
		}
	}
	#endregion
}
