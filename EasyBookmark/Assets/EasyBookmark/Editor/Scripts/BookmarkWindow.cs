using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyBookmark
{
	public class BookmarkWindow : EditorWindow, ITestElementCallbackReceiver
	{
		ListView listView;
		VisualElement box;
		SaveData saveData;
		List<string> guids;
		HashSet<string> assetPaths;

		BookmarkItem dragStartItem;

		readonly Dictionary<AssetCategory, IAssetBehaviour> assetBehaviours = new Dictionary<AssetCategory, IAssetBehaviour>
		{
			{ AssetCategory.Directory, new DirectoryAssetBehaviour() },
			{ AssetCategory.Other, new OtherAssetBehaviour() },
			{ AssetCategory.Prefab, new PrefabAssetBehaviour() },
			{ AssetCategory.Scene, new SceneAssetBehaviour() },
			{ AssetCategory.NotExists, new NullAssetBehaviour() },
		};


		void CreateGUI()
		{
			var treeStorage = VisualTreeStorage.Load();
			treeStorage.BookmarkWindow.CloneTree(rootVisualElement);

			var categorizer = new AssetCategorizer();

			saveData = new SaveData(Const.SavePath);
			guids = new List<string>(saveData.Load());
			assetPaths = new HashSet<string>(guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)));

			listView = rootVisualElement.Q<ListView>();
			listView.itemsSource = guids;
			listView.makeItem = () => new BookmarkItem();
			listView.bindItem = (element, i) =>
			{
				var guid = guids[i];
				var target = element as BookmarkItem;
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				var category = categorizer.Categorize(assetPath);
				target.Initialize(guid, assetPath, category, this);
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
				if (assetPaths.Contains(path))
				{
					continue;
				}

				guids.Add(AssetDatabase.AssetPathToGUID(path));
				assetPaths.Add(path);
			}

			listView.Refresh();
			saveData.Save(guids);
		}

		void OnDragUpdate(DragUpdatedEvent e)
		{
			foreach (var path in DragAndDrop.paths)
			{
				if (!assetPaths.Contains(path))
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Move;
					return;
				}
			}

			DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
		}

		void ITestElementCallbackReceiver.OnSelectButton(BookmarkItem item)
		{
			assetBehaviours[item.Category].Select(item.AssetPath);
		}

		void ITestElementCallbackReceiver.OnOpenButton(BookmarkItem item)
		{
			assetBehaviours[item.Category].Open(item.AssetPath);
		}

		void ITestElementCallbackReceiver.OnDeleteButton(BookmarkItem item)
		{
			var index = guids.IndexOf(item.Guid);
			if (index == -1)
			{
				return;
			}

			guids.RemoveAt(index);
			assetPaths.Remove(item.AssetPath);
			saveData.Save(guids);
			listView.Refresh();
		}

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