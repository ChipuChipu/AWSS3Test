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

	#region Core Methods
	public static void InitializeFiles()
	{
		LocalFileList = LocalAssetLoader.InitializeFileList ();
	}
		
	public static void LoadFiles()
	{
		ModifiedFileList = LocalAssetLoader.LoadFiles (LocalFileList);
	}

	public static void UpdateFileList()
	{
		LocalAssetLoader.UpdateFileList (ModifiedFileList, LocalFileList);
		ModifiedFileList.Clear ();
	}
		
	public static void CompareDictionaries(Dictionary<string, FileEntry> S3FileList)
	{
		ModifiedFileList = LocalAssetLoader.CompareDictionaries (LocalFileList, S3FileList);
	}
	#endregion

	#region Helper Methods
	public static void UnloadAllLists()
	{
		LocalFileList.Clear ();
		ModifiedFileList.Clear ();
	}

	public static List<FileEntry> GetModifiedFileList()
	{
		return ModifiedFileList;
	}

	public static Dictionary<string, FileEntry> GetLocalFileList()
	{
		return LocalFileList;
	}
	#endregion
}
