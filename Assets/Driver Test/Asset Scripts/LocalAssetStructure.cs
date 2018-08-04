using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AssetStruct;

public class LocalAssetStructure : Singleton<LocalAssetStructure>
{
	[RuntimeInitializeOnLoadMethodAttribute (RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeStructure()
	{
		InitializeSingleton ();
	}

	[ReadOnly]
	public int ModifiedFileListCount = 0;
	[ReadOnly]
	public int FileListCount = 0;

	private Dictionary<string, FileEntry> _FileList;
	static Dictionary<string, FileEntry> FileList{
		get{ return Instance._FileList; }

		set
		{
			Instance.FileListCount = value.Count;
			Instance._FileList = value;
		}
	}

	private List<FileEntry> _ModifiedFileList;
	static List<FileEntry> ModifiedFileList{
		get{ return Instance._ModifiedFileList; }
		set
		{ 
			Instance.ModifiedFileListCount = value.Count;
			Instance._ModifiedFileList = value;
		}
	}

	public static void LoadInitialFiles()
	{
		FileList = LocalAssetLoader.InitializeFileList ();
	}

	public static Dictionary<string,FileEntry> DebugLoadDummyFileList()
	{
		return LocalAssetLoader.InitializeFileList ();
	}

	public static void LoadFiles()
	{
		ModifiedFileList = LocalAssetLoader.LoadFiles (FileList);
	}

	public static void UpdateFileList()
	{
		FileList = LocalAssetLoader.UpdateFileList (ModifiedFileList, FileList);
	}
		
	public static void CompareDictionaries(Dictionary<string, FileEntry> newFileList)
	{
		ModifiedFileList = LocalAssetLoader.CompareDictionaries (FileList, newFileList);
	}

	#region Helper Functions
	public static void UnloadAllFiles()
	{
		FileList.Clear ();
		ModifiedFileList.Clear ();
	}

	public static List<FileEntry> GetModifiedFileList()
	{
		return ModifiedFileList;
	}

	public static Dictionary<string, FileEntry> GetFileList()
	{
		return FileList;
	}
	#endregion

	void Awake()
	{
		_ModifiedFileList = new List<FileEntry> ();
		_FileList = new Dictionary<string, FileEntry> ();
	}
}
