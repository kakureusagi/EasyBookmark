using UnityEditor;
using UnityEngine;

namespace EasyBookmark
{
	public class PrefabAssetBehaviour : IAssetBehaviour
	{
		public void Select(string assetPath)
		{
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
		}

		public void Open(string assetPath)
		{
			var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			AssetDatabase.OpenAsset(asset);
		}
	}
}