using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

// Writes compile results and console errors/warnings to <project>/Logs so they can be inspected outside the editor.
[InitializeOnLoad]
public static class AdventuresDiagLogger
{
    static readonly string CompileLog = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "adv_compile.txt");
    static readonly string ConsoleLog = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "adv_console.txt");
    static readonly object Lock = new object();

    static AdventuresDiagLogger()
    {
        CompilationPipeline.compilationStarted += _ => { try { File.WriteAllText(CompileLog, "compile started " + DateTime.Now + "\n"); } catch { } };
        CompilationPipeline.assemblyCompilationFinished += OnAssembly;
        Application.logMessageReceivedThreaded -= OnLog;
        Application.logMessageReceivedThreaded += OnLog;
        Append(ConsoleLog, "--- domain reload " + DateTime.Now + " ---\n");
    }

    static void OnAssembly(string asm, CompilerMessage[] msgs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ASSEMBLY " + Path.GetFileName(asm) + " messages=" + msgs.Length);
        foreach (var m in msgs)
            if (m.type == CompilerMessageType.Error || m.type == CompilerMessageType.Warning && m.message.Contains("Adventures"))
                sb.AppendLine(m.type + ": " + m.file + "(" + m.line + "): " + m.message);
        Append(CompileLog, sb.ToString());
    }

    static void OnLog(string condition, string stack, LogType type)
    {
        if (type == LogType.Log && !condition.StartsWith("[ADV]")) return;
        var s = "[" + type + "] " + condition + "\n";
        if (type == LogType.Exception || type == LogType.Error) s += stack + "\n";
        Append(ConsoleLog, s);
    }

    static void Append(string path, string text)
    {
        lock (Lock) { try { File.AppendAllText(path, text); } catch { } }
    }
}
