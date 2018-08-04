using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AssetStruct;

public static class LocalAssetLoader
{
	#region Initilize FileList
	public static Dictionary<string, FileEntry> InitializeFileList()
	{
		Dictionary<string, FileEntry> temp = new Dictionary<string, FileEntry> ();

		foreach (string path in GetAllFilePaths()) 
			temp.Add (path, new FileEntry (Path.GetFileName (path), path, new FileInfo (path).Length, FileEntry.Status.Unmodified, File.GetLastWriteTime (path)));

		return temp;
	}
	#endregion

	#region Load Files
	public static List<FileEntry> LoadFiles(Dictionary<string, FileEntry> InitialFileList)
	{
		List<FileEntry> ModifiedFileList = new List<FileEntry> ();
		List<string> keys = InitialFileList.Keys.ToList ();
		List<string> pathList = GetAllFilePaths ().OfType<string>().ToList();

		foreach (string path in pathList) 
		{
			DateTime dt = File.GetLastWriteTime (path);
			if (InitialFileList.ContainsKey (path)) 
			{
				Int64 initialFileSize = InitialFileList [path].fileSize;
				Int64 newFileSize = new FileInfo (path).Length;

				if (InitialFileList [path].dTime != dt) 
				{
					keys.Remove (path);
					ModifiedFileList.Add (new FileEntry (Path.GetFileName (path), path, newFileSize, FileEntry.Status.Modified, dt));
				} 
				else if (initialFileSize != newFileSize) 
				{
					keys.Remove (path);
					ModifiedFileList.Add (new FileEntry (Path.GetFileName (path), path, newFileSize, FileEntry.Status.Modified, dt));
				}
			}

			else if (!InitialFileList.ContainsKey(path))
				ModifiedFileList.Add (new FileEntry (Path.GetFileName(path), path, new FileInfo (Path.GetFileName (path)).Length, FileEntry.Status.Added, dt));
		}

		foreach (string path in keys) 
		{
			if (!pathList.Contains(path)) 
			{
				DateTime dt = File.GetLastWriteTime (path);
				ModifiedFileList.Add (new FileEntry (Path.GetFileName(path), path, new FileInfo (Path.GetFileName (path)).Length, FileEntry.Status.Removed, dt));
			}
		}

		return ModifiedFileList;
	}
	#endregion

	#region Update FileList
	public static Dictionary<string, FileEntry> UpdateFileList(List<FileEntry> modifiedFileList, Dictionary<string, FileEntry> fileList)
	{
		Dictionary<string, FileEntry> temp = new Dictionary<string, FileEntry> ();

		foreach (KeyValuePair<string, FileEntry> entry in fileList) 
		{
			if (entry.Value.state == FileEntry.Status.Unmodified && modifiedFileList.Contains (entry.Value)) 
				temp.Add (entry.Key, entry.Value);
		}

		foreach (FileEntry entry in modifiedFileList) 
		{
			if (temp.ContainsKey (entry.path)) 
			{
				if ((temp [entry.path].state != entry.state) && (entry.state != FileEntry.Status.Removed))
					temp [entry.path] = entry;
				
				else if (entry.state == FileEntry.Status.Added)
					temp.Add (entry.path, entry);
				
				else if (entry.state == FileEntry.Status.Removed)
					File.Delete (entry.path);
			}
		}

		return temp;
	}
	#endregion

	#region Compare Dictionaries
	public static List<FileEntry> CompareDictionaries(Dictionary<string, FileEntry> FileList, Dictionary<string, FileEntry> newFileList)
	{
		List<FileEntry> temp = new List<FileEntry> ();

		if (FileList.Count != newFileList.Count)
			return temp = newFileList.Values.ToList ();

		foreach (KeyValuePair<string, FileEntry> entry in newFileList) 
		{
			if (FileList.ContainsKey (entry.Key))
			{
				if (FileList [entry.Value.path] != entry.Value)
					temp.Add (entry.Value);
				
				else if (entry.Value.state == FileEntry.Status.Added)
					temp.Add (entry.Value);
				
				else if (entry.Value.state == FileEntry.Status.Removed)
					File.Delete (entry.Value.path);
			}
		}

		return temp;
	}
	#endregion

	#region Helper Functions
	public static string[] GetAllFilePaths()
	{
		// return Directory.GetFiles(Application.persistentDataPath, "*.*", SearchOption.AllDirectories);
		return Directory.GetFiles ("C:\\Users\\Joshu\\Desktop\\Dump", "*.*", SearchOption.AllDirectories);
	}
	#endregion
}
