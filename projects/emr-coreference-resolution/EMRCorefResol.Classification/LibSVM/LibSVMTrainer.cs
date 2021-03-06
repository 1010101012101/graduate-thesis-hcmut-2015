﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using HCMUT.EMRCorefResol.Classification.LibSVM.Internal;

namespace HCMUT.EMRCorefResol.Classification.LibSVM
{
    public class LibSVMTrainer : ITrainer
    {
        private LibSVMClassifier _classifier;
        private readonly string _saveDir;

        public string ModelsDir { get { return _saveDir; } }

        public LibSVMTrainer(string saveDir)
        {
            _saveDir = saveDir;
        }

        public LibSVMTrainer() : this(string.Empty) { }

        public IClasProblemSerializer ProblemSerializer
        {
            get { return LibSVMProblemSerializer.Instance; }
        }

        public IClassifier GetClassifier()
        {
            if (_classifier == null)
            {
                _classifier = new LibSVMClassifier(_saveDir);
            }
            return _classifier;
        }

        public void Train(Type instanceType, ClasProblem problem, GenericConfig config)
        {
            var name = instanceType.Name;
            var prbPath = Path.Combine(_saveDir, $"{name}.prb");
            ProblemSerializer.Serialize(problem, prbPath, true);
            problem = null;

            Train(instanceType, prbPath, config);
        }

        public void Train<T>(ClasProblem problem, GenericConfig config) where T : IClasInstance
        {
            Train(typeof(T), problem, config);
        }

        public void Train<T>(string problemPath, GenericConfig config) where T : IClasInstance
        {
            Train(typeof(T), problemPath, config);
        }

        public void Train(Type instanceType, string problemPath, GenericConfig config)
        {
            var name = instanceType.Name;

            //var scaledPrbPath = Path.Combine(_problemDir, $"{name}-Train.scaled");
            //var sfPath = Path.Combine(_modelDir, $"{name}.sf");
            var modelPath = Path.Combine(_saveDir, $"{name}.model");

            // scale
            //Console.WriteLine("Scaling problem...");
            //var scaledData = LibSVM.RunSVMScale(0d, 1d, sfPath, problemPath);

            Console.WriteLine("Reading problems...");
            var data = File.ReadAllText(problemPath);
            var problem = LibSVM.ReadProblem(data);

            if (problem.Length <= 0)
            {
                Console.WriteLine("Problem contains no instances.");
                return;
            }

            Console.WriteLine("Calculating cost weights...");
            int[] labels; double[] weights;
            LibSVM.CalcWeights(problem, out labels, out weights);
            Console.WriteLine("Cost weights: " +
                $"{string.Join(" ", Enumerable.Range(0, labels.Length).Select(i => $"{labels[i]}:{weights[i]:N2}"))}");
            
            var svmParam = new SVMParameter()
            {
                Type = SVMType.C_SVC,
                Kernel = SVMKernel.RBF,
                C = 1,
                Gamma = 1d / problem.GetDimension(),
                Probability = true,
                Shrinking = false
            };

            {
                bool applyWeights = false;
                if (config != null && config.TryGetConfig(LibSVMConfig.ApplyWeights, out applyWeights))
                {                    
                    if (applyWeights)
                    {
                        Console.WriteLine("Applying the above weights...");
                        svmParam.WeightLabels = labels;
                        svmParam.Weights = weights;
                    }
                }
            }

            {
                var cacheSize = 100;
                if (config != null && config.TryGetConfig(LibSVMConfig.CacheSize, out cacheSize))
                {
                    svmParam.CacheSize = cacheSize;
                }
            }

            {
                double cost = -1, gamma = -1;

                if (config != null)
                {
                    if (config.TryGetConfig(LibSVMConfig.Gamma, out gamma))
                    {
                        svmParam.Gamma = gamma;
                    }

                    if (config.TryGetConfig(LibSVMConfig.Cost, out cost))
                    {
                        svmParam.C = cost;
                    }
                }
            }

            data = null;
            problem = null;

            // train
            bool shouldLog = false;
            if (config != null)
            {
                config.TryGetConfig(LibSVMConfig.ShouldLog, out shouldLog);
            }

            Console.WriteLine($"Training using parameters: c={svmParam.C}, g={svmParam.Gamma}, cacheSize={svmParam.CacheSize}...");
            LibSVM.RunSVMTrain(svmParam, problemPath, modelPath, shouldLog);
            Console.WriteLine("Done!");
        }
    }
}
