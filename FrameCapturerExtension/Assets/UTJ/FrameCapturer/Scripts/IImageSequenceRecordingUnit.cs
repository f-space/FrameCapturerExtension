using System;
using UnityEngine;

namespace UTJ
{
	public interface IImageSequenceRecordingUnit : IDisposable
	{
		#region Properties

		IImageSequenceEncoder Encoder { get; }

		Type EncoderType { get; }

		bool Recording { get; }

		Camera Camera { get; set; }

		Shader CopyShader { get; set; }

		#endregion

		#region Methods

		void BeginRecording();

		void EndRecording();

		void Export(string path, int number);

		void ReleaseResources();

		#endregion
	}

	public interface IImageSequenceRecordingUnit<out T> : IImageSequenceRecordingUnit where T : IImageSequenceEncoder
	{
		#region Properties

		new T Encoder { get; }

		#endregion
	}
}
