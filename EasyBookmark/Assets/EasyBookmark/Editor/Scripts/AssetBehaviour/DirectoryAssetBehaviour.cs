using System;
using System.Reflection;
using UnityEditor;

namespace EasyBookmark
{
	public class DirectoryAssetBehaviour : IAssetBehaviour
	{
		class ProjectView
		{
			readonly Assembly editorAssembly;
			readonly Type projectBrowserType;
			readonly MethodInfo GetFolderInstanceIdsMethod;
			readonly MethodInfo SetFolderSelectionMethod;
			readonly MethodInfo OpenSelectedFoldersMethod;
			readonly FieldInfo ViewModeField;

			public ProjectView()
			{
				editorAssembly = Assembly.Load("UnityEditor");

				projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
				GetFolderInstanceIdsMethod = projectBrowserType.GetMethod("GetFolderInstanceIDs", BindingFlags.NonPublic | BindingFlags.Static);
				SetFolderSelectionMethod = projectBrowserType.GetMethod("SetFolderSelection", 0, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int[]), typeof(bool) }, null);
				ViewModeField = projectBrowserType.GetField("m_ViewMode", BindingFlags.NonPublic | BindingFlags.Instance);
				OpenSelectedFoldersMethod = projectBrowserType.GetMethod("OpenSelectedFolders", BindingFlags.NonPublic | BindingFlags.Static);
			}

			public void SelectDirectory(string assetPath)
			{
				var window = EditorWindow.GetWindow(projectBrowserType, false, "Project", false);
				window.Focus();
				var viewMode = (int)ViewModeField.GetValue(window);

				if (viewMode == 0)
				{
					// ProjectViewが1カラムのときはフォルダを展開する
					OpenSelectedFoldersMethod.Invoke(null, null);
				}
				else
				{
					var directoryPaths = new[] { assetPath };
					var directoryIds = (int[])GetFolderInstanceIdsMethod.Invoke(null, new object[] { directoryPaths });
					SetFolderSelectionMethod.Invoke(window, new object[] { directoryIds, true });
				}
			}
		}

		readonly ProjectView projectView = new ProjectView();

		public void Select(string assetPath)
		{
			// これを呼ぶとディレクトリへのフォーカスがうまく機能しない（２回クリックすることになる）
			// Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			projectView.SelectDirectory(assetPath);
		}

		public void Open(string assetPath)
		{
		}
	}
}