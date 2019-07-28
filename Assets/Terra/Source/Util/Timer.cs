using System.Diagnostics;

namespace Terra.Util { 
    public class Timer {
        private readonly Stopwatch _sw = new Stopwatch();

        public Timer Start() {
            _sw.Start();
            return this;
        }

        public void StopAndPrint(string service = "") {
            string label = service == "" ? "" : "[" + service + "] ";

            _sw.Stop();
            UnityEngine.Debug.Log(label + "Elapsed Time: " + _sw.ElapsedMilliseconds);
        }

        public void StopAndPrintMT(string service = "") {
            MTDispatch.Instance().Enqueue(() => StopAndPrint(service));
        }
    }
}
