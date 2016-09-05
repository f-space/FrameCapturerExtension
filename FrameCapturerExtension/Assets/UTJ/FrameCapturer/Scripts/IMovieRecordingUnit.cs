using System;
using UnityEngine;

namespace UTJ
{
	public interface IMovieRecordingUnit : IDisposable
	{
		#region Properties

		IMovieEncoder Encoder { get; }

		Type EncoderType { get; }

		bool Recording { get; }

		int FrameCount { get; }

		Camera Camera { get; set; }

		Shader CopyShader { get; set; }

		RenderTexture ScratchBuffer { get; }

		#endregion

		#region Methods

		void BeginRecording();

		void EndRecording();

		void RecordImage(double time);

		void RecordAudio(float[] samples, int channels);

		void ReleaseResources();

		#endregion
	}

	public interface IMovieRecordingUnit<out T> : IMovieRecordingUnit where T : IMovieEncoder
	{
		#region Properties

		new T Encoder { get; }

		#endregion
	}
}
