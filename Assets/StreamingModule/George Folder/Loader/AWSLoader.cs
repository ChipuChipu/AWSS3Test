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

public class AWSLoader {
	
	//Bucket ID and S3 Information

	/*
	 * public static string _S3BucketName = "armediacontentff";
	public static string _IdentityPoolId = "us-east-2:f76cc60a-2a14-4510-a808-b09d833de5f5";
	public static string _CognitoIdentityRegion = "us-east-2";
	public static string _S3Region = "us-east-2";
	*/

	public static string _S3BucketName = "armediacontent";
	public static string _IdentityPoolId = "us-east-2:78d08b49-d0ce-48e1-a7a6-b84b7a64a40e";
	public static string _CognitoIdentityRegion = "us-east-2";
	public static string _S3Region = "us-east-2";

	//Cached Object Response
	public static ListObjectsResponse Response;
	public static DateTime ResponseTime;

	//Cached credentials
	private static AWSCredentials _credentials;

	//Cached client
	private static IAmazonS3 _s3Client;

	//Creation of event delegates to call when finished async.
	public delegate void ResponseReceivedEvent(ListObjectsResponse responseObject, string bucketName, string s3Region);
	public static ResponseReceivedEvent OnResponseReceived;

	//Creation of event delegate when finished downloading
	public delegate void FinishedDownloadEvent(FileEntry file);
	public static FinishedDownloadEvent OnDownloadFinished;

	//=========================================================== Static Get/Set Functions =========================================================================

	private static RegionEndpoint CognitoIdentityRegion
	{
		get { return RegionEndpoint.GetBySystemName (_CognitoIdentityRegion); }
	}

	private static RegionEndpoint S3Region
	{
		get { return RegionEndpoint.GetBySystemName (_S3Region); }
	}

	private static AWSCredentials Credentials
	{
		get 
		{
			if (_credentials == null)
				_credentials = new CognitoAWSCredentials (_IdentityPoolId, CognitoIdentityRegion);
			return _credentials;
		}
	}

	private static IAmazonS3 s3Client
	{
		get
		{
			if (_s3Client == null)
				_s3Client = new AmazonS3Client (Credentials, S3Region);
			return _s3Client;
		}
	}

	public static void RequestS3BucketObjects () {
		RequestS3BucketObjects (s3Client, _S3BucketName);
	}

	//============================================================================== AWS Object Loader Functions =============================================================

	public static void RequestS3BucketObjects (IAmazonS3 Client, string S3BucketName) {
		var request = new ListObjectsRequest () 
		{
			BucketName = S3BucketName
		};

		Client.ListObjectsAsync (request, (responseObject) => 
			{
				ReceivedS3BucketObjectsResponse(responseObject);
			});

	}

	static void ReceivedS3BucketObjectsResponse(AmazonServiceResult<ListObjectsRequest, ListObjectsResponse> responseObject) {
		Response = responseObject.Response;
		ResponseTime = DateTime.Now;

		if (OnResponseReceived != null) {
			OnResponseReceived (Response, _S3BucketName, _S3Region);
		}
	}

	//============================================================= Subscription Functions ===============================================================

	public static void SubscribeOnResponseReceived (ResponseReceivedEvent callback) {
		OnResponseReceived += callback;
	}

	public static void UnsubscribeOnResponseReceived (ResponseReceivedEvent callback) {
		OnResponseReceived -= callback;
	}

	//============================================================ File Downloading =====================================================================

	public static void S3GetObjects(FileEntry file) {
		S3GetObjects (s3Client, _S3BucketName, file, Application.persistentDataPath + "/");
	}

	public static void S3GetObjects(FileEntry file, string downloadDirectory) {
		S3GetObjects (s3Client, _S3BucketName, file, downloadDirectory);
	}

	public static void S3GetObjects(IAmazonS3 Client, string S3BucketName, FileEntry file, string downloadDirectory)
	{
		GetObject(Client, S3BucketName, file, downloadDirectory);
	}

	public static void S3GetObjects(IAmazonS3 Client, string S3BucketName, List<FileEntry> ModifiedLocalFileList) {
		S3GetObjects (Client, S3BucketName, ModifiedLocalFileList, Application.persistentDataPath + "/");
	}

	//Download functions
	public static void S3GetObjects(IAmazonS3 Client, string S3BucketName, List<FileEntry> ModifiedLocalFileList, string downloadDirectory)
	{
		foreach (FileEntry entry in ModifiedLocalFileList) 
		{
			GetObject(Client, S3BucketName, entry, downloadDirectory);
		}
	}

	static void GetObject(IAmazonS3 Client, string S3BucketName, FileEntry file, string downloadDirectory)
	{
		try
		{
			Client.GetObjectAsync (S3BucketName, file.Path, (responseObj) => 
				{
					var response = responseObj.Response;

					Debug.Log("Downloading...");
					Debug.Log(downloadDirectory + file.FileName);
					if (response.ResponseStream != null)
					{
						Debug.Log("RESPONSE");
						using (var fs = File.Create(downloadDirectory + file.FileName))
						{
							Debug.Log("Buffer...");
							byte[] buffer = new byte[10000000];
							int count = 0;
							while ((count = response.ResponseStream.Read(buffer, 0, buffer.Length)) != 0)
								fs.Write(buffer, 0, count);
							fs.Flush();
						}

						if (OnDownloadFinished != null) {
							OnDownloadFinished(file);
						}
					}
				});					
		}

		catch (Exception e)
		{
			Debug.Log ("Exception in PostFile: " + e.Message);
		}
	}

}
