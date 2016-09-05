using System;
using UnityEngine;

namespace UTJ
{
	public interface IMovieEncoder : IDisposable
	{
		#region Properties

		MovieEncoderSettings Settings { get; }

		bool Seekable { get; }

		bool Editable { get; }

		bool CaptureVideo { get; }

		bool CaptureAudio { get; }

		bool Initialized { get; }

		bool Recording { get; }

		int FrameCount { get; }

		string Extension { get; }

		#endregion

		#region Methods

		void Initialize();

		void Reset();

		void BeginRecording();

		void EndRecording();

		void RecordImage(RenderTexture texture, double time);

		void RecordAudio(float[] samples, int channels);

		bool Flush(string path);

		#endregion
	}

	public interface ISeekableMovieEncoder : IMovieEncoder
	{
		#region Methods

		bool Flush(string path, int beginFrame = 0, int endFrame = -1);

		int GetExpectedFileSize(int beginFrame = 0, int endFrame = -1);

		void GetFrameData(RenderTexture texture, int frame);

		#endregion
	}

	public interface IEditableMovieEncoder : ISeekableMovieEncoder
	{
		#region Methods

		void EraseFrame(int beginFrame = 0, int endFrame = -1);

		#endregion
	}


}
