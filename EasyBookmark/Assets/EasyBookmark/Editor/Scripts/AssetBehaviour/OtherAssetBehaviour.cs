using UnityEditor;
using UnityEngine;

namespace EasyBookmark
{
	public class OtherAssetBehaviour : IAssetBehaviour
	{
		public void Select(string assetPath)
		{
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
		}

		public void Open(string assetPath)
		{
		}
	}
}