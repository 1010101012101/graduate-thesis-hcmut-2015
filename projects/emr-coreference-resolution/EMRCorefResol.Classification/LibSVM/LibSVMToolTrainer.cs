﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using static HCMUT.EMRCorefResol.Logging.LoggerFactory;

namespace HCMUT.EMRCorefResol.Classification.LibSVM
{
    public class LibSVMToolTrainer : ITrainer
    {
        private readonly LibSVMToolClassifier _classifier;

        private readonly string _saveDir;
        private readonly string _problemDir, _modelDir;

        public string ModelsDir { get { return _modelDir; } }

        public LibSVMToolTrainer(string saveDir)
        {
            _saveDir = saveDir;
            _problemDir = Path.Combine(_saveDir, "Problems");
            _modelDir = Path.Combine(_saveDir, "Models");

            Directory.CreateDirectory(_problemDir);
            Directory.CreateDirectory(_modelDir);

            _classifier = new LibSVMToolClassifier(_modelDir);
        }

        public LibSVMToolTrainer() : this("Classification\\LibSVMTools") { }

        public IClasProblemSerializer ProblemSerializer
        {
            get { return LibSVMProblemSerializer.Instance; }
        }

        public IClassifier GetClassifier()
        {
            return _classifier;
        }

        public void Train(Type instanceType, ClasProblem problem)
        {
            var name = instanceType.Name;

            var rawPrbPath = Path.Combine(_problemDir, $"{name}-training.prb");
            var scaledPrbPath = Path.Combine(_problemDir, $"{name}-training.scaled");
            var sfPath = Path.Combine(_modelDir, $"{name}.sf");
            var modelPath = Path.Combine(_modelDir, $"{name}.model");

            ProblemSerializer.Serialize(problem, rawPrbPath);

            // scale
            LibSVMTools.RunSVMScale(0d, 1d, sfPath, rawPrbPath, scaledPrbPath);

            // train
            GetLogger().Info($"Training {name} problem...");
            LibSVMTools.RunSVMTrain(SVMType.C_SVC, SVMKernel.RBF, scaledPrbPath, modelPath);
        }

        public void Train<T>(ClasProblem problem) where T : IClasInstance
        {
            Train(typeof(T), problem);
        }
    }
}
