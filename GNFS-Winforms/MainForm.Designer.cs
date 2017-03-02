namespace GNFS_Winforms
{
	partial class MainForm
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
			this.tbOutput = new System.Windows.Forms.TextBox();
			this.tbN = new System.Windows.Forms.TextBox();
			this.tbBase = new System.Windows.Forms.TextBox();
			this.tbBound = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.btnGetFactorBases = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.tbDegree = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// tbOutput
			// 
			this.tbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbOutput.Location = new System.Drawing.Point(4, 128);
			this.tbOutput.Multiline = true;
			this.tbOutput.Name = "tbOutput";
			this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbOutput.Size = new System.Drawing.Size(703, 352);
			this.tbOutput.TabIndex = 0;
			this.tbOutput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbOutput_KeyUp);
			// 
			// tbN
			// 
			this.tbN.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbN.Location = new System.Drawing.Point(20, 4);
			this.tbN.Name = "tbN";
			this.tbN.Size = new System.Drawing.Size(687, 20);
			this.tbN.TabIndex = 1;
			// 
			// tbBase
			// 
			this.tbBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbBase.Location = new System.Drawing.Point(348, 28);
			this.tbBase.Name = "tbBase";
			this.tbBase.Size = new System.Drawing.Size(359, 20);
			this.tbBase.TabIndex = 2;
			// 
			// tbBound
			// 
			this.tbBound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbBound.Enabled = false;
			this.tbBound.Location = new System.Drawing.Point(348, 76);
			this.tbBound.Name = "tbBound";
			this.tbBound.Size = new System.Drawing.Size(359, 20);
			this.tbBound.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(15, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "N";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(264, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(83, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Polynomial base";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(248, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(99, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Smoothness Bound";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnGetFactorBases
			// 
			this.btnGetFactorBases.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGetFactorBases.Location = new System.Drawing.Point(631, 100);
			this.btnGetFactorBases.Name = "btnGetFactorBases";
			this.btnGetFactorBases.Size = new System.Drawing.Size(75, 23);
			this.btnGetFactorBases.TabIndex = 7;
			this.btnGetFactorBases.Text = "Step 1";
			this.btnGetFactorBases.UseVisualStyleBackColor = true;
			this.btnGetFactorBases.Click += new System.EventHandler(this.btnGetFactorBases_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(252, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(93, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Polynomial degree";
			this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbDegree
			// 
			this.tbDegree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbDegree.Location = new System.Drawing.Point(348, 52);
			this.tbDegree.Name = "tbDegree";
			this.tbDegree.Size = new System.Drawing.Size(359, 20);
			this.tbDegree.TabIndex = 8;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(710, 482);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.tbDegree);
			this.Controls.Add(this.btnGetFactorBases);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tbBound);
			this.Controls.Add(this.tbBase);
			this.Controls.Add(this.tbN);
			this.Controls.Add(this.tbOutput);
			this.MinimumSize = new System.Drawing.Size(500, 300);
			this.Name = "MainForm";
			this.Text = "GNFS";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tbOutput;
		private System.Windows.Forms.TextBox tbN;
		private System.Windows.Forms.TextBox tbBase;
		private System.Windows.Forms.TextBox tbBound;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnGetFactorBases;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbDegree;
	}
}

