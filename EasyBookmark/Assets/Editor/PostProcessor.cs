using System.Linq;
using UnityEditor;

public class PostProcessor : AssetPostprocessor
{
	public static bool FileChangedOnLastPostProcess { get; private set; }

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		FileChangedOnLastPostProcess = deletedAssets.Any() || movedAssets.Any() || movedFromAssetPaths.Any();
	}
}