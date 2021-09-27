using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyBookmark
{
	// [CreateAssetMenu(menuName = "EasyBookmark/Create Tree Container")]
	public class VisualTreeStorage : ScriptableObject
	{
		public VisualTreeAsset BookmarkWindow => bookmarkWindow;
		public VisualTreeAsset BookmarkItem => bookmarkItem;


		[SerializeField]
		VisualTreeAsset bookmarkWindow = default;

		[SerializeField]
		VisualTreeAsset bookmarkItem = default;

		static VisualTreeStorage instance;

		public static VisualTreeStorage Load()
		{
			if (instance == null)
			{
				foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(VisualTreeStorage)}"))
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					var storage = AssetDatabase.LoadAssetAtPath<VisualTreeStorage>(path);
					if (storage != null)
					{
						instance = storage;
						break;
					}
				}
			}

			return instance;
		}
	}
}