using System;
using System.Collections;
using UnityEngine;

namespace Fsp.FrameCapaturerExtxension
{
	[RequireComponent(typeof(Camera))]
	internal sealed class MovieEventHook : MonoBehaviour
	{
		#region Fields

		private CameraType type;

		private bool skipFrame;

		#endregion

		#region Events

		public event Action EndOfFrame;

		public event Action<float[], int> AudioFilterRead;

		#endregion

		#region Messages

		private void Awake()
		{
			Camera camera = GetComponent<Camera>();

			this.type = camera.cameraType;
			this.skipFrame = true;
			this.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;
		}

		private void OnPostRender()
		{
			if (type == CameraType.Game)
			{
				StartCoroutine(Wait());
			}
			else
			{
				if (skipFrame)
				{
					skipFrame = false;
				}
				else
				{
					OnEndOfFrame();
				}
			}
		}

		private IEnumerator Wait()
		{
			yield return new WaitForEndOfFrame();

			OnEndOfFrame();
		}

		private void OnEndOfFrame()
		{
			if (EndOfFrame != null) EndOfFrame();
		}

		private void OnAudioFilterRead(float[] samples, int channels)
		{
			if (AudioFilterRead != null) AudioFilterRead(samples, channels);
		}

		#endregion
	}
}
