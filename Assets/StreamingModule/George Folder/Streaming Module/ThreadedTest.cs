using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetStruct;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using System.Threading;

public class ThreadedTest : MonoBehaviour {

	public bool runningThread = false;

	void Start() {
		UnityInitializer.AttachToGameObject (this.gameObject);
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
		AWSPathLoader.SubscribeAWSPathStructureToAWSLoader ();

		runningThread = true;

		Thread testThread = new Thread (new ThreadStart(ThreadedFunction));
		testThread.Start ();

	}

	void ThreadedFunction() {
		int i = 0;
		while (runningThread) {
			i++;
			Debug.Log (i);
			AWSLoader.RequestS3BucketObjects ();
			Thread.Sleep (5000);
			if (i > 10) {
				runningThread = false;
			}
		}
	}

}
