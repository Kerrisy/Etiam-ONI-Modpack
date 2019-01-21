﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MaterialColor.IO
{
    public class ConfigWatcher : IRender1000ms, IDisposable
    {
        private bool _configuratorStateChanged;
        private bool _elementColorInfosChanged;

        private FileSystemWatcher _colorInfosWatcher;
        private FileSystemWatcher _mainConfigWatcher;

        public ConfigWatcher()
        {
            this.SubscribeToFileChangeNotifier();
        }

        void IRender1000ms.Render1000ms(float dt)
        {
            if (this._elementColorInfosChanged || this._configuratorStateChanged)
            {
                Painter.Refresh();
                this._elementColorInfosChanged = this._configuratorStateChanged = false;
            }
        }

        public void Dispose()
        {
            this._mainConfigWatcher.Dispose();
            this._colorInfosWatcher.Dispose();
        }

        private void MainConfigChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                State.LoadMainConfig();
                this._configuratorStateChanged = true;
            }
            catch (Exception ex)
            {
                Common.Logger.Log("Main config load failed.", ex);
            }
        }

        private void OnElementColorsInfosChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                State.LoadElementColors();
                this._elementColorInfosChanged = true;
            }
            catch (Exception ex)
            {
                Common.Logger.Log("Element colors load failed.", ex);
            }
        }

        private void SubscribeToFileChangeNotifier()
        {
            const string jsonFilter = "*.json";

            this._colorInfosWatcher = new FileSystemWatcher(Paths.ElementColorsDirectory, jsonFilter);
            this._mainConfigWatcher = new FileSystemWatcher(Paths.MaterialConfigPath, "Config.json");

            this._colorInfosWatcher.Changed += this.OnElementColorsInfosChanged;
            this._mainConfigWatcher.Changed += this.MainConfigChanged;

            this._mainConfigWatcher.EnableRaisingEvents = this._colorInfosWatcher.EnableRaisingEvents = true;
        }
    }
}
