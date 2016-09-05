using System;
using UnityEngine;
using UTJ;

namespace Fsp.FrameCapaturerExtxension
{
	public interface IEditorMovieRecorder : IDisposable
	{
		#region Properties

		IMovieRecordingUnit RecordingUnit { get; }

		IMovieEncoder Encoder { get; }

		EncoderType EncoderType { get; }

		Camera Camera { get; set; }

		int MinUpdateRate { get; set; }

		bool Seekable { get; }

		bool Editable { get; }

		bool Recording { get; }

		int FrameCount { get; }

		#endregion

		#region Methods

		void UpdateSettings(IMovieRecorderSettings source, Camera camera);

		void Reset();

		bool BeginRecording();

		bool EndRecording();

		bool Save(string path, int beginFrame = 0, int endFrame = -1);

		int GetExpectedFileSize(int beginFrame = 0, int endFrame = -1);

		void GetFrameData(RenderTexture texture, int frame);

		void EraseFrame(int beginFrame = 0, int endFrame = -1);

		#endregion
	}

	public interface IEditorMovieRecorder<out T> : IEditorMovieRecorder where T : IMovieEncoder
	{
		#region Properties

		new IMovieRecordingUnit<T> RecordingUnit { get; }

		new T Encoder { get; }

		#endregion
	}
}
