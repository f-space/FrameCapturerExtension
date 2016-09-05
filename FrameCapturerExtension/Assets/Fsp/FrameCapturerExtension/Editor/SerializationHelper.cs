using UnityEditor;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Fsp.FrameCapaturerExtxension
{
	internal static class SerializationHelper
	{
		#region Methods

		public static bool Load<T>(out T instance, string key) where T : ScriptableObject
		{
			instance = ScriptableObject.CreateInstance<T>();
			instance.hideFlags = HideFlags.DontSave;

			if (EditorPrefs.HasKey(key))
			{
				string json = EditorPrefs.GetString(key);
				JsonUtility.FromJsonOverwrite(json, instance);

				return true;
			}

			return false;
		}

		public static bool Save<T>(ref T instance, string key, bool dispose) where T : ScriptableObject
		{
			if (instance)
			{
				string json = JsonUtility.ToJson(instance);
				EditorPrefs.SetString(key, json);

				if (dispose)
				{
					UnityObject.DestroyImmediate(instance);
					instance = null;
				}

				return true;
			}

			return false;
		}

		#endregion
	}
}
