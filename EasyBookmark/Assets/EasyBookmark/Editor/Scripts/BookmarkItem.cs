using System;
using EasyBookmark;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor
{
	public class BookmarkItem : VisualElement
	{
		public static readonly int Height = 22;

		static readonly string TopBorderClass = "border-top";
		static readonly string BottomBorderClass = "border-bottom";

		public string AssetPath { get; private set; }
		public AssetCategory Category { get; private set; }

		VisualElement root;
		Label pathLabel;
		Button openButton;
		Button selectButton;
		Button deleteButton;
		ITestElementCallbackReceiver callbackReceiver;
		
		LinePosition linePosition;
		bool hasEnter;

		public BookmarkItem()
		{
			var treeStorage = VisualTreeStorage.Load();
			var container = treeStorage.BookmarkItem.Instantiate();
			hierarchy.Add(container);

			root = container.Q<VisualElement>("root");

			pathLabel = container.Q<Label>("path");

			openButton = container.Q<Button>("openButton");
			openButton.clicked += OnOpenButton;

			selectButton = container.Q<Button>("selectButton");
			selectButton.clicked += OnSelectButton;

			deleteButton = container.Q<Button>("deleteButton");
			deleteButton.clicked += OnDeleteButton;

			RegisterCallback<MouseDownEvent>(OnMouseDown);
			RegisterCallback<DragEnterEvent>(OnDragEnter);
			RegisterCallback<DragLeaveEvent>(OnDragLeave);
			RegisterCallback<DragPerformEvent>(OnDragPerform);
			RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
		}

		public void Initialize(string assetPath, AssetCategory category, ITestElementCallbackReceiver callbackReceiver)
		{
			this.callbackReceiver = callbackReceiver;

			AssetPath = assetPath;
			pathLabel.text = assetPath;

			openButton.SetEnabled(category == AssetCategory.Scene || category == AssetCategory.Prefab);
			selectButton.SetEnabled(category != AssetCategory.NotExists);
		}

		void OnOpenButton()
		{
			callbackReceiver.OnOpenButton(this);
		}

		void OnSelectButton()
		{
			callbackReceiver.OnSelectButton(this);
		}

		void OnDeleteButton()
		{
			callbackReceiver.OnDeleteButton(this);
		}

		void OnDragLeave(DragLeaveEvent e)
		{
			DisableLine();
		}

		void OnDragEnter(DragEnterEvent e)
		{
			if (DragAndDrop.GetGenericData(nameof(BookmarkItem)) != null)
			{
				hasEnter = true;
			}
		}

		void OnDragUpdate(DragUpdatedEvent e)
		{
			if (hasEnter)
			{
				linePosition = e.localMousePosition.y > Height / 2.0f ? LinePosition.Bottom : LinePosition.Top;
				DragAndDrop.visualMode = DragAndDropVisualMode.Move;
				EnableLine(linePosition);
			}
			else
			{
				linePosition = LinePosition.None;
				DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
			}
		}

		void OnDragPerform(DragPerformEvent e)
		{
			if (hasEnter)
			{
				callbackReceiver.OnDragPerform(this, linePosition);
				linePosition = LinePosition.None;
				DisableLine();
				hasEnter = false;
			}
		}

		void OnMouseDown(MouseDownEvent e)
		{
			DragAndDrop.paths = Array.Empty<string>();
			DragAndDrop.objectReferences = Array.Empty<Object>();
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.SetGenericData(nameof(BookmarkItem), this);
			DragAndDrop.StartDrag(nameof(BookmarkItem));

			callbackReceiver.OnDragStart(this);
			hasEnter = true;
		}

		void EnableLine(LinePosition position)
		{
			DisableLine();
			
			if (position == LinePosition.Top)
			{
				root.AddToClassList(TopBorderClass);
			}
			else
			{
				root.AddToClassList(BottomBorderClass);
			}
		}

		void DisableLine()
		{
			root.RemoveFromClassList(TopBorderClass);
			root.RemoveFromClassList(BottomBorderClass);
		}
	}
}