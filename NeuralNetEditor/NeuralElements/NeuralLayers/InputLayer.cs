using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NeuralNetEditor.Helpers;

namespace NeuralNetEditor.NeuralElements.NeuralLayers
{
    internal class InputLayer : NeuralLayer
    {
        public ObservableCollection<uint> InputShape { get; private set; } = new ObservableCollection<uint>();

        private byte inputSize;
        public new byte InputSize {  
            get => inputSize;
            set {
                inputSize = value;
                while (InputShape.Count != inputSize)
                {
                    if (InputShape.Count > inputSize)
                    {
                        InputShape.RemoveAt(InputShape.Count - 1);
                    }
                    else if (InputShape.Count < inputSize) 
                    {
                        InputShape.Add(0);
                    }
                }
            }
        }

        public InputLayer() : base()
        {
            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = "Input";
            Canvas.SetLeft(titleTextBlock, 10);
            Canvas.SetTop(titleTextBlock, 10);
            DrawableLayer.Children.Add(titleTextBlock);

            TextBlock inputShapeTextBlock = new TextBlock();
            inputShapeTextBlock.Text = "( )";
            Canvas.SetLeft(inputShapeTextBlock, 10);
            Canvas.SetTop(inputShapeTextBlock, 25);
            InputShape.CollectionChanged += (sender, args) =>
            {
                inputShapeTextBlock.Text = InputShape.ToArray().ToArrayString();
            };
            DrawableLayer.Children.Add(inputShapeTextBlock);
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
