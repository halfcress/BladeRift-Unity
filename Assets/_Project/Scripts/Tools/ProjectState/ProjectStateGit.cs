using System.IO;
using UnityEngine;

// -----------------------------
// GIT HELPERS
// -----------------------------

public static class ProjectStateGit
{
    public static string SafeGit(string args)
    {
        try
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = args;
                process.StartInfo.WorkingDirectory = projectRoot;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.StandardError.ReadToEnd();
                process.WaitForExit();

                return process.ExitCode == 0 ? output : "";
            }
        }
        catch
        {
            return "";
        }
    }
}
