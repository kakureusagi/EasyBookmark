using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EasyBookmark
{
	public class SaveData
	{
		[Serializable]
		class Data
		{
			public string[] Guids = Array.Empty<string>();
		}


		readonly Data data;
		readonly FileInfo saveFile;

		public SaveData(string savePath)
		{
			saveFile = new FileInfo(savePath);
			data = LoadImpl();
		}

		public void Save(IEnumerable<string> guids)
		{
			data.Guids = guids.ToArray();
			SaveImpl(data);
		}

		public string[] Load()
		{
			return data.Guids.ToArray();
		}

		private Data LoadImpl()
		{
			if (saveFile.Exists)
			{
				try
				{
					var json = File.ReadAllText(saveFile.FullName);
					return JsonUtility.FromJson<Data>(json) ?? new Data();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					return new Data();
				}
			}

			return new Data();
		}

		private void SaveImpl(Data data)
		{
			var json = JsonUtility.ToJson(data);
			if (!saveFile.Directory.Exists)
			{
				saveFile.Directory.Create();
			}

			File.WriteAllText(saveFile.FullName, json);
		}
	}
}