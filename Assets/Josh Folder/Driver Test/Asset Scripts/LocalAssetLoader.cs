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

	#region Compare Dictionaries
	// Creates a list 
	public static List<FileEntry> CompareDictionaries(Dictionary<string, FileEntry> LocalFileList, Dictionary<string, FileEntry> S3FileList)
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

	#region Helper Functions
	public static string[] GetAllFilePaths()
	{
		//Directory.CreateDirectory (Application.persistentDataPath + "Dump");
		//return Directory.GetFiles(Application.persistentDataPath + "Dump", "*.*", SearchOption.AllDirectories);
		Directory.CreateDirectory(S3AssetStructure.DirectoryPath);
		return Directory.GetFiles (S3AssetStructure.DirectoryPath, "*.*");
	}
	#endregion
}

