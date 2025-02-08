using NeuralNetEditor.NeuralElements.ActivationFunctions;
using NeuralNetEditor.NeuralElements.NeuralLayerFactories;
using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeuralNetEditor.Helpers
{
    internal class LayerEditorGenerator
    {
        internal static void Generate(ContentControl contentControl, NeuralLayer neuralLayer)
        {
            switch (neuralLayer)
            {
                case InputLayer inputLayer:
                    GenerateInputLayerEditor(contentControl, inputLayer);
                    break;
                case DenseLayer denseLayer:
                    GenerateDenseLayerEditor(contentControl, denseLayer);
                    break;
                case Conv2DLayer conv2DLayer:
                    GenerateConv2DLayerEditor(contentControl, conv2DLayer);
                    break;
                case MaxPooling2DLayer maxPooling2DLayer:
                    GenerateMaxPooling2DLayerEditor(contentControl, maxPooling2DLayer);
                    break;
                case FlattenLayer flattenLayer:
                    GenerateFlattenLayerEditor(contentControl, flattenLayer);
                    break;
            }
        } 

        private static void GenerateInputLayerEditor(ContentControl contentControl, InputLayer inputLayer)
        {
            contentControl.Content = null;
    
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock() { Text = "Input Layer"});

            var inputSizeStackPanel = new StackPanel();
            stackPanel.Children.Add(inputSizeStackPanel);

            inputSizeStackPanel.Orientation = Orientation.Horizontal;
            inputSizeStackPanel.Children.Add(new TextBlock() { Text = "Входной размер:" });

            var inputSizeTextBox = new TextBox() { Text = inputLayer.InputSize.ToString(), Width = 35};
            inputSizeStackPanel.Children.Add(inputSizeTextBox);
            
            var inputShapeStackPanel = new StackPanel();
            inputShapeStackPanel.Orientation = Orientation.Vertical;

            var inputSizeButton = new Button() { Content = "Применить" };
            inputSizeButton.Click += (s, e) => {
                try
                {
                    var inputSize = byte.Parse(inputSizeTextBox.Text);
                    if (inputSize < 1 || inputSize > 3) throw new Exception();
                    inputLayer.InputSize = inputSize;
                }
                catch
                {
                    MessageBox.Show("Необходимо число от 1 до 3 включительно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                inputShapeStackPanel.Children.Clear();
                foreach (var inputShapeUnit in inputLayer.InputShape)
                {
                    TextBox textBox = new TextBox();
                    textBox.Text = inputShapeUnit.ToString();
                    textBox.Width = 200;
                    textBox.Margin = new Thickness(5);

                    inputShapeStackPanel.Children.Add(textBox);
                }
            };
            stackPanel.Children.Add(inputSizeButton);

            foreach (var inputShapeUnit in inputLayer.InputShape)
            {
                TextBox textBox = new TextBox();
                textBox.Text = inputShapeUnit.ToString();
                textBox.Width = 200;
                textBox.Margin = new Thickness(5);

                inputShapeStackPanel.Children.Add(textBox);
            }

            var inputShapeButton = new Button() { Content = "Применить" };
            inputShapeButton.Click += (s, e) =>
            {
                var inputSize = new uint[inputLayer.InputSize];
                try
                {
                    for (int i = 0; i < inputLayer.InputSize; i++)
                    {
                        var textBox = (TextBox)inputShapeStackPanel.Children[i];
                        inputSize[i] = uint.Parse(textBox.Text);
                    }
                }
                catch 
                {
                    MessageBox.Show("Необходимо положительное целое число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                for (int i = 0;i < inputLayer.InputSize; i++)
                {
                    inputLayer.InputShape[i] = inputSize[i];
                }
            };

            stackPanel.Children.Add(inputShapeStackPanel);
            stackPanel.Children.Add(inputShapeButton);

            contentControl.Content = stackPanel;
        }

        private static void GenerateDenseLayerEditor(ContentControl contentControl, DenseLayer denseLayer)
        {
            contentControl.Content = null;

            var editorStackPanel = new StackPanel();
            editorStackPanel.Children.Add(new TextBlock() { Text = "Dense Layer" });

            var inputSizeStackPanel = new StackPanel();
            editorStackPanel.Children.Add(inputSizeStackPanel);

            inputSizeStackPanel.Orientation = Orientation.Horizontal;
            inputSizeStackPanel.Children.Add(new TextBlock() { Text = "Кол-во нейронов:" });

            var inputSizeTextBox = new TextBox() { Text = denseLayer.NeuronAmount.ToString(), Width = 35 };
            inputSizeStackPanel.Children.Add(inputSizeTextBox);

            var inputShapeStackPanel = new StackPanel();
            inputShapeStackPanel.Orientation = Orientation.Vertical;

            var inputSizeButton = new Button() { Content = "Применить" };
            inputSizeButton.Click += (s, e) => {
                try
                {
                    denseLayer.NeuronAmount = uint.Parse(inputSizeTextBox.Text);
                    if ( denseLayer.NeuronAmount == 0 )
                    {
                        throw new Exception();
                    }
                }
                catch {
                    MessageBox.Show("Необходимо целое число больше 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            editorStackPanel.Children.Add(inputSizeButton);

            var comboBox = new ComboBox();
            comboBox.ItemsSource = new ActivationFunction[]
            {
                new Relu(),
                new Sigmoid(),
                new Softmax()
            };
            comboBox.SelectedIndex = denseLayer.ActivationFunction == null ? 0 : denseLayer.ActivationFunction switch
            {
                Relu => 0,
                Sigmoid => 1,
                Softmax => 2,
                _ => 0
            };
            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is ActivationFunction activationFunction)
                {
                    denseLayer.ActivationFunction = activationFunction;
                }
            };

            editorStackPanel.Children.Add(comboBox);

            contentControl.Content = editorStackPanel;
        }

        private static void GenerateConv2DLayerEditor(ContentControl contentControl, Conv2DLayer conv2dLayer)
        {
            contentControl.Content = null;

            var editorStackPanel = new StackPanel();
            editorStackPanel.Children.Add(new TextBlock() { Text = "Conv2D Layer" });

            var inputSizeStackPanel = new StackPanel();
            editorStackPanel.Children.Add(inputSizeStackPanel);

            inputSizeStackPanel.Orientation = Orientation.Horizontal;
            inputSizeStackPanel.Children.Add(new TextBlock() { Text = "Кол-во нейронов:" });

            var inputSizeTextBox = new TextBox() { Text = conv2dLayer.NeuronAmount.ToString(), Width = 35 };
            inputSizeStackPanel.Children.Add(inputSizeTextBox);

            var inputSizeButton = new Button() { Content = "Применить" };
            inputSizeButton.Click += (s, e) => {
                uint neuronAmount;
                try
                {
                    neuronAmount = uint.Parse(inputSizeTextBox.Text);
                    if (neuronAmount == 0)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    MessageBox.Show("Необходимо целое число больше 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                conv2dLayer.NeuronAmount = neuronAmount;
            };
            editorStackPanel.Children.Add(inputSizeButton);

            editorStackPanel.Children.Add(new TextBlock() { Text = "Размер ядра" });

            var kernelSizeStackPanel = new StackPanel();
            editorStackPanel.Children.Add(kernelSizeStackPanel);
            kernelSizeStackPanel.Orientation = Orientation.Horizontal;

            var firstKernelSizeTextBox = new TextBox() { Text = conv2dLayer.KernelSize[0].ToString(), Width = 45 };
            kernelSizeStackPanel.Children.Add(firstKernelSizeTextBox);

            var xKernelSozeTextBlock = new TextBlock() { Text = "x", Width = 10};
            kernelSizeStackPanel.Children.Add(xKernelSozeTextBlock);

            var secondKernelSizeTextBox = new TextBox() { Text = conv2dLayer.KernelSize[1].ToString(), Width = 45 };
            kernelSizeStackPanel.Children.Add(secondKernelSizeTextBox);

            var kernelSizeButton = new Button() { Content = "Применить" };
            kernelSizeButton.Click += (s, e) => {
                uint firstKernelSize, secondKernelSize;
                try
                {
                    firstKernelSize = uint.Parse(firstKernelSizeTextBox.Text);
                    secondKernelSize = uint.Parse(secondKernelSizeTextBox.Text);
                    if (firstKernelSize == 0 || secondKernelSize == 0)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    MessageBox.Show("Необходимо целое число больше 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                conv2dLayer.KernelSize[0] = firstKernelSize;
                conv2dLayer.KernelSize[1] = secondKernelSize;
            };
            editorStackPanel.Children.Add(kernelSizeButton);

            var comboBox = new ComboBox();
            comboBox.ItemsSource = new ActivationFunction[]
            {
                new Relu(),
                new Sigmoid(),
                new Softmax()
            };
            comboBox.SelectedIndex = conv2dLayer.ActivationFunction == null ? 0 : conv2dLayer.ActivationFunction switch
            {
                Relu => 0,
                Sigmoid => 1,
                Softmax => 2,
                _ => 0
            };
            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is ActivationFunction activationFunction)
                {
                    conv2dLayer.ActivationFunction = activationFunction;
                }
            };

            editorStackPanel.Children.Add(comboBox);

            contentControl.Content = editorStackPanel;
        }
        private static void GenerateMaxPooling2DLayerEditor(ContentControl contentControl, MaxPooling2DLayer maxPooling2dLayer)
        {
            contentControl.Content = null;

            var editorStackPanel = new StackPanel();
            editorStackPanel.Children.Add(new TextBlock() { Text = "Conv2D Layer" });


            editorStackPanel.Children.Add(new TextBlock() { Text = "Размер ядра" });

            var kernelSizeStackPanel = new StackPanel();
            editorStackPanel.Children.Add(kernelSizeStackPanel);
            kernelSizeStackPanel.Orientation = Orientation.Horizontal;

            var firstKernelSizeTextBox = new TextBox() { Text = maxPooling2dLayer.KernelSize[0].ToString(), Width = 45 };
            kernelSizeStackPanel.Children.Add(firstKernelSizeTextBox);

            var xKernelSozeTextBlock = new TextBlock() { Text = "x", Width = 10 };
            kernelSizeStackPanel.Children.Add(xKernelSozeTextBlock);

            var secondKernelSizeTextBox = new TextBox() { Text = maxPooling2dLayer.KernelSize[1].ToString(), Width = 45 };
            kernelSizeStackPanel.Children.Add(secondKernelSizeTextBox);

            var kernelSizeButton = new Button() { Content = "Применить" };
            kernelSizeButton.Click += (s, e) => {
                uint firstKernelSize, secondKernelSize;
                try
                {
                    firstKernelSize = uint.Parse(firstKernelSizeTextBox.Text);
                    secondKernelSize = uint.Parse(secondKernelSizeTextBox.Text);
                    if (firstKernelSize == 0 || secondKernelSize == 0)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    MessageBox.Show("Необходимо целое число больше 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                maxPooling2dLayer.KernelSize[0] = firstKernelSize;
                maxPooling2dLayer.KernelSize[1] = secondKernelSize;
            };

            editorStackPanel.Children.Add(kernelSizeButton);

            contentControl.Content = editorStackPanel;
        }

        private static void GenerateFlattenLayerEditor(ContentControl contentControl, FlattenLayer flattenLayer)
        {
            contentControl.Content = null;

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock() { Text = "Flatten Layer" });

            contentControl.Content = stackPanel;
        }
    }
}
