namespace JBK.Tools.OplReader
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
            MainMenuStrip = new MenuStrip();
            programToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            OpenOPLFileDialog = new OpenFileDialog();
            DataGrid = new DataGridView();
            MainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DataGrid).BeginInit();
            SuspendLayout();
            // 
            // MainMenuStrip
            // 
            MainMenuStrip.Items.AddRange(new ToolStripItem[] { programToolStripMenuItem });
            MainMenuStrip.Location = new Point(0, 0);
            MainMenuStrip.Name = "MainMenuStrip";
            MainMenuStrip.Size = new Size(800, 24);
            MainMenuStrip.TabIndex = 1;
            MainMenuStrip.Text = "menuStrip2";
            // 
            // programToolStripMenuItem
            // 
            programToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, closeToolStripMenuItem });
            programToolStripMenuItem.Name = "programToolStripMenuItem";
            programToolStripMenuItem.Size = new Size(65, 20);
            programToolStripMenuItem.Text = "Program";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(103, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(103, 22);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // OpenOPLFileDialog
            // 
            OpenOPLFileDialog.FileName = "openFileDialog1";
            OpenOPLFileDialog.Filter = "OPL-Files|*.opl|All Files|*.*";
            // 
            // DataGrid
            // 
            DataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            DataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGrid.Location = new Point(12, 33);
            DataGrid.Name = "DataGrid";
            DataGrid.Size = new Size(776, 405);
            DataGrid.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(DataGrid);
            Controls.Add(MainMenuStrip);
            Name = "MainForm";
            Text = "OPL Daten";
            MainMenuStrip.ResumeLayout(false);
            MainMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)DataGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip MainMenuStrip;
        private ToolStripMenuItem programToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private OpenFileDialog OpenOPLFileDialog;
        private DataGridView DataGrid;
    }
}
