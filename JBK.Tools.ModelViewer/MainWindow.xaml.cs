using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace JBK.Tools.ModelViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point? lastMousePosition;
        private double lastZoomDistance;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCamera();
        }

        private void InitializeCamera()
        {
            camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 10),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 60
            };
            viewport3D.Camera = camera;
            lastZoomDistance = camera.Position.Z;
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Model Files (*.gb)|*.gb|All Files (*.*)|*.*",
                Title = "Select a Model File"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                ModelFileFormat.ModelFileFormat fileFormat = new ModelFileFormat.ModelFileFormat();
                fileFormat.Read(openFileDialog.FileName);
                Load(fileFormat);
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveMeshToFbx(_CurrentLoaded, "C:\\Users\\Jascha\\Desktop\\exported_model.dae");
        }

        private Model3DGroup _CurrentLoaded;

        private void Load(ModelFileFormat.ModelFileFormat file)
        {
            DiffuseMaterial sampleMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
            _CurrentLoaded = new Model3DGroup();

            foreach (var mesh in file.meshes)
            {
                GeometryModel3D model = new GeometryModel3D();
                Point3DCollection positions = new Point3DCollection(mesh.Header.vertex_count);
                Vector3DCollection normals = new Vector3DCollection();

                ushort[] rawIndices = mesh.Header.face_type == 1 ? ConvertTriangleStripToList(mesh.Indices).ToArray() : mesh.Indices;
                Int32Collection indices = new Int32Collection(rawIndices.Select(i => (int)i));

                foreach (var item in mesh.Vertecies)
                {
                    switch ((ModelLoader.Enums.VertexType)mesh.Header.vertex_type)
                    {
                        case ModelLoader.Enums.VertexType.Rigid:
                            ModelFileFormat.VertexRigid rigid = (ModelFileFormat.VertexRigid)item;
                            positions.Add(new Point3D(rigid.Position.X, rigid.Position.Y, rigid.Position.Z));
                            normals.Add(new Vector3D(rigid.Normal.X, rigid.Normal.Y, rigid.Normal.Z));
                            break;
                        case ModelLoader.Enums.VertexType.RigidDouble:
                            ModelFileFormat.VertexRigidDouble rigidDouble = (ModelFileFormat.VertexRigidDouble)item;
                            positions.Add(new Point3D(rigidDouble.Position.X, rigidDouble.Position.Y, rigidDouble.Position.Z));
                            normals.Add(new Vector3D(rigidDouble.Normal.X, rigidDouble.Normal.Y, rigidDouble.Normal.Z));
                            break;
                        case ModelLoader.Enums.VertexType.Blend1:
                            ModelFileFormat.VertexBlend1 blend1 = (ModelFileFormat.VertexBlend1)item;
                            positions.Add(new Point3D(blend1.Position.X, blend1.Position.Y, blend1.Position.Z));
                            normals.Add(new Vector3D(blend1.Normal.X, blend1.Normal.Y, blend1.Normal.Z));
                            break;
                        case ModelLoader.Enums.VertexType.Blend2:
                            ModelFileFormat.VertexBlend2 blend2 = (ModelFileFormat.VertexBlend2)item;
                            positions.Add(new Point3D(blend2.Position.X, blend2.Position.Y, blend2.Position.Z));
                            normals.Add(new Vector3D(blend2.Normal.X, blend2.Normal.Y, blend2.Normal.Z));
                            break;
                        case ModelLoader.Enums.VertexType.Blend3:
                            ModelFileFormat.VertexBlend3 blend3 = (ModelFileFormat.VertexBlend3)item;
                            positions.Add(new Point3D(blend3.Position.X, blend3.Position.Y, blend3.Position.Z));
                            normals.Add(new Vector3D(blend3.Normal.X, blend3.Normal.Y, blend3.Normal.Z));
                            break;
                        case ModelLoader.Enums.VertexType.Blend4:
                            ModelFileFormat.VertexBlend4 blend4 = (ModelFileFormat.VertexBlend4)item;
                            positions.Add(new Point3D(blend4.Position.X, blend4.Position.Y, blend4.Position.Z));
                            normals.Add(new Vector3D(blend4.Normal.X, blend4.Normal.Y, blend4.Normal.Z));
                            break;
                        default: throw new ArgumentException("Unsupported vertex type: " + mesh.Header.vertex_type);
                    }
                }

                model.Geometry = new MeshGeometry3D
                {
                    Positions = positions,
                    Normals = normals,
                    TriangleIndices = indices
                };

                model.Material = sampleMaterial;
                _CurrentLoaded.Children.Add(model);
            }

            // Create a ModelVisual3D and add it to the Viewport
            var modelVisual = new ModelVisual3D { Content = _CurrentLoaded };
            viewport3D.Children.Add(modelVisual);
        }

        private void RotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //rotate.Angle = rotationSlider.Value;
        }

        private List<ushort> ConvertTriangleStripToList(ushort[] strip)
        {
            var list = new List<ushort>();
            for (int i = 0; i + 2 < strip.Length; i++)
            {
                ushort i0 = strip[i];
                ushort i1 = strip[i + 1];
                ushort i2 = strip[i + 2];

                // Skip degenerate triangles
                if (i0 == i1 || i1 == i2 || i0 == i2)
                    continue;

                if (i % 2 == 0)
                    list.AddRange(new[] { i0, i1, i2 });
                else
                    list.AddRange(new[] { i1, i0, i2 });
            }
            return list;
        }

        // Class variables for zoom control
        //private const double MinZoomDistance = 1.0;
        //private const double MaxZoomDistance = 100.0;
        private const double ZoomSensitivity = 0.010;

        private void viewport3D_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Zoom(e.Delta);
            e.Handled = true;

        }

        private void Zoom(int delta)
        {
            const double zoomFactor = 0.001;

            // Calculate current distance from camera to target (assuming origin as target)
            Vector3D cameraToTarget = new Point3D(0, 0, 0) - camera.Position;
            double currentDistance = cameraToTarget.Length;

            // Calculate zoom amount (proportional to current distance)
            double zoom = delta * zoomFactor * currentDistance;

            // Get normalized look direction
            Vector3D lookDirection = camera.LookDirection;
            lookDirection.Normalize();

            // Calculate new position
            Point3D newPosition = camera.Position + lookDirection * zoom;

            // Calculate new distance
            Vector3D newCameraToTarget = new Point3D(0, 0, 0) - newPosition;
            double newDistance = newCameraToTarget.Length;

            // Don't zoom through the target (keep minimum distance)
            const double minDistance = 0.5;
            if (newDistance > minDistance)
            {
                camera.Position = newPosition;
            }
        }

        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                lastMousePosition = e.GetPosition(viewport3D);
                viewport3D.CaptureMouse();
            }
        }

        private void Viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                viewport3D.ReleaseMouseCapture();
                lastMousePosition = null;
            }
        }

        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastMousePosition.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(viewport3D);
                Vector delta = currentPosition - lastMousePosition.Value;

                RotateCamera(delta.X * 0.5, delta.Y * 0.5);

                lastMousePosition = currentPosition;
            }
        }

        private void RotateCamera(double horizontalAngle, double verticalAngle)
        {
            // Create rotation transforms
            var horizontalRot = new AxisAngleRotation3D(new Vector3D(0, 1, 0), horizontalAngle);
            var verticalRot = new AxisAngleRotation3D(new Vector3D(1, 0, 0), verticalAngle);

            // Create transform groups
            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new RotateTransform3D(horizontalRot));
            transformGroup.Children.Add(new RotateTransform3D(verticalRot));

            // Apply transform to camera position
            Point3D rotatedPosition = transformGroup.Transform(camera.Position);
            camera.Position = rotatedPosition;

            // Update look direction (pointing at origin)
            camera.LookDirection = new Point3D(0, 0, 0) - camera.Position;
        }

        public void SaveMeshToFbx(Model3DGroup modelGroup, string filePath)
        {
            // Create Assimp scene
            var scene = new Assimp.Scene();

            // Create root node
            var rootNode = new Assimp.Node("Root");
            scene.RootNode = rootNode;

            // Convert each GeometryModel3D to Assimp mesh
            int meshIndex = 0;
            foreach (var model in modelGroup.Children)
            {
                if (model is GeometryModel3D geometryModel)
                {
                    var mesh = ConvertToAssimpMesh(geometryModel, meshIndex++);
                    scene.Meshes.Add(mesh);

                    // Create node for this mesh
                    var node = new Assimp.Node($"Mesh_{meshIndex}");
                    node.MeshIndices.Add(scene.MeshCount - 1);
                    rootNode.Children.Add(node);
                }
            }

            // Export to FBX binary
            var exporter = new Assimp.AssimpContext();
            var exportFormats = exporter.GetSupportedExportFormats();
            bool exported = exporter.ExportFile(scene, filePath, "dae",
                Assimp.PostProcessSteps.None); // or add appropriate post-processing steps
        }

        private Assimp.Mesh ConvertToAssimpMesh(GeometryModel3D geometryModel, int index)
        {
            var meshGeometry = geometryModel.Geometry as MeshGeometry3D;
            if (meshGeometry == null)
                throw new ArgumentException("Model must contain MeshGeometry3D");

            var mesh = new Assimp.Mesh($"Mesh_{index}", Assimp.PrimitiveType.Triangle);

            // Convert positions
            foreach (var point in meshGeometry.Positions)
            {
                mesh.Vertices.Add(new Assimp.Vector3D((float)point.X, (float)point.Y, (float)point.Z));
            }

            // Convert triangles
            for (int i = 0; i < meshGeometry.TriangleIndices.Count; i += 3)
            {
                var face = new Assimp.Face();
                face.Indices.Add(meshGeometry.TriangleIndices[i]);
                face.Indices.Add(meshGeometry.TriangleIndices[i + 1]);
                face.Indices.Add(meshGeometry.TriangleIndices[i + 2]);
                mesh.Faces.Add(face);
            }

            // Convert normals if available
            if (meshGeometry.Normals != null && meshGeometry.Normals.Count == meshGeometry.Positions.Count)
            {
                mesh.Normals.AddRange(meshGeometry.Normals
                    .Select(n => new Assimp.Vector3D((float)n.X, (float)n.Y, (float)n.Z)));
            }

            // Convert texture coordinates if available
            if (meshGeometry.TextureCoordinates != null && meshGeometry.TextureCoordinates.Count == meshGeometry.Positions.Count)
            {
                var texCoords = new List<Assimp.Vector3D>();
                foreach (var texCoord in meshGeometry.TextureCoordinates)
                {
                    texCoords.Add(new Assimp.Vector3D((float)texCoord.X, (float)texCoord.Y, 0));
                }
                mesh.TextureCoordinateChannels[0].AddRange(texCoords);
            }

            return mesh;
        }
    }
}