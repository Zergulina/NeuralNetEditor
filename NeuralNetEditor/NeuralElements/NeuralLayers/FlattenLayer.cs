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
            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = "Flatten";
            Canvas.SetLeft(titleTextBlock, 10);
            Canvas.SetTop(titleTextBlock, 10);
            DrawableLayer.Children.Add(titleTextBlock);
        }
        public override string ConvertToKeras()
        {
            throw new NotImplementedException();
        }

        public override bool CheckPreviosLayerCompatibility(NeuralLayer previosLayer)
        {
            throw new NotImplementedException();
        }
    }
}
