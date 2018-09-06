using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class StreamingARMediaBehaviour : MonoBehaviour {

	VideoPlayer player;

	// Use this for initialization
	void Start () {

		player = GetComponent<VideoPlayer> ();

	}

	public bool test1 = false;
	
	// Update is called once per frame
	void Update () {

		if (test1) {
			player.Stop ();
			AWSPathStructure.PathEntry testEntry = AWSPathStructure.AWSDirectory.GetDirectoryInFileSystem ("Streaming/CoolAsset");
			player.source = VideoSource.Url;
			foreach (var testVidep in testEntry.nextPath) {
				player.url = testEntry.nextPath [testVidep.Key].fileData.Url;
				Debug.Log (player.url);
				break;
			}
			player.Play ();
			test1 = false;
		}
		
	}

}
