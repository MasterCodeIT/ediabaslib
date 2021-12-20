﻿using System;
using System.Threading;
using System.Threading.Tasks;
using PsdzClient.Programing;

namespace WebPsdzClient.App_Data
{
    public class SessionContainer : IDisposable
    {
        public delegate void UpdateDisplayDelegate();
        public ProgrammingJobs ProgrammingJobs { get; private set; }
        public CancellationTokenSource Cts { get; private set; }

        private bool _taskActive;
        public bool TaskActive
        {
            get
            {
                lock (_lockObject)
                {
                    return _taskActive;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    _taskActive = value;
                }
            }
        }

        private string _statusText;
        public string StatusText
        {
            get
            {
                lock (_lockObject)
                {
                    return _statusText;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _statusText = value;
                }
            }
        }

        private bool _disposed;
        private readonly object _lockObject = new object();
        private UpdateDisplayDelegate _updateDisplay;

        public SessionContainer(string dealerId)
        {
            ProgrammingJobs = new ProgrammingJobs(dealerId);
            ProgrammingJobs.UpdateStatusEvent += UpdateStatus;
            StatusText = string.Empty;
        }

        public void UpdateStatus(string message = null)
        {
            StatusText = message ?? string.Empty;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            _updateDisplay?.Invoke();
        }

        public void StartProgrammingService(UpdateDisplayDelegate updateHandler, string istaFolder)
        {
            if (TaskActive)
            {
                return;
            }

            _updateDisplay = updateHandler;
            Cts = new CancellationTokenSource();
            StartProgrammingServiceTask(istaFolder).ContinueWith(task =>
            {
                TaskActive = false;
                Cts.Dispose();
                Cts = null;
                UpdateDisplay();
                _updateDisplay = null;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> StartProgrammingServiceTask(string istaFolder)
        {
            return await Task.Run(() => ProgrammingJobs.StartProgrammingService(Cts, istaFolder)).ConfigureAwait(false);
        }

        public void StopProgrammingService(UpdateDisplayDelegate updateHandler)
        {
            if (TaskActive)
            {
                return;
            }

            _updateDisplay = updateHandler;
            StopProgrammingServiceTask().ContinueWith(task =>
            {
                TaskActive = false;
                UpdateDisplay();
                _updateDisplay = null;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> StopProgrammingServiceTask()
        {
            return await Task.Run(() => ProgrammingJobs.StopProgrammingService(Cts)).ConfigureAwait(false);
        }

        public void ConnectVehicle(UpdateDisplayDelegate updateHandler, string istaFolder, string ipAddress, bool icomConnection)
        {
            if (TaskActive)
            {
                return;
            }

            if (ProgrammingJobs.PsdzContext?.Connection != null)
            {
                return;
            }

            _updateDisplay = updateHandler;
            Cts = new CancellationTokenSource();
            ConnectVehicleTask(istaFolder, ipAddress, icomConnection).ContinueWith(task =>
            {
                TaskActive = false;
                Cts.Dispose();
                Cts = null;
                UpdateDisplay();
                _updateDisplay = null;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> ConnectVehicleTask(string istaFolder, string ipAddress, bool icomConnection)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ProgrammingJobs.ConnectVehicle(Cts, istaFolder, ipAddress, icomConnection)).ConfigureAwait(false);
        }

        public void DisconnectVehicle(UpdateDisplayDelegate updateHandler)
        {
            if (TaskActive)
            {
                return;
            }

            if (ProgrammingJobs.PsdzContext?.Connection == null)
            {
                return;
            }

            _updateDisplay = updateHandler;
            DisconnectVehicleTask().ContinueWith(task =>
            {
                TaskActive = false;
                UpdateDisplay();
                _updateDisplay = null;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> DisconnectVehicleTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ProgrammingJobs.DisconnectVehicle(Cts)).ConfigureAwait(false);
        }

        public void VehicleFunctions(UpdateDisplayDelegate updateHandler, ProgrammingJobs.OperationType operationType)
        {
            if (TaskActive)
            {
                return;
            }

            if (ProgrammingJobs.PsdzContext?.Connection == null)
            {
                return;
            }

            _updateDisplay = updateHandler;
            Cts = new CancellationTokenSource();
            VehicleFunctionsTask(operationType).ContinueWith(task =>
            {
                TaskActive = false;
                Cts.Dispose();
                Cts = null;
                UpdateDisplay();
                _updateDisplay = null;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> VehicleFunctionsTask(ProgrammingJobs.OperationType operationType)
        {
            return await Task.Run(() => ProgrammingJobs.VehicleFunctions(Cts, operationType)).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                while (TaskActive)
                {
                    Thread.Sleep(100);
                }

                StopProgrammingService(null);

                if (ProgrammingJobs != null)
                {
                    ProgrammingJobs.Dispose();
                    ProgrammingJobs = null;
                }

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
