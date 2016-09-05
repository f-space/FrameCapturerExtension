using System;
using UnityEngine;

namespace UTJ
{
	public interface IMovieRecorder
	{
		#region Properties

		IMovieRecordingUnit RecordingUnit { get; }

		IMovieEncoder Encoder { get; }

		Type EncoderType { get; }

		bool Seekable { get; }

		bool Editable { get; }

		bool Recording { get; }

		int FrameCount { get; }

		#endregion

		#region Methods

		void Clear();

		bool BeginRecording();

		bool EndRecording();

		bool Save(out string path, int beginFrame = 0, int endFrame = -1);

		int GetExpectedFileSize(int beginFrame = 0, int endFrame = -1);

		void GetFrameData(RenderTexture texture, int frame);

		void EraseFrame(int beginFrame = 0, int endFrame = -1);

		#endregion
	}

	public interface IMovieRecorder<out T> : IMovieRecorder where T : IMovieEncoder
	{
		#region Properties

		new IMovieRecordingUnit<T> RecordingUnit { get; }

		new T Encoder { get; }

		#endregion
	}
}
