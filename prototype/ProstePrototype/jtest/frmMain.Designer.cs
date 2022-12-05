namespace jtest
{
    partial class frmMain
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
            this.btnXml2Json = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnXml2Json
            // 
            this.btnXml2Json.Location = new System.Drawing.Point(62, 36);
            this.btnXml2Json.Name = "btnXml2Json";
            this.btnXml2Json.Size = new System.Drawing.Size(115, 23);
            this.btnXml2Json.TabIndex = 0;
            this.btnXml2Json.Text = "XML 2 JSON";
            this.btnXml2Json.UseVisualStyleBackColor = false;
            this.btnXml2Json.Click += new System.EventHandler(this.btnXml2Json_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnXml2Json);
            this.Name = "frmMain";
            this.Text = "Test";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnXml2Json;
    }
}
