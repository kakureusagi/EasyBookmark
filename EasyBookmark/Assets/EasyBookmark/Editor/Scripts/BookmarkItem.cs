using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EasyBookmark
{
	public class BookmarkItem : VisualElement
	{
		public static readonly int Height = 22;

		static readonly string TopBorderClass = "border-top";
		static readonly string BottomBorderClass = "border-bottom";
		static readonly string NotExistsClass = "not-exists";

		public string AssetPath { get; private set; }
		public string Guid { get; private set; }
		public AssetCategory Category { get; private set; }

		readonly VisualElement root;
		readonly VisualElement texture;
		readonly Label pathLabel;
		readonly Button openButton;
		readonly Button selectButton;
		readonly Button deleteButton;

		ITestElementCallbackReceiver callbackReceiver;
		LinePosition linePosition;
		bool hasEntered;
		Vector2 mouseDownPosition;

		public BookmarkItem()
		{
			var treeStorage = VisualTreeStorage.Load();
			var container = treeStorage.BookmarkItem.Instantiate();
			hierarchy.Add(container);

			root = container.Q<VisualElement>("root");

			texture = container.Q<VisualElement>("texture");

			pathLabel = container.Q<Label>("path");

			openButton = container.Q<Button>("openButton");
			openButton.clicked += OnOpenButton;

			selectButton = container.Q<Button>("selectButton");
			selectButton.clicked += OnSelectButton;

			deleteButton = container.Q<Button>("deleteButton");
			deleteButton.clicked += OnDeleteButton;

			RegisterCallback<PointerDownEvent>(OnPointerDown);
			RegisterCallback<PointerMoveEvent>(OnPointerMove);
			RegisterCallback<PointerUpEvent>(OnPointerUp);
			RegisterCallback<DragEnterEvent>(OnDragEnter);
			RegisterCallback<DragLeaveEvent>(OnDragLeave);
			RegisterCallback<DragPerformEvent>(OnDragPerform);
			RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
		}

		public void Initialize(string guid, string assetPath, AssetCategory category, ITestElementCallbackReceiver callbackReceiver)
		{
			Guid = guid;
			AssetPath = assetPath;
			Category = category;
			this.callbackReceiver = callbackReceiver;

			if (category == AssetCategory.NotExists)
			{
				pathLabel.text = AssetPath + " <color=yellow>(DELETED)</color>";
				pathLabel.AddToClassList(NotExistsClass);
			}
			else
			{
				pathLabel.text = AssetPath;
			}

			openButton.SetEnabled(category == AssetCategory.Scene || category == AssetCategory.Prefab);
			selectButton.SetEnabled(category != AssetCategory.NotExists);
			texture.style.backgroundImage = new StyleBackground(AssetDatabase.GetCachedIcon(AssetPath) as Texture2D);
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
				hasEntered = true;
			}
		}

		void OnDragUpdate(DragUpdatedEvent e)
		{
			if (hasEntered)
			{
				linePosition = e.localMousePosition.y > Height / 2.0f ? LinePosition.Bottom : LinePosition.Top;

				if (e.mousePosition != mouseDownPosition)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Move;
					EnableLine(linePosition);
				}

				e.StopImmediatePropagation();
			}
			else
			{
				linePosition = LinePosition.None;
				DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
			}
		}

		void OnDragPerform(DragPerformEvent e)
		{
			if (hasEntered)
			{
				callbackReceiver.OnDragPerform(this, linePosition);
				linePosition = LinePosition.None;
				DisableLine();
				hasEntered = false;

				DragAndDrop.paths = null;
				DragAndDrop.objectReferences = null;
			}
		}

		void OnPointerDown(PointerDownEvent e)
		{
			DragAndDrop.paths = Array.Empty<string>();
			DragAndDrop.objectReferences = Array.Empty<Object>();
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.SetGenericData(nameof(BookmarkItem), this);
			DragAndDrop.StartDrag(nameof(BookmarkItem));

			callbackReceiver.OnDragStart(this);
			mouseDownPosition = e.originalMousePosition;
			hasEntered = true;
		}

		void OnPointerUp(PointerUpEvent evt)
		{
		}

		void OnPointerMove(PointerMoveEvent e)
		{
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