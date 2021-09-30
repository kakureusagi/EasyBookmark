using System;
using UnityEditor;

namespace EasyBookmark
{
	public class PostProcessor : AssetPostprocessor
	{
		static Action callback;

		public static void RegisterAssetChangedCallback(Action callback)
		{
			PostProcessor.callback += callback;
		}

		public static void UnregisterAssetChangedCallback(Action callback)
		{
			PostProcessor.callback -= callback;
		}


		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			callback?.Invoke();
		}
	}
}