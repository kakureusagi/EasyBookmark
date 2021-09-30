namespace EasyBookmark
{
	public interface ITestElementCallbackReceiver
	{
		void OnSelectButton(BookmarkItem item);
		void OnOpenButton(BookmarkItem item);
		void OnDeleteButton(BookmarkItem item);

		void OnDragStart(BookmarkItem item);
		void OnDragMove(BookmarkItem item);
		void OnDragPerform(BookmarkItem item, LinePosition linePosition);
	}
}