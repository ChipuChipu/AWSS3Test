using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetStruct
{	
	public struct FileEntry
	{
		public enum Status {Unmodified, Added, Modified, Removed, Download, Upload}

		public string FileName;
		public string Path;
		public long FileSize;
		public Status State;
		public DateTime ModifiedTime;
		public DateTime CreationTime;

		public FileEntry(string n = null, string p = null, long fs = -1, Status s = Status.Unmodified, DateTime mt = default(DateTime), DateTime ct = default(DateTime))
		{
			FileName = n;
			Path = p;
			FileSize = fs;
			State = s;
			ModifiedTime = mt;
			CreationTime = ct;
		} // End Constructor

		public static bool operator == (FileEntry fe1, FileEntry fe2)
		{
			return fe1.Equals (fe2);
		}

		public static bool operator != (FileEntry fe1, FileEntry fe2)
		{
			return !fe1.Equals(fe2);
		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			FileEntry fe = (FileEntry)obj;

			return ((fe.FileName == FileName)  && (fe.FileSize == FileSize)  && (fe.ModifiedTime == ModifiedTime));
		}

		public override int GetHashCode()
		{
			int hash = 19;
			hash = hash * 23 + FileName.GetHashCode ();
			hash = hash * 23 + Path.GetHashCode ();
			return hash;
		}

	} // End Struct
} // End Namespace