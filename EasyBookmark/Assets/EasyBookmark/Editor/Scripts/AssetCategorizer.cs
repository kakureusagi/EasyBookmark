using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetCategorizer
{
	public AssetCategory Categorize(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		if (!File.Exists(path) && !Directory.Exists(path))
		{
			return AssetCategory.NotExists;
		}

		if (path.EndsWith(".prefab", StringComparison.InvariantCulture))
		{
			return AssetCategory.Prefab;
		}

		if (path.EndsWith(".unity", StringComparison.InvariantCulture))
		{
			return AssetCategory.Scene;
		}

		return AssetCategory.Other;
	}
}