using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetEditor.NeuralElements.NeuralLayerFactories
{
    internal class InputLayerFactory : NeuralLayerFactory
    {
        public override NeuralLayer GetNeuralLayer() => new InputLayer();
        public override string ToString() => "Input Layer";
    }
}
