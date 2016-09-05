using UnityEngine;

namespace UTJ
{
	[RequireComponent(typeof(Camera))]
	public abstract class MovieRecorder<T> : VideoRecorder<T> where T : IMovieEncoder
	{
		#region Messages

		protected void OnAudioFilterRead(float[] samples, int channels)
		{
			if (RecordingUnit.Recording)
			{
				RecordingUnit.RecordAudio(samples, channels);
			}
		}

		#endregion
	}
}
