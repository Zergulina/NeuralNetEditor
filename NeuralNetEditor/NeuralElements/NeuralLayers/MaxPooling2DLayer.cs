using NeuralNetEditor.Helpers;
using NeuralNetEditor.NeuralElements.ActivationFunctions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeuralNetEditor.NeuralElements.NeuralLayers
{
    internal class MaxPooling2DLayer : NeuralLayer
    {
        public ObservableCollection<uint> KernelSize { get; private set; } = new ObservableCollection<uint>(new uint[2]);
        public MaxPooling2DLayer()
        {
            InputSize = 1;

            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = "MaxPooling2D";
            Canvas.SetLeft(titleTextBlock, 10);
            Canvas.SetTop(titleTextBlock, 10);
            DrawableLayer.Children.Add(titleTextBlock);

            TextBlock kernelTextBlock = new TextBlock() { Text = "(0, 0)" };
            KernelSize.CollectionChanged += (sender, args) =>
            {
                kernelTextBlock.Text = KernelSize.ToArray().ToArrayString();
            };
            Canvas.SetLeft(kernelTextBlock, 10);
            Canvas.SetTop(kernelTextBlock, 25);
            DrawableLayer.Children.Add(kernelTextBlock);
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
