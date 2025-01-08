using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetEditor.NeuralElements.NeuralLayerFactories
{
    internal class FlattenLayerFactory : NeuralLayerFactory
    {
        public override NeuralLayer GetNeuralLayer() => new FlattenLayer();
        public override string ToString() => "Flatten Layer";
    }
}
