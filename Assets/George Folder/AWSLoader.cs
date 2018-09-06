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

public class AWSLoader {
	
	//Bucket ID and S3 Information
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
	public delegate void ResponseReceivedEvent(ListObjectsResponse responseObject, string bucketName);
	public static ResponseReceivedEvent OnResponseReceived;

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
			OnResponseReceived (Response, _S3BucketName);
		}
	}

	//============================================================= Subscription Functions ===============================================================

	public static void SubscribeOnResponseReceived (ResponseReceivedEvent callback) {
		OnResponseReceived += callback;
	}

	public static void UnsubscribeOnResponseReceived (ResponseReceivedEvent callback) {
		OnResponseReceived -= callback;
	}
}
