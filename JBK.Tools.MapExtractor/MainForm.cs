using JBK.Tools.MapExtractor.Maps;
using System.Data;

namespace JBK.Tools.MapExtractor
{
    public partial class MainForm : Form
    {
        ClientMap? _map;

        public MainForm()
        {
            InitializeComponent();
            DataHeightmap.AutoGenerateColumns = true;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (OpenClientMapDialog.ShowDialog() == DialogResult.OK)
                {
                    _map = new ClientMap(OpenClientMapDialog.FileName);

                    SaveImageDialog.FileName = $"map_{_map.X}_{_map.Y}";
                    PicHeightmap.Image = _map.HeightMap.GetImage();
                    PicColormap.Image = _map.ColorMap.GetImage();
                    PicObjectmap.Image = _map.ObjectMap.GetImage();
                    PicTexmap1.Image = _map.TextureMaps[0]?.GetImage();
                    PicTexmap2.Image = _map.TextureMaps[1]?.GetImage();
                    PicTexmap3.Image = _map.TextureMaps[2]?.GetImage();
                    PicTexmap4.Image = _map.TextureMaps[3]?.GetImage();
                    PicTexmap5.Image = _map.TextureMaps[4]?.GetImage();
                    PicTexmap6.Image = _map.TextureMaps[5]?.GetImage();
                    PicTexmap7.Image = _map.TextureMaps[6]?.GetImage();

                    SetHeightMapData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void SetHeightMapData()
        {
            if (_map is null) return;

            DataTable data = new();
            data.Columns.Add("X");
            data.Columns.Add("Y");
            data.Columns.Add("Value");

            for (int i = 0; i < 257; i++)
            {
                for (int j = 0; j < 257; j++)
                {
                    data.Rows.Add(i, j, _map.HeightMap.Map[i, j]);
                }
            }
            DataHeightmap.DataSource = data;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Save(Bitmap img)
        {
            if (SaveImageDialog.ShowDialog() == DialogResult.OK)
            {
                img.Save(SaveImageDialog.FileName);
            }
        }

        private void HeightmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_map is null) return;
            if (_map is null) return;

            SaveImageDialog.Filter = "TIFF Image|*.tiff|All Files|*.*";
            SaveImageDialog.FileName = $"Heightmap_{_map.X}_{_map.Y}.tiff";

            if (SaveImageDialog.ShowDialog() == DialogResult.OK)
            {
                _map.HeightMap.Save16BitHeightmap(SaveImageDialog.FileName);
            }
        }

        private void ColormapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_map is null) return;
            Save(_map.ColorMap.GetImage());
        }

        private void ObjectmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_map is null) return;
            Save(_map.ObjectMap.GetImage());
        }

        private void TexturemapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_map is null) return;
            int index = (int)((ToolStripMenuItem)sender).Tag;
            Save(_map.TextureMaps[index].GetImage());
        }

        private void GetHeightmapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BatchDialog.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(BatchDialog.SelectedPath);
                if (!dir.Exists) return;

                LabelConverting.Visible = true;
                ProgressBatch.Visible = true;
                ProgressBatch.Value = 0;

                FileInfo[] files = dir.GetFiles("*.kcm");
                ProgressBatch.Maximum = files.Length;

                BatchWorker.RunWorkerAsync(files);

            }
        }

        private void BatchWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            FileInfo[] files = e.Argument as FileInfo[];

            string path = "HeightMaps";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            for (int i = 0; i < files.Length; i++)
            {
                BatchWorker.ReportProgress(i);
                ClientMap map = new ClientMap(files[i].FullName);
                string fileName = Path.Combine(path, Path.ChangeExtension(files[i].Name, ".tiff"));
                map.HeightMap.Save16BitHeightmap(fileName);
            }
        }

        private void BatchWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ProgressBatch.Value = e.ProgressPercentage;
        }

        private void BatchWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            LabelConverting.Visible = false;
            ProgressBatch.Visible = false;
        }
    }
}