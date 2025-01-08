using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetEditor.NeuralElements.NeuralLayerFactories
{
    internal class MaxPooling2DLayerFactory : NeuralLayerFactory
    {
        public override NeuralLayer GetNeuralLayer() => new MaxPooling2DLayer();
        public override string ToString() => "MaxPooling2D Layer";
    }
}
