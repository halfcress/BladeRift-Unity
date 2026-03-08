using System;
using System.IO;
using System.Text;
using UnityEngine;

// -----------------------------
// DEBUG JOURNAL
// -----------------------------

public static class ProjectStateJournal
{
    public static void EnsureDebugJournalExists()
    {
        if (!File.Exists(ProjectStatePaths.DebugJournalPath))
        {
            File.WriteAllText(ProjectStatePaths.DebugJournalPath,
@"# DEBUG_JOURNAL

> Bu dosya debug denemelerini, basarisiz yollari ve cikarimlari tutar.
> Amac: Ayni seyi tekrar tekrar denememek.

", Encoding.UTF8);
        }
    }

    public static void AppendManualEntry()
    {
        string latestDebug   = ProjectStateSerializer.GetLatestSnapshotFile(ProjectStatePaths.DebugRoot);
        string latestWorking = ProjectStateSerializer.GetLatestSnapshotFile(ProjectStatePaths.WorkingRoot);

        string headCommitShort   = ProjectStateGit.SafeGit("rev-parse --short HEAD");
        string headCommitMessage = ProjectStateGit.SafeGit("log -1 --pretty=%s");

        string entry =
$@" 

## {DateTime.Now:yyyy-MM-dd HH:mm:ss} - Debug Session

### Problem
- [Buraya sorunu yaz]

### Context
- Latest DEBUG snapshot: {(string.IsNullOrEmpty(latestDebug) ? "none" : Path.GetFileName(latestDebug))}
- Latest WORKING snapshot: {(string.IsNullOrEmpty(latestWorking) ? "none" : Path.GetFileName(latestWorking))}
- Latest commit: {(string.IsNullOrEmpty(headCommitShort) ? "unknown" : headCommitShort)}
- Commit message: {(string.IsNullOrEmpty(headCommitMessage) ? "unknown" : headCommitMessage)}

### Attempts
- [ ] Denenen sey 1
- [ ] Denenen sey 2

### Result
- [ ] Fail
- [ ] Partial
- [ ] Success

### Decision
- [ ] Sonraki adim
";

        File.AppendAllText(ProjectStatePaths.DebugJournalPath, entry, Encoding.UTF8);
    }

    public static void AppendAutomaticDebugJournalEntry(string snapshotPath, FullSnapshot full)
    {
        EnsureDebugJournalExists();

        string latestWorking = ProjectStateSerializer.GetLatestSnapshotFile(ProjectStatePaths.WorkingRoot);

        string entry =
$@" 

## {DateTime.Now:yyyy-MM-dd HH:mm:ss} - Auto DEBUG Snapshot

### Problem
- [Bu snapshot neden alindi, sonra doldur]

### Context
- Snapshot file: {Path.GetFileName(snapshotPath)}
- Scene: {full.meta.activeSceneName}
- Root object count: {full.meta.rootObjectCount}
- Total object count: {full.meta.totalGameObjectCount}
- Latest commit: {(string.IsNullOrEmpty(full.meta.headCommitShort) ? "unknown" : full.meta.headCommitShort)}
- Commit message: {(string.IsNullOrEmpty(full.meta.headCommitMessage) ? "unknown" : full.meta.headCommitMessage)}
- Latest WORKING snapshot: {(string.IsNullOrEmpty(latestWorking) ? "none" : Path.GetFileName(latestWorking))}

### Attempts
- [ ] Denenen sey 1

### Result
- [ ] Fail
- [ ] Partial
- [ ] Success

### Decision
- [ ] Sonraki adim
";

        File.AppendAllText(ProjectStatePaths.DebugJournalPath, entry, Encoding.UTF8);
    }
}
