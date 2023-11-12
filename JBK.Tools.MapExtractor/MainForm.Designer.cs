namespace JBK.Tools.MapExtractor
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MainMenu = new MenuStrip();
            programToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            heightmapToolStripMenuItem = new ToolStripMenuItem();
            colormapToolStripMenuItem = new ToolStripMenuItem();
            objectmapToolStripMenuItem = new ToolStripMenuItem();
            texturemapToolStripMenuItem = new ToolStripMenuItem();
            texturemap1ToolStripMenuItem = new ToolStripMenuItem();
            texturemap2ToolStripMenuItem = new ToolStripMenuItem();
            texturemap3ToolStripMenuItem = new ToolStripMenuItem();
            texturemap4ToolStripMenuItem = new ToolStripMenuItem();
            texturemap5ToolStripMenuItem = new ToolStripMenuItem();
            texturemap6ToolStripMenuItem = new ToolStripMenuItem();
            texturemap7ToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            PicHeightmap = new PictureBox();
            OpenClientMapDialog = new OpenFileDialog();
            SaveImageDialog = new SaveFileDialog();
            tabControl1 = new TabControl();
            PageHeightmap = new TabPage();
            TabCtlHeightmap = new TabControl();
            PageHeightmapImage = new TabPage();
            PageHeightmapData = new TabPage();
            DataHeightmap = new DataGridView();
            X = new DataGridViewTextBoxColumn();
            Y = new DataGridViewTextBoxColumn();
            Val1 = new DataGridViewTextBoxColumn();
            Val2 = new DataGridViewTextBoxColumn();
            PageColormap = new TabPage();
            PicColormap = new PictureBox();
            PageObjectmap = new TabPage();
            PicObjectmap = new PictureBox();
            PageTexturemaps = new TabPage();
            tabControl3 = new TabControl();
            PageTex1 = new TabPage();
            PicTexmap1 = new PictureBox();
            PageTex2 = new TabPage();
            PicTexmap2 = new PictureBox();
            PageTex3 = new TabPage();
            PicTexmap3 = new PictureBox();
            PageTex4 = new TabPage();
            PicTexmap4 = new PictureBox();
            PageTex5 = new TabPage();
            PicTexmap5 = new PictureBox();
            PageTex6 = new TabPage();
            PicTexmap6 = new PictureBox();
            PageTex7 = new TabPage();
            PicTexmap7 = new PictureBox();
            MainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicHeightmap).BeginInit();
            tabControl1.SuspendLayout();
            PageHeightmap.SuspendLayout();
            TabCtlHeightmap.SuspendLayout();
            PageHeightmapImage.SuspendLayout();
            PageHeightmapData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DataHeightmap).BeginInit();
            PageColormap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicColormap).BeginInit();
            PageObjectmap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicObjectmap).BeginInit();
            PageTexturemaps.SuspendLayout();
            tabControl3.SuspendLayout();
            PageTex1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicTexmap1).BeginInit();
            PageTex2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicTexmap2).BeginInit();
            PageTex3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicTexmap3).BeginInit();
            PageTex4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicTexmap4).BeginInit();
            PageTex5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicTexmap5).BeginInit();
            PageTex6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicTexmap6).BeginInit();
            PageTex7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicTexmap7).BeginInit();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            MainMenu.Items.AddRange(new ToolStripItem[] { programToolStripMenuItem });
            MainMenu.Location = new Point(0, 0);
            MainMenu.Name = "MainMenu";
            MainMenu.Size = new Size(623, 24);
            MainMenu.TabIndex = 0;
            MainMenu.Text = "menuStrip1";
            // 
            // programToolStripMenuItem
            // 
            programToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveToolStripMenuItem, toolStripMenuItem1, exitToolStripMenuItem });
            programToolStripMenuItem.Name = "programToolStripMenuItem";
            programToolStripMenuItem.Size = new Size(65, 20);
            programToolStripMenuItem.Text = "Program";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(103, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += this.OpenToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { heightmapToolStripMenuItem, colormapToolStripMenuItem, objectmapToolStripMenuItem, texturemapToolStripMenuItem });
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(103, 22);
            saveToolStripMenuItem.Text = "Save";
            // 
            // heightmapToolStripMenuItem
            // 
            heightmapToolStripMenuItem.Name = "heightmapToolStripMenuItem";
            heightmapToolStripMenuItem.Size = new Size(136, 22);
            heightmapToolStripMenuItem.Text = "Heightmap";
            heightmapToolStripMenuItem.Click += this.HeightmapToolStripMenuItem_Click;
            // 
            // colormapToolStripMenuItem
            // 
            colormapToolStripMenuItem.Name = "colormapToolStripMenuItem";
            colormapToolStripMenuItem.Size = new Size(136, 22);
            colormapToolStripMenuItem.Text = "Colormap";
            colormapToolStripMenuItem.Click += this.ColormapToolStripMenuItem_Click;
            // 
            // objectmapToolStripMenuItem
            // 
            objectmapToolStripMenuItem.Name = "objectmapToolStripMenuItem";
            objectmapToolStripMenuItem.Size = new Size(136, 22);
            objectmapToolStripMenuItem.Text = "Objectmap";
            objectmapToolStripMenuItem.Click += this.ObjectmapToolStripMenuItem_Click;
            // 
            // texturemapToolStripMenuItem
            // 
            texturemapToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { texturemap1ToolStripMenuItem, texturemap2ToolStripMenuItem, texturemap3ToolStripMenuItem, texturemap4ToolStripMenuItem, texturemap5ToolStripMenuItem, texturemap6ToolStripMenuItem, texturemap7ToolStripMenuItem });
            texturemapToolStripMenuItem.Name = "texturemapToolStripMenuItem";
            texturemapToolStripMenuItem.Size = new Size(136, 22);
            texturemapToolStripMenuItem.Text = "Texturemap";
            // 
            // texturemap1ToolStripMenuItem
            // 
            texturemap1ToolStripMenuItem.Name = "texturemap1ToolStripMenuItem";
            texturemap1ToolStripMenuItem.Size = new Size(145, 22);
            texturemap1ToolStripMenuItem.Tag = "0";
            texturemap1ToolStripMenuItem.Text = "Texturemap 1";
            texturemap1ToolStripMenuItem.Click += this.TexturemapToolStripMenuItem_Click;
            // 
            // texturemap2ToolStripMenuItem
            // 
            texturemap2ToolStripMenuItem.Name = "texturemap2ToolStripMenuItem";
            texturemap2ToolStripMenuItem.Size = new Size(145, 22);
            texturemap2ToolStripMenuItem.Tag = "1";
            texturemap2ToolStripMenuItem.Text = "Texturemap 2";
            texturemap2ToolStripMenuItem.Click += this.TexturemapToolStripMenuItem_Click;
            // 
            // texturemap3ToolStripMenuItem
            // 
            texturemap3ToolStripMenuItem.Name = "texturemap3ToolStripMenuItem";
            texturemap3ToolStripMenuItem.Size = new Size(145, 22);
            texturemap3ToolStripMenuItem.Tag = "2";
            texturemap3ToolStripMenuItem.Text = "Texturemap 3";
            texturemap3ToolStripMenuItem.Click += this.TexturemapToolStripMenuItem_Click;
            // 
            // texturemap4ToolStripMenuItem
            // 
            texturemap4ToolStripMenuItem.Name = "texturemap4ToolStripMenuItem";
            texturemap4ToolStripMenuItem.Size = new Size(145, 22);
            texturemap4ToolStripMenuItem.Tag = "3";
            texturemap4ToolStripMenuItem.Text = "Texturemap 4";
            texturemap4ToolStripMenuItem.Click += this.TexturemapToolStripMenuItem_Click;
            // 
            // texturemap5ToolStripMenuItem
            // 
            texturemap5ToolStripMenuItem.Name = "texturemap5ToolStripMenuItem";
            texturemap5ToolStripMenuItem.Size = new Size(145, 22);
            texturemap5ToolStripMenuItem.Tag = "4";
            texturemap5ToolStripMenuItem.Text = "Texturemap 5";
            texturemap5ToolStripMenuItem.Click += this.TexturemapToolStripMenuItem_Click;
            // 
            // texturemap6ToolStripMenuItem
            // 
            texturemap6ToolStripMenuItem.Name = "texturemap6ToolStripMenuItem";
            texturemap6ToolStripMenuItem.Size = new Size(145, 22);
            texturemap6ToolStripMenuItem.Tag = "5";
            texturemap6ToolStripMenuItem.Text = "Texturemap 6";
            texturemap6ToolStripMenuItem.Click += this.TexturemapToolStripMenuItem_Click;
            // 
            // texturemap7ToolStripMenuItem
            // 
            texturemap7ToolStripMenuItem.Name = "texturemap7ToolStripMenuItem";
            texturemap7ToolStripMenuItem.Size = new Size(145, 22);
            texturemap7ToolStripMenuItem.Tag = "6";
            texturemap7ToolStripMenuItem.Text = "Texturemap 7";
            texturemap7ToolStripMenuItem.Click += this.TexturemapToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(100, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(103, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += this.ExitToolStripMenuItem_Click;
            // 
            // PicHeightmap
            // 
            PicHeightmap.Dock = DockStyle.Fill;
            PicHeightmap.Location = new Point(3, 3);
            PicHeightmap.Name = "PicHeightmap";
            PicHeightmap.Size = new Size(571, 367);
            PicHeightmap.SizeMode = PictureBoxSizeMode.Zoom;
            PicHeightmap.TabIndex = 0;
            PicHeightmap.TabStop = false;
            // 
            // OpenClientMapDialog
            // 
            OpenClientMapDialog.Filter = "Client Map|*.kcm|All Files|*.*";
            // 
            // SaveImageDialog
            // 
            SaveImageDialog.Filter = "PNG Image|*.png|All Files|*.*";
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(PageHeightmap);
            tabControl1.Controls.Add(PageColormap);
            tabControl1.Controls.Add(PageObjectmap);
            tabControl1.Controls.Add(PageTexturemaps);
            tabControl1.Location = new Point(12, 35);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(599, 435);
            tabControl1.TabIndex = 1;
            // 
            // PageHeightmap
            // 
            PageHeightmap.Controls.Add(TabCtlHeightmap);
            PageHeightmap.Location = new Point(4, 24);
            PageHeightmap.Name = "PageHeightmap";
            PageHeightmap.Padding = new Padding(3);
            PageHeightmap.Size = new Size(591, 407);
            PageHeightmap.TabIndex = 0;
            PageHeightmap.Text = "Heightmap";
            PageHeightmap.UseVisualStyleBackColor = true;
            // 
            // TabCtlHeightmap
            // 
            TabCtlHeightmap.Controls.Add(PageHeightmapImage);
            TabCtlHeightmap.Controls.Add(PageHeightmapData);
            TabCtlHeightmap.Dock = DockStyle.Fill;
            TabCtlHeightmap.Location = new Point(3, 3);
            TabCtlHeightmap.Name = "TabCtlHeightmap";
            TabCtlHeightmap.SelectedIndex = 0;
            TabCtlHeightmap.Size = new Size(585, 401);
            TabCtlHeightmap.TabIndex = 1;
            // 
            // PageHeightmapImage
            // 
            PageHeightmapImage.Controls.Add(PicHeightmap);
            PageHeightmapImage.Location = new Point(4, 24);
            PageHeightmapImage.Name = "PageHeightmapImage";
            PageHeightmapImage.Padding = new Padding(3);
            PageHeightmapImage.Size = new Size(577, 373);
            PageHeightmapImage.TabIndex = 0;
            PageHeightmapImage.Text = "Image";
            PageHeightmapImage.UseVisualStyleBackColor = true;
            // 
            // PageHeightmapData
            // 
            PageHeightmapData.Controls.Add(DataHeightmap);
            PageHeightmapData.Location = new Point(4, 24);
            PageHeightmapData.Name = "PageHeightmapData";
            PageHeightmapData.Padding = new Padding(3);
            PageHeightmapData.Size = new Size(577, 373);
            PageHeightmapData.TabIndex = 1;
            PageHeightmapData.Text = "Data";
            PageHeightmapData.UseVisualStyleBackColor = true;
            // 
            // DataHeightmap
            // 
            DataHeightmap.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataHeightmap.Columns.AddRange(new DataGridViewColumn[] { X, Y, Val1, Val2 });
            DataHeightmap.Dock = DockStyle.Fill;
            DataHeightmap.Location = new Point(3, 3);
            DataHeightmap.Name = "DataHeightmap";
            DataHeightmap.RowTemplate.Height = 25;
            DataHeightmap.Size = new Size(571, 367);
            DataHeightmap.TabIndex = 0;
            // 
            // X
            // 
            X.HeaderText = "X";
            X.Name = "X";
            // 
            // Y
            // 
            Y.HeaderText = "Y";
            Y.Name = "Y";
            // 
            // Val1
            // 
            Val1.HeaderText = "Calculated";
            Val1.Name = "Val1";
            // 
            // Val2
            // 
            Val2.HeaderText = "Actual";
            Val2.Name = "Val2";
            // 
            // PageColormap
            // 
            PageColormap.Controls.Add(PicColormap);
            PageColormap.Location = new Point(4, 24);
            PageColormap.Name = "PageColormap";
            PageColormap.Padding = new Padding(3);
            PageColormap.Size = new Size(591, 407);
            PageColormap.TabIndex = 1;
            PageColormap.Text = "Colormap";
            PageColormap.UseVisualStyleBackColor = true;
            // 
            // PicColormap
            // 
            PicColormap.Dock = DockStyle.Fill;
            PicColormap.Location = new Point(3, 3);
            PicColormap.Name = "PicColormap";
            PicColormap.Size = new Size(585, 401);
            PicColormap.SizeMode = PictureBoxSizeMode.Zoom;
            PicColormap.TabIndex = 1;
            PicColormap.TabStop = false;
            // 
            // PageObjectmap
            // 
            PageObjectmap.Controls.Add(PicObjectmap);
            PageObjectmap.Location = new Point(4, 24);
            PageObjectmap.Name = "PageObjectmap";
            PageObjectmap.Padding = new Padding(3);
            PageObjectmap.Size = new Size(591, 407);
            PageObjectmap.TabIndex = 2;
            PageObjectmap.Text = "Objectmap";
            PageObjectmap.UseVisualStyleBackColor = true;
            // 
            // PicObjectmap
            // 
            PicObjectmap.Dock = DockStyle.Fill;
            PicObjectmap.Location = new Point(3, 3);
            PicObjectmap.Name = "PicObjectmap";
            PicObjectmap.Size = new Size(585, 401);
            PicObjectmap.SizeMode = PictureBoxSizeMode.Zoom;
            PicObjectmap.TabIndex = 1;
            PicObjectmap.TabStop = false;
            // 
            // PageTexturemaps
            // 
            PageTexturemaps.Controls.Add(tabControl3);
            PageTexturemaps.Location = new Point(4, 24);
            PageTexturemaps.Name = "PageTexturemaps";
            PageTexturemaps.Padding = new Padding(3);
            PageTexturemaps.Size = new Size(591, 407);
            PageTexturemaps.TabIndex = 3;
            PageTexturemaps.Text = "Texturemaps";
            PageTexturemaps.UseVisualStyleBackColor = true;
            // 
            // tabControl3
            // 
            tabControl3.Controls.Add(PageTex1);
            tabControl3.Controls.Add(PageTex2);
            tabControl3.Controls.Add(PageTex3);
            tabControl3.Controls.Add(PageTex4);
            tabControl3.Controls.Add(PageTex5);
            tabControl3.Controls.Add(PageTex6);
            tabControl3.Controls.Add(PageTex7);
            tabControl3.Dock = DockStyle.Fill;
            tabControl3.Location = new Point(3, 3);
            tabControl3.Name = "tabControl3";
            tabControl3.SelectedIndex = 0;
            tabControl3.Size = new Size(585, 401);
            tabControl3.TabIndex = 2;
            // 
            // PageTex1
            // 
            PageTex1.Controls.Add(PicTexmap1);
            PageTex1.Location = new Point(4, 24);
            PageTex1.Name = "PageTex1";
            PageTex1.Padding = new Padding(3);
            PageTex1.Size = new Size(577, 373);
            PageTex1.TabIndex = 0;
            PageTex1.Text = "Nr. 1";
            PageTex1.UseVisualStyleBackColor = true;
            // 
            // PicTexmap1
            // 
            PicTexmap1.Dock = DockStyle.Fill;
            PicTexmap1.Location = new Point(3, 3);
            PicTexmap1.Name = "PicTexmap1";
            PicTexmap1.Size = new Size(571, 367);
            PicTexmap1.SizeMode = PictureBoxSizeMode.Zoom;
            PicTexmap1.TabIndex = 1;
            PicTexmap1.TabStop = false;
            // 
            // PageTex2
            // 
            PageTex2.Controls.Add(PicTexmap2);
            PageTex2.Location = new Point(4, 24);
            PageTex2.Name = "PageTex2";
            PageTex2.Padding = new Padding(3);
            PageTex2.Size = new Size(577, 373);
            PageTex2.TabIndex = 1;
            PageTex2.Text = "Nr. 2";
            PageTex2.UseVisualStyleBackColor = true;
            // 
            // PicTexmap2
            // 
            PicTexmap2.Dock = DockStyle.Fill;
            PicTexmap2.Location = new Point(3, 3);
            PicTexmap2.Name = "PicTexmap2";
            PicTexmap2.Size = new Size(571, 367);
            PicTexmap2.SizeMode = PictureBoxSizeMode.Zoom;
            PicTexmap2.TabIndex = 2;
            PicTexmap2.TabStop = false;
            // 
            // PageTex3
            // 
            PageTex3.Controls.Add(PicTexmap3);
            PageTex3.Location = new Point(4, 24);
            PageTex3.Name = "PageTex3";
            PageTex3.Padding = new Padding(3);
            PageTex3.Size = new Size(577, 373);
            PageTex3.TabIndex = 2;
            PageTex3.Text = "Nr. 3";
            PageTex3.UseVisualStyleBackColor = true;
            // 
            // PicTexmap3
            // 
            PicTexmap3.Dock = DockStyle.Fill;
            PicTexmap3.Location = new Point(3, 3);
            PicTexmap3.Name = "PicTexmap3";
            PicTexmap3.Size = new Size(571, 367);
            PicTexmap3.SizeMode = PictureBoxSizeMode.Zoom;
            PicTexmap3.TabIndex = 2;
            PicTexmap3.TabStop = false;
            // 
            // PageTex4
            // 
            PageTex4.Controls.Add(PicTexmap4);
            PageTex4.Location = new Point(4, 24);
            PageTex4.Name = "PageTex4";
            PageTex4.Padding = new Padding(3);
            PageTex4.Size = new Size(577, 373);
            PageTex4.TabIndex = 3;
            PageTex4.Text = "Nr. 4";
            PageTex4.UseVisualStyleBackColor = true;
            // 
            // PicTexmap4
            // 
            PicTexmap4.Dock = DockStyle.Fill;
            PicTexmap4.Location = new Point(3, 3);
            PicTexmap4.Name = "PicTexmap4";
            PicTexmap4.Size = new Size(571, 367);
            PicTexmap4.SizeMode = PictureBoxSizeMode.Zoom;
            PicTexmap4.TabIndex = 2;
            PicTexmap4.TabStop = false;
            // 
            // PageTex5
            // 
            PageTex5.Controls.Add(PicTexmap5);
            PageTex5.Location = new Point(4, 24);
            PageTex5.Name = "PageTex5";
            PageTex5.Padding = new Padding(3);
            PageTex5.Size = new Size(577, 373);
            PageTex5.TabIndex = 4;
            PageTex5.Text = "Nr. 5";
            PageTex5.UseVisualStyleBackColor = true;
            // 
            // PicTexmap5
            // 
            PicTexmap5.Dock = DockStyle.Fill;
            PicTexmap5.Location = new Point(3, 3);
            PicTexmap5.Name = "PicTexmap5";
            PicTexmap5.Size = new Size(571, 367);
            PicTexmap5.SizeMode = PictureBoxSizeMode.Zoom;
            PicTexmap5.TabIndex = 2;
            PicTexmap5.TabStop = false;
            // 
            // PageTex6
            // 
            PageTex6.Controls.Add(PicTexmap6);
            PageTex6.Location = new Point(4, 24);
            PageTex6.Name = "PageTex6";
            PageTex6.Padding = new Padding(3);
            PageTex6.Size = new Size(577, 373);
            PageTex6.TabIndex = 5;
            PageTex6.Text = "Nr. 6";
            PageTex6.UseVisualStyleBackColor = true;
            // 
            // PicTexmap6
            // 
            PicTexmap6.Dock = DockStyle.Fill;
            PicTexmap6.Location = new Point(3, 3);
            PicTexmap6.Name = "PicTexmap6";
            PicTexmap6.Size = new Size(571, 367);
            PicTexmap6.SizeMode = PictureBoxSizeMode.Zoom;
            PicTexmap6.TabIndex = 2;
            PicTexmap6.TabStop = false;
            // 
            // PageTex7
            // 
            PageTex7.Controls.Add(PicTexmap7);
            PageTex7.Location = new Point(4, 24);
            PageTex7.Name = "PageTex7";
            PageTex7.Padding = new Padding(3);
            PageTex7.Size = new Size(577, 373);
            PageTex7.TabIndex = 6;
            PageTex7.Text = "Nr. 7";
            PageTex7.UseVisualStyleBackColor = true;
            // 
            // PicTexmap7
            // 
            PicTexmap7.Dock = DockStyle.Fill;
            PicTexmap7.Location = new Point(3, 3);
            PicTexmap7.Name = "PicTexmap7";
            PicTexmap7.Size = new Size(571, 367);
            PicTexmap7.SizeMode = PictureBoxSizeMode.Zoom;
            PicTexmap7.TabIndex = 2;
            PicTexmap7.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(623, 482);
            this.Controls.Add(tabControl1);
            this.Controls.Add(MainMenu);
            this.MainMenuStrip = MainMenu;
            this.Name = "MainForm";
            this.Text = "Map Extractor";
            MainMenu.ResumeLayout(false);
            MainMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PicHeightmap).EndInit();
            tabControl1.ResumeLayout(false);
            PageHeightmap.ResumeLayout(false);
            TabCtlHeightmap.ResumeLayout(false);
            PageHeightmapImage.ResumeLayout(false);
            PageHeightmapData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)DataHeightmap).EndInit();
            PageColormap.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicColormap).EndInit();
            PageObjectmap.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicObjectmap).EndInit();
            PageTexturemaps.ResumeLayout(false);
            tabControl3.ResumeLayout(false);
            PageTex1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicTexmap1).EndInit();
            PageTex2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicTexmap2).EndInit();
            PageTex3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicTexmap3).EndInit();
            PageTex4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicTexmap4).EndInit();
            PageTex5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicTexmap5).EndInit();
            PageTex6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicTexmap6).EndInit();
            PageTex7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PicTexmap7).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private MenuStrip MainMenu;
        private ToolStripMenuItem programToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem heightmapToolStripMenuItem;
        private ToolStripMenuItem colormapToolStripMenuItem;
        private ToolStripMenuItem objectmapToolStripMenuItem;
        private ToolStripMenuItem texturemapToolStripMenuItem;
        private ToolStripMenuItem texturemap1ToolStripMenuItem;
        private ToolStripMenuItem texturemap2ToolStripMenuItem;
        private ToolStripMenuItem texturemap3ToolStripMenuItem;
        private ToolStripMenuItem texturemap4ToolStripMenuItem;
        private ToolStripMenuItem texturemap5ToolStripMenuItem;
        private ToolStripMenuItem texturemap6ToolStripMenuItem;
        private ToolStripMenuItem texturemap7ToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private OpenFileDialog OpenClientMapDialog;
        private SaveFileDialog SaveImageDialog;
        private TabControl tabControl1;
        private TabControl TabCtlHeightmap;
        private TabPage PageHeightmap;
        private TabPage PageColormap;
        private TabPage PageObjectmap;
        private TabPage PageTexturemaps;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private TabPage tabPage7;
        private TabPage tabPage8;
        private TabPage tabPage9;
        private TabPage tabPage10;
        private TabPage PageHeightmapImage;
        private TabPage PageHeightmapData;
        private TabPage PageTex1;
        private TabPage PageTex2;
        private TabPage PageTex3;
        private TabPage PageTex4;
        private TabPage PageTex5;
        private TabPage PageTex6;
        private TabPage PageTex7;
        private DataGridView DataHeightmap;
        private TabControl tabControl3;
        private PictureBox PicHeightmap;
        private PictureBox PicColormap;
        private PictureBox PicObjectmap;
        private PictureBox PicTexmap1;
        private PictureBox PicTexmap2;
        private PictureBox PicTexmap3;
        private PictureBox PicTexmap4;
        private PictureBox PicTexmap5;
        private PictureBox PicTexmap6;
        private PictureBox PicTexmap7;
        private DataGridViewTextBoxColumn X;
        private DataGridViewTextBoxColumn Y;
        private DataGridViewTextBoxColumn Val1;
        private DataGridViewTextBoxColumn Val2;
    }
}