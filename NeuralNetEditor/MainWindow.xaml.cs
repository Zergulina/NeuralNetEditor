using Microsoft.Win32;
using NeuralNetEditor.Helpers;
using NeuralNetEditor.NeuralElements;
using NeuralNetEditor.NeuralElements.NeuralLayerFactories;
using NeuralNetEditor.NeuralElements.NeuralLayers;
using NeuralNetEditor.NeuralNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
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

            TextBlock xCursorPosTextBox = new TextBlock();
            xCursorPosTextBox.SetBinding(TextBlock.TextProperty, new Binding("XCursorPos")
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = "Координата X: {0}"
            });
            navigationStackPanel.Children.Add(xCursorPosTextBox);

            TextBlock yCursorPosTextBox = new TextBlock();
            yCursorPosTextBox.SetBinding(TextBlock.TextProperty, new Binding("YCursorPos")
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = "Координата Y: {0}"
            });
            navigationStackPanel.Children.Add(yCursorPosTextBox);
            
            Button toStartPosButton = new Button();
            toStartPosButton.Content = "Начальная позиция";
            toStartPosButton.Click += (s, e) =>
            {
                foreach (var layer in layers)
                {
                    Canvas.SetLeft(layer.DrawableLayer, Canvas.GetLeft(layer.DrawableLayer) - XCameraOffset);
                    Canvas.SetTop(layer.DrawableLayer, Canvas.GetTop(layer.DrawableLayer) - YCameraOffset);
                }
                foreach (var connection in connections)
                {
                    connection.StartPoint = new Point(connection.StartPoint.X - XCameraOffset, connection.StartPoint.Y - YCameraOffset);
                    connection.EndPoint = new Point(connection.EndPoint.X - XCameraOffset, connection.EndPoint.Y - YCameraOffset);
                }
                if (currentConnection != null)
                {
                    currentConnection.StartPoint = new Point(currentConnection.StartPoint.X - XCameraOffset, currentConnection.StartPoint.Y - YCameraOffset);
                    currentConnection.EndPoint = new Point(currentConnection.EndPoint.X - XCameraOffset, currentConnection.EndPoint.Y - YCameraOffset);
                }
                XCameraOffset = 0;
                YCameraOffset = 0;
            };
            navigationStackPanel.Children.Add(toStartPosButton);
        }

        private NeuralLayerFactory? selectedLayerFactory = null;
        private NeuralLayer? _selectedLayer = null;
        private NeuralLayer? SelectedLayer {
            get => _selectedLayer;
            set {
                if (_selectedLayer != null) _selectedLayer.IsSelected = false;
                _selectedLayer = value;
                if (_selectedLayer != null) { 
                    _selectedLayer.IsSelected = true;
                    LayerEditorGenerator.Generate(neuralLayerEditor, _selectedLayer);
                }
            }
        }
        private Connection? selectedConnection = null;

        private List<NeuralLayer> layers = new List<NeuralLayer>();
        private List<Connection> connections= new List<Connection>();

        private Point startDraggingPosition;
        private Point draggingPosition;
        private bool isDragging = false;

        private string? filename;
        public string? Filename
        {
            get => filename;
            set
            {
                if (filename != value)
                {
                    filename = value;
                    safeMenuItem.IsEnabled = filename != null;
                }
            }
        }

        private Point cameraDraggingPosition;
        public double XCameraOffset { get; set; }
        public double YCameraOffset { get; set; }
        private double xCursorPos;
        public double XCursorPos
        {
            get
            {
                return xCursorPos;
            }
            set
            {
                if (value != xCursorPos)
                {
                    xCursorPos = value;
                    OnPropertyChanged("XCursorPos");
                }
            }
        }
        private double yCursorPos;
        public double YCursorPos
        {
            get
            {
                return yCursorPos;
            }
            set
            {
                if (value != yCursorPos)
                {
                    yCursorPos = value;
                    OnPropertyChanged("YCursorPos");
                }
            }
        }
        private bool isCameraDragging = false;

        private bool isRBClickedOnNeuralLayer = false;

        private Connection? currentConnection = null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            if (isRBClickedOnNeuralLayer)
            {
                isRBClickedOnNeuralLayer = false;
                return;
            }
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
            isRBClickedOnNeuralLayer = true;
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
                if (SelectedLayer.OutConnections.Any(x => x.EndLayer ==  prevSelectedLayer))
                {
                    neuralCanvas.Children.Remove(currentConnection.DrawableConnection);
                    currentConnection = null;
                    return;
                }
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
                connections.Add(currentConnection);
                currentConnection = null;
            }
        }

        private void DrawableConnection_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isRBClickedOnNeuralLayer = true;
            selectedConnection = layers.SelectMany(x => x.InConnections).First(x => x.DrawableConnection == sender);
            ContextMenu contextMenu = (ContextMenu)FindResource("connectionContextMenu");
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        private void neuralCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var currentPosition = e.GetPosition(neuralCanvas);
            XCursorPos = currentPosition.X - XCameraOffset;
            YCursorPos = currentPosition.Y + YCameraOffset;
            if (e.LeftButton == MouseButtonState.Pressed && isDragging)
            {
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
                if (currentConnection != null)
                {
                    currentConnection.StartPoint = new Point(currentConnection.StartPoint.X + currentPosition.X - cameraDraggingPosition.X, currentConnection.StartPoint.Y + currentPosition.Y - cameraDraggingPosition.Y);
                    currentConnection.EndPoint = new Point(currentConnection.EndPoint.X + currentPosition.X - cameraDraggingPosition.X, currentConnection.EndPoint.Y + currentPosition.Y - cameraDraggingPosition.Y);
                }
                foreach (NeuralLayer layer in layers)
                {
                    Canvas.SetLeft(layer.DrawableLayer, Canvas.GetLeft(layer.DrawableLayer) + currentPosition.X - cameraDraggingPosition.X);
                    Canvas.SetTop(layer.DrawableLayer, Canvas.GetTop(layer.DrawableLayer) + currentPosition.Y - cameraDraggingPosition.Y);
                }
                foreach (Connection connection in connections)
                {
                    connection.StartPoint = new Point(connection.StartPoint.X + currentPosition.X - cameraDraggingPosition.X, connection.StartPoint.Y + currentPosition.Y - cameraDraggingPosition.Y);
                    connection.EndPoint = new Point(connection.EndPoint.X + currentPosition.X - cameraDraggingPosition.X, connection.EndPoint.Y + currentPosition.Y - cameraDraggingPosition.Y);
                }
                XCameraOffset = XCameraOffset + currentPosition.X - cameraDraggingPosition.X;
                YCameraOffset = YCameraOffset + currentPosition.Y - cameraDraggingPosition.Y;
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
                connections.Remove(connection);
            }
            foreach (var connection in SelectedLayer.InConnections)
            {
                connection.StartLayer.OutConnections.Remove(connection);
                neuralCanvas.Children.Remove(connection.DrawableConnection);
                connections.Remove(connection);
            }
            if (currentConnection != null)
            {
                neuralCanvas.Children.Remove(currentConnection.DrawableConnection);
                currentConnection = null;
                return;
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

            connections.Remove(selectedConnection);

            selectedConnection = null;
        }

        private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Новый проект"; 
            saveFileDialog.DefaultExt = ".nne"; 
            saveFileDialog.Filter = "Файлы NeuralNetEditor (.nne)|*.nne"; 
            saveFileDialog.Title = "Схоранить как";

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string filename = saveFileDialog.FileName;
                FileWorking.SaveToFile(layers, connections, XCameraOffset, YCameraOffset, filename);
                Filename = filename;
            }
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "Новый проект";
            openFileDialog.DefaultExt = ".nne";
            openFileDialog.Filter = "Файлы NeuralNetEditor (.nne)|*.nne";
            openFileDialog.Title = "Сохранить как";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filename = openFileDialog.FileName;
                (layers, connections) = FileWorking.ReadFromFile(filename);
                Filename = filename;
                XCameraOffset = 0;
                YCameraOffset = 0;
                neuralCanvas.Children.Clear();
                currentConnection = null;
                foreach (var layer in layers)
                {
                    neuralCanvas.Children.Add(layer.DrawableLayer);
                    layer.DrawableLayer.MouseRightButtonDown += DrawableLayer_MouseRightButtonDown;
                    layer.DrawableLayer.MouseLeftButtonDown += DrawableLayer_MouseLeftButtonDown;
                }
                foreach (var connection in connections)
                {
                    neuralCanvas.Children.Add(connection.DrawableConnection);
                    connection.DrawableConnection.MouseRightButtonDown += DrawableConnection_MouseRightButtonDown;
                }
            }
        }

        private void SafeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Filename == null) return;
            FileWorking.SaveToFile(layers, connections, XCameraOffset, YCameraOffset, Filename);
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Filename = null;
            XCameraOffset = 0;
            YCameraOffset = 0;
            neuralCanvas.Children.Clear();
            layers.Clear();
            connections.Clear();
        }

        private void CheckErrorButton_Click(object sender, RoutedEventArgs e)
        {
            NeuralNetGraph neuralLayerGraph = new NeuralNetGraph(layers);
            neuralLayerGraph.CheckIsOk(); 
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            NeuralNetGraph neuralLayerGraph = new NeuralNetGraph(layers);
            var pyCode = neuralLayerGraph.ConvertToKeras();
            if (pyCode != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "Новая нейросеть";
                saveFileDialog.DefaultExt = ".py";
                saveFileDialog.Filter = "Файлы Python (.py)|*.py";
                saveFileDialog.Title = "Сохранить код нейросети как";

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filename = saveFileDialog.FileName;
                    using (StreamWriter sw = new StreamWriter(filename, false))
                    {
                        sw.WriteLine(pyCode);
                    }
                }
            }
        }
    }
}