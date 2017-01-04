using System.IO;
using UnityEditor;
using UnityEngine;

public static class PackageExporter
{
	#region Constants

	private const string MenuPath = "Assets/Export FrameCapturer";

	private const string FrameCapturerPath = "Assets/UTJ";

	private const string FrameCapturerExtensionPath = "Assets/Fsp";

	private const string StreamingAssetPath = "Assets/StreamingAssets/UTJ/FrameCapturer";

	private const string LicensePath = StreamingAssetPath + "/License.txt";

	private const string OpenH264LicensePath = StreamingAssetPath + "/OpenH264_BinaryLicense.txt";

	private const string FAACSelfBuildPath = StreamingAssetPath + "/FAAC_SelfBuild.zip";

	private const string OutputPath = "../../Packages";

	private const string Extension = ".unitypackage";

	private const string FileName_FrameCapturer = "FrameCapturer" + Extension;

	private const string FileName_FrameCapturerExtension = "FrameCapturerExtension" + Extension;

	private const string FileName_FAACSelfBuild = "FAACSelfBuild" + Extension;

	private const string FileName_Full = "Full" + Extension;

	private const ExportPackageOptions Options = ExportPackageOptions.Interactive | ExportPackageOptions.Recurse;

	#endregion

	#region Methods

	[MenuItem(MenuPath + "/All")]
	public static void ExportAll()
	{
		ExportFrameCapturer();
		ExportFrameCapturerExtension();
		ExportFAACSelfBuild();
		ExportFull();
	}

	[MenuItem(MenuPath + "/FrameCapturer")]
	public static void ExportFrameCapturer()
	{
		string[] assetPaths =
		{
			FrameCapturerPath,
			LicensePath,
			OpenH264LicensePath,
		};
		string fileName = GetOutputPath(FileName_FrameCapturer);

		AssetDatabase.ExportPackage(assetPaths, fileName, Options);
	}

	[MenuItem(MenuPath + "/FrameCapturerExtension")]
	public static void ExportFrameCapturerExtension()
	{
		string assetPath = FrameCapturerExtensionPath;
		string fileName = GetOutputPath(FileName_FrameCapturerExtension);

		AssetDatabase.ExportPackage(assetPath, fileName, Options);
	}

	[MenuItem(MenuPath + "/FAACSelfBuild")]
	public static void ExportFAACSelfBuild()
	{
		string assetPath = FAACSelfBuildPath;
		string fileName = GetOutputPath(FileName_FAACSelfBuild);

		AssetDatabase.ExportPackage(assetPath, fileName, Options);
	}

	[MenuItem(MenuPath + "/Full")]
	public static void ExportFull()
	{
		string[] assetPaths =
		{
			FrameCapturerPath,
			FrameCapturerExtensionPath,
			LicensePath,
			OpenH264LicensePath,
			FAACSelfBuildPath,
		};
		string fileName = GetOutputPath(FileName_Full);

		AssetDatabase.ExportPackage(assetPaths, fileName, Options);
	}

	private static string GetOutputPath(string name)
	{
		string path = Path.GetFullPath(Path.Combine(Application.dataPath, OutputPath + "/" + name));

		Directory.CreateDirectory(Path.GetDirectoryName(path));

		return path;
	}

	#endregion
}
