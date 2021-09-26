using System.Collections.Generic;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyBookmark
{
	public class BookmarkWindow : EditorWindow, ITestElementCallbackReceiver
	{
		[MenuItem("aa/bb")]
		public static void Aaa()
		{
			GetWindow<BookmarkWindow>();
		}

		ListView listView;
		VisualElement box;
		AssetCategorizer categorizer;
		SaveData saveData;
		List<string> assetPaths;

		void CreateGUI()
		{
			var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIElements/BookmarkWindow.uxml");
			tree.CloneTree(rootVisualElement);

			categorizer = new AssetCategorizer();

			saveData = new SaveData(Const.SavePath);
			assetPaths = new List<string>(saveData.Load());

			listView = rootVisualElement.Q<ListView>();
			listView.itemsSource = assetPaths;
			listView.makeItem = () => new BookmarkItem();
			listView.bindItem = (element, i) =>
			{
				var assetPath = assetPaths[i];
				var category = categorizer.Categorize(assetPath);
				var target = element as BookmarkItem;
				target.Initialize(assetPath, category, this);
			};
			listView.selectionType = SelectionType.Single;
			listView.itemHeight = BookmarkItem.Height;

			box = rootVisualElement.Q<VisualElement>("box");
			box.RegisterCallback<DragPerformEvent>(OnDragPerform);
			box.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
		}

		void OnDisable()
		{
			box.UnregisterCallback<DragPerformEvent>(OnDragPerform);
			box.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
		}
		
		void OnDragPerform(DragPerformEvent e)
		{
			if (!DragAndDrop.paths.Any())
			{
				return;
			}
			
			foreach (var path in DragAndDrop.paths)
			{
				Debug.Log(path);
				if (assetPaths.Any(p => p == path))
				{
					continue;
				}
				
				assetPaths.Add(path);
			}

			listView.Refresh();
			saveData.Save(assetPaths);
		}

		void OnDragUpdate(DragUpdatedEvent e)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Move;
		}

		void ITestElementCallbackReceiver.OnSelectButton(BookmarkItem item)
		{
			if (item.Category == AssetCategory.NotExists)
			{
				return;
			}

			Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(item.AssetPath);
		}

		void ITestElementCallbackReceiver.OnOpenButton(BookmarkItem item)
		{
			if (item.Category == AssetCategory.Scene)
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(item.AssetPath);
				}
			}
			else if (item.Category == AssetCategory.Prefab)
			{
				var asset = AssetDatabase.LoadAssetAtPath<Object>(item.AssetPath);
				AssetDatabase.OpenAsset(asset);
			}
		}

		void ITestElementCallbackReceiver.OnDeleteButton(BookmarkItem item)
		{
			var index = assetPaths.IndexOf(item.AssetPath);
			if (index == -1)
			{
				return;
			}

			assetPaths.RemoveAt(index);
			saveData.Save(assetPaths);
			listView.Refresh();
		}


		BookmarkItem dragStartItem;
		void ITestElementCallbackReceiver.OnDragStart(BookmarkItem item)
		{
			dragStartItem = item;
		}

		void ITestElementCallbackReceiver.OnDragMove(BookmarkItem item)
		{
		}

		void ITestElementCallbackReceiver.OnDragPerform(BookmarkItem item, LinePosition linePosition)
		{
			if (dragStartItem == item)
			{
				return;
			}
			
			var beforeIndex = assetPaths.IndexOf(dragStartItem.AssetPath);
			var afterIndex = assetPaths.IndexOf(item.AssetPath);

			if (afterIndex > beforeIndex)
			{
				if (linePosition == LinePosition.Top)
				{
					--afterIndex;
				}
			}
			else
			{
				if (linePosition == LinePosition.Bottom)
				{
					++afterIndex;
				}
			}
			
			if (beforeIndex == afterIndex)
			{
				return;
			}
			

			if (afterIndex > beforeIndex)
			{
				var path = assetPaths[beforeIndex];
				for (var i = beforeIndex; i < afterIndex; i++)
				{
					assetPaths[i] = assetPaths[i + 1];
				}

				assetPaths[afterIndex] = path;
			}
			else
			{
				var path = assetPaths[beforeIndex];
				for (var i = beforeIndex; i > afterIndex; i--)
				{
					assetPaths[i] = assetPaths[i - 1];
				}

				assetPaths[afterIndex] = path;
			}

			saveData.Save(assetPaths);
			listView.Refresh();
		}
	}
}
