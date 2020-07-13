namespace XMLrepair
{
    partial class XMLrepair
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.XMLPath = new System.Windows.Forms.TextBox();
            this.GetXML = new System.Windows.Forms.Button();
            this.FixMXL = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(4, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(450, 43);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select a file to check for invalid characters, well formedness and overall XML va" +
    "lidity. The cleaned file will be saved to the same location with repaired prepen" +
    "ded to the original file name.";
            // 
            // XMLPath
            // 
            this.XMLPath.AllowDrop = true;
            this.XMLPath.BackColor = System.Drawing.SystemColors.Window;
            this.XMLPath.Location = new System.Drawing.Point(12, 85);
            this.XMLPath.Name = "XMLPath";
            this.XMLPath.Size = new System.Drawing.Size(333, 20);
            this.XMLPath.TabIndex = 1;
            this.XMLPath.WordWrap = false;
            this.XMLPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.XMLrepair_DragDrop);
            this.XMLPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.XMLrepair_DragEnter);
            // 
            // GetXML
            // 
            this.GetXML.AutoEllipsis = true;
            this.GetXML.Location = new System.Drawing.Point(351, 83);
            this.GetXML.Name = "GetXML";
            this.GetXML.Size = new System.Drawing.Size(82, 23);
            this.GetXML.TabIndex = 2;
            this.GetXML.TabStop = false;
            this.GetXML.Text = "&Browse";
            this.GetXML.UseVisualStyleBackColor = true;
            this.GetXML.Click += new System.EventHandler(this.GetXML_Click);
            // 
            // FixMXL
            // 
            this.FixMXL.AutoEllipsis = true;
            this.FixMXL.Location = new System.Drawing.Point(15, 113);
            this.FixMXL.Name = "FixMXL";
            this.FixMXL.Size = new System.Drawing.Size(105, 27);
            this.FixMXL.TabIndex = 5;
            this.FixMXL.Text = "&Repair File";
            this.FixMXL.UseVisualStyleBackColor = true;
            this.FixMXL.Click += new System.EventHandler(this.FixMXL_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(405, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "(When selecting an XAP or ZIP, there will be a slight delay while performing a CR" +
    "C integrity check)";
            // 
            // XMLrepair
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(438, 148);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FixMXL);
            this.Controls.Add(this.GetXML);
            this.Controls.Add(this.XMLPath);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "XMLrepair";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "XML Repair";
            this.Load += new System.EventHandler(this.XMLrepair_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.XMLrepair_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.XMLrepair_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox XMLPath;
        private System.Windows.Forms.Button GetXML;
        private System.Windows.Forms.Button FixMXL;
        private System.Windows.Forms.Label label2;
    }
}

