using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;

namespace MonoGameLibrary.Content;

public static class ContentManagerExtensions
{
    /// <summary>
    /// Check if the given <paramref name="watchedAsset"/> xnb file has a newer write-time than the last loaded version of the asset.
    /// If the local file has been updated, reload the asset and return true.
    /// </summary>
    /// <param name="manager">The <see cref="ContentManager"/> that loaded the asset originally</param>
    /// <param name="watchedAsset">The asset that will be reloaded if the xnb file is newer </param>
    /// <param name="oldAsset">If the asset has been reloaded, this out parameter will be set to the previous version of the asset before the newer version was loaded. </param>
    /// <typeparam name="T"></typeparam>
    /// <returns>true when asset was reloaded; false otherwise. </returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool TryRefresh<T>(this ContentManager manager, WatchedAsset<T> watchedAsset, out T oldAsset)
    {
        oldAsset = default;
        
        if (manager != watchedAsset.Owner)
            throw new ArgumentException($"Used the wrong ContentManager to refresh {watchedAsset.AssetName}");

        var path = Path.Combine(manager.RootDirectory, watchedAsset.AssetName) + ".xnb";
        var lastWriteTime = File.GetLastWriteTime(path);
        
        if (lastWriteTime <= watchedAsset.UpdatedAt)
        {
            return false;
        }

        if (IsFileLocked(path)) return false; // wait for the file to not be locked.
        
        manager.UnloadAsset(watchedAsset.AssetName);
        oldAsset = watchedAsset.Asset;
        watchedAsset.Asset = manager.Load<T>(watchedAsset.AssetName);
        watchedAsset.UpdatedAt = lastWriteTime;
        
        return true;
    }
    
    private static bool IsFileLocked(string path)
    {
        try
        {
            using FileStream _ = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            // File is not locked
            return false;
        }
        catch (IOException)
        {
            // File is locked or inaccessible
            return true;
        }
    }
    
    /// <summary>
    /// Load an asset and wrap it with the metadata required to refresh it later using the <see cref="TryRefresh{T}"/> function
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="assetName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static WatchedAsset<T> Watch<T>(this ContentManager manager, string assetName)
    {
        var asset = manager.Load<T>(assetName);
        return new WatchedAsset<T>
        {
            AssetName = assetName,
            Asset = asset,
            UpdatedAt = DateTimeOffset.Now,
            Owner = manager
        };
    }
    
    /// <summary>
    /// Load an Effect into the <see cref="Material"/> wrapper class
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public static Material WatchMaterial(this ContentManager manager, string assetName)
    {
        return new Material(manager.Watch<Effect>(assetName));
    }
    
    
    [Conditional("DEBUG")]
    public static void StartContentWatcherTask()
    {
        var args = Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            // if the application was started with the --no-reload option, then do not start the watcher.
            if (arg == "--no-reload") return;
        }

        // identify the project directory
        var projectFile = Assembly.GetEntryAssembly().GetName().Name + ".csproj";
        var current = Directory.GetCurrentDirectory();
        string projectDirectory = null;

        while (current != null && projectDirectory == null)
        {
            if (File.Exists(Path.Combine(current, projectFile)))
            {
                // the valid project csproj exists in the directory
                projectDirectory = current;
            }
            else
            {
                // try looking in the parent directory.
                //  When there is no parent directory, the variable becomes 'null'
                current = Path.GetDirectoryName(current);
            }
        }

        // if no valid project was identified, then it is impossible to start the watcher
        if (string.IsNullOrEmpty(projectDirectory)) return;
        
        // start the watcher process
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build -t:WatchContent --tl:off",
            WorkingDirectory = projectDirectory,
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false,
            CreateNoWindow = false
        });
        
        // when this program exits, make sure to emit a kill signal to the watcher process
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                /* ignore */
            }
        };
    }
}