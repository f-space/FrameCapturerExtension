using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UTJ
{
	public static class DisposalHelper
	{
		#region Methods

		public static T Mark<T>(T @object) where T : UnityObject
		{
			@object.hideFlags = HideFlags.DontSave;

			return @object;
		}

		public static void Dispose<T>(ref T @object) where T : UnityObject
		{
			if (@object) UnityObject.DestroyImmediate(@object);

			@object = null;
		}

		public static void Dispose(ref RenderTexture @object)
		{
			if (@object)
			{
				@object.Release();

				UnityObject.DestroyImmediate(@object);
			}

			@object = null;
		}

		#endregion
	}
}
