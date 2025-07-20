using System.Text;

namespace JBK.Tools.OplReader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            DataGrid.AutoGenerateColumns = true;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenOPLFileDialog.ShowDialog() == DialogResult.OK)
            {
                using FileStream fileStream = new FileStream(OpenOPLFileDialog.FileName, FileMode.Open);
                using BinaryReader reader = new BinaryReader(fileStream);

                OPL data = new OPL(reader);

                DataGrid.DataSource = data.Items.Select(item => new
                {
                    Path = Encoding.UTF8.GetString(item.PathBytes),
                    PositionX = item.Position.X,
                    PositionY = item.Position.Y,
                    PositionZ = item.Position.Z,
                    RotationX = item.Rotation.X,
                    RotationY = item.Rotation.Y,
                    RotationZ = item.Rotation.Z,
                    RotationW = item.Rotation.W,
                    ScaleX = item.Scale.X,
                    ScaleY = item.Scale.Y,
                    ScaleZ = item.Scale.Z
                }).ToList();

            }
        }
    }
}
