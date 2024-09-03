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
			tbOutput = new System.Windows.Forms.TextBox();
			tbN = new System.Windows.Forms.TextBox();
			tbBase = new System.Windows.Forms.TextBox();
			tbBound = new System.Windows.Forms.TextBox();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			btnCreate = new System.Windows.Forms.Button();
			label4 = new System.Windows.Forms.Label();
			tbDegree = new System.Windows.Forms.TextBox();
			btnFindRelations = new System.Windows.Forms.Button();
			btnFindSquares = new System.Windows.Forms.Button();
			btnMatrix = new System.Windows.Forms.Button();
			panelButtons = new System.Windows.Forms.Panel();
			groupBox4 = new System.Windows.Forms.GroupBox();
			btnLoad = new System.Windows.Forms.Button();
			btnSave = new System.Windows.Forms.Button();
			panelFunctions = new System.Windows.Forms.Panel();
			groupBox3 = new System.Windows.Forms.GroupBox();
			btnPrintRelations = new System.Windows.Forms.Button();
			btnPurgeRough = new System.Windows.Forms.Button();
			groupBox2 = new System.Windows.Forms.GroupBox();
			groupBox1 = new System.Windows.Forms.GroupBox();
			panelCancel = new System.Windows.Forms.Panel();
			btnCancel = new System.Windows.Forms.Button();
			label5 = new System.Windows.Forms.Label();
			tbRelationQuantity = new System.Windows.Forms.TextBox();
			label6 = new System.Windows.Forms.Label();
			tbRelationValueRange = new System.Windows.Forms.TextBox();
			btnIncreaseSmoothnessBound = new System.Windows.Forms.Button();
			linkGitHubProject = new System.Windows.Forms.LinkLabel();
			label7 = new System.Windows.Forms.Label();
			panelButtons.SuspendLayout();
			groupBox4.SuspendLayout();
			panelFunctions.SuspendLayout();
			groupBox3.SuspendLayout();
			groupBox2.SuspendLayout();
			groupBox1.SuspendLayout();
			panelCancel.SuspendLayout();
			SuspendLayout();
			// 
			// tbOutput
			// 
			tbOutput.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbOutput.Location = new System.Drawing.Point(5, 338);
			tbOutput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			tbOutput.Multiline = true;
			tbOutput.Name = "tbOutput";
			tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			tbOutput.Size = new System.Drawing.Size(956, 384);
			tbOutput.TabIndex = 6;
			tbOutput.KeyDown += tbOutput_KeyDown;
			// 
			// tbN
			// 
			tbN.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbN.Location = new System.Drawing.Point(27, 6);
			tbN.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			tbN.Name = "tbN";
			tbN.Size = new System.Drawing.Size(931, 27);
			tbN.TabIndex = 0;
			// 
			// tbBase
			// 
			tbBase.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbBase.Location = new System.Drawing.Point(156, 85);
			tbBase.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			tbBase.Name = "tbBase";
			tbBase.Size = new System.Drawing.Size(345, 27);
			tbBase.TabIndex = 2;
			// 
			// tbBound
			// 
			tbBound.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbBound.Location = new System.Drawing.Point(156, 45);
			tbBound.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			tbBound.Name = "tbBound";
			tbBound.Size = new System.Drawing.Size(245, 27);
			tbBound.TabIndex = 1;
			tbBound.Text = "61";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(5, 12);
			label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(20, 20);
			label1.TabIndex = 4;
			label1.Text = "N";
			label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(44, 91);
			label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(117, 20);
			label2.TabIndex = 5;
			label2.Text = "Polynomial base";
			label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(23, 51);
			label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(136, 20);
			label3.TabIndex = 6;
			label3.Text = "Smoothness Bound";
			label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnCreate
			// 
			btnCreate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			btnCreate.Location = new System.Drawing.Point(148, 23);
			btnCreate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnCreate.Name = "btnCreate";
			btnCreate.Size = new System.Drawing.Size(128, 35);
			btnCreate.TabIndex = 0;
			btnCreate.Text = "Create";
			btnCreate.UseVisualStyleBackColor = true;
			btnCreate.Click += btnCreate_Click;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(28, 122);
			label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(133, 20);
			label4.TabIndex = 9;
			label4.Text = "Polynomial degree";
			label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbDegree
			// 
			tbDegree.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbDegree.Location = new System.Drawing.Point(156, 115);
			tbDegree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			tbDegree.Name = "tbDegree";
			tbDegree.Size = new System.Drawing.Size(27, 27);
			tbDegree.TabIndex = 3;
			// 
			// btnFindRelations
			// 
			btnFindRelations.Location = new System.Drawing.Point(21, 23);
			btnFindRelations.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnFindRelations.Name = "btnFindRelations";
			btnFindRelations.Size = new System.Drawing.Size(111, 35);
			btnFindRelations.TabIndex = 0;
			btnFindRelations.Text = "Find Relations";
			btnFindRelations.UseVisualStyleBackColor = true;
			btnFindRelations.Click += btnFindRelations_Click;
			// 
			// btnFindSquares
			// 
			btnFindSquares.Location = new System.Drawing.Point(21, 25);
			btnFindSquares.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnFindSquares.Name = "btnFindSquares";
			btnFindSquares.Size = new System.Drawing.Size(181, 35);
			btnFindSquares.TabIndex = 0;
			btnFindSquares.Text = "Find Square Root Direct";
			btnFindSquares.UseVisualStyleBackColor = true;
			btnFindSquares.Click += btnFindSquares_Click;
			// 
			// btnMatrix
			// 
			btnMatrix.Location = new System.Drawing.Point(21, 25);
			btnMatrix.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnMatrix.Name = "btnMatrix";
			btnMatrix.Size = new System.Drawing.Size(128, 35);
			btnMatrix.TabIndex = 0;
			btnMatrix.Text = "Matrix Solve";
			btnMatrix.UseVisualStyleBackColor = true;
			btnMatrix.Click += btnMatrix_Click;
			// 
			// panelButtons
			// 
			panelButtons.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			panelButtons.Controls.Add(groupBox4);
			panelButtons.Controls.Add(panelFunctions);
			panelButtons.Location = new System.Drawing.Point(508, 40);
			panelButtons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			panelButtons.Name = "panelButtons";
			panelButtons.Size = new System.Drawing.Size(451, 295);
			panelButtons.TabIndex = 14;
			// 
			// groupBox4
			// 
			groupBox4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox4.Controls.Add(btnLoad);
			groupBox4.Controls.Add(btnSave);
			groupBox4.Controls.Add(btnCreate);
			groupBox4.Location = new System.Drawing.Point(15, 9);
			groupBox4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox4.Name = "groupBox4";
			groupBox4.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox4.Size = new System.Drawing.Size(421, 62);
			groupBox4.TabIndex = 20;
			groupBox4.TabStop = false;
			groupBox4.Text = "1) Create Polynomial, Factor Bases && Roots";
			// 
			// btnLoad
			// 
			btnLoad.Location = new System.Drawing.Point(8, 23);
			btnLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnLoad.Name = "btnLoad";
			btnLoad.Size = new System.Drawing.Size(128, 35);
			btnLoad.TabIndex = 2;
			btnLoad.Text = "Load";
			btnLoad.UseVisualStyleBackColor = true;
			btnLoad.Click += btnLoad_Click;
			// 
			// btnSave
			// 
			btnSave.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			btnSave.Location = new System.Drawing.Point(288, 23);
			btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnSave.Name = "btnSave";
			btnSave.Size = new System.Drawing.Size(128, 35);
			btnSave.TabIndex = 1;
			btnSave.Text = "Save";
			btnSave.UseVisualStyleBackColor = true;
			btnSave.Click += btnSave_Click;
			// 
			// panelFunctions
			// 
			panelFunctions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panelFunctions.Controls.Add(groupBox3);
			panelFunctions.Controls.Add(groupBox2);
			panelFunctions.Controls.Add(groupBox1);
			panelFunctions.Enabled = false;
			panelFunctions.Location = new System.Drawing.Point(11, 75);
			panelFunctions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			panelFunctions.Name = "panelFunctions";
			panelFunctions.Size = new System.Drawing.Size(432, 215);
			panelFunctions.TabIndex = 16;
			// 
			// groupBox3
			// 
			groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox3.Controls.Add(btnPrintRelations);
			groupBox3.Controls.Add(btnPurgeRough);
			groupBox3.Controls.Add(btnFindRelations);
			groupBox3.Location = new System.Drawing.Point(4, 5);
			groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox3.Name = "groupBox3";
			groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox3.Size = new System.Drawing.Size(421, 62);
			groupBox3.TabIndex = 20;
			groupBox3.TabStop = false;
			groupBox3.Text = "2) Sieve Relations";
			// 
			// btnPrintRelations
			// 
			btnPrintRelations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
			btnPrintRelations.Location = new System.Drawing.Point(236, -91);
			btnPrintRelations.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnPrintRelations.Name = "btnPrintRelations";
			btnPrintRelations.Size = new System.Drawing.Size(121, 57);
			btnPrintRelations.TabIndex = 1;
			btnPrintRelations.Text = "Print Relations";
			btnPrintRelations.UseVisualStyleBackColor = true;
			btnPrintRelations.Click += btnPrintRelations_Click;
			// 
			// btnPurgeRough
			// 
			btnPurgeRough.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			btnPurgeRough.Location = new System.Drawing.Point(357, 23);
			btnPurgeRough.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnPurgeRough.Name = "btnPurgeRough";
			btnPurgeRough.Size = new System.Drawing.Size(60, 35);
			btnPurgeRough.TabIndex = 2;
			btnPurgeRough.Text = "Purge";
			btnPurgeRough.UseVisualStyleBackColor = true;
			btnPurgeRough.Click += btnPurgeRough_Click;
			// 
			// groupBox2
			// 
			groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox2.Controls.Add(btnMatrix);
			groupBox2.Location = new System.Drawing.Point(4, 72);
			groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox2.Name = "groupBox2";
			groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox2.Size = new System.Drawing.Size(421, 62);
			groupBox2.TabIndex = 19;
			groupBox2.TabStop = false;
			groupBox2.Text = "3) Matrix";
			// 
			// groupBox1
			// 
			groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox1.Controls.Add(btnFindSquares);
			groupBox1.Location = new System.Drawing.Point(4, 140);
			groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			groupBox1.Size = new System.Drawing.Size(421, 68);
			groupBox1.TabIndex = 18;
			groupBox1.TabStop = false;
			groupBox1.Text = "4) Square Root Solve";
			// 
			// panelCancel
			// 
			panelCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			panelCancel.Controls.Add(btnCancel);
			panelCancel.Location = new System.Drawing.Point(451, 222);
			panelCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			panelCancel.Name = "panelCancel";
			panelCancel.Size = new System.Drawing.Size(385, 86);
			panelCancel.TabIndex = 15;
			// 
			// btnCancel
			// 
			btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			btnCancel.Location = new System.Drawing.Point(237, 5);
			btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new System.Drawing.Size(144, 49);
			btnCancel.TabIndex = 14;
			btnCancel.Text = "Cancel";
			btnCancel.UseVisualStyleBackColor = true;
			btnCancel.Click += btnCancel_Click;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(32, 160);
			label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(125, 20);
			label5.TabIndex = 17;
			label5.Text = "Relation quantity:";
			label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbRelationQuantity
			// 
			tbRelationQuantity.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbRelationQuantity.Location = new System.Drawing.Point(156, 155);
			tbRelationQuantity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			tbRelationQuantity.Name = "tbRelationQuantity";
			tbRelationQuantity.Size = new System.Drawing.Size(345, 27);
			tbRelationQuantity.TabIndex = 4;
			tbRelationQuantity.Text = "70";
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new System.Drawing.Point(7, 192);
			label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(148, 20);
			label6.TabIndex = 19;
			label6.Text = "Relation value range:";
			label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tbRelationValueRange
			// 
			tbRelationValueRange.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbRelationValueRange.Location = new System.Drawing.Point(156, 186);
			tbRelationValueRange.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			tbRelationValueRange.Name = "tbRelationValueRange";
			tbRelationValueRange.Size = new System.Drawing.Size(345, 27);
			tbRelationValueRange.TabIndex = 5;
			tbRelationValueRange.Text = "200";
			// 
			// btnIncreaseSmoothnessBound
			// 
			btnIncreaseSmoothnessBound.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			btnIncreaseSmoothnessBound.Enabled = false;
			btnIncreaseSmoothnessBound.Location = new System.Drawing.Point(405, 43);
			btnIncreaseSmoothnessBound.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			btnIncreaseSmoothnessBound.Name = "btnIncreaseSmoothnessBound";
			btnIncreaseSmoothnessBound.Size = new System.Drawing.Size(100, 35);
			btnIncreaseSmoothnessBound.TabIndex = 20;
			btnIncreaseSmoothnessBound.Text = "Increase";
			btnIncreaseSmoothnessBound.UseVisualStyleBackColor = true;
			btnIncreaseSmoothnessBound.Click += btnIncreaseSmoothnessBound_Click;
			// 
			// linkGitHubProject
			// 
			linkGitHubProject.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			linkGitHubProject.AutoSize = true;
			linkGitHubProject.Location = new System.Drawing.Point(684, 728);
			linkGitHubProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			linkGitHubProject.Name = "linkGitHubProject";
			linkGitHubProject.Size = new System.Drawing.Size(283, 20);
			linkGitHubProject.TabIndex = 21;
			linkGitHubProject.TabStop = true;
			linkGitHubProject.Text = "https://github.com/AdamWhiteHat/GNFS";
			linkGitHubProject.LinkClicked += linkGitHubProject_LinkClicked;
			// 
			// label7
			// 
			label7.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			label7.AutoSize = true;
			label7.Location = new System.Drawing.Point(543, 729);
			label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(146, 20);
			label7.TabIndex = 22;
			label7.Text = "It's open source!  -->";
			// 
			// MainForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(967, 752);
			Controls.Add(label7);
			Controls.Add(linkGitHubProject);
			Controls.Add(btnIncreaseSmoothnessBound);
			Controls.Add(label6);
			Controls.Add(tbRelationValueRange);
			Controls.Add(label5);
			Controls.Add(tbRelationQuantity);
			Controls.Add(panelButtons);
			Controls.Add(label4);
			Controls.Add(tbDegree);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(tbBound);
			Controls.Add(tbBase);
			Controls.Add(tbN);
			Controls.Add(tbOutput);
			Controls.Add(panelCancel);
			Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			MinimumSize = new System.Drawing.Size(661, 436);
			Name = "MainForm";
			Text = "A C# Implementation of the General Number Field Sieve";
			Shown += MainForm_Shown;
			panelButtons.ResumeLayout(false);
			groupBox4.ResumeLayout(false);
			panelFunctions.ResumeLayout(false);
			groupBox3.ResumeLayout(false);
			groupBox2.ResumeLayout(false);
			groupBox1.ResumeLayout(false);
			panelCancel.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.TextBox tbOutput;
		private System.Windows.Forms.TextBox tbN;
		private System.Windows.Forms.TextBox tbBase;
		private System.Windows.Forms.TextBox tbBound;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnCreate;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbDegree;
		private System.Windows.Forms.Button btnFindRelations;
		private System.Windows.Forms.Button btnFindSquares;
		private System.Windows.Forms.Button btnMatrix;
		private System.Windows.Forms.Panel panelButtons;
		private System.Windows.Forms.Panel panelCancel;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel panelFunctions;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnPurgeRough;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tbRelationQuantity;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbRelationValueRange;
		private System.Windows.Forms.Button btnPrintRelations;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.Button btnIncreaseSmoothnessBound;
		private System.Windows.Forms.LinkLabel linkGitHubProject;
		private System.Windows.Forms.Label label7;
	}
}

