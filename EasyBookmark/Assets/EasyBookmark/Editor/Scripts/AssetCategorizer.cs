using System;
using System.IO;

namespace EasyBookmark
{
	public class AssetCategorizer
	{
		public AssetCategory Categorize(string assetPath)
		{
			if (assetPath == null)
			{
				throw new ArgumentNullException(nameof(assetPath));
			}

			if (assetPath == "")
			{
				throw new ArgumentOutOfRangeException(nameof(assetPath));
			}

			if (Directory.Exists(assetPath))
			{
				return AssetCategory.Directory;
			}

			if (File.Exists(assetPath))
			{
				if (assetPath.EndsWith(".prefab", StringComparison.InvariantCultureIgnoreCase))
				{
					return AssetCategory.Prefab;
				}

				if (assetPath.EndsWith(".unity", StringComparison.InvariantCultureIgnoreCase))
				{
					return AssetCategory.Scene;
				}

				return AssetCategory.Other;
			}

			return AssetCategory.NotExists;
		}
	}
}