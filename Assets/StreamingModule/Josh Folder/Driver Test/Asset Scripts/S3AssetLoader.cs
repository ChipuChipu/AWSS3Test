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

	#region S3 PostFiles
	public static void S3PostAllFiles(IAmazonS3 Client, string S3BucketName)
	{
		foreach (string path in GetAllFilePaths())
			PostFile (Client, S3BucketName, path, Path.GetFileName (path));
	}
	#endregion

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
