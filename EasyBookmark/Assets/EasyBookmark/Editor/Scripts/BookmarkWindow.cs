using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
		List<string> guids;

		void CreateGUI()
		{
			var treeStorage = VisualTreeStorage.Load();
			treeStorage.BookmarkWindow.CloneTree(rootVisualElement);

			categorizer = new AssetCategorizer();

			saveData = new SaveData(Const.SavePath);
			guids = new List<string>(saveData.Load());

			listView = rootVisualElement.Q<ListView>();
			listView.itemsSource = guids;
			listView.makeItem = () => new BookmarkItem();
			listView.bindItem = (element, i) =>
			{
				var guid = guids[i];
				var target = element as BookmarkItem;
				target.Initialize(guid, categorizer, this);
			};
			listView.selectionType = SelectionType.Single;
			listView.itemHeight = BookmarkItem.Height;
			
			box = rootVisualElement.Q<VisualElement>("box");
			box.RegisterCallback<DragPerformEvent>(OnDragPerform);
			box.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
			PostProcessor.RegisterAssetChangedCallback(OnAssetChanged);
		}

		void OnDisable()
		{
			box.UnregisterCallback<DragPerformEvent>(OnDragPerform);
			box.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
			PostProcessor.UnregisterAssetChangedCallback(OnAssetChanged);
		}

		void OnAssetChanged()
		{
			listView.Refresh();
		}
		
		void OnDragPerform(DragPerformEvent e)
		{
			if (!DragAndDrop.paths.Any())
			{
				return;
			}
			
			foreach (var path in DragAndDrop.paths)
			{
				var guid = AssetDatabase.AssetPathToGUID(path);
				if (guids.Any(x => x == guid))
				{
					continue;
				}
				
				guids.Add(guid);
			}

			listView.Refresh();
			saveData.Save(guids);
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
			var index = guids.IndexOf(item.Guid);
			if (index == -1)
			{
				return;
			}

			guids.RemoveAt(index);
			saveData.Save(guids);
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
			
			var beforeIndex = guids.IndexOf(dragStartItem.Guid);
			var afterIndex = guids.IndexOf(item.Guid);

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
				var guid = guids[beforeIndex];
				for (var i = beforeIndex; i < afterIndex; i++)
				{
					guids[i] = guids[i + 1];
				}

				guids[afterIndex] = guid;
			}
			else
			{
				var guid = guids[beforeIndex];
				for (var i = beforeIndex; i > afterIndex; i--)
				{
					guids[i] = guids[i - 1];
				}

				guids[afterIndex] = guid;
			}

			saveData.Save(guids);
			listView.Refresh();
		}
	}
}
