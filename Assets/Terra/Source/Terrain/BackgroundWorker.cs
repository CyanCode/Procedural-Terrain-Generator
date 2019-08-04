using System;
using System.Collections.Generic;
using System.Threading;
using Terra.Util;
using UnityEngine;

public class BackgroundWorker {
    private struct Job {
        public Action func;
        public Action onComplete;

        public Job(Action func, Action onComplete) {
            this.func = func;
            this.onComplete = onComplete;
        }
    }

    private readonly Queue<Job> _jobs = new Queue<Job>();
    private Thread _worker = null;
    private bool _kill = false;
    private static readonly object _enqueueLock = new object();
    private static readonly object _dequeueLock = new object();

    /// <summary>
    /// Adds this function to the thread queue.
    /// </summary>
    /// <param name="func">Function to queue</param>
    /// <param name="onComplete">Called when function has finished</param>
    public void Enqueue(Action func, Action onComplete) {
        lock (_enqueueLock) {
            _jobs.Enqueue(new Job(func, onComplete));
        }

        if (_worker == null || !_worker.IsAlive) {
            Start();
        }
    }

    /// <summary>
    /// Kills the background thread. If a job is running it is finished before 
    /// killing the thread.
    /// </summary>
    public void Stop() {
        _kill = true;
    }

    /// <summary>
    /// Force kills the background thread even if a job is running.
    /// </summary>
    public void ForceStop() {
        if (_worker != null) {
            _jobs.Clear();
            _worker.Abort();
            _worker = null;
        }
    }

    private void Start() {
        _worker = new Thread(() => {
            while (!_kill) {
                lock(_dequeueLock) { 
                    try {
                        if (_jobs.Count > 0) {
                            Job job = _jobs.Dequeue();
                            job.func();
                            job.onComplete();
                        }
                    } catch (Exception) {
                        _kill = true;
                    }
                }
            }
        });

        _worker.Start();
    }
}
