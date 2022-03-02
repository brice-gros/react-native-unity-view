using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;

public class Build : MonoBehaviour {
    static readonly string ProjectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    static readonly string apkPath = Path.Combine(ProjectPath, "Builds/" + Application.productName + ".apk");

    private static readonly string androidLocalPropertiesPath =
        Path.GetFullPath(Path.Combine(ProjectPath, "../../android/local.properties"));
    private static readonly string androidExportPath =
        Path.GetFullPath(Path.Combine(ProjectPath, "../../android/unityLibrary"));

    private static readonly string iosExportPath =
        Path.GetFullPath(Path.Combine(ProjectPath, "../../ios/unityLibrary"));

    [MenuItem("ReactNative/Export Android (Unity 2020.3.*) %&n", false, 1)]
    public static void DoBuildAndroidLibrary() {
        DoBuildAndroid(Path.Combine(apkPath, "unityLibrary"));
        if (File.Exists(androidLocalPropertiesPath)) {
          Debug.Log($"local.properties already exists and won't be copied: {androidLocalPropertiesPath}");
        } else {
          File.Copy(Path.Combine(apkPath, "local.properties"), androidLocalPropertiesPath);
        }
        Copy(Path.Combine(apkPath, "launcher/src/main/res"), Path.Combine(androidExportPath, "src/main/res"));
    }

    public static BuildOptions GetBuildOptions() {
        var options = BuildOptions.AcceptExternalModificationsToPlayer;
        // Enable debugging from the Build Settings UI check boxes
        if (UnityEditor.EditorUserBuildSettings.development) {
          options |= BuildOptions.Development;
        }
        if (UnityEditor.EditorUserBuildSettings.allowDebugging) {
          options |= BuildOptions.AllowDebugging;
        }
        return options;
    }

    public static void DoBuildAndroid(String buildPath) {
        if (Directory.Exists(apkPath)) {
            Directory.Delete(apkPath, true);
        }
        if (Directory.Exists(androidExportPath)) {
            Directory.Delete(androidExportPath, true);
        }
        Directory.CreateDirectory(apkPath);

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        var report = BuildPipeline.BuildPlayer(
            GetEnabledScenes(),
            apkPath,
            BuildTarget.Android,
            GetBuildOptions()
        );

        if (report.summary.result != BuildResult.Succeeded) {
            throw new Exception("Build failed");
        }

        Copy(buildPath, androidExportPath);
        // Modify build.gradle
        var build_file = Path.Combine(androidExportPath, "build.gradle");
        var build_text = File.ReadAllText(build_file);
        build_text = build_text.Replace("com.android.application", "com.android.library");
        build_text = build_text.Replace("bundle {", "splits {");
        build_text = build_text.Replace("enableSplit = false", "enable false");
        build_text = build_text.Replace("enableSplit = true", "enable true");
        build_text = build_text.Replace("implementation fileTree(dir: 'libs', include: ['*.jar'])", "implementation ':unity-classes'");
        // build_text = Regex.Replace(build_text, @"\n.*applicationId '.+'.*\n", "\n");
        File.WriteAllText(build_file, build_text);

        // Modify AndroidManifest.xml
        var manifest_file = Path.Combine(androidExportPath, "src/main/AndroidManifest.xml");
        var manifest_text = File.ReadAllText(manifest_file);
        manifest_text = Regex.Replace(manifest_text, @"<application .*>", "<application>");
        Regex regex = new Regex(@"<activity.*>(\s|\S)+?</activity>", RegexOptions.Multiline);
        manifest_text = regex.Replace(manifest_text, "");
        File.WriteAllText(manifest_file, manifest_text);
    }

    // TODO: ios NOT ported yet
    //[MenuItem("ReactNative/Export IOS (Unity 2020.3.*) %&i", false, 3)]
    public static void DoBuildIOS() {
        if (Directory.Exists(iosExportPath)) {
            Directory.Delete(iosExportPath, true);
        }

        EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release;

        var report = BuildPipeline.BuildPlayer(
            GetEnabledScenes(),
            iosExportPath,
            BuildTarget.iOS,
            GetBuildOptions()
        );

        if (report.summary.result != BuildResult.Succeeded) {
            throw new Exception("Build failed");
        }
    }

    static void Copy(string source, string destinationPath) {
        if (Directory.Exists(destinationPath))
            Directory.Delete(destinationPath, true);

        Directory.CreateDirectory(destinationPath);

        foreach (string dirPath in Directory.GetDirectories(source, "*",
            SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(source, destinationPath));

        foreach (string newPath in Directory.GetFiles(source, "*.*",
            SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(source, destinationPath), true);
    }

    static string[] GetEnabledScenes() {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        return scenes;
    }
}
