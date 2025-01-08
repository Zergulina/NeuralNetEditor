using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetEditor.NeuralElements.NeuralLayerFactories
{
    internal class Conv2DLayerFactory : NeuralLayerFactory
    {
        public override NeuralLayer GetNeuralLayer() => new Conv2DLayer();
        public override string ToString() => "Conv2D Layer";
    }
}
