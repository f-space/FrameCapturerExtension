using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace UTJ
{
	public abstract class RecordingUnitBase<T> : IDisposable where T : IDisposable
	{
		#region Constants

		private const string OffscreenShaderKeyword = "OFFSCREEN";

		#endregion

		#region Fields

		protected readonly T encoder;

		private bool autoDisposeEncoder;

		private Shader copyShader;

		private Material copyMaterial;

		private Mesh quad;

		private bool dirty;

		private bool disposed;

		#endregion

		#region Properties

		public T Encoder { get { return encoder; } }

		public bool AutoDisposeEncoder
		{
			get { return autoDisposeEncoder; }
			set { autoDisposeEncoder = value; }
		}

		public Shader CopyShader
		{
			get { return copyShader; }
			set
			{
				if (copyShader != value)
				{
					copyShader = value;

					dirty = true;
				}
			}
		}

		protected Material CopyMaterial { get { return copyMaterial; } }

		protected Mesh QuadMesh { get { return quad; } }

		#endregion

		#region Constructors

		public RecordingUnitBase(T encoder, bool autoDisposeEncoder)
		{
			if (encoder == null) throw new ArgumentNullException("encoder");

			this.encoder = encoder;
			this.autoDisposeEncoder = autoDisposeEncoder;
		}

		~RecordingUnitBase()
		{
			if (!disposed)
			{
				Dispose(false);

				disposed = true;
			}
		}

		#endregion

		#region Methods

		public virtual void ReleaseResources()
		{
			DisposalHelper.Dispose(ref quad);
			DisposalHelper.Dispose(ref copyMaterial);
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

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseResources();

				if (autoDisposeEncoder)
				{
					encoder.Dispose();
				}
			}
		}

		protected void CreateCopyMaterial(bool offscreen)
		{
			if (!copyMaterial || dirty)
			{
				if (copyMaterial) UnityObject.Destroy(copyMaterial);

				Shader shader = copyShader ? copyShader : LoadDefaultShader();
				Material material = new Material(shader);

				copyMaterial = DisposalHelper.Mark(material);

				dirty = false;
			}

			if (offscreen)
			{
				copyMaterial.EnableKeyword(OffscreenShaderKeyword);
			}
			else
			{
				copyMaterial.DisableKeyword(OffscreenShaderKeyword);
			}
		}

		protected void CreateQuadMesh()
		{
			if (!quad)
			{
				quad = DisposalHelper.Mark(ResourceHelper.CreateFullscreenQuad());
			}
		}

		private static Shader LoadDefaultShader()
		{
			return ResourceHelper.LoadCopyShader();
		}

		#endregion
	}
}
