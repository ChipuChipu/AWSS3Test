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
	#region S3 ListObjects NOTE: NEED TO FIX ASYNC ISSUE
	public static Dictionary<string, FileEntry> S3ListObjects(IAmazonS3 Client, string S3BucketName)
	{
		Dictionary<string, FileEntry> FileList = new Dictionary<string, FileEntry> ();

		var request = new ListObjectsRequest () 
		{
			BucketName = S3BucketName
		};

		Client.ListObjectsAsync (request, (responseObject) => 
		{
			try 
			{
				responseObject.Response.S3Objects.ForEach((o) =>
				{
					FileEntry entry = new FileEntry(o.Key, GetURL(S3BucketName, o.Key), o.Size, FileEntry.Status.Unmodified, (DateTime)o.LastModified, (DateTime)o.LastModified);
					FileList.Add(entry.path, entry);
				});

				if (S3AssetStructure.OnAsyncRetrieved != null)
					S3AssetStructure.OnAsyncRetrieved(FileList);
			} 

			catch (AmazonS3Exception e) 
			{
				throw e;
			}
		});

		return FileList;
	}
	#endregion

	#region S3 GetObjects
	public static void S3GetObjects(IAmazonS3 Client, string S3BucketName, List<FileEntry> ModifiedFileList)
	{
		foreach (FileEntry temp in ModifiedFileList) 
			GetObject(Client, S3BucketName, temp.fileName, temp.path);
	}
	#endregion

	#region S3 PostFiles
	public static void S3PostFiles(IAmazonS3 Client, string S3BucketName)
	{
		foreach (string path in GetAllFilePaths())
			PostObject (Client, S3BucketName, path, Path.GetFileName (path));
	}
	#endregion

	#region Helper Functions
	public static string GetURL(string S3BucketName, string fileName)
	{
		return "https://s3.amazonaws.com//" + S3BucketName + "//" + fileName;
	}

	public static string[] GetAllFilePaths()
	{
		return Directory.GetFiles ("C:\\Users\\Joshu\\Desktop\\Dump\\", "*.*", SearchOption.AllDirectories);
	}

	public static void GetObject(IAmazonS3 Client, string S3BucketName, string fileName, string destinationPath)
	{
		Client.GetObjectAsync (S3BucketName, fileName, (responseObj) => 
		{
			var response = responseObj.Response;

			if (response.ResponseStream != null)
			{
				using (var fs = File.Create(destinationPath))
				{
					byte[] buffer = new byte[10000000];
					int count = 0;
					while ((count = response.ResponseStream.Read(buffer, 0, buffer.Length)) != 0)
						fs.Write(buffer, 0, count);
					fs.Flush();
				}
			}

			else
				Debug.Log("S3 File Download Failed: " + fileName);
		});
	}

	public static void PostObject(IAmazonS3 Client, string S3BucketName, string path, string fileName)
	{
		var stream = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.Read);
		var request = new PostObjectRequest () 
		{
			Bucket = S3BucketName,
			Key = fileName,
			InputStream = stream,
			CannedACL = S3CannedACL.PublicRead
		};

		Client.PostObjectAsync (request, (requestObject) => 
		{
			if (requestObject.Exception != null)
				throw requestObject.Exception;
		});
	}
	#endregion
}
