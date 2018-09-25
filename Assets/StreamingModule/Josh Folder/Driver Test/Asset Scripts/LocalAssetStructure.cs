using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AssetStruct;

public class LocalAssetStructure : Singleton<LocalAssetStructure>
{
	#region Initialization
	[RuntimeInitializeOnLoadMethodAttribute (RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeStructure()
	{
		InitializeSingleton ();
	}

	[ReadOnly]
	public int ModifiedFileListCount = 0;

	[ReadOnly]
	public int LocalFileListCount = 0;

	private Dictionary<string, FileEntry> _LocalFileList;
	static Dictionary<string, FileEntry> LocalFileList
	{
		get{ return Instance._LocalFileList; }

		set
		{
			Instance.LocalFileListCount = value.Count;
			Instance._LocalFileList = value;
		}
	}

	private List<FileEntry> _ModifiedFileList;
	static List<FileEntry> ModifiedFileList
	{
		get{ return Instance._ModifiedFileList; }
		set
		{ 
			Instance.ModifiedFileListCount = value.Count;
			Instance._ModifiedFileList = value;
		}
	}

	void Awake()
	{
		_ModifiedFileList = new List<FileEntry> ();
		_LocalFileList = new Dictionary<string, FileEntry> ();
	}

	#endregion

	#region Compare Dictionaries

	public static void GetDictionaryDifferenceList(Dictionary<string, FileEntry> S3FileList)
	{
		ModifiedFileList = GetDictionaryDifferenceList (LocalFileList, S3FileList);
	}

	// Creates a list 
	public static List<FileEntry> GetDictionaryDifferenceList (Dictionary<string, FileEntry> LocalFileList, Dictionary<string, FileEntry> S3FileList)
	{
		if (S3FileList.Count == 0 && LocalFileList.Count == 0)
		{
			Debug.Log("I proc'd");
			Debug.Log("S3FileList.Count: " + S3FileList.Count);
			Debug.Log("LocalFileList.Count: " + LocalFileList.Count);
			return null;
		}

		IEnumerable oldFiles = LocalFileList.Keys.Except (S3FileList.Keys).ToList();
		List<string> removedFileList = oldFiles.Cast<string> ().ToList ();
		List<FileEntry> modifiedFileList = new List<FileEntry> ();
		FileEntry newEntry = new FileEntry ();

		// Go through all elements of S3's File List
		foreach (KeyValuePair<string, FileEntry> entryPair in S3FileList)
		{

			// Check if the file exists on both Local and S3
			if ((LocalFileList.ContainsKey (entryPair.Key))) 
			{
				// Check if Local has Modified Versions
				if (LocalFileList [entryPair.Key].State == FileEntry.Status.Modified)
				{
					Debug.Log ("<CompareDictionaries> Modified File Flagged! <File: " + entryPair.Key + ">");
					// Add a new entry designating for Download <Was previously the logic for Uploading but due to an issue, Uploading is now redownloading the file.> 
					newEntry = LocalFileList [entryPair.Key];
					newEntry.State = FileEntry.Status.Download;
					modifiedFileList.Add (newEntry);

					// Revert LocalFileList's status back to Unmodified
					newEntry.State = FileEntry.Status.Unmodified;
					LocalFileList [entryPair.Key] = newEntry;
				}

				// Check to see if Local has an undocumented Modified Version
				if ((LocalFileList [entryPair.Key].FileSize != entryPair.Value.FileSize) && (LocalFileList [entryPair.Key].State != FileEntry.Status.Modified)) 
				{
					Debug.Log ("<CompareDictionaries> Undocumented Modified File Flagged! <File: " + entryPair.Key + ">");
					// Add a new entry designating for Download
					newEntry = entryPair.Value;
					newEntry.State = FileEntry.Status.Download;
					newEntry.Path = S3AssetStructure.CachePath + "\\" + newEntry.FileName;
					modifiedFileList.Add (newEntry);

					// Revert LocalFileList's Status back to Unmodified
					newEntry.State = FileEntry.Status.Unmodified;
					LocalFileList [entryPair.Key] = newEntry;
				}
			}

			// Else the file exists on S3 and does not exist on Local
			else 
			{
				Debug.Log ("<CompareDictionaries> Added File Flagged! <File: " + entryPair.Key + ">");
				// Add a new entry designating for Download
				newEntry = entryPair.Value;
				newEntry.State = FileEntry.Status.Download;
				newEntry.Path = S3AssetStructure.CachePath + "\\"+ newEntry.FileName;
				modifiedFileList.Add (newEntry);

				// Revert LocalFileList's Status back to Unmodified
				newEntry.State = FileEntry.Status.Unmodified;
				LocalFileList [entryPair.Key] = newEntry;
			}
		}

		// Check if the File exists on Local but not on S3
		foreach (string fileName in removedFileList)
		{
			// Sync up both S3 and Local by removing files that should not be there
			if (!S3FileList.ContainsKey (fileName)) 
			{
				Debug.Log ("<CompareDictionaries> Removed File Flagged! <File: " + fileName + ">");
				if (File.Exists (LocalFileList [fileName].Path))
					File.Delete (LocalFileList [fileName].Path);
				LocalFileList.Remove (fileName);
			}
		}

		return modifiedFileList;
	}
	#endregion

	#region Helper Methods
	public static void UnloadAllLists()
	{
		ClearModifiedList ();
		ClearLocalFileList ();
	}
		
	public static void ClearLocalFileList() {
		LocalFileList.Clear ();
	}

	public static void ClearModifiedList() {
		ModifiedFileList.Clear ();
	}

	public static void SetLocalFileList (Dictionary<string, FileEntry> fileList) {
		LocalFileList = fileList;
	}

	public static void SetModifiedList(List<FileEntry> modifiedList) {
		ModifiedFileList = modifiedList;
	}

	public static Dictionary<string, FileEntry> GetLocalFileList()
	{
		return LocalFileList;
	}

	public static List<FileEntry> GetModifiedFileList()
	{
		return ModifiedFileList;
	}
	#endregion
}
