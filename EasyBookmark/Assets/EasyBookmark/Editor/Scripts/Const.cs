using UnityEngine;

namespace EasyBookmark
{
	public static class Const
	{
		public static string SavePath => $"{Application.persistentDataPath}/{nameof(EasyBookmark)}/save.json";
	}
}