using UnityEditor;

namespace EasyBookmark
{
	public class BookmarkTool
	{
		[MenuItem("Window/EasyBookmark")]
		public static void OpenEasyBookmarkWindow()
		{
			EditorWindow.GetWindow<BookmarkWindow>();
		}
	}
}