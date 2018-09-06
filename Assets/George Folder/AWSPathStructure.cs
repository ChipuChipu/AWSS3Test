﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetStruct;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using AssetStruct;

public class AWSPathStructure : Singleton<AWSPathStructure>
{

	/*
	public void Start() {
		FileSystem test = new FileSystem ();
		FileEntry test1 = new FileEntry ("test1", "A/B/test1");
		FileEntry test2 = new FileEntry ("test2", "A/C/test2");
		FileEntry test3 = new FileEntry ("test3", "A/B/test3");
		FileEntry test4 = new FileEntry ("test4", "B/B/test4");
		test.AddFileEntrytoFileSystem (test1);
		test.AddFileEntrytoFileSystem (test2);
		test.AddFileEntrytoFileSystem (test3);
		test.AddFileEntrytoFileSystem (test4);
		test.DisplayFilesOnDirectory ();
	}
	*/

	public delegate void RetrievedDirectoryEvent (FileSystem directory);

	public static RetrievedDirectoryEvent OnRetrievedDirectory;

	FileSystem _AWSDirectory;

	public static FileSystem AWSDirectory {
		get {
			return Instance._AWSDirectory;
		}
		set {
			Instance._AWSDirectory = value;
		}
	}

	void Awake ()
	{
		UnityInitializer.AttachToGameObject (this.gameObject);
		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
		AWSPathLoader.SubscribeAWSPathStructureToAWSLoader ();
	}

	public bool testBool = false;
	public string testDirectory;
	public bool testBool1 = false;

	void Update() {

		if (testBool) {
			AWSLoader.RequestS3BucketObjects ();
			//AWSDirectory.DisplayFilesOnDirectory ();
			testBool = false;
		}

		if (testBool1) {
			AWSDirectory.DisplayFilesOnDirectory (AWSDirectory.GetDirectoryInFileSystem (testDirectory));
			testBool1 = false;
		}

	}

	public static void RenderAWSResponse (ListObjectsResponse responseObject, string S3BucketName)
	{

		AWSDirectory = new FileSystem ();

		Debug.Log ("Rendering Response...");

		try {
			responseObject.S3Objects.ForEach ((o) => {
				Debug.Log ("Filename: " + o.Key);
				FileEntry entry = new FileEntry (GetAssetName (o.Key), o.Key, GetURL (S3BucketName, o.Key), o.Size, FileEntry.Status.Unmodified, (DateTime)o.LastModified, (DateTime)o.LastModified);
				AWSDirectory.AddFileEntrytoFileSystem (entry);
			});

			if (OnRetrievedDirectory != null) {
				OnRetrievedDirectory (AWSDirectory);
			}
		} catch (AmazonS3Exception e) {
			throw e;
		}

	}

	public static string GetURL (string S3BucketName, string fileName)
	{
		return "https://s3.amazonaws.com//" + S3BucketName + "//" + fileName;
	}

	public static string GetAssetName (string awsName)
	{
		string[] textArray = awsName.Split ('/');
		return textArray [textArray.Length - 1];
	}

	public class PathEntry
	{
		public FileEntry fileData;
		public Dictionary <string, PathEntry> nextPath;
		public PathEntry parentEntry;

		public PathEntry (FileEntry fileData, Dictionary<string, PathEntry> nextPath = null, PathEntry parentEntry = null)
		{
			this.fileData = fileData;
			this.nextPath = nextPath;
			this.parentEntry = parentEntry;
		}

		public PathEntry (string name, PathEntry parentEntry) : this (new FileEntry (name), null, parentEntry)
		{
		}

		public PathEntry () : this (new FileEntry (), null, null)
		{
		}

		public bool AddPathEntry (PathEntry pathEntry)
		{
			if (!nextPath.ContainsKey (pathEntry.fileData.FileName)) {
				nextPath.Add (pathEntry.fileData.FileName, pathEntry);
				pathEntry.parentEntry = this;
				return true;
			} else {
				return false;
			}
		}

		public bool RemovePathEntry (PathEntry fileEntry)
		{
			return RemovePathEntry (fileEntry.fileData.FileName);
		}

		public bool RemovePathEntry (string fileEntryName)
		{
			return nextPath.Remove (fileEntryName);
		}
	}



	public class FileSystem
	{
		
		public PathEntry Root;
		public Dictionary<string, PathEntry> FileIndex;

		public FileSystem ()
		{
			Root = new PathEntry (new FileEntry ("Root"));
			FileIndex = new Dictionary<string, PathEntry> ();
		}

		public bool AddFileEntrytoFileSystem (FileEntry fileEntry)
		{
			PathEntry temporalPathEntry = new PathEntry (fileEntry);
			return AddFileEntrytoFileSystem (temporalPathEntry);
		}

		public bool AddFileEntrytoFileSystem (PathEntry pathEntry)
		{
			FileIndex.Add (pathEntry.fileData.Path, pathEntry);
			return AddFileEntrytoFileSystem (pathEntry, Root, pathEntry.fileData.Path);
		}

		public bool AddFileEntrytoFileSystem (PathEntry pathEntry, PathEntry current, string currentPathRender)
		{
			
			string[] temporalPathRender = currentPathRender.Split (new char[] { '/' }, 2);

			if (current.nextPath == null) {
				current.nextPath = new Dictionary<string, PathEntry> ();
			}

			if (temporalPathRender.Length > 1) {
				if (!current.nextPath.ContainsKey (temporalPathRender [0])) {
					current.nextPath.Add (temporalPathRender [0], new PathEntry (temporalPathRender [0], current));
				}
				return AddFileEntrytoFileSystem (pathEntry, current.nextPath [temporalPathRender [0]], temporalPathRender [1]);
			} else {
				//This is to render the paths but not the empty field.
				if (temporalPathRender[0] != "") {
					return current.AddPathEntry (pathEntry);
				} else {
					return false;
				}
			}

		}

		public PathEntry GetDirectoryInFileSystem(string path) {
			return GetDirectoryInFileSystem (Root, path);
		}

		public PathEntry GetDirectoryInFileSystem(PathEntry current, string path) {
			string[] temporalPathRender = path.Split (new char[] { '/' }, 2);
			if (current.nextPath.ContainsKey (temporalPathRender [0])) {
				PathEntry subDirectory = current.nextPath [temporalPathRender [0]];
				if (temporalPathRender.Length > 1) {
					return GetDirectoryInFileSystem (subDirectory, temporalPathRender[1]);
				} else {
					if (temporalPathRender [0] != "") {
						return subDirectory;
					} else {
						return current;
					}
				}
			} else {
				if (temporalPathRender [0] == "") { 
					return current;
				} else {
					Debug.LogError ("Invalid file path.");
					return  null;
				}
			}
		}

		public void DisplayFilesOnDirectory ()
		{

			DisplayFilesOnDirectory (Root);

		}

		public void DisplayFilesOnDirectory (PathEntry current)
		{
			if (current != null) {
				if (current.nextPath != null) {
					foreach (var dir in current.nextPath) {
						Debug.Log (dir.Value.fileData.FileName);
						DisplayFilesOnDirectory (dir.Value);
					}
				}
			} else {
				Debug.LogError ("Null current value. Cannot display.");
			}
		}

	}

}
