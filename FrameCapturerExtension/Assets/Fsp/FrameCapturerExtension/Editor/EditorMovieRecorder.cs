using System;
using UnityEngine;
using UTJ;

using UnityObject = UnityEngine.Object;

namespace Fsp.FrameCapaturerExtxension
{
	public abstract class EditorMovieRecorder<T> : IEditorMovieRecorder<T> where T : IMovieEncoder
	{
		#region Constants

		public const int DefaultMinUpdateRate = 30;

		public const int MinMinUpdateRate = 1;

		public const int MaxMinUpdateRate = 60;

		#endregion

		#region Fields

		private readonly IMovieRecordingUnit<T> unit;

		private int minUpdateRate = DefaultMinUpdateRate;

		private int frameCounter;

		private int storedVSyncCount;

		private int storedTargetFrameRate;

		private MovieEventHook hook;

		private bool disposed;

		#endregion

		#region Properties

		public IMovieRecordingUnit<T> RecordingUnit { get { return unit; } }

		public T Encoder { get { return unit.Encoder; } }

		public abstract EncoderType EncoderType { get; }

		public Camera Camera
		{
			get { return unit.Camera; }
			set { unit.Camera = value; }
		}

		public bool Seekable { get { return unit.Encoder.Seekable; } }

		public bool Editable { get { return unit.Encoder.Editable; } }

		public bool Recording { get { return unit.Recording; } }

		public int FrameCount { get { return unit.FrameCount; } }

		public int MinUpdateRate
		{
			get { return minUpdateRate; }
			set { minUpdateRate = Math.Max(MinMinUpdateRate, Math.Min(MaxMinUpdateRate, value)); }
		}

		IMovieRecordingUnit IEditorMovieRecorder.RecordingUnit { get { return RecordingUnit; } }

		IMovieEncoder IEditorMovieRecorder.Encoder { get { return Encoder; } }

		private int FrameInterval
		{
			get
			{
				int frameRate = unit.Encoder.Settings.FrameRate;
				int interval = minUpdateRate / frameRate;
				if (frameRate * interval != minUpdateRate) interval++;

				return interval;
			}
		}

		private int UpdateRate
		{
			get
			{
				int frameRate = unit.Encoder.Settings.FrameRate;
				int interval = minUpdateRate / frameRate;
				int updateRate = frameRate * interval;
				if (updateRate < minUpdateRate) updateRate += frameRate;

				return updateRate;
			}
		}

		#endregion

		#region Constructors

		public EditorMovieRecorder(IMovieRecordingUnit<T> unit)
		{
			this.unit = unit;
		}

		~EditorMovieRecorder()
		{
			if (!disposed)
			{
				Dispose(false);

				disposed = true;
			}
		}

		#endregion

		#region Methods

		public void Reset()
		{
			unit.Encoder.Reset();
		}

		public bool BeginRecording()
		{
			if (unit.Recording || !unit.Camera) return false;

			ApplySettings();

			unit.BeginRecording();

			AttachHook();

			return true;
		}

		public bool EndRecording()
		{
			if (!unit.Recording) return false;

			DetachHook();

			unit.EndRecording();

			RestoreSettings();

			return true;
		}

		public bool Save(string path, int beginFrame = 0, int endFrame = -1)
		{
			T encoder = unit.Encoder;
			if (encoder.Seekable)
			{
				ISeekableMovieEncoder seekable = (ISeekableMovieEncoder)encoder;

				return seekable.Flush(path, beginFrame, endFrame);
			}
			else
			{
				return encoder.Flush(path);
			}
		}

		public int GetExpectedFileSize(int beginFrame = 0, int endFrame = -1)
		{
			if (!unit.Encoder.Seekable) throw new NotSupportedException();

			ISeekableMovieEncoder seekable = (ISeekableMovieEncoder)unit.Encoder;

			return seekable.GetExpectedFileSize(beginFrame, endFrame);
		}

		public void GetFrameData(RenderTexture texture, int frame)
		{
			if (!unit.Encoder.Seekable) throw new NotSupportedException();

			ISeekableMovieEncoder seekable = (ISeekableMovieEncoder)unit.Encoder;

			seekable.GetFrameData(texture, frame);
		}

		public void EraseFrame(int beginFrame = 0, int endFrame = -1)
		{
			if (!unit.Encoder.Editable) throw new NotSupportedException();

			IEditableMovieEncoder editable = (IEditableMovieEncoder)unit.Encoder;

			editable.EraseFrame(beginFrame, endFrame);
		}

		public void Dispose()
		{
			if (!disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);

				disposed = true;
			}
		}

		void IEditorMovieRecorder.UpdateSettings(IMovieRecorderSettings source, Camera camera)
		{
			UpdateSettings(source, camera);
		}

		protected virtual void UpdateSettings(IMovieRecorderSettings source, Camera camera)
		{
			MovieEncoderSettings settings = Encoder.Settings;

			switch (source.ResolutionMode)
			{
				case MovieResolutionMode.Custom:
					settings.ResolutionWidth = source.ResolutionWidth;
					settings.ResolutionHeight = source.ResolutionHeight;
					break;
				case MovieResolutionMode.Width:
					settings.ResolutionWidth = source.ResolutionWidth;
					settings.ResolutionHeight = Mathf.RoundToInt(source.ResolutionWidth / camera.aspect);
					break;
				case MovieResolutionMode.Height:
					settings.ResolutionWidth = Mathf.RoundToInt(source.ResolutionHeight * camera.aspect);
					settings.ResolutionHeight = source.ResolutionHeight;
					break;
				case MovieResolutionMode.Camera:
					settings.ResolutionWidth = camera.pixelWidth;
					settings.ResolutionHeight = camera.pixelHeight;
					break;
				default:
					throw new InvalidOperationException();
			}

			settings.FrameRateMode = source.FrameRateMode;
			settings.FrameRate = source.FrameRate;

			MinUpdateRate = source.MinUpdateRate;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				DetachHook();

				unit.Dispose();
			}
		}

		private void ApplySettings()
		{
			frameCounter = 0;
			storedVSyncCount = QualitySettings.vSyncCount;
			storedTargetFrameRate = Application.targetFrameRate;

			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = UpdateRate;
		}

		private void RestoreSettings()
		{
			QualitySettings.vSyncCount = storedVSyncCount;
			Application.targetFrameRate = storedTargetFrameRate;
		}

		private void AttachHook()
		{
			Camera camera = unit.Camera;
			if (camera)
			{
				hook = camera.gameObject.AddComponent<MovieEventHook>();

				if (unit.Encoder.CaptureVideo) hook.EndOfFrame += OnEndOfFrame;
				if (unit.Encoder.CaptureAudio) hook.AudioFilterRead += OnAudioFilterRead;
			}
		}

		private void DetachHook()
		{
			if (hook)
			{
				hook.EndOfFrame -= OnEndOfFrame;
				hook.AudioFilterRead -= OnAudioFilterRead;

				UnityObject.DestroyImmediate(hook);

				hook = null;
			}
		}

		private void OnEndOfFrame()
		{
			if (unit.Recording)
			{
				if (frameCounter == 0)
				{
					unit.RecordImage(Time.unscaledTime);
				}

				frameCounter = (frameCounter + 1) % FrameInterval;
			}
		}

		private void OnAudioFilterRead(float[] samples, int channels)
		{
			if (unit.Recording)
			{
				unit.RecordAudio(samples, channels);
			}
		}

		#endregion
	}
}
