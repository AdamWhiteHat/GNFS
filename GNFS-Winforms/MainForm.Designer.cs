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
			this.btnCreateGnfs = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.tbDegree = new System.Windows.Forms.TextBox();
			this.btnFindRelations = new System.Windows.Forms.Button();
			this.btnFindSquares = new System.Windows.Forms.Button();
			this.btnMatrix = new System.Windows.Forms.Button();
			this.panelButtons = new System.Windows.Forms.Panel();
			this.panelFunctions = new System.Windows.Forms.Panel();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.btnSerialize = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnPrintRelations = new System.Windows.Forms.Button();
			this.btnPurgeRough = new System.Windows.Forms.Button();
			this.btnLatticeSieve = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnCollectSquares = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.panelCancel = new System.Windows.Forms.Panel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.tbRelationQuantity = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tbRelationValueRange = new System.Windows.Forms.TextBox();
			this.panelButtons.SuspendLayout();
			this.panelFunctions.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.panelCancel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tbOutput
			// 
			this.tbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbOutput.Location = new System.Drawing.Point(-4, 246);
			this.tbOutput.Multiline = true;
			this.tbOutput.Name = "tbOutput";
			this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbOutput.Size = new System.Drawing.Size(718, 241);
			this.tbOutput.TabIndex = 0;
			this.tbOutput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbOutput_KeyUp);
			// 
			// tbN
			// 
			this.tbN.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbN.Location = new System.Drawing.Point(20, 4);
			this.tbN.Name = "tbN";
			this.tbN.Size = new System.Drawing.Size(702, 20);
			this.tbN.TabIndex = 1;
			// 
			// tbBase
			// 
			this.tbBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbBase.Location = new System.Drawing.Point(117, 52);
			this.tbBase.Name = "tbBase";
			this.tbBase.Size = new System.Drawing.Size(258, 20);
			this.tbBase.TabIndex = 2;
			// 
			// tbBound
			// 
			this.tbBound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbBound.Location = new System.Drawing.Point(117, 26);
			this.tbBound.Name = "tbBound";
			this.tbBound.Size = new System.Drawing.Size(258, 20);
			this.tbBound.TabIndex = 3;
			this.tbBound.Text = "61";
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
			this.label2.Location = new System.Drawing.Point(33, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(83, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Polynomial base";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(17, 30);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(99, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Smoothness Bound";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnCreateGnfs
			// 
			this.btnCreateGnfs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCreateGnfs.Location = new System.Drawing.Point(16, 15);
			this.btnCreateGnfs.Name = "btnCreateGnfs";
			this.btnCreateGnfs.Size = new System.Drawing.Size(96, 23);
			this.btnCreateGnfs.TabIndex = 7;
			this.btnCreateGnfs.Text = "Create";
			this.btnCreateGnfs.UseVisualStyleBackColor = true;
			this.btnCreateGnfs.Click += new System.EventHandler(this.btnCreateGnfs_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(21, 76);
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
			this.tbDegree.Location = new System.Drawing.Point(117, 72);
			this.tbDegree.Name = "tbDegree";
			this.tbDegree.Size = new System.Drawing.Size(18, 20);
			this.tbDegree.TabIndex = 8;
			// 
			// btnFindRelations
			// 
			this.btnFindRelations.Location = new System.Drawing.Point(16, 15);
			this.btnFindRelations.Name = "btnFindRelations";
			this.btnFindRelations.Size = new System.Drawing.Size(83, 23);
			this.btnFindRelations.TabIndex = 10;
			this.btnFindRelations.Text = "Find Relations";
			this.btnFindRelations.UseVisualStyleBackColor = true;
			this.btnFindRelations.Click += new System.EventHandler(this.btnFindRelations_Click);
			// 
			// btnFindSquares
			// 
			this.btnFindSquares.Location = new System.Drawing.Point(16, 16);
			this.btnFindSquares.Name = "btnFindSquares";
			this.btnFindSquares.Size = new System.Drawing.Size(136, 23);
			this.btnFindSquares.TabIndex = 11;
			this.btnFindSquares.Text = "Find Square Root Direct";
			this.btnFindSquares.UseVisualStyleBackColor = true;
			this.btnFindSquares.Click += new System.EventHandler(this.btnFindSquares_Click);
			// 
			// btnMatrix
			// 
			this.btnMatrix.Location = new System.Drawing.Point(16, 16);
			this.btnMatrix.Name = "btnMatrix";
			this.btnMatrix.Size = new System.Drawing.Size(96, 23);
			this.btnMatrix.TabIndex = 13;
			this.btnMatrix.Text = "Matrix Solve";
			this.btnMatrix.UseVisualStyleBackColor = true;
			this.btnMatrix.Click += new System.EventHandler(this.btnMatrix_Click);
			// 
			// panelButtons
			// 
			this.panelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelButtons.Controls.Add(this.groupBox4);
			this.panelButtons.Controls.Add(this.panelFunctions);
			this.panelButtons.Location = new System.Drawing.Point(381, 26);
			this.panelButtons.Name = "panelButtons";
			this.panelButtons.Size = new System.Drawing.Size(338, 214);
			this.panelButtons.TabIndex = 14;
			// 
			// panelFunctions
			// 
			this.panelFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelFunctions.Controls.Add(this.groupBox3);
			this.panelFunctions.Controls.Add(this.groupBox2);
			this.panelFunctions.Controls.Add(this.groupBox1);
			this.panelFunctions.Enabled = false;
			this.panelFunctions.Location = new System.Drawing.Point(8, 49);
			this.panelFunctions.Name = "panelFunctions";
			this.panelFunctions.Size = new System.Drawing.Size(324, 156);
			this.panelFunctions.TabIndex = 16;
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox4.Controls.Add(this.btnSerialize);
			this.groupBox4.Controls.Add(this.btnCreateGnfs);
			this.groupBox4.Location = new System.Drawing.Point(11, 6);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(316, 40);
			this.groupBox4.TabIndex = 20;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "1) Create Polynomial, Factor Bases && Roots";
			// 
			// btnSerialize
			// 
			this.btnSerialize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSerialize.Location = new System.Drawing.Point(214, 15);
			this.btnSerialize.Name = "btnSerialize";
			this.btnSerialize.Size = new System.Drawing.Size(96, 23);
			this.btnSerialize.TabIndex = 17;
			this.btnSerialize.Text = "Save";
			this.btnSerialize.UseVisualStyleBackColor = true;
			this.btnSerialize.Click += new System.EventHandler(this.btnSerialize_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.btnPrintRelations);
			this.groupBox3.Controls.Add(this.btnPurgeRough);
			this.groupBox3.Controls.Add(this.btnFindRelations);
			this.groupBox3.Controls.Add(this.btnLatticeSieve);
			this.groupBox3.Location = new System.Drawing.Point(3, 3);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(316, 40);
			this.groupBox3.TabIndex = 20;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "2) Sieve Relations";
			// 
			// btnPrintRelations
			// 
			this.btnPrintRelations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.btnPrintRelations.Location = new System.Drawing.Point(177, 15);
			this.btnPrintRelations.Name = "btnPrintRelations";
			this.btnPrintRelations.Size = new System.Drawing.Size(91, 23);
			this.btnPrintRelations.TabIndex = 20;
			this.btnPrintRelations.Text = "Print Relations";
			this.btnPrintRelations.UseVisualStyleBackColor = true;
			this.btnPrintRelations.Click += new System.EventHandler(this.btnPrintRelations_Click);
			// 
			// btnPurgeRough
			// 
			this.btnPurgeRough.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPurgeRough.Location = new System.Drawing.Point(268, 15);
			this.btnPurgeRough.Name = "btnPurgeRough";
			this.btnPurgeRough.Size = new System.Drawing.Size(45, 23);
			this.btnPurgeRough.TabIndex = 11;
			this.btnPurgeRough.Text = "Purge";
			this.btnPurgeRough.UseVisualStyleBackColor = true;
			this.btnPurgeRough.Click += new System.EventHandler(this.btnPurgeRough_Click);
			// 
			// btnLatticeSieve
			// 
			this.btnLatticeSieve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.btnLatticeSieve.Enabled = false;
			this.btnLatticeSieve.Location = new System.Drawing.Point(99, 15);
			this.btnLatticeSieve.Name = "btnLatticeSieve";
			this.btnLatticeSieve.Size = new System.Drawing.Size(78, 23);
			this.btnLatticeSieve.TabIndex = 12;
			this.btnLatticeSieve.Text = "Lattice sieve";
			this.btnLatticeSieve.UseVisualStyleBackColor = true;
			this.btnLatticeSieve.Click += new System.EventHandler(this.btnLatticeSieve_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.btnCollectSquares);
			this.groupBox2.Controls.Add(this.btnMatrix);
			this.groupBox2.Location = new System.Drawing.Point(3, 47);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(316, 40);
			this.groupBox2.TabIndex = 19;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "3) Matrix";
			// 
			// btnCollectSquares
			// 
			this.btnCollectSquares.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.btnCollectSquares.Enabled = false;
			this.btnCollectSquares.Location = new System.Drawing.Point(112, 16);
			this.btnCollectSquares.Name = "btnCollectSquares";
			this.btnCollectSquares.Size = new System.Drawing.Size(96, 23);
			this.btnCollectSquares.TabIndex = 14;
			this.btnCollectSquares.Text = "CollectSquares";
			this.btnCollectSquares.UseVisualStyleBackColor = true;
			this.btnCollectSquares.Click += new System.EventHandler(this.btnCollectSquares_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.btnFindSquares);
			this.groupBox1.Location = new System.Drawing.Point(3, 91);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(316, 44);
			this.groupBox1.TabIndex = 18;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "4) Square Root Solve";
			// 
			// panelCancel
			// 
			this.panelCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelCancel.Controls.Add(this.btnCancel);
			this.panelCancel.Location = new System.Drawing.Point(338, 144);
			this.panelCancel.Name = "panelCancel";
			this.panelCancel.Size = new System.Drawing.Size(289, 56);
			this.panelCancel.TabIndex = 15;
			this.panelCancel.Visible = false;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point(178, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(108, 32);
			this.btnCancel.TabIndex = 14;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(24, 101);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(89, 13);
			this.label5.TabIndex = 17;
			this.label5.Text = "Relation quantity:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbRelationQuantity
			// 
			this.tbRelationQuantity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbRelationQuantity.Location = new System.Drawing.Point(117, 98);
			this.tbRelationQuantity.Name = "tbRelationQuantity";
			this.tbRelationQuantity.Size = new System.Drawing.Size(258, 20);
			this.tbRelationQuantity.TabIndex = 16;
			this.tbRelationQuantity.Text = "70";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(5, 122);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(108, 13);
			this.label6.TabIndex = 19;
			this.label6.Text = "Relation value range:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbRelationValueRange
			// 
			this.tbRelationValueRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbRelationValueRange.Location = new System.Drawing.Point(117, 118);
			this.tbRelationValueRange.Name = "tbRelationValueRange";
			this.tbRelationValueRange.Size = new System.Drawing.Size(258, 20);
			this.tbRelationValueRange.TabIndex = 18;
			this.tbRelationValueRange.Text = "200";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(725, 489);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.tbRelationValueRange);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.tbRelationQuantity);
			this.Controls.Add(this.panelButtons);
			this.Controls.Add(this.panelCancel);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.tbDegree);
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
			this.panelButtons.ResumeLayout(false);
			this.panelFunctions.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.panelCancel.ResumeLayout(false);
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
		private System.Windows.Forms.Button btnCreateGnfs;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbDegree;
		private System.Windows.Forms.Button btnFindRelations;
		private System.Windows.Forms.Button btnFindSquares;
		private System.Windows.Forms.Button btnMatrix;
		private System.Windows.Forms.Panel panelButtons;
		private System.Windows.Forms.Panel panelCancel;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel panelFunctions;
		private System.Windows.Forms.Button btnSerialize;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnPurgeRough;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnCollectSquares;
		private System.Windows.Forms.Button btnLatticeSieve;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tbRelationQuantity;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbRelationValueRange;
		private System.Windows.Forms.Button btnPrintRelations;
		private System.Windows.Forms.GroupBox groupBox4;
	}
}

