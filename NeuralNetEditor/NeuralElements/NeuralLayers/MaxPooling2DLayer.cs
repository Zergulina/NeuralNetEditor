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
            InputSize = 3;
            OutputSize = 3;

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
        public override string ConvertToSafeRecord(double xCameraOffset, double yCameraOffset) => $"MaxPooling2D {KernelSize[0]} {KernelSize[1]} {Canvas.GetLeft(DrawableLayer) - xCameraOffset} {Canvas.GetTop(DrawableLayer) - yCameraOffset}";
        public override string ConvertToKeras() => $"layers.MaxPooling2D(({KernelSize[0]}, {KernelSize[1]}))";

        public override List<NeuralLayer> CheckPreviosLayersCompatibility()
        {
            var errorPrevLayers = new List<NeuralLayer>();
            foreach(var prevLayer in InConnections.Select(x => x.StartLayer))
            {
                if (prevLayer.OutputSize != 3) errorPrevLayers.Add(prevLayer);
            }

            return errorPrevLayers;
        }
    }
}
