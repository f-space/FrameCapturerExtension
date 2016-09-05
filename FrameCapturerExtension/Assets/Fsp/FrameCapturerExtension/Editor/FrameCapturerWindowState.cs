using UnityEditor;
using UnityEngine;

namespace Fsp.FrameCapaturerExtxension
{
	internal class FrameCapturerWindowState : ScriptableObject
	{
		#region Fields

#pragma warning disable 0414

		[SerializeField]
		private Camera m_TargetCamera;

#pragma warning restore 0414

		private SerializedObject @this;

		private SerializedProperty propTargetCamera;

		#endregion

		#region Properties

		public SerializedObject SerializedObject { get { return @this; } }

		public SerializedProperty TargetCameraProperty { get { return propTargetCamera; } }

		public Camera TargetCamera
		{
			get { return propTargetCamera.objectReferenceValue as Camera; }
			set { propTargetCamera.objectReferenceValue = value; }
		}

		#endregion

		#region Messages

		protected void OnEnable()
		{
			@this = new SerializedObject(this);

			propTargetCamera = @this.FindProperty("m_TargetCamera");

			hideFlags = HideFlags.DontSave;
		}

		protected void OnDisable()
		{
			@this = null;

			propTargetCamera = null;
		}

		#endregion
	}
}
