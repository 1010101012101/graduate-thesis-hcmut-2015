﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using static HCMUT.EMRCorefResol.Logging.LoggerFactory;
using System.Runtime.InteropServices;
using HCMUT.EMRCorefResol.Classification.LibSVM.Internal;
using System.Globalization;

namespace HCMUT.EMRCorefResol.Classification.LibSVM
{
    enum SVMType : int
    {
        C_SVC = 0,
        NU_SVC = 1,
        ONE_CLASS = 2,
        EPSILON_SVR = 3,
        NU_SVR = 4
    }

    enum SVMKernel : int
    {
        LINEAR = 0,
        POLY = 1,
        RBF = 2,
        SIGMOID = 3
    }

    static class LibSVM
    {
        public const string ToolsPath = "LibSVM";
        public const string SVMTrainPath = ToolsPath + "\\svm-train.exe";
        public const string SVMScalePath = ToolsPath + "\\svm-scale.exe";
        public const string SVMPredictPath = ToolsPath + "\\svm-predict.exe";
        public const string DllPath = ToolsPath + "\\libsvm.dll";

        public static void RunSVMScale(double lower, double upper, string sfPath, string dataPath, string scaledDataPath)
        {
            var pInfo = new ProcessStartInfo()
            {
                FileName = SVMScalePath,
                Arguments = $"-l {lower} -u {upper} -s {sfPath} {dataPath}",
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(pInfo);

            using (var sw = new StreamWriter(scaledDataPath))
            {
                sw.Write(p.StandardOutput.ReadToEnd());
            }
        }

        public static void RunSVMScale(string sfPath, string dataPath, string scaledDataPath)
        {
            var pInfo = new ProcessStartInfo()
            {
                FileName = SVMScalePath,
                Arguments = $"-r {sfPath} {dataPath}",
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(pInfo);

            using (var sw = new StreamWriter(scaledDataPath))
            {
                sw.Write(p.StandardOutput.ReadToEnd());
            }
        }

        public static string RunSVMScale(string sfPath, string dataPath)
        {
            var pInfo = new ProcessStartInfo()
            {
                FileName = SVMScalePath,
                Arguments = $"-r {sfPath} {dataPath}",
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(pInfo);
            return p.StandardOutput.ReadToEnd();
        }

        public static void RunSVMTrain(SVMType type, SVMKernel kernel, string dataPath, string modelPath, bool shouldLog)
        {
            var pInfo = new ProcessStartInfo()
            {
                FileName = SVMTrainPath,
                Arguments = $"-s {type} -t {kernel} {dataPath} {modelPath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var p = Process.Start(pInfo);
            var output = p.StandardOutput.ReadToEnd();
            if (shouldLog)
            {
                GetLogger().Info(output);
            }
        }

        public static void RunSVMPredict(string dataPath, string modelPath, string outputPath, bool shouldLog)
        {
            var pInfo = new ProcessStartInfo()
            {
                FileName = SVMPredictPath,
                Arguments = $"{dataPath} {modelPath} {outputPath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var p = Process.Start(pInfo);
            var output = p.StandardOutput.ReadToEnd();
            if (shouldLog)
            {
                GetLogger().Info(output);
            }
        }

        public static SVMModel LoadModel(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
            {
                return null;
            }

            IntPtr ptr_filename = Marshal.StringToHGlobalAnsi(modelPath);

            IntPtr ptr_model = LoadModelNative(ptr_filename);

            Marshal.FreeHGlobal(ptr_filename);
            ptr_filename = IntPtr.Zero;

            if (ptr_model == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                SVMModel model = SVMModel.Convert(ptr_model);

                // There is a little memory leackage here !!!
                FreeModelContent(ptr_model);
                ptr_model = IntPtr.Zero;

                return model;
            }
        }

        public static double Predict(SVMModel model, SVMNode[] x)
        {
            IntPtr ptr_model = SVMModel.Allocate(model);

            if (ptr_model == IntPtr.Zero)
                throw new ArgumentNullException("ptr_model");

            List<SVMNode> nodes = x.Select(a => a.Clone()).ToList();
            nodes.Add(new SVMNode(-1, 0));
            IntPtr ptr_nodes = SVMNode.Allocate(nodes.ToArray());

            double result = PredictNative(ptr_model, ptr_nodes);

            SVMNode.Free(ptr_nodes);
            SVMModel.Free(ptr_model);

            return result;
        }

        public static SVMProblem ReadProblem(string content)
        {
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            SVMProblem problem = new SVMProblem();
            using (var sr = new StringReader(content))
            {
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        break;

                    string[] list = line.Trim().Split(' ');

                    double y = System.Convert.ToDouble(list[0].Trim(), provider);

                    List<SVMNode> nodes = new List<SVMNode>();
                    for (int i = 1; i < list.Length; i++)
                    {
                        string[] temp = list[i].Split(':');
                        SVMNode node = new SVMNode();
                        node.Index = System.Convert.ToInt32(temp[0].Trim());
                        node.Value = System.Convert.ToDouble(temp[1].Trim(), provider);
                        nodes.Add(node);
                    }

                    problem.Add(nodes.ToArray(), y);
                }
            }

            return problem;
        }

        /// <param name="modelPath">string</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "svm_load_model")]
        private static extern IntPtr LoadModelNative(IntPtr modelPath);

        /// <param name="model">svm_model</param>
        /// <param name="x">svm_node[]</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "svm_predict")]
        private static extern double PredictNative(IntPtr model, IntPtr x);

        /// <param name="model">svm_model</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "svm_free_model_content")]
        private static extern void FreeModelContent(IntPtr model);
    }
}
