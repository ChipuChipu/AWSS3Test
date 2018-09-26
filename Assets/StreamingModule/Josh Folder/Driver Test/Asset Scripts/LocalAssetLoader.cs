using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AssetStruct;

// NOTE: Dictionary setup has changed from <string FilePath, FileEntry> to <string FileName, FileEntry>

public static class LocalAssetLoader
{

	public static string DirectoryPath = Application.persistentDataPath + "/S3LocalTest/";	// Local Directory used for checks. (Ignores all files not in the directory)

	public static void InitializeFiles()
	{
		LocalAssetStructure.SetLocalFileList (InitializeFileList ());
	}

	#region Initilize FileList
	public static Dictionary<string, FileEntry> InitializeFileList()
	{
		Dictionary<string, FileEntry> localFileList = new Dictionary<string, FileEntry> ();

		foreach (string path in GetAllFilePaths()) 
		{
			try
			{
				localFileList.Add (Path.GetFileName (path), new FileEntry 
					{
						FileName = Path.GetFileName(path),
						Path = path,
						FileSize = new FileInfo(path).Length,
						State = FileEntry.Status.Unmodified,
						ModifiedTime = File.GetLastWriteTime(path),
						CreationTime = File.GetCreationTime(path)
					});
			}
			catch(Exception e) {
				throw e;
			}
		}

		return localFileList;
	}
	#endregion

	//========================================================= File Sorting ========================================================================

	public static void UpdateModifiedFilesToLocalAssetStructure () {
		LocalAssetStructure.SetModifiedList (GetModifiedFiles());
	}

	public static List<FileEntry> GetModifiedFiles()
	{
		return LoadFiles (LocalAssetStructure.GetLocalFileList());
	}

	#region Load Files
	// Generates a list of files that contain all modified files.
	public static List<FileEntry> LoadFiles(Dictionary<string, FileEntry> LocalFileList)
	{
		Dictionary<string, FileEntry> currentFileList = InitializeFileList();
		IEnumerable oldFiles = LocalFileList.Keys.Except(currentFileList.Keys).ToList();
		List<FileEntry> modifiedFileList = new List<FileEntry>();
		List<string> removedFileList = oldFiles.Cast<string>().ToList();
		FileEntry newEntry = new FileEntry();

		// Go through all found files in the most recent file list
		foreach (KeyValuePair<string, FileEntry> entryPair in currentFileList)
		{
			// Check if the file exists in both lists
			if (LocalFileList.ContainsKey(entryPair.Key))
			{
				// Check for Modification
				if (entryPair.Value.FileSize != LocalFileList[entryPair.Key].FileSize)
				{
					Debug.Log("<LoadFiles> Modified File Flagged! <File: " + entryPair.Key + ">");
					newEntry = entryPair.Value;
					newEntry.State = FileEntry.Status.Modified;
					modifiedFileList.Add(newEntry);
				}
			}

			// Else a new file must have been added
			else
			{
				Debug.Log("<LoadFiles> Added File Flagged! <File: " + entryPair.Key + ">");
				newEntry = entryPair.Value;
				newEntry.State = FileEntry.Status.Added;
				modifiedFileList.Add(newEntry);
			}
		}

		// Check if a file does not exist anymore
		foreach (string fileName in removedFileList)
		{
			// If the file no longer exists in the most current file list, it was recently removed
			if (!currentFileList.ContainsKey(fileName))
			{
				if (LocalFileList.ContainsKey(fileName))
				{
					Debug.Log("<LoadFiles> Removed File Flagged! <File: " + fileName + ">");
					newEntry = LocalFileList[fileName];
					newEntry.State = FileEntry.Status.Removed;
					modifiedFileList.Add(newEntry);
				}
			}
		}

		return modifiedFileList;
	}
	#endregion

	public static void UpdateFileList()
	{
		UpdateFileList (LocalAssetStructure.GetModifiedFileList(), LocalAssetStructure.GetLocalFileList());
		LocalAssetStructure.ClearModifiedList();
	}

	//================================================================================================================================

	//================================================== File Deletion / Update ======================================================
	#region Update FileList
	public static void UpdateFileList(List<FileEntry> modifiedFileList, Dictionary<string, FileEntry> LocalFileList)
	{
		if (modifiedFileList.Count != 0 && modifiedFileList != null) 
		{
			foreach (FileEntry entry in modifiedFileList) 
			{
				if (LocalFileList.ContainsKey (entry.FileName)) 
				{
					if (entry.State == FileEntry.Status.Modified)
						LocalFileList [entry.FileName] = entry;

					else if (entry.State == FileEntry.Status.Added)
						LocalFileList [entry.FileName] = entry;

					else if (entry.State == FileEntry.Status.Removed) 
					{
						Debug.Log ("<CompareDictionaries> We deleted a file! <File: " + entry.FileName + ">");
						if (File.Exists (entry.Path))
							File.Delete (entry.Path);

						LocalFileList.Remove (entry.FileName);
					}
				}
			}			
		}
	}
	#endregion

	//===============================================================================================================================

	#region Helper Functions
	public static string[] GetAllFilePaths()
	{
		Directory.CreateDirectory(DirectoryPath);
		return Directory.GetFiles (DirectoryPath, "*.*");
	}
	#endregion
}

