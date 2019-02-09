﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Patcher.Extensions;
using Patcher.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Patcher
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<Tuple<string, string>> _patchList;
        private string _tempPath;
        private CancellationTokenSource _cts;
        public MainWindow()
        {
            _patchList = new List<Tuple<string, string>>();
            _cts = new CancellationTokenSource();
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            Init();
            InitPatch();
            Task.Run(async () =>
            {
                var processes = Process.GetProcessesByName("Macro");
                foreach (var process in processes)
                {
                    process.Kill();
                }
                await RunPatch(_cts.Token);
                Process.Start(@".\Macro.exe");
                Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            });
        }
        private void Init()
        {
            _tempPath = Path.GetTempPath() + @"Macro";
            btnCancel.Content = ObjectCache.GetValue("Cancel");
        }
        private void InitEvent()
        {
            btnCancel.Click += Button_Click;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender.Equals(btnCancel))
            {
                if (this.MessageShow("", ObjectCache.GetValue("CancelPatch").ToString(), MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        DialogTitleFontSize = 0.1F,
                        MaximumBodyHeight = 500,
                    }) == MessageDialogResult.Affirmative)
                {
                    btnCancel.IsEnabled = false;
                    _cts.Cancel();
                }
            }
        }
        private void InitPatch()
        {
            try
            {
                while (Directory.Exists(_tempPath))
                {
                    Directory.Delete(_tempPath, true);
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            
            Directory.CreateDirectory(_tempPath);
            Directory.CreateDirectory($@"{_tempPath}\backup");

            CheckPatchList();
        }
        private Task RunPatch(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<Task>();
            Dispatcher.InvokeAsync(() =>
            {
                Backup().ContinueWith(task =>
                {
                    return DownloadFiles(token);
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                        return Patching(token);
                    return task;
                }).ContinueWith(async task =>
                {
                    if (task.Result.Status != TaskStatus.RanToCompletion)
                        await Rollback();
                    tcs.SetResult(task.Result);
                });
            }, System.Windows.Threading.DispatcherPriority.Input, token);

            return tcs.Task;
        }
        private void CheckPatchList()
        {
            try
            {
                lblState.Content = ObjectCache.GetValue("SearchPatchList");
                var request = (HttpWebRequest)WebRequest.Create(ObjectCache.GetValue("PatchUrl").ToString());
                using (var response = request.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream()))
                    {
                        var datas = stream.ReadToEnd();
                        if (datas.StartsWith("[") & (datas.EndsWith("]") || datas.EndsWith($"{']'}{"\r\n"}")))
                        {
                            var lastIdx = datas.LastIndexOf(']');
                            var tokens = datas.Remove(0, 1).Remove(lastIdx - 1, datas.Count() - lastIdx - 2).Trim().Replace("\"", "").Replace("\r\n", "").Trim().Split(',').ToArray();
                            _patchList.AddRange(tokens.Where(r => r.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Count() == 2).Select(r =>
                            {
                                var split = r.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                return new Tuple<string, string>(split[0], split[1]);
                            }).ToList());
                        }
                    }
                }
                lblCount.Content = $"(0/{_patchList.Count})";
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        private Task DownloadFiles(CancellationToken token)
        {
            for (int i = 0; i < _patchList.Count; ++i)
            {
                try
                {
                    if (token.IsCancellationRequested)
                        return Task.FromCanceled(token);
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{ObjectCache.GetValue("Download")} : {_patchList[i].Item1}" ;
                        lblCount.Content = $"({i + 1}/{_patchList.Count})";
                        progress.Value = 0;
                    });

                    var request = (HttpWebRequest)WebRequest.Create(_patchList[i].Item2);
                    request.ContentType = "application/octet-stream";
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        var total = response.ContentLength;
                        using (var stream = response.GetResponseStream())
                        {
                            if (_patchList[i].Item1.Contains(@"\"))
                            {
                                var index = _patchList[i].Item1.LastIndexOf(@"\");
                                Directory.CreateDirectory($@"{_tempPath}\{_patchList[i].Item1.Substring(0, index)}");
                            }

                            using (var fs = new FileStream($@"{_tempPath}\{_patchList[i].Item1}", FileMode.Create, FileAccess.Write))
                            {
                                var buffer = new byte[4096];
                                var read = 0;
                                var current = 0L;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fs.Write(buffer, 0, read);
                                    current += read;
                                    Dispatcher.Invoke(() =>
                                    {
                                        progress.Value = 100 - total * 1.0 / current;
                                    });
                                }
                                fs.Flush();
                                Dispatcher.Invoke(() =>
                                {
                                    progress.Value = 100D;
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    i--;
                }
            }
            return Task.CompletedTask;
        }
        private Task Backup()
        {
            var path = $@"{_tempPath}\backup\";
            for(int i=0; i<_patchList.Count; ++i)
            {
                if (_patchList[i].Item1.Contains(@"\"))
                {
                    var index = _patchList[i].Item1.LastIndexOf(@"\");
                    Directory.CreateDirectory($@"{path}\{_patchList[i].Item1.Substring(0, index)}");
                }
                if(File.Exists(_patchList[i].Item1))
                    File.Move(_patchList[i].Item1, $"{path}{_patchList[i].Item1}");
            }
            return Task.CompletedTask;
        }
        private Task Patching(CancellationToken token)
        {
            for(int i=0; i< _patchList.Count; ++i)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);

                Dispatcher.Invoke(() =>
                {
                    lblState.Content = $"{ObjectCache.GetValue("Patching")} : {_patchList[i].Item1}";
                    lblCount.Content = $"({i + 1}/{_patchList.Count})";
                    progress.Value = 0;
                });
                
                if (_patchList[i].Item1.Contains(@"\"))
                {
                    var index = _patchList[i].Item1.LastIndexOf(@"\");
                    Directory.CreateDirectory($"{_patchList[i].Item1.Substring(0, index)}");
                }
                if (File.Exists(_patchList[i].Item1))
                {
                    File.Delete(_patchList[i].Item1);
                }

                if (File.Exists($@"{_tempPath}\{_patchList[i].Item1}"))
                    File.Move($@"{_tempPath}\{_patchList[i].Item1}", _patchList[i].Item1);
            }
            return Task.CompletedTask;
        }
        private Task Rollback()
        {
            try
            {
                var backupPath = $@"{_tempPath}\backup\";
                for (int i = 0; i < _patchList.Count; ++i)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblState.Content = $"{ObjectCache.GetValue("Rollback")} : {_patchList[i].Item1}";
                        lblCount.Content = $"({i + 1}/{_patchList.Count})";
                        progress.Value = 0;
                    });
                    if (_patchList[i].Item1.Contains(@"\"))
                    {
                        var index = _patchList[i].Item1.LastIndexOf(@"\");
                        Directory.CreateDirectory($"{_patchList[i].Item1.Substring(0, index)}");
                    }
                    File.Delete(_patchList[i].Item1);
                    if (File.Exists($@"{backupPath}\{_patchList[i].Item1}"))
                        File.Move($"{backupPath}{_patchList[i].Item1}", _patchList[i].Item1);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        } 
    }
}