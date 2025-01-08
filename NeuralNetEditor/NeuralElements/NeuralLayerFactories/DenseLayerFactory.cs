using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetEditor.NeuralElements.NeuralLayerFactories
{
    internal class DenseLayerFactory : NeuralLayerFactory
    {
        public override NeuralLayer GetNeuralLayer() => new DenseLayer();
        public override string ToString() => "Dense Layer";
    }
}
