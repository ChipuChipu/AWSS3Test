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
	#region S3 LoadObjects
	public static void S3LoadObjects(IAmazonS3 Client, string S3BucketName)
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
                            Debug.Log("Filename: " + o.Key);
							FileEntry entry = new FileEntry(o.Key, GetURL(S3BucketName, o.Key), o.Size, FileEntry.Status.Unmodified, (DateTime)o.LastModified, (DateTime)o.LastModified);
							FileList.Add(entry.FileName, entry);
						});

					if (S3AssetStructure.OnAsyncRetrieved != null)
						S3AssetStructure.OnAsyncRetrieved(FileList);
				} 

				catch (AmazonS3Exception e) 
				{
					throw e;
				}
			});

		// There is a point of uncertainty here. I am not sure if the S3Filelist is populated correctly by this point.
		// I am depending on the delegate for S3FileList to work correctly. [See line 38]

        if (S3AssetStructure.OnAsyncDownloaded != null)
        {
            foreach (string path in GetAllFilePaths(S3AssetStructure.CachePath))
            {
                if (File.Exists(path))
                {
                    Debug.Log("path inside loadfiles: " + path);
                    File.Copy(path, S3AssetStructure.DirectoryPath + "//" + Path.GetFileName(path), true);
                    File.Delete(path);
                }
            }

            S3AssetStructure.OnAsyncDownloaded = null;
        }
	}
	#endregion

	#region S3 GetObjects
	public static void S3GetObjects(IAmazonS3 Client, string S3BucketName, List<FileEntry> ModifiedLocalFileList)
	{
		foreach (FileEntry entry in ModifiedLocalFileList) 
		{
			GetObject(Client, S3BucketName, entry.FileName, entry.Path);
		}

	}
	#endregion

	#region S3 PostFiles
	public static void S3PostAllFiles(IAmazonS3 Client, string S3BucketName)
	{
		foreach (string path in GetAllFilePaths())
			PostFile (Client, S3BucketName, path, Path.GetFileName (path));
	}
	#endregion

	#region Helper Functions
	public static string GetURL(string S3BucketName, string fileName)
	{
		return "https://s3.amazonaws.com//" + S3BucketName + "//" + fileName;
	}

    public static string[] GetAllFilePaths()
    {
        //Directory.CreateDirectory (Application.persistentDataPath + "Dump");
        //return Directory.GetFiles(Application.persistentDataPath + "Dump", "*.*", SearchOption.AllDirectories);

        Directory.CreateDirectory(S3AssetStructure.DirectoryPath);
        return Directory.GetFiles(S3AssetStructure.DirectoryPath, "*.*");
    }

    public static string[] GetAllFilePaths(string path)
	{
		//Directory.CreateDirectory (Application.persistentDataPath + "Dump");
		//return Directory.GetFiles(Application.persistentDataPath + "Dump", "*.*", SearchOption.AllDirectories);

		Directory.CreateDirectory (path);
		return Directory.GetFiles (path, "*.*");
	}

	public static void GetObject(IAmazonS3 Client, string S3BucketName, string destinationPath, string fileName)
	{
		try
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

						S3AssetStructure.OnAsyncDownloaded += new S3AssetStructure.OnAsyncDownloadedEvent (S3AssetStructure.OnAsyncDownloadedFile);
					}
				});					
		}

		catch (Exception e)
		{
			Debug.Log ("Exception in PostFile: " + e.Message);
		}
	}

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
