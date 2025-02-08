using NeuralNetEditor.NeuralElements;
using NeuralNetEditor.NeuralElements.ActivationFunctions;
using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace NeuralNetEditor.Helpers
{
    internal static class FileWorking
    {
        internal static void SaveToFile(List<NeuralLayer> neuralLayers, List<Connection> connections, double xCameraOffset, double yCameraOffset, string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                var dict = new Dictionary<NeuralLayer, uint>();
                uint index = 0;
                foreach (NeuralLayer layer in neuralLayers)
                {
                    sw.WriteLine(layer.ConvertToSafeRecord(xCameraOffset, yCameraOffset));
                    dict.Add(layer, index++);
                }
                sw.WriteLine("-");
                foreach (Connection connection in connections)
                {
                    sw.WriteLine($"{dict[connection.startLayer]} {dict[connection.endLayer]}");
                }
            }
        }
        internal static (List<NeuralLayer>, List<Connection>) ReadFromFile(string path)
        {
            var neuralLayers = new List<NeuralLayer>();
            var connections = new List<Connection>();


            using (StreamReader sr = new StreamReader(path))
            {
                var dict = new Dictionary<uint, NeuralLayer>();
                uint index = 0;
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == "-") break;

                    var neuralLayerData = line.Split(" ");
                    switch (neuralLayerData[0])
                    {
                        case "Conv2D":
                            var conv2Dlayer = new Conv2DLayer();
                            conv2Dlayer.NeuronAmount = uint.Parse(neuralLayerData[1]);
                            conv2Dlayer.KernelSize[0] = uint.Parse(neuralLayerData[2]);
                            conv2Dlayer.KernelSize[1] = uint.Parse(neuralLayerData[3]);
                            conv2Dlayer.ActivationFunction = GetActivationFunction(neuralLayerData[4]);
                            Canvas.SetLeft(conv2Dlayer.DrawableLayer, double.Parse(neuralLayerData[5]));
                            Canvas.SetTop(conv2Dlayer.DrawableLayer, double.Parse(neuralLayerData[6]));
                            Canvas.SetZIndex(conv2Dlayer.DrawableLayer, 1);
                            neuralLayers.Add(conv2Dlayer);
                            dict.Add(index++, conv2Dlayer);
                            break;
                        case "Dense":
                            var denseLayer = new DenseLayer();
                            denseLayer.NeuronAmount = uint.Parse(neuralLayerData[1]);
                            denseLayer.ActivationFunction = GetActivationFunction(neuralLayerData[2]);
                            Canvas.SetLeft(denseLayer.DrawableLayer, double.Parse(neuralLayerData[3]));
                            Canvas.SetTop(denseLayer.DrawableLayer, double.Parse(neuralLayerData[4]));
                            Canvas.SetZIndex(denseLayer.DrawableLayer, 1);
                            neuralLayers.Add(denseLayer);
                            dict.Add(index++, denseLayer);
                            break;
                        case "Flatten":
                            var flattenLayer = new FlattenLayer();
                            Canvas.SetLeft(flattenLayer.DrawableLayer, double.Parse(neuralLayerData[1]));
                            Canvas.SetTop(flattenLayer.DrawableLayer, double.Parse(neuralLayerData[2]));
                            Canvas.SetZIndex(flattenLayer.DrawableLayer, 1);
                            neuralLayers.Add(flattenLayer);
                            dict.Add(index++, flattenLayer);
                            break;
                        case "Input":
                            var inputLayer = new InputLayer();
                            var inputSize = byte.Parse(neuralLayerData[1]);
                            if (inputSize > 4) throw new Exception();
                            inputLayer.InputSize = inputSize;
                            for (var i = 0; i < inputSize; i++)
                            {
                                inputLayer.InputShape[i] = uint.Parse(neuralLayerData[2 + i]);
                            }
                            Canvas.SetLeft(inputLayer.DrawableLayer, double.Parse(neuralLayerData[2 + inputSize]));
                            Canvas.SetTop(inputLayer.DrawableLayer, double.Parse(neuralLayerData[3 + inputSize]));
                            Canvas.SetZIndex(inputLayer.DrawableLayer, 1);
                            neuralLayers.Add(inputLayer);
                            dict.Add(index++, inputLayer);
                            break;
                        case "MaxPooling2D":
                            var maxPooling2Dlayer = new MaxPooling2DLayer();
                            maxPooling2Dlayer.KernelSize[0] = uint.Parse(neuralLayerData[1]);
                            maxPooling2Dlayer.KernelSize[1] = uint.Parse(neuralLayerData[2]);
                            Canvas.SetLeft(maxPooling2Dlayer.DrawableLayer, double.Parse(neuralLayerData[3]));
                            Canvas.SetTop(maxPooling2Dlayer.DrawableLayer, double.Parse(neuralLayerData[4]));
                            Canvas.SetZIndex(maxPooling2Dlayer.DrawableLayer, 1);
                            neuralLayers.Add(maxPooling2Dlayer);
                            dict.Add(index++, maxPooling2Dlayer);
                            break;
                    }
                }
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var connectionData = line.Split(" ");
                    var startLayer = dict[uint.Parse(connectionData[0])];
                    var endLayer = dict[uint.Parse(connectionData[1])];
                    var connection = new Connection(startLayer, endLayer);
                    startLayer.OutConnections.Add(connection);
                    endLayer.InConnections.Add(connection);
                    connections.Add(connection);
                }

            }
            return (neuralLayers, connections);
        }

        private static ActivationFunction GetActivationFunction(string name) => name switch
        {
            "relu" => new Relu(),
            "sigmoid" => new Sigmoid(),
            "softmax" => new Softmax(),
        };
    }
}
