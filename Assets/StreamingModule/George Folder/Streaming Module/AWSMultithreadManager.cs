using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using System.Threading;

public class AWSMultithreadManager : Singleton<AWSMultithreadManager> {

	[RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeStructure()
	{
		InitializeSingleton();
	}

	public bool runningThread = false;

	void Awake() {
		UnityInitializer.AttachToGameObject (this.gameObject);
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
		AWSPathLoader.SubscribeAWSPathStructureToAWSLoader ();

		runningThread = true;

		Thread testThread = new Thread (new ThreadStart(ThreadedFunction));
		testThread.Start ();
	}

	void OnDestroy() {
		runningThread = false;
	}

	void ThreadedFunction() {
		int i = 0;
		while (runningThread) {
			i++;
			Debug.Log (i);
			AWSLoader.RequestS3BucketObjects ();
			Thread.Sleep (30000);
			if (i > 20) {
				runningThread = false;
			}
		}
	}

}
