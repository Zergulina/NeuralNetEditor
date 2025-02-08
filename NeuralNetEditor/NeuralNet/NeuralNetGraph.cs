using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeuralNetEditor.NeuralNet
{
    internal class NeuralNetGraph
    {
        private List<NeuralLayer> layers;
        public List<List<NeuralLayer>> Cycles { get; set; } = new List<List<NeuralLayer>>();
        private List<InputLayer>  inputLayers = new List<InputLayer>();

        private enum VertexColor
        {
            White,
            Grey,
            Black
        }

        public NeuralNetGraph(List<NeuralLayer> layers)
        {
            this.layers = layers;
            foreach (var layer in layers)
            {
                if (!(layer is InputLayer)) continue;
                inputLayers.Add(layer as InputLayer);
            }
        }

        public bool CheckIsOk()
        {
            bool isOk = true;  

            Cycles.Clear();

            Dictionary<NeuralLayer, VertexColor> dict = new Dictionary<NeuralLayer, VertexColor>();
            foreach (var layer in layers)
            {
                dict.Add(layer, VertexColor.White);
                layer.CheckPreviosLayersCompatibility();
            }

            foreach (var layer in layers)
            {
                if (dict[layer] == VertexColor.White)
                {
                    DFS(layer, null, new List<NeuralLayer>(), dict);
                }
            }
            
            foreach(var cycle in Cycles)
            {
                foreach (var layer in cycle)
                {
                    layer.IsError = true;
                }
            }

            if (Cycles.Count > 0) isOk = false;

            foreach (var cycle in Cycles)
            {
                foreach (var layer in cycle)
                {
                    layer.IsError = true;
                }
            }

            foreach (var layer in layers)
            {
                if (layer.InConnections.Count() == 0 && !(layer is InputLayer))
                {
                    layer.IsError = true;
                    isOk = false;
                }

                var errorLayers = layer.CheckPreviosLayersCompatibility();
                if (errorLayers.Count == 0) continue;
                isOk = false;
                layer.IsError = true;
                errorLayers.ForEach(x => x.IsError = true);
            }

            return isOk;
        }

        private void DFS(NeuralLayer current, NeuralLayer? parent, List<NeuralLayer> path, Dictionary<NeuralLayer, VertexColor> dict)
        {
            dict[current] = VertexColor.Grey;
            path.Add(current);

            foreach (var neighbor in current.OutConnections.Select(x => x.EndLayer))
            {
                if (dict[neighbor] == VertexColor.White)
                {
                    DFS(neighbor, current, path, dict);
                }
                else if (dict[neighbor] == VertexColor.Grey && neighbor != parent)
                {
                    var cycle = new List<NeuralLayer>();
                    for (int i = path.IndexOf(neighbor); i < path.Count; i++)
                    {
                        cycle.Add(path[i]);
                    }
                    Cycles.Add(cycle);
                }
            }

            dict[current] = VertexColor.Black;
            path.RemoveAt(path.Count - 1);
        }
        public string? ConvertToKeras()
        {
            if (!CheckIsOk()) return null;

            var result = "from tensorflow import keras\nfrom tensorflow.keras import layers\n\n";

            var vertexDict = new Dictionary<NeuralLayer, string>();
            var graphBranchNumber = 0;

            //Инициализируем вершины входными слоями
            for (int i = 0; i < inputLayers.Count(); i++)
            {
                vertexDict.Add(inputLayers[i], $"");
            }

            var waitingVertexes = new Dictionary<NeuralLayer, List<(NeuralLayer, string)>>();

            var endVertexList = new List<string>();
            var inputVertexList = new List<string>();

            while (vertexDict.Count > 0)
            {
                var newVertexDict = new Dictionary<NeuralLayer, string>();

                foreach (var vertex in vertexDict)
                {
                    //начинается новая ветка
                    var currentVertex = new KeyValuePair<NeuralLayer, string>(vertex.Key, $"x_{graphBranchNumber++}");
                    result += $"\n{currentVertex.Value} = {currentVertex.Key.ConvertToKeras()}" + (inputLayers.Contains(vertex.Key) ? "" : $"({vertex.Value})") + "\n";
                    if (inputLayers.Contains(currentVertex.Key)) inputVertexList.Add(currentVertex.Value);

                    while (true)
                    {
                        var breakFlag = false;
                        if (currentVertex.Key.OutConnections.Select(x => x.EndLayer).Any(x => x.InConnections.Count > 1))
                        {
                            breakFlag = true;
                            foreach(var multiplyInputVertex in currentVertex.Key.OutConnections.Select(x => x.EndLayer).Where(x => x.InConnections.Count > 1)) {
                                if (waitingVertexes.ContainsKey(multiplyInputVertex))
                                {
                                    waitingVertexes[multiplyInputVertex].Add((currentVertex.Key, currentVertex.Value));
                                }
                                else
                                {
                                    waitingVertexes.Add(multiplyInputVertex, new List<(NeuralLayer, string)> { (currentVertex.Key, currentVertex.Value) });
                                }
                            }
                        }
                        if (currentVertex.Key.OutConnections.Count() > 1)
                        {
                            breakFlag = true;
                            foreach (var nextVertex in currentVertex.Key.OutConnections.Select(x => x.EndLayer))
                            {
                                if (!waitingVertexes.Keys.Any(x => x== nextVertex))
                                {
                                    newVertexDict.Add(nextVertex, currentVertex.Value);
                                }
                            }
                        }
                        if (currentVertex.Key.OutConnections.Count() == 0)
                        {
                            breakFlag = true;
                            endVertexList.Add(currentVertex.Value);
                        }
                        if (breakFlag)
                        {
                            break;
                        }
                        currentVertex = new KeyValuePair<NeuralLayer, string>(currentVertex.Key.OutConnections[0].EndLayer, currentVertex.Value);
                        result += $"{currentVertex.Value} = {currentVertex.Key.ConvertToKeras()}({currentVertex.Value})\n";
                    }

                    var newWaitingVertexes = new Dictionary<NeuralLayer, List<(NeuralLayer, string)>>();

                    foreach (var waitingVertex in waitingVertexes)
                    {
                        if (waitingVertex.Key.InConnections.All(x => waitingVertex.Value.Select(x => x.Item1).Contains(x.StartLayer))) {
                            result += $"\nx_{graphBranchNumber} = layers.add([{string.Join(',', waitingVertex.Value.Select(x => $"{x.Item2}"))}])\n";
                            newVertexDict.Add(waitingVertex.Key, $"x_{graphBranchNumber}");
                        }
                        else
                        {
                            newWaitingVertexes.Add(waitingVertex.Key, waitingVertex.Value);
                        }
                    }
                    waitingVertexes = newWaitingVertexes;
                }
                vertexDict = newVertexDict;
            }

            result += $"\nmodel = keras.Model(inputs=[{string.Join(',', inputVertexList.Select(x => $"{x}"))}], outputs={{{string.Join(',', endVertexList.Select(x => $"\"{x}\": {x}"))}}},)";

            return result;
        }
    }
}
