using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EasyBookmark
{
	public class SceneAssetBehaviour : IAssetBehaviour
	{
		public void Select(string assetPath)
		{
			Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
		}

		public void Open(string assetPath)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				EditorSceneManager.OpenScene(assetPath);
			}
		}
	}
}