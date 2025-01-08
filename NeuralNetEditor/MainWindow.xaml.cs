using NeuralNetEditor.Helpers;
using NeuralNetEditor.NeuralElements;
using NeuralNetEditor.NeuralElements.NeuralLayerFactories;
using NeuralNetEditor.NeuralElements.NeuralLayers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.Pkcs;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuralNetEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindowxaml.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            layerComboBox.ItemsSource = new NeuralLayerFactory[]
            {
                new InputLayerFactory(),
                new DenseLayerFactory(),
                new Conv2DLayerFactory(),
                new MaxPooling2DLayerFactory(),
                new FlattenLayerFactory(),
            };
        }

        private NeuralLayerFactory? selectedLayerFactory = null;
        private NeuralLayer? _selectedLayer = null;
        private NeuralLayer? SelectedLayer {
            get => _selectedLayer;
            set {
                _selectedLayer?.SetIsSelected(false);
                _selectedLayer = value;
                _selectedLayer?.SetIsSelected(true);

                LayerEditorGenerator.Generate(neuralLayerEditor, _selectedLayer);
            }
        }
        private Connection? selectedConnection = null;

        private List<NeuralLayer> layers = new List<NeuralLayer>();

        private Point startDraggingPosition;
        private Point draggingPosition;
        private bool isDragging = false;

        private Point cameraDraggingPosition;
        private Point cameraOffset;
        private bool isCameraDragging = false;

        private Connection? currentConnection = null;

        private bool CheckIsAreaInArea (double x1_1, double x1_2, double y1_1, double y1_2, double x2_1, double x2_2, double y2_1, double y2_2)
        {
            return ((x1_1 >= x2_1 && x1_1 <= x2_2) || (x1_2 >= x2_1 && x1_2 <= x2_2)) && ((y1_1 >= y2_1 && y1_1 <= y2_2) || (y1_2 >= y2_1 && y1_2 <= y2_2));
        }

        private void layerComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (layerComboBox.SelectedItem is NeuralLayerFactory neuralLayerFactory) {
                selectedLayerFactory = neuralLayerFactory;
            }
        }

        private void neuralCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != neuralCanvas) return;
            isCameraDragging = true;
            cameraDraggingPosition = e.GetPosition(neuralCanvas);
        }

        private void neuralCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isCameraDragging)
            {
                isCameraDragging = false;
            }
        }

        private void neuralCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentConnection != null)
            {
                neuralCanvas.Children.Remove(currentConnection.DrawableConnection);
                currentConnection = null;
                return;
            }
            if (selectedLayerFactory != null)
            {
                var layer = selectedLayerFactory.GetNeuralLayer();
                var p = e.GetPosition(neuralCanvas);

                foreach (var l in layers)
                {
                    var x = Canvas.GetLeft(l.DrawableLayer);
                    var y = Canvas.GetTop(l.DrawableLayer);
                    if (CheckIsAreaInArea(x, x + l.DrawableLayer.Width, y, y + l.DrawableLayer.Height, p.X - layer.DrawableLayer.Width / 2, p.X + layer.DrawableLayer.Width / 2, p.Y - layer.DrawableLayer.Height / 2, p.Y + layer.DrawableLayer.Height / 2))
                    {
                        return;
                    }
                }

                layer.DrawableLayer.MouseRightButtonDown += DrawableLayer_MouseRightButtonDown;
                layer.DrawableLayer.MouseLeftButtonDown += DrawableLayer_MouseLeftButtonDown;

                layers.Add(layer);
                Canvas.SetLeft(layer.DrawableLayer, p.X - layer.DrawableLayer.Width/2);
                Canvas.SetTop(layer.DrawableLayer, p.Y - layer.DrawableLayer.Height / 2);
                Canvas.SetZIndex(layer.DrawableLayer, 1);
                neuralCanvas.Children.Add(layer.DrawableLayer);
            }
        }

        private void neuralCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Canvas.SetLeft(SelectedLayer.DrawableLayer, startDraggingPosition.X);
                Canvas.SetTop(SelectedLayer.DrawableLayer, startDraggingPosition.Y);
                Canvas.SetZIndex(SelectedLayer.DrawableLayer, 1);
                isDragging = false;

                foreach (var connection in SelectedLayer.OutConnections)
                {
                    connection.StartPoint = new Point(startDraggingPosition.X + SelectedLayer.DrawableLayer.Width / 2, startDraggingPosition.Y + SelectedLayer.DrawableLayer.Height / 2);
                }

                foreach (var connection in SelectedLayer.InConnections)
                {
                    connection.EndPoint = new Point(startDraggingPosition.X + SelectedLayer.DrawableLayer.Width / 2, startDraggingPosition.Y + SelectedLayer.DrawableLayer.Height / 2);
                }
            }
            if (isCameraDragging)
            {
                isCameraDragging = false;
            }
        }

        private void DrawableLayer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedLayer = layers.Find(x => x.DrawableLayer == sender);
            ContextMenu contextMenu = (ContextMenu)FindResource("neuralLayerContextMenu");
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        private void DrawableLayer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var prevSelectedLayer = SelectedLayer;
            SelectedLayer = layers.Find(x => x.DrawableLayer == sender);
            if (SelectedLayer == null) return;
            if (!isDragging)
            {
                Canvas.SetZIndex(SelectedLayer.DrawableLayer, 2);
                draggingPosition = e.GetPosition(SelectedLayer.DrawableLayer);
                startDraggingPosition = new Point(Canvas.GetLeft(SelectedLayer.DrawableLayer), Canvas.GetTop(SelectedLayer.DrawableLayer));
                isDragging = true;
            }
            if (currentConnection != null && prevSelectedLayer != null && prevSelectedLayer != SelectedLayer)
            {
                foreach (var connection in SelectedLayer.InConnections)
                {
                    if (connection.StartLayer == prevSelectedLayer)
                    {
                        neuralCanvas.Children.Remove(currentConnection.DrawableConnection);
                        currentConnection = null;
                        return;
                    }
                }
                currentConnection.EndLayer = SelectedLayer;
                prevSelectedLayer.OutConnections.Add(currentConnection);
                SelectedLayer.InConnections.Add(currentConnection);
                currentConnection.DrawableConnection.MouseRightButtonDown += DrawableConnection_MouseRightButtonDown;
                currentConnection = null;
            }
        }

        private void DrawableConnection_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedConnection = layers.SelectMany(x => x.InConnections).First(x => x.DrawableConnection == sender);
            ContextMenu contextMenu = (ContextMenu)FindResource("connectionContextMenu");
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        private void neuralCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && isDragging)
            {
                var currentPosition = e.GetPosition(neuralCanvas);
                Canvas.SetLeft(SelectedLayer.DrawableLayer, currentPosition.X - draggingPosition.X);
                Canvas.SetTop(SelectedLayer.DrawableLayer, currentPosition.Y - draggingPosition.Y);

                foreach (var connection in SelectedLayer.OutConnections)
                {
                    connection.StartPoint = new Point(currentPosition.X - draggingPosition.X + SelectedLayer.DrawableLayer.Width / 2, currentPosition.Y - draggingPosition.Y + SelectedLayer.DrawableLayer.Height / 2);
                }

                foreach (var connection in SelectedLayer.InConnections)
                {
                    connection.EndPoint = new Point(currentPosition.X - draggingPosition.X + SelectedLayer.DrawableLayer.Width / 2, currentPosition.Y - draggingPosition.Y + SelectedLayer.DrawableLayer.Height / 2);
                }
            }

            if (currentConnection != null)
            {
                currentConnection.EndPoint = e.GetPosition(neuralCanvas);
            }

            if (isCameraDragging)
            {
                var currentPosition = e.GetPosition(neuralCanvas);
                foreach (UIElement child in neuralCanvas.Children)
                {
                    Canvas.SetLeft(child, Canvas.GetLeft(child) + currentPosition.X - cameraDraggingPosition.X);
                    Canvas.SetTop(child, Canvas.GetTop(child) + currentPosition.Y - cameraDraggingPosition.Y);
                    cameraOffset = new Point(cameraOffset.X + currentPosition.X - cameraDraggingPosition.X, cameraOffset.Y + currentPosition.Y - cameraDraggingPosition.Y);
                }
                cameraDraggingPosition = currentPosition;
            }
        }

        private void neuralCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                var p = e.GetPosition(neuralCanvas);

                foreach (var l in layers)
                {
                    if (l.DrawableLayer == SelectedLayer.DrawableLayer) continue;
                    var x = Canvas.GetLeft(l.DrawableLayer);
                    var y = Canvas.GetTop(l.DrawableLayer);
                    if (CheckIsAreaInArea(x, x + l.DrawableLayer.Width, y, y + l.DrawableLayer.Height, Canvas.GetLeft(SelectedLayer.DrawableLayer), Canvas.GetLeft(SelectedLayer.DrawableLayer) + SelectedLayer.DrawableLayer.Width, Canvas.GetTop(SelectedLayer.DrawableLayer), Canvas.GetTop(SelectedLayer.DrawableLayer) + SelectedLayer.DrawableLayer.Height))
                    {
                        Canvas.SetLeft(SelectedLayer.DrawableLayer, startDraggingPosition.X);
                        Canvas.SetTop(SelectedLayer.DrawableLayer, startDraggingPosition.Y);
                        foreach (var connection in SelectedLayer.OutConnections)
                        {
                            connection.StartPoint = new Point(startDraggingPosition.X + SelectedLayer.DrawableLayer.Width / 2, startDraggingPosition.Y + SelectedLayer.DrawableLayer.Height / 2);
                        }

                        foreach (var connection in SelectedLayer.InConnections)
                        {
                            connection.EndPoint = new Point(startDraggingPosition.X + SelectedLayer.DrawableLayer.Width / 2, startDraggingPosition.Y + SelectedLayer.DrawableLayer.Height / 2);
                        }
                    }
                    Canvas.SetZIndex(SelectedLayer.DrawableLayer, 1);
                }
            }
        }

        private void DeleteLayerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLayer == null) return;

            foreach (var connection in SelectedLayer.OutConnections)
            {
                connection.EndLayer.InConnections.Remove(connection);
                neuralCanvas.Children.Remove(connection.DrawableConnection);
            }
            foreach (var connection in SelectedLayer.InConnections)
            {
                connection.StartLayer.OutConnections.Remove(connection);
                neuralCanvas.Children.Remove(connection.DrawableConnection);
            }
            layers.Remove(SelectedLayer);
            neuralCanvas.Children.Remove(SelectedLayer.DrawableLayer);
        }

        private void AddConnectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLayer == null) return;
            currentConnection = new Connection(SelectedLayer);
            neuralCanvas.Children.Add(currentConnection.DrawableConnection);
        }

        private void DeleteConnectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectedConnection == null) return;

            selectedConnection.StartLayer.OutConnections.Remove(selectedConnection);
            selectedConnection.EndLayer.InConnections.Remove(selectedConnection);

            neuralCanvas.Children.Remove(selectedConnection.DrawableConnection);

            selectedConnection = null;
        }
    }
}
