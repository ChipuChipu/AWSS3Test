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

public class StreamExtractor : MonoBehaviour {

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

	}

	public static string GetURL(string S3BucketName, string fileName)
	{
		return "https://s3.amazonaws.com//" + S3BucketName + "//" + fileName;
	}

}
