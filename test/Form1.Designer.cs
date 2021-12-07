namespace Test
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this._XReportButton = new System.Windows.Forms.Button();
            this._ZReportButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this._incomeButton = new System.Windows.Forms.Button();
            this._outcomeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(73, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Print Cheque";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // _XReportButton
            // 
            this._XReportButton.Location = new System.Drawing.Point(73, 56);
            this._XReportButton.Name = "_XReportButton";
            this._XReportButton.Size = new System.Drawing.Size(113, 23);
            this._XReportButton.TabIndex = 1;
            this._XReportButton.Text = "XReport";
            this._XReportButton.UseVisualStyleBackColor = true;
            this._XReportButton.Click += new System.EventHandler(this._XReportButton_Click);
            // 
            // _ZReportButton
            // 
            this._ZReportButton.Location = new System.Drawing.Point(73, 85);
            this._ZReportButton.Name = "_ZReportButton";
            this._ZReportButton.Size = new System.Drawing.Size(113, 23);
            this._ZReportButton.TabIndex = 2;
            this._ZReportButton.Text = "ZReport";
            this._ZReportButton.UseVisualStyleBackColor = true;
            this._ZReportButton.Click += new System.EventHandler(this._ZReportButton_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(73, 206);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(113, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "TestToDouble()";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // _incomeButton
            // 
            this._incomeButton.Location = new System.Drawing.Point(73, 114);
            this._incomeButton.Name = "_incomeButton";
            this._incomeButton.Size = new System.Drawing.Size(113, 23);
            this._incomeButton.TabIndex = 4;
            this._incomeButton.Text = "Income";
            this._incomeButton.UseVisualStyleBackColor = true;
            this._incomeButton.Click += new System.EventHandler(this._incomeButton_Click);
            // 
            // _outcomeButton
            // 
            this._outcomeButton.Location = new System.Drawing.Point(73, 143);
            this._outcomeButton.Name = "_outcomeButton";
            this._outcomeButton.Size = new System.Drawing.Size(113, 23);
            this._outcomeButton.TabIndex = 5;
            this._outcomeButton.Text = "Outcome";
            this._outcomeButton.UseVisualStyleBackColor = true;
            this._outcomeButton.Click += new System.EventHandler(this._outcomeButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 241);
            this.Controls.Add(this._outcomeButton);
            this.Controls.Add(this._incomeButton);
            this.Controls.Add(this.button2);
            this.Controls.Add(this._ZReportButton);
            this.Controls.Add(this._XReportButton);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button _XReportButton;
        private System.Windows.Forms.Button _ZReportButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button _incomeButton;
        private System.Windows.Forms.Button _outcomeButton;
    }
}

