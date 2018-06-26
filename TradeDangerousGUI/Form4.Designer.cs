﻿namespace TDHelper
{
    partial class Form4
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form4));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.overrideDisableNetLogs = new System.Windows.Forms.CheckBox();
            this.overrideDoNotUpdate = new System.Windows.Forms.CheckBox();
            this.overrideCopySystemToClipboard = new System.Windows.Forms.CheckBox();
            this.overrideGroupBox = new System.Windows.Forms.GroupBox();
            this.tvFontSelectorButton = new System.Windows.Forms.Button();
            this.currentTVFontBox = new System.Windows.Forms.TextBox();
            this.currentTVFontLabel = new System.Windows.Forms.Label();
            this.validateNetLogsPath = new System.Windows.Forms.Button();
            this.validateTDPath = new System.Windows.Forms.Button();
            this.validatePythonPath = new System.Windows.Forms.Button();
            this.netLogsPathBox = new System.Windows.Forms.TextBox();
            this.tdPathBox = new System.Windows.Forms.TextBox();
            this.pythonPathBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.resetButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.edapiGroup = new System.Windows.Forms.GroupBox();
            this.edapiUserBox = new System.Windows.Forms.TextBox();
            this.edapiPassBox = new System.Windows.Forms.TextBox();
            this.edapiUserLabel = new System.Windows.Forms.Label();
            this.edapiPassLabel = new System.Windows.Forms.Label();
            this.overrideGroupBox.SuspendLayout();
            this.edapiGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(97, 190);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(257, 20);
            this.textBox1.TabIndex = 13;
            this.toolTip1.SetToolTip(this.textBox1, "This text will be added on to the end of the Run command [caution!]");
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.generic_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 193);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Run parameters:";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(300, 294);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 16;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.Location = new System.Drawing.Point(12, 294);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // overrideDisableNetLogs
            // 
            this.overrideDisableNetLogs.AutoSize = true;
            this.overrideDisableNetLogs.Location = new System.Drawing.Point(6, 14);
            this.overrideDisableNetLogs.Name = "overrideDisableNetLogs";
            this.overrideDisableNetLogs.Size = new System.Drawing.Size(107, 17);
            this.overrideDisableNetLogs.TabIndex = 2;
            this.overrideDisableNetLogs.Text = "Disable Net Logs";
            this.overrideDisableNetLogs.UseVisualStyleBackColor = true;
            // 
            // overrideDoNotUpdate
            // 
            this.overrideDoNotUpdate.AutoSize = true;
            this.overrideDoNotUpdate.Location = new System.Drawing.Point(6, 37);
            this.overrideDoNotUpdate.Name = "overrideDoNotUpdate";
            this.overrideDoNotUpdate.Size = new System.Drawing.Size(130, 17);
            this.overrideDoNotUpdate.TabIndex = 3;
            this.overrideDoNotUpdate.Text = "Disable Auto-updating";
            this.overrideDoNotUpdate.UseVisualStyleBackColor = true;
            // 
            // overrideCopySystemToClipboard
            // 
            this.overrideCopySystemToClipboard.AutoSize = true;
            this.overrideCopySystemToClipboard.Location = new System.Drawing.Point(6, 60);
            this.overrideCopySystemToClipboard.Name = "overrideCopySystemToClipboard";
            this.overrideCopySystemToClipboard.Size = new System.Drawing.Size(244, 17);
            this.overrideCopySystemToClipboard.TabIndex = 4;
            this.overrideCopySystemToClipboard.Text = "Copy unrecognized system names to clipboard";
            this.overrideCopySystemToClipboard.UseVisualStyleBackColor = true;
            // 
            // overrideGroupBox
            // 
            this.overrideGroupBox.Controls.Add(this.tvFontSelectorButton);
            this.overrideGroupBox.Controls.Add(this.currentTVFontBox);
            this.overrideGroupBox.Controls.Add(this.currentTVFontLabel);
            this.overrideGroupBox.Controls.Add(this.validateNetLogsPath);
            this.overrideGroupBox.Controls.Add(this.validateTDPath);
            this.overrideGroupBox.Controls.Add(this.validatePythonPath);
            this.overrideGroupBox.Controls.Add(this.netLogsPathBox);
            this.overrideGroupBox.Controls.Add(this.tdPathBox);
            this.overrideGroupBox.Controls.Add(this.pythonPathBox);
            this.overrideGroupBox.Controls.Add(this.label4);
            this.overrideGroupBox.Controls.Add(this.label3);
            this.overrideGroupBox.Controls.Add(this.label2);
            this.overrideGroupBox.Controls.Add(this.overrideCopySystemToClipboard);
            this.overrideGroupBox.Controls.Add(this.overrideDisableNetLogs);
            this.overrideGroupBox.Controls.Add(this.overrideDoNotUpdate);
            this.overrideGroupBox.Controls.Add(this.label1);
            this.overrideGroupBox.Controls.Add(this.textBox1);
            this.overrideGroupBox.Location = new System.Drawing.Point(12, 69);
            this.overrideGroupBox.Name = "overrideGroupBox";
            this.overrideGroupBox.Size = new System.Drawing.Size(363, 219);
            this.overrideGroupBox.TabIndex = 7;
            this.overrideGroupBox.TabStop = false;
            this.overrideGroupBox.Text = "Overrides";
            // 
            // tvFontSelectorButton
            // 
            this.tvFontSelectorButton.Location = new System.Drawing.Point(330, 86);
            this.tvFontSelectorButton.Name = "tvFontSelectorButton";
            this.tvFontSelectorButton.Size = new System.Drawing.Size(24, 20);
            this.tvFontSelectorButton.TabIndex = 6;
            this.tvFontSelectorButton.Text = "...";
            this.toolTip1.SetToolTip(this.tvFontSelectorButton, "Ctrl+Click to reset the TreeView font to the default");
            this.tvFontSelectorButton.UseVisualStyleBackColor = true;
            this.tvFontSelectorButton.Click += new System.EventHandler(this.tvFontSelectorButton_Click);
            // 
            // currentTVFontBox
            // 
            this.currentTVFontBox.Location = new System.Drawing.Point(97, 86);
            this.currentTVFontBox.Name = "currentTVFontBox";
            this.currentTVFontBox.Size = new System.Drawing.Size(227, 20);
            this.currentTVFontBox.TabIndex = 5;
            // 
            // currentTVFontLabel
            // 
            this.currentTVFontLabel.AutoSize = true;
            this.currentTVFontLabel.Location = new System.Drawing.Point(12, 89);
            this.currentTVFontLabel.Name = "currentTVFontLabel";
            this.currentTVFontLabel.Size = new System.Drawing.Size(79, 13);
            this.currentTVFontLabel.TabIndex = 13;
            this.currentTVFontLabel.Text = "TreeView Font:";
            // 
            // validateNetLogsPath
            // 
            this.validateNetLogsPath.Location = new System.Drawing.Point(330, 164);
            this.validateNetLogsPath.Name = "validateNetLogsPath";
            this.validateNetLogsPath.Size = new System.Drawing.Size(24, 20);
            this.validateNetLogsPath.TabIndex = 12;
            this.validateNetLogsPath.Text = "...";
            this.validateNetLogsPath.UseVisualStyleBackColor = true;
            this.validateNetLogsPath.Click += new System.EventHandler(this.validateNetLogsPath_Click);
            // 
            // validateTDPath
            // 
            this.validateTDPath.Location = new System.Drawing.Point(330, 138);
            this.validateTDPath.Name = "validateTDPath";
            this.validateTDPath.Size = new System.Drawing.Size(24, 20);
            this.validateTDPath.TabIndex = 10;
            this.validateTDPath.Text = "...";
            this.validateTDPath.UseVisualStyleBackColor = true;
            this.validateTDPath.Click += new System.EventHandler(this.validateTDPath_Click);
            // 
            // validatePythonPath
            // 
            this.validatePythonPath.Location = new System.Drawing.Point(330, 112);
            this.validatePythonPath.Name = "validatePythonPath";
            this.validatePythonPath.Size = new System.Drawing.Size(24, 20);
            this.validatePythonPath.TabIndex = 8;
            this.validatePythonPath.Text = "...";
            this.validatePythonPath.UseVisualStyleBackColor = true;
            this.validatePythonPath.Click += new System.EventHandler(this.validatePythonPath_Click);
            // 
            // netLogsPathBox
            // 
            this.netLogsPathBox.Location = new System.Drawing.Point(97, 164);
            this.netLogsPathBox.Name = "netLogsPathBox";
            this.netLogsPathBox.Size = new System.Drawing.Size(227, 20);
            this.netLogsPathBox.TabIndex = 11;
            // 
            // tdPathBox
            // 
            this.tdPathBox.Location = new System.Drawing.Point(97, 138);
            this.tdPathBox.Name = "tdPathBox";
            this.tdPathBox.Size = new System.Drawing.Size(227, 20);
            this.tdPathBox.TabIndex = 9;
            // 
            // pythonPathBox
            // 
            this.pythonPathBox.Location = new System.Drawing.Point(97, 112);
            this.pythonPathBox.Name = "pythonPathBox";
            this.pythonPathBox.Size = new System.Drawing.Size(227, 20);
            this.pythonPathBox.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 168);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Net Logs Path:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Python Path:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 141);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "TD Path:";
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point(151, 294);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(84, 23);
            this.resetButton.TabIndex = 15;
            this.resetButton.Text = "Reset Settings";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // edapiGroup
            // 
            this.edapiGroup.Controls.Add(this.edapiPassLabel);
            this.edapiGroup.Controls.Add(this.edapiUserLabel);
            this.edapiGroup.Controls.Add(this.edapiPassBox);
            this.edapiGroup.Controls.Add(this.edapiUserBox);
            this.edapiGroup.Location = new System.Drawing.Point(12, 13);
            this.edapiGroup.Name = "edapiGroup";
            this.edapiGroup.Size = new System.Drawing.Size(363, 50);
            this.edapiGroup.TabIndex = 9;
            this.edapiGroup.TabStop = false;
            this.edapiGroup.Text = "EDAPI";
            // 
            // edapiUserBox
            // 
            this.edapiUserBox.Location = new System.Drawing.Point(70, 19);
            this.edapiUserBox.Name = "edapiUserBox";
            this.edapiUserBox.Size = new System.Drawing.Size(100, 20);
            this.edapiUserBox.TabIndex = 0;
            // 
            // edapiPassBox
            // 
            this.edapiPassBox.Location = new System.Drawing.Point(253, 19);
            this.edapiPassBox.Name = "edapiPassBox";
            this.edapiPassBox.Size = new System.Drawing.Size(100, 20);
            this.edapiPassBox.TabIndex = 1;
            this.edapiPassBox.UseSystemPasswordChar = true;
            // 
            // edapiUserLabel
            // 
            this.edapiUserLabel.AutoSize = true;
            this.edapiUserLabel.Location = new System.Drawing.Point(6, 22);
            this.edapiUserLabel.Name = "edapiUserLabel";
            this.edapiUserLabel.Size = new System.Drawing.Size(58, 13);
            this.edapiUserLabel.TabIndex = 2;
            this.edapiUserLabel.Text = "Username:";
            // 
            // edapiPassLabel
            // 
            this.edapiPassLabel.AutoSize = true;
            this.edapiPassLabel.Location = new System.Drawing.Point(191, 22);
            this.edapiPassLabel.Name = "edapiPassLabel";
            this.edapiPassLabel.Size = new System.Drawing.Size(56, 13);
            this.edapiPassLabel.TabIndex = 3;
            this.edapiPassLabel.Text = "Password:";
            // 
            // Form4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(387, 329);
            this.Controls.Add(this.edapiGroup);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.overrideGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form4";
            this.ShowIcon = false;
            this.Text = "Misc. Settings";
            this.Load += new System.EventHandler(this.Form4_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.generic_KeyDown);
            this.overrideGroupBox.ResumeLayout(false);
            this.overrideGroupBox.PerformLayout();
            this.edapiGroup.ResumeLayout(false);
            this.edapiGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox overrideDisableNetLogs;
        private System.Windows.Forms.CheckBox overrideDoNotUpdate;
        private System.Windows.Forms.CheckBox overrideCopySystemToClipboard;
        private System.Windows.Forms.GroupBox overrideGroupBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button validateNetLogsPath;
        private System.Windows.Forms.Button validateTDPath;
        private System.Windows.Forms.Button validatePythonPath;
        private System.Windows.Forms.TextBox netLogsPathBox;
        private System.Windows.Forms.TextBox tdPathBox;
        private System.Windows.Forms.TextBox pythonPathBox;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button tvFontSelectorButton;
        private System.Windows.Forms.TextBox currentTVFontBox;
        private System.Windows.Forms.Label currentTVFontLabel;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox edapiGroup;
        private System.Windows.Forms.Label edapiPassLabel;
        private System.Windows.Forms.Label edapiUserLabel;
        private System.Windows.Forms.TextBox edapiPassBox;
        private System.Windows.Forms.TextBox edapiUserBox;
    }
}