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
    internal class Conv2DLayer : NeuralLayer
    {
        private uint neuronAmount = 0;
        public uint NeuronAmount
        {
            get
            {
                return neuronAmount;
            }
            set
            {
                if (neuronAmount != value)
                {
                    neuronAmount = value;
                    OnPropertyChanged("NeuronAmount");
                }
            }
        }
        public ActivationFunction activationFunction = new Relu();
        public ActivationFunction ActivationFunction
        {
            get
            {
                return activationFunction;
            }
            set
            {
                if (activationFunction != value)
                {
                    activationFunction = value;
                    OnPropertyChanged("ActivationFunction");
                }
            }
        }

        public ObservableCollection<uint> KernelSize { get; private set; } = new ObservableCollection<uint>(new uint[2]);
        public Conv2DLayer()
        {
            InputSize = 1;

            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = "Conv2D";
            Canvas.SetLeft(titleTextBlock, 10);
            Canvas.SetTop(titleTextBlock, 10);
            DrawableLayer.Children.Add(titleTextBlock);

            TextBlock neuronAmountTextBox = new TextBlock();
            neuronAmountTextBox.SetBinding(TextBlock.TextProperty, new Binding("NeuronAmount")
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = "Нейроны: {0}"
            });
            Canvas.SetLeft(neuronAmountTextBox, 10);
            Canvas.SetTop(neuronAmountTextBox, 25);
            DrawableLayer.Children.Add(neuronAmountTextBox);

            TextBlock kernelTextBlock = new TextBlock() { Text = "(0, 0)" };
            KernelSize.CollectionChanged += (sender, args) =>
            {
                kernelTextBlock.Text = KernelSize.ToArray().ToArrayString();
            };
            Canvas.SetLeft(kernelTextBlock, 10);
            Canvas.SetTop(kernelTextBlock, 40);
            DrawableLayer.Children.Add(kernelTextBlock);

            TextBlock activationFunctionTextBox = new TextBlock();
            activationFunctionTextBox.SetBinding(TextBlock.TextProperty, new Binding("ActivationFunction")
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });
            Canvas.SetLeft(activationFunctionTextBox, 10);
            Canvas.SetTop(activationFunctionTextBox, 55);
            DrawableLayer.Children.Add(activationFunctionTextBox);
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
