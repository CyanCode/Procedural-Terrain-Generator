/*
Copyright 2015 Pim de Witte All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Reflection;
using Terra.Source;
using UnityEditor;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Terra.Util {
	/// Author: Pim de Witte (pimdewitte.com) and contributors
	/// <summary>
	/// A thread-safe class which holds a queue with actions to execute on the next Update() method. It can be used to make calls to the main thread for
	/// things such as UI Manipulation in Unity. It was developed for use in combination with the Firebase Unity plugin, which uses separate threads for event handling
	/// </summary>
	[ExecuteInEditMode]
	public class MTDispatch: MonoBehaviour {
        private static bool IsExitingPlayMode = false;
		private static readonly Queue<Action> _executionQueue = new Queue<Action>();

		void Update() {
			lock (_executionQueue) {
				while (_executionQueue.Count > 0) {
					_executionQueue.Dequeue().Invoke();
				}
			}
		}

        void Start() {
            //Register play mode state handler once
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

	    void OnPlayModeStateChange(PlayModeStateChange state) { 
	        if (state == PlayModeStateChange.ExitingPlayMode) {
                IsExitingPlayMode = true;
	        }
	    }

        /// <summary>
        /// Locks the queue and adds the IEnumerator to the queue
        /// </summary>
        /// <param name="action">IEnumerator function that will be executed from the main thread.</param>
        public void Enqueue(IEnumerator action) {
			lock (_executionQueue) {
                if (!TerraConfig.Instance.IsEditor) {
                    _executionQueue.Enqueue(() => {
                        StartCoroutine(action);
                    });
                } else {
                    EditorDispatcher.Dispatch(action);
                }
			}
		}

		/// <summary>
		/// Locks the queue and adds the Action to the queue
		/// </summary>
		/// <param name="action">function that will be executed from the main thread.</param>
		public void Enqueue(Action action) {
			Enqueue(ActionWrapper(action));
		}

		IEnumerator ActionWrapper(Action a) {
			a();
			yield return null;
		}

        [SerializeField]
		private static MTDispatch _instance = null;

		public static bool Exists() {
			return _instance != null;
		}

		public static MTDispatch Instance() {
            if (!Exists() && !IsExitingPlayMode) {
			    throw new Exception("MTDispatch could not find the MTDispatch Component. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
            }

			return _instance;
		}

        void OnEnable() {
            Init();
        }

		void Awake() {
            Init();
		}

		void OnDestroy() {
			_instance = null;
		}

        void Init() {
            if (_instance == null) {
                _instance = this;

                if (Application.isPlaying) {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
        }
    }

    internal static class EditorDispatcher {
        private static readonly Queue<Action> dispatchQueue = new Queue<Action>();
        private static double timeSliceLimit = 10.0; // in miliseconds
        private static Stopwatch timer;

        static EditorDispatcher() {
            EditorApplication.update += Update;
            timer = new Stopwatch();
        }

        private static void Update() {
            lock (dispatchQueue) {
                int dispatchCount = 0;

                timer.Reset();
                timer.Start();

                while (dispatchQueue.Count > 0 && timer.Elapsed.TotalMilliseconds <= timeSliceLimit) {
                    dispatchQueue.Dequeue().Invoke();

                    dispatchCount++;
                }

                timer.Stop();
            }
        }

        /// <summary>
        /// Send an Action Delegate to be run on the main thread. See EditorDispatchActions for some common usecases.
        /// </summary>
        /// <param name="task">An action delegate to run on the main thread</param>
        /// <returns>An AsyncDispatch that can be used to track if the dispatch has completed.</returns>
        public static AsyncDispatch Dispatch(Action task) {
            lock (dispatchQueue) {
                AsyncDispatch dispatch = new AsyncDispatch();

                // enqueue a new task that runs the supplied task and completes the dispatcher 
                dispatchQueue.Enqueue(() => { task(); dispatch.FinishedDispatch(); });

                return dispatch;
            }
        }

        /// <summary>
        /// Send a Coroutine to be run on the main thread. See EditorDispatchActions for some common usecases.
        /// </summary>
        /// <param name="task">A coroutine to run on the main thread</param>
        /// <param name="showUI">if the Editor Corotine runner should run a progress UI</param>
        /// <returns>An AsyncDispatch that can be used to track if the coroutine has been dispatched & completed.</returns>
        public static AsyncDispatch Dispatch(IEnumerator task, bool showUI = false) {
            // you need this system for this to work! https://gist.github.com/LotteMakesStuff/16b5f2fc108f9a0201950c797d53cfbf
            lock (dispatchQueue) {
                AsyncDispatch dispatch = new AsyncDispatch();

                dispatchQueue.Enqueue(() =>
                {
                    if (showUI) {
                        EditorCoroutineRunner.StartCoroutineWithUI(DispatchCorotine(task, dispatch), "Dispatcher task", false);
                    } else {
                        EditorCoroutineRunner.StartCoroutine(task);
                    }
                });

                return dispatch;
            }
        }

        private static IEnumerator DispatchCorotine(IEnumerator dispatched, AsyncDispatch tracker) {
            yield return dispatched;
            tracker.FinishedDispatch();
        }
    }

    /// <summary>
    /// Represents the progress of the dispatched action. Can be yielded to in a coroutine.
    /// If not using coroutines, look at the IsDone property to find out when its okay to proceed.
    /// </summary>
    internal class AsyncDispatch : CustomYieldInstruction {
        public bool IsDone { get; private set; }
        public override bool keepWaiting { get { return !IsDone; } }


        /// <summary>
        /// Flags this dispatch as completed.
        /// </summary>
        internal void FinishedDispatch() {
            IsDone = true;
        }
    }


    internal static class EditorDispatchActions {
        #region play mode
        public static void TogglePlayMode() {
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
        public static void EnterPlayMode() {
            EditorApplication.isPlaying = true;
        }
        public static void ExitPlayMode() {
            EditorApplication.isPlaying = false;
        }

        public static void TogglePausePlayMode() {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
        public static void PausePlayMode() {
            EditorApplication.isPaused = true;
        }
        public static void UnpausePlayMode() {
            EditorApplication.isPaused = false;
        }

        public static void Step() {
            EditorApplication.Step();
        }
        #endregion

        public static void Build() {
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget), EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
        }

        public static void Beep() {
            EditorApplication.Beep();
        }
    }

    internal class EditorCoroutineRunner {
        [MenuItem("Window/Lotte's Coroutine Runner: Demo")]
        public static void DemoEditorCoroutines() {
            // adds a menu item to test the coroutine system. 
            if (!Application.isPlaying) {
                // lets fire off the demo coroutine with a UI so we can see what its doing. We could also run it without a UI by using EditorCoroutineRunner.StartCoroutine(...)
                EditorCoroutineRunner.StartCoroutineWithUI(DemoCoroutiune(), "Lotte's Coroutine Demo", true);
            }
        }

        static IEnumerator DemoCoroutiune() {
            // You can code editor coroutines exactly like you would a normal unity coroutine
            Debug.Log("Step: 0");
            yield return null;

            // all the normal return types that work with regular Unity coroutines should work here! for example lets wait for a second
            Debug.Log("Step: 1");
            yield return new WaitForSeconds(1);

            // We can also yeild any type that extends Unitys CustomYieldInstruction class. here we are going to use EditorStatusUpdate. this allows us to yield and update the
            // editor coroutine UI at the same time!
            yield return new EditorStatusUpdate("coroutine is running", 0.2f);

            // We can also yield to nested coroutines
            Debug.Log("Step: 2");

            yield return EditorCoroutineRunner.StartCoroutine(DemoTwo());
            EditorCoroutineRunner.UpdateUIProgressBar(0.35f); // we can use the UpdateUI helper methods to update the UI whenever, without yielding a EditorStatusUpdate
            yield return DemoTwo(); // it shouldnt matter how we start the nested coroutine, the editor runner can hadle it

            // we can even yield a UnityWebRequest object if we want to grab data from the internets!
            Debug.Log("Step: 3");

            // for example, lets as random.org to generate us a list of random numbers and shove it into the console
            var www = new UnityWebRequest("https://www.random.org/integers/?num=100&min=1&max=1000&col=1&base=10&col=5&format=plain&rnd=new");
            yield return www;
            Debug.Log(www.ToString());

            EditorCoroutineRunner.UpdateUI("Half way!", 0.5f);
            yield return new WaitForSeconds(1);

            // Finally lets do a long runnig task and split its updates over many frames to keep the editor responsive
            Debug.Log("Step: 4");
            var test = 1000;
            yield return new WaitUntil(() => {
                test--;
                EditorCoroutineRunner.UpdateUI("Crunching Numbers: " + test, 0.5f + (((1000 - test) / 1000f) * 0.5f));
                return (test <= 0);
            });
            Debug.Log("Done!!");
        }

        static IEnumerator DemoTwo() {
            Debug.Log("TESTTWO: Starting second test coroutine");
            yield return new WaitForSeconds(1.2f);
            Debug.Log("TESTTWO: finished second test coroutine");
        }

        [MenuItem("Window/Lotte's Coroutine Runner: Force kill coroutines")]
        public static void KillAllCoroutines() {
            // force kills all running coroutines if something goes wrong.
            EditorUtility.ClearProgressBar();
            uiCoroutineState = null;
            coroutineStates.Clear();
            finishedThisUpdate.Clear();
        }

        private static List<EditorCoroutineState> coroutineStates;
        private static List<EditorCoroutineState> finishedThisUpdate;
        private static EditorCoroutineState uiCoroutineState;

        /// <summary>
        /// Start a coroutine. equivilent of calling StartCoroutine on a mono behaviour
        /// </summary>
        public static EditorCoroutine StartCoroutine(IEnumerator coroutine) {
            return StoreCoroutine(new EditorCoroutineState(coroutine));
        }

        /// <summary>
        /// Start a coroutine and display a progress UI. only one EditorCoroutine can display a UI at once. equivilent of calling StartCoroutine on a mono behaviour
        /// </summary>
        /// <param name="coroutine">coroutine to run</param>
        /// <param name="title">Text to show in the UIs title bar</param>
        /// <param name="isCancelable">Displays a cancel button if true</param>
        public static EditorCoroutine StartCoroutineWithUI(IEnumerator coroutine, string title, bool isCancelable = false) {
            if (uiCoroutineState != null) {
                Debug.LogError("EditorCoroutineRunner only supports running one coroutine that draws a GUI! [" + title + "]");
                return null;
            }
            EditorCoroutineRunner.uiCoroutineState = new EditorCoroutineState(coroutine, title, isCancelable);
            return StoreCoroutine(uiCoroutineState);
        }

        // Creates objects to manage the coroutines lifecycle and stores them away to be processed
        private static EditorCoroutine StoreCoroutine(EditorCoroutineState state) {
            if (coroutineStates == null) {
                coroutineStates = new List<EditorCoroutineState>();
                finishedThisUpdate = new List<EditorCoroutineState>();
            }

            if (coroutineStates.Count == 0)
                EditorApplication.update += Runner;

            coroutineStates.Add(state);

            return state.editorCoroutineYieldInstruction;
        }

        /// <summary>
        /// Updates the status label in the EditorCoroutine runner UI
        /// </summary>
        public static void UpdateUILabel(string label) {
            if (uiCoroutineState != null && uiCoroutineState.showUI) {
                uiCoroutineState.Label = label;
            }
        }

        /// <summary>
        /// Updates the progress bar in the EditorCoroutine runner UI
        /// </summary>
        public static void UpdateUIProgressBar(float percent) {
            if (uiCoroutineState != null && uiCoroutineState.showUI) {
                uiCoroutineState.PercentComplete = percent;
            }
        }

        /// <summary>
        /// Updates the status label and progress bar in the EditorCoroutine runner UI
        /// </summary>
        public static void UpdateUI(string label, float percent) {
            if (uiCoroutineState != null && uiCoroutineState.showUI) {
                uiCoroutineState.Label = label;
                uiCoroutineState.PercentComplete = percent;
            }
        }

        // Manages running active coroutines!
        private static void Runner() {
            // Tick all the coroutines we have stored
            for (int i = 0; i < coroutineStates.Count; i++) {
                TickState(coroutineStates[i]);
            }

            // if a coroutine was finished whilst we were ticking, clear it out now
            for (int i = 0; i < finishedThisUpdate.Count; i++) {
                coroutineStates.Remove(finishedThisUpdate[i]);

                if (uiCoroutineState == finishedThisUpdate[i]) {
                    uiCoroutineState = null;
                    EditorUtility.ClearProgressBar();
                }
            }
            finishedThisUpdate.Clear();

            // stop the runner if were done.
            if (coroutineStates.Count == 0) {
                EditorApplication.update -= Runner;
            }
        }

        private static void TickState(EditorCoroutineState state) {
            if (state.IsValid) {
                // This coroutine is still valid, give it a chance to tick!
                state.Tick();

                // if this coroutine is the active UI coroutine, give it a chance to update the UI
                if (state.showUI && uiCoroutineState == state) {
                    uiCoroutineState.UpdateUI();
                }
            } else {
                // We have finished running the coroutine, lets scrap it
                finishedThisUpdate.Add(state);
            }
        }

        // Special thanks to Thomas for donating the following two methods 
        // Github: @ThomasBousquet
        // Twitter: @VimesTom    
        private static bool KillCoroutine(ref EditorCoroutine coroutine, ref List<EditorCoroutineState> states) {
            foreach (EditorCoroutineState state in states) {
                if (state.editorCoroutineYieldInstruction == coroutine) {
                    states.Remove(state);
                    coroutine = null;
                    return true;
                }
            }
            return false;
        }

        public static void KillCoroutine(ref EditorCoroutine coroutine) {
            if (uiCoroutineState.editorCoroutineYieldInstruction == coroutine) {
                uiCoroutineState = null;
                coroutine = null;
                EditorUtility.ClearProgressBar();
                return;
            }
            if (KillCoroutine(ref coroutine, ref coroutineStates))
                return;

            if (KillCoroutine(ref coroutine, ref finishedThisUpdate))
                return;
        }
    }

    internal class EditorCoroutineState {
        private IEnumerator coroutine;
        public bool IsValid {
            get { return coroutine != null; }
        }
        public EditorCoroutine editorCoroutineYieldInstruction;

        // current state
        private object current;
        private Type currentType;
        private float timer; // for WaitForSeconds support    
        private EditorCoroutine nestedCoroutine; // for tracking nested coroutines that are not started with EditorCoroutineRunner.StartCoroutine
        private DateTime lastUpdateTime;

        // UI
        public bool showUI;
        private bool cancelable;
        private bool canceled;
        private string title;
        public string Label;
        public float PercentComplete;

        public EditorCoroutineState(IEnumerator coroutine) {
            this.coroutine = coroutine;
            editorCoroutineYieldInstruction = new EditorCoroutine();
            showUI = false;
            lastUpdateTime = DateTime.Now;
        }

        public EditorCoroutineState(IEnumerator coroutine, string title, bool isCancelable) {
            this.coroutine = coroutine;
            editorCoroutineYieldInstruction = new EditorCoroutine();
            showUI = true;
            cancelable = isCancelable;
            this.title = title;
            Label = "initializing....";
            PercentComplete = 0.0f;

            lastUpdateTime = DateTime.Now;
        }

        public void Tick() {
            if (coroutine != null) {
                // First check if we have been canceled by the UI. If so, we need to stop before doing any wait processing
                if (canceled) {
                    Stop();
                    return;
                }

                // Did the last Yield want us to wait?
                bool isWaiting = false;
                var now = DateTime.Now;
                if (current != null) {
                    if (currentType == typeof(WaitForSeconds)) {
                        // last yield was a WaitForSeconds. Lets update the timer.
                        var delta = now - lastUpdateTime;
                        timer -= (float)delta.TotalSeconds;

                        if (timer > 0.0f) {
                            isWaiting = true;
                        }
                    } else if (currentType == typeof(WaitForEndOfFrame) || currentType == typeof(WaitForFixedUpdate)) {
                        // These dont make sense in editor, so we will treat them the same as a null return...
                        isWaiting = false;
                    } else if (currentType == typeof(UnityWebRequest)) {
                        // Web download request, lets see if its done!
                        var www = current as UnityWebRequest;
                        if (!www.isDone) {
                            isWaiting = true;
                        }
                    } else if (currentType.IsSubclassOf(typeof(CustomYieldInstruction))) {
                        // last yield was a custom yield type, lets check its keepWaiting property and react to that
                        var yieldInstruction = current as CustomYieldInstruction;
                        if (yieldInstruction.keepWaiting) {
                            isWaiting = true;
                        }
                    } else if (currentType == typeof(EditorCoroutine)) {
                        // Were waiting on another coroutine to finish
                        var editorCoroutine = current as EditorCoroutine;
                        if (!editorCoroutine.HasFinished) {
                            isWaiting = true;
                        }
                    } else if (typeof(IEnumerator).IsAssignableFrom(currentType)) {
                        // if were just seeing an enumerator lets assume that were seeing a nested coroutine that has been passed in without calling start.. were start it properly here if we need to
                        if (nestedCoroutine == null) {
                            nestedCoroutine = EditorCoroutineRunner.StartCoroutine(current as IEnumerator);
                            isWaiting = true;
                        } else {
                            isWaiting = !nestedCoroutine.HasFinished;
                        }

                    } else if (currentType == typeof(Coroutine)) {
                        // UNSUPPORTED
                        UnityEngine.Debug.LogError("Nested Coroutines started by Unity's defaut StartCoroutine method are not supported in editor! please use EditorCoroutineRunner.Start instead. Canceling.");
                        canceled = true;
                    } else {
                        // UNSUPPORTED
                        Debug.LogError("Unsupported yield (" + currentType + ") in editor coroutine!! Canceling.");
                        canceled = true;
                    }
                }
                lastUpdateTime = now;

                // have we been canceled?
                if (canceled) {
                    Stop();
                    return;
                }

                if (!isWaiting) {
                    // nope were good! tick the coroutine!
                    bool update = coroutine.MoveNext();

                    if (update) {
                        // yup the coroutine returned true so its been ticked...

                        // lets see what it actually yielded
                        current = coroutine.Current;
                        if (current != null) {
                            // is it a type we have to do extra processing on?
                            currentType = current.GetType();

                            if (currentType == typeof(WaitForSeconds)) {
                                // its a WaitForSeconds... lets use reflection to pull out how long the actual wait is for so we can process the wait
                                var wait = current as WaitForSeconds;
                                FieldInfo m_Seconds = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (m_Seconds != null) {
                                    timer = (float)m_Seconds.GetValue(wait);
                                }
                            } else if (currentType == typeof(EditorStatusUpdate)) {
                                // Special case yield that wants to update the UI!
                                var updateInfo = current as EditorStatusUpdate;
                                if (updateInfo.HasLabelUpdate) {
                                    Label = updateInfo.Label;
                                }
                                if (updateInfo.HasPercentUpdate) {
                                    PercentComplete = updateInfo.PercentComplete;
                                }
                            }
                        }
                    } else {
                        // Coroutine returned false so its finally finished!!
                        Stop();
                    }
                }
            }
        }

        private void Stop() {
            // Coroutine has finished! do some cleanup...
            coroutine = null;
            editorCoroutineYieldInstruction.HasFinished = true;
        }

        public void UpdateUI() {
            if (cancelable) {
                canceled = EditorUtility.DisplayCancelableProgressBar(title, Label, PercentComplete);
            } else {
                EditorUtility.DisplayProgressBar(title, Label, PercentComplete);
            }
        }
    }

    /// <summary>
    /// Coroutine Yield instruction that allows an Editor Coroutine to update the Coroutine runner UI
    /// </summary>
    internal class EditorStatusUpdate : CustomYieldInstruction {
        public string Label;
        public float PercentComplete;

        public bool HasLabelUpdate;
        public bool HasPercentUpdate;

        public override bool keepWaiting {
            get {
                // always go to the next update
                return false;
            }
        }

        public EditorStatusUpdate(string label) {
            HasPercentUpdate = false;

            HasLabelUpdate = true;
            Label = label;
        }

        public EditorStatusUpdate(float percent) {
            HasPercentUpdate = true;
            PercentComplete = percent;

            HasLabelUpdate = false;
        }

        public EditorStatusUpdate(string label, float percent) {
            HasPercentUpdate = true;
            PercentComplete = percent;

            HasLabelUpdate = true;
            Label = label;
        }
    }

    /// <summary>
    /// Created when an Editor Coroutine is started, can be yielded to to allow another coroutine to finish first.
    /// </summary>
    internal class EditorCoroutine : YieldInstruction {
        public bool HasFinished;
    }
}