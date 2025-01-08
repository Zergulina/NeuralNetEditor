using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetEditor.NeuralElements.NeuralLayerFactories
{
    internal abstract class NeuralLayerFactory
    {
        public abstract NeuralLayer GetNeuralLayer();
    }
}
