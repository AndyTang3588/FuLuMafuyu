using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using System.IO;

public class hdiffMenu : EditorWindow
{
    private const string Name = "FacialTracking Patch";
    private string OLDPATH;
    private string PATPATH;
    private string processOutput = "";
    private void OnEnable() 
    {
        this.minSize = new Vector2(800, 500);
        this.maxSize = new Vector2(800, 500);
        OLDPATH = System.IO.Path.Combine(Application.dataPath, "PLUSONE\\Mafuyu\\FBX\\Mafuyu.fbx");
        PATPATH = System.IO.Path.Combine(Application.dataPath, "FacialTracking\\FacialPatch\\_hdiff\\Mafuyu_FT.hdiff");

        // hdiffReadme.txt 파일의 내용을 초기로 읽어 processOutput에 할당
        string textFilePath = System.IO.Path.Combine(Application.dataPath, "FacialTracking\\FacialPatch\\Readme.txt");
        if (File.Exists(textFilePath))
            processOutput = File.ReadAllText(textFilePath);
        else
            processOutput = "";
    }

    [MenuItem("FT Patch/" + Name)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<hdiffMenu>("GUI Hdiff " + Name);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Start"))
        {
            string batFileFullPath = System.IO.Path.Combine(Application.dataPath, "FacialTracking\\FacialPatch\\hpatchz.exe");
            BackupFile(OLDPATH);
            RunBatFile(batFileFullPath, OLDPATH, PATPATH);
            AssetDatabase.Refresh();
        }

        EditorGUILayout.TextArea(processOutput, GUILayout.ExpandHeight(true));
    }

    void RunBatFile(string filePath, string oldPath, string patPath)
    {
        processOutput = "";
        processOutput += "9B0N_FBX_HPATCHZ_IMM\n";
        processOutput += "v0.4\n";
        processOutput += "https://github.com/sisong/HDiffPatch\n";
        processOutput += "https://github.com/BonhyeonGu\n";
        Process process = new Process();
        string arguments = string.Format("-f \"{0}\" \"{1}\" \"{0}\"", oldPath, patPath);
        process.StartInfo.FileName = filePath;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;

        process.Start();
        processOutput += process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        processOutput += "process completion =>" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
    }

    void BackupFile(string path)
    {
        if (System.IO.File.Exists(path))
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
            string extension = System.IO.Path.GetExtension(path);
            
            string backupFileName = string.Format("{0}_{1}{2}", 
                                                fileNameWithoutExtension, 
                                                DateTime.Now.ToString("yyyyMMdd_HHmmss"), 
                                                extension);

            string backupPath = System.IO.Path.Combine(directory, backupFileName);

            System.IO.File.Copy(path, backupPath);
        }
    }
}
