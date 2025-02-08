using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeuralNetEditor.NeuralElements.NeuralLayers
{
    internal class FlattenLayer : NeuralLayer
    {
        public FlattenLayer()
        {
            OutputSize = 1;

            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = "Flatten";
            Canvas.SetLeft(titleTextBlock, 10);
            Canvas.SetTop(titleTextBlock, 10);
            DrawableLayer.Children.Add(titleTextBlock);
        }
        public override string ConvertToSafeRecord(double xCameraOffset, double yCameraOffset) => $"Flatten {Canvas.GetLeft(DrawableLayer) - xCameraOffset} {Canvas.GetTop(DrawableLayer) - yCameraOffset}";
        public override string ConvertToKeras() => "layers.Flatten()";

        public override List<NeuralLayer> CheckPreviosLayersCompatibility()
        {
            return new List<NeuralLayer>();
        }
    }
}
