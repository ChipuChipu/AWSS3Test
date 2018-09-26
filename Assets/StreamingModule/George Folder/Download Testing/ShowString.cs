using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ShowString : MonoBehaviour {

	public Text text;

	// Update is called once per frame
	void Update () {

		if (Directory.Exists (LocalAssetLoader.DirectoryPath)) {
			string[] files = Directory.GetFiles (LocalAssetLoader.DirectoryPath);
			string completeString = "";
			foreach (var txt in files) {
				completeString += txt + "\n";
			}
			text.text = completeString;
		}

	}
}
