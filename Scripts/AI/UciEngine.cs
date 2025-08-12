// Assets/Scripts/AI/UciEngine.cs
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RetroChess.AI {
    public class UciEngine : IDisposable {
        Process _proc;
        StreamWriter _stdin;
        StreamReader _stdout;
        Thread _readThread;
        readonly ConcurrentQueue<string> _lines = new();
        readonly AutoResetEvent _lineEvent = new(false);
        bool _running = false;

        int _elo = 1200;
        int _skill = -1;
        bool _limitStrength = true;

        public bool IsRunning => _running;

        public void ConfigureWeak(int elo = 1200, int skill = -1, bool limit = true) {
            _elo = Mathf.Clamp(elo, 600, 3000);
            _skill = (skill >= 0) ? Mathf.Clamp(skill, 0, 20) : -1;
            _limitStrength = limit;
        }

        string EnginePath() {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return Path.Combine(Application.streamingAssetsPath, "stockfish.exe");
#else
            return Path.Combine(Application.streamingAssetsPath, "stockfish");
#endif
        }

        public bool Start() {
            if (_running) return true;
            string path = EnginePath();
            if (!File.Exists(path)) {
                UnityEngine.Debug.LogError($"[UCI] Engine not found: {path}");
                return false;
            }
            try {
                _proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = path,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardInputEncoding = Encoding.ASCII,
                        StandardOutputEncoding = Encoding.ASCII
                    }
                };
                _proc.Start();
                _stdin = _proc.StandardInput;
                _stdout = _proc.StandardOutput;
                _running = true;

                _readThread = new Thread(ReadLoop) { IsBackground = true };
                _readThread.Start();

                Send("uci");
                WaitFor("uciok", 3000);

                if (_limitStrength) {
                    Send("setoption name UCI_LimitStrength value true");
                    Send($"setoption name UCI_Elo value {_elo}");
                }
                if (_skill >= 0) {
                    Send($"setoption name Skill Level value {_skill}");
                }

                Send("isready");
                WaitFor("readyok", 3000);

                return true;
            } catch (Exception e) {
                UnityEngine.Debug.LogError($"[UCI] Start failed: {e}");
                Stop();
                return false;
            }
        }

        public void Stop() {
            if (!_running) return;
            try {
                try { Send("quit"); } catch {}
                _running = false;
                _stdin?.Close();
                _stdout?.Close();
                if (!_proc.HasExited) _proc.Kill();
                _proc.Dispose();
            } catch {}
            _proc = null; _stdin = null; _stdout = null;
        }

        void ReadLoop() {
            try {
                while (_running && !_stdout.EndOfStream) {
                    var line = _stdout.ReadLine();
                    if (line == null) break;
                    _lines.Enqueue(line);
                    _lineEvent.Set();
                }
            } catch {}
        }

        void Send(string cmd) {
            if (!_running) return;
            _stdin.WriteLine(cmd);
            _stdin.Flush();
        }

        bool WaitFor(string token, int timeoutMs) {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs) {
                while (_lines.TryDequeue(out var line)) {
                    if (line.Contains(token)) return true;
                }
                _lineEvent.WaitOne(10);
            }
            return false;
        }

        public async Task<string> GetBestMoveAsync(string fenOrStartPos, string[] uciMoves, int depth, int movetimeMs, CancellationToken ct) {
            if (!IsRunning) {
                if (!Start()) return null;
            }
            while (_lines.TryDequeue(out _)) {}

            if (fenOrStartPos == "startpos") {
                var pos = new StringBuilder("position startpos");
                if (uciMoves != null && uciMoves.Length > 0) {
                    pos.Append(" moves");
                    for (int i=0;i<uciMoves.Length;i++) pos.Append(' ').Append(uciMoves[i]);
                }
                Send(pos.ToString());
            } else {
                var pos = new StringBuilder("position fen ").Append(fenOrStartPos);
                if (uciMoves != null && uciMoves.Length > 0) {
                    pos.Append(" moves");
                    for (int i=0;i<uciMoves.Length;i++) pos.Append(' ').Append(uciMoves[i]);
                }
                Send(pos.ToString());
            }

            if (depth > 0) Send($"go depth {depth}");
            else if (movetimeMs > 0) Send($"go movetime {movetimeMs}");
            else Send("go");

            string best = null;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try {
                while (!ct.IsCancellationRequested) {
                    if (movetimeMs > 0 && sw.ElapsedMilliseconds > movetimeMs + 500) break;
                    while (_lines.TryDequeue(out var line)) {
                        if (line.StartsWith("bestmove")) {
                            var parts = line.Split(' ');
                            if (parts.Length >= 2) best = parts[1];
                            return best;
                        }
                    }
                    await Task.Delay(10, ct);
                }
            } catch (TaskCanceledException) {}
            return best;
        }

        public void Dispose() { Stop(); }
    }
}