using UnityEngine;

namespace UTJ
{
	public static class ResourceHelper
	{
		#region Constants

		private const string CopyShaderName = "UTJ/FrameCapturer/CopyFrameBuffer";

		#endregion

		#region Methods

		public static Shader LoadCopyShader()
		{
			return Shader.Find(CopyShaderName);
		}

		public static Mesh CreateFullscreenQuad()
		{
			Vector3[] vertices =
			{
				new Vector3( 1.0f, 1.0f, 0.0f),
				new Vector3(-1.0f, 1.0f, 0.0f),
				new Vector3(-1.0f,-1.0f, 0.0f),
				new Vector3( 1.0f,-1.0f, 0.0f),
			};
			int[] indices = { 0, 1, 2, 2, 3, 0 };

			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = indices;

			return mesh;
		}

		#endregion
	}
}
