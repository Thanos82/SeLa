namespace ODataClassesGenerator
{
    partial class Form1
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
            this.button_ReadOData = new System.Windows.Forms.Button();
            this.radioButt_JS = new System.Windows.Forms.RadioButton();
            this.radioButt_TS = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_ReadOData
            // 
            this.button_ReadOData.Location = new System.Drawing.Point(60, 160);
            this.button_ReadOData.Name = "button_ReadOData";
            this.button_ReadOData.Size = new System.Drawing.Size(127, 23);
            this.button_ReadOData.TabIndex = 0;
            this.button_ReadOData.Text = "GenerateProxy";
            this.button_ReadOData.UseVisualStyleBackColor = true;
            this.button_ReadOData.Click += new System.EventHandler(this.button_ReadOData_Click);
            // 
            // radioButt_JS
            // 
            this.radioButt_JS.AutoSize = true;
            this.radioButt_JS.Location = new System.Drawing.Point(79, 80);
            this.radioButt_JS.Name = "radioButt_JS";
            this.radioButt_JS.Size = new System.Drawing.Size(75, 17);
            this.radioButt_JS.TabIndex = 1;
            this.radioButt_JS.TabStop = true;
            this.radioButt_JS.Text = "JavaScript";
            this.radioButt_JS.UseVisualStyleBackColor = true;
            // 
            // radioButt_TS
            // 
            this.radioButt_TS.AutoSize = true;
            this.radioButt_TS.Checked = true;
            this.radioButt_TS.Location = new System.Drawing.Point(79, 104);
            this.radioButt_TS.Name = "radioButt_TS";
            this.radioButt_TS.Size = new System.Drawing.Size(76, 17);
            this.radioButt_TS.TabIndex = 2;
            this.radioButt_TS.TabStop = true;
            this.radioButt_TS.Text = "TypeScript";
            this.radioButt_TS.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select the output language:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radioButt_TS);
            this.Controls.Add(this.radioButt_JS);
            this.Controls.Add(this.button_ReadOData);
            this.Name = "Form1";
            this.Text = "oDATA classes generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_ReadOData;
        private System.Windows.Forms.RadioButton radioButt_JS;
        private System.Windows.Forms.RadioButton radioButt_TS;
        private System.Windows.Forms.Label label1;
    }
}

