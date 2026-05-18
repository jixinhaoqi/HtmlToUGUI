using System;
using System.IO;
using UnityEngine;

namespace Xxhq.Htmltougui.Editor
{
    /// <summary>
    /// 文件系统监视服务，封装 FileSystemWatcher 的创建、路径更新、事件管理和生命周期。
    /// </summary>
    public class FileWatcherService : IDisposable
    {
        private FileSystemWatcher _watcher;
        private bool _disposed;
        private float _lastEventTime;

        private const float DebounceInterval = 0.3f;
        /// <summary>
        /// 文件更改事件的处理方法，带有防抖动处理。
        /// </summary>
        public event FileSystemEventHandler Changed;
        /// <summary>
        /// 文件删除事件的处理方法，带有防抖动处理。
        /// </summary>
        public event FileSystemEventHandler Deleted;
        /// <summary>
        /// 文件重命名事件的处理方法，带有防抖动处理。
        /// </summary>
        public event RenamedEventHandler Renamed;
        /// <summary>
        /// 文件系统错误事件的处理方法。
        /// </summary>
        public event ErrorEventHandler Error;

        /// <summary>
        /// 开始监视文件路径。如果文件不存在，则不执行任何操作。
        /// </summary>
        /// <param name="filePath">要监视的文件路径</param>
        public virtual void StartWatching(string filePath)
        {
            StopWatching();
            if (!File.Exists(filePath)) return;

            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath).Replace('\\', '/'),
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                    | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnChanged;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
        }

        /// <summary>
        /// 停止监视文件路径。如果当前没有正在监视的文件，则不执行任何操作。
        /// </summary>
        public virtual void StopWatching()
        {
            if (_watcher == null) return;

            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnChanged;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;
            _watcher.Dispose();
            _watcher = null;
        }

        /// <summary>
        /// 更新监视的文件路径。如果文件不存在，则停止监视。
        /// </summary>
        /// <param name="filePath">要监视的文件路径</param>
        public virtual void UpdatePath(string filePath)
        {
            if (_watcher == null || !_watcher.EnableRaisingEvents) return;

            if (File.Exists(filePath))
            {
                _watcher.Path = Path.GetDirectoryName(filePath).Replace('\\', '/');
                _watcher.Filter = Path.GetFileName(filePath);
            }
        }
        /// <summary>
        /// 释放资源。
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopWatching();
        }

        //TODO: 防抖动处理存在问题，在非主线程中无法使用UnityEditor.EditorApplication.timeSinceStartup
        private bool ShouldSuppress()
        {
            return false;
            //该方法被其他线程执行，而EditorApplication.timeSinceStartup无法在非unity主线程中执行
            //float now = (float)UnityEditor.EditorApplication.timeSinceStartup;
            //if (now - _lastEventTime < DebounceInterval)
            //    return true;
            //_lastEventTime = now;
            //return false;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (ShouldSuppress()) return;
            UnityEditor.EditorApplication.delayCall += () => Changed?.Invoke(sender, e);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (ShouldSuppress()) return;
            UnityEditor.EditorApplication.delayCall += () => Deleted?.Invoke(sender, e);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            UnityEditor.EditorApplication.delayCall += () => Renamed?.Invoke(sender, e);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            UnityEditor.EditorApplication.delayCall += () => Error?.Invoke(sender, e);
        }
    }
}
