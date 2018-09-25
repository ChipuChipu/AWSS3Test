using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DownloadDriver : MonoBehaviour {

	public bool start = false;
	public bool creation = false;

	public GameObject display;
	public GameObject exists;
	public GameObject deleted;

	void FinishedDownload(FileEntry test) {
		display.SetActive (true);
		Debug.Log ("Downloaded file!");
		if (File.Exists (Application.persistentDataPath + "/" + "V1 Alpha Old labels - blue - 0_19 .mp4")) {
			exists.SetActive (true);
		}
		File.Delete (Application.persistentDataPath + "/" + "V1 Alpha Old labels - blue - 0_19 .mp4");
		if (!File.Exists (Application.persistentDataPath + "/" + "V1 Alpha Old labels - blue - 0_19 .mp4")) {
			deleted.SetActive (true);
		}
		Debug.Log ("Deleted!");
	}

	void Start() {
		AWSLoader.OnDownloadFinished += new AWSLoader.FinishedDownloadEvent (FinishedDownload);
		Invoke ("DownloadTempFile", 5f);
	}
	
	// Update is called once per frame
	void Update () {

		if (creation) {
			File.Create (Application.persistentDataPath + "/" + "C.txt");
			creation = false;
		}

		if (start) {
			AWSPathStructure.PathEntry file = AWSPathStructure.AWSDirectory.GetDirectoryInFileSystem ("Streaming/[0,12,255]ChihuahuaCervezaOldBottle-Blue/V1 Alpha Old labels - blue - 0_19 .mp4");
			AWSPathStructure.AWSDirectory.DisplayFilesOnDirectory (file);
			Debug.Log (file.fileData.FileName);
			Debug.Log (file.fileData.Path);
			AWSLoader.S3GetObjects (file.fileData);
			start = false;
		}
		
	}

	void DownloadTempFile() {
		AWSPathStructure.PathEntry file = AWSPathStructure.AWSDirectory.GetDirectoryInFileSystem ("Streaming/[0,12,255]ChihuahuaCervezaOldBottle-Blue/V1 Alpha Old labels - blue - 0_19 .mp4");
		AWSPathStructure.AWSDirectory.DisplayFilesOnDirectory (file);
		Debug.Log ("Running downloading file... \n\n\n\n");
		Debug.Log (file.fileData.FileName);
		Debug.Log (file.fileData.Path);
		AWSLoader.S3GetObjects (file.fileData);
	}

}
