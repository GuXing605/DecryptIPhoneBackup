namespace DecryptBackup
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnDecrypte = new System.Windows.Forms.Button();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.lbMsg = new System.Windows.Forms.Label();
            this.btnFolder = new System.Windows.Forms.Button();
            this.lbPwd = new System.Windows.Forms.Label();
            this.tbPwd = new System.Windows.Forms.TextBox();
            this.tbShowInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnDecrypte
            // 
            this.btnDecrypte.Font = new System.Drawing.Font("宋体", 16F);
            this.btnDecrypte.Location = new System.Drawing.Point(430, 72);
            this.btnDecrypte.Name = "btnDecrypte";
            this.btnDecrypte.Size = new System.Drawing.Size(103, 34);
            this.btnDecrypte.TabIndex = 5;
            this.btnDecrypte.Text = "解密";
            this.btnDecrypte.UseVisualStyleBackColor = true;
            this.btnDecrypte.Click += new System.EventHandler(this.BtnDecrypt_Click);
            // 
            // tbPath
            // 
            this.tbPath.Font = new System.Drawing.Font("宋体", 12F);
            this.tbPath.Location = new System.Drawing.Point(120, 44);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(207, 26);
            this.tbPath.TabIndex = 1;
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.Font = new System.Drawing.Font("宋体", 12F);
            this.lbMsg.Location = new System.Drawing.Point(26, 52);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(88, 16);
            this.lbMsg.TabIndex = 0;
            this.lbMsg.Text = "备份文件夹";
            // 
            // btnFolder
            // 
            this.btnFolder.Font = new System.Drawing.Font("宋体", 12F);
            this.btnFolder.Location = new System.Drawing.Point(333, 40);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(42, 30);
            this.btnFolder.TabIndex = 2;
            this.btnFolder.Text = "...";
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // lbPwd
            // 
            this.lbPwd.AutoSize = true;
            this.lbPwd.Font = new System.Drawing.Font("宋体", 12F);
            this.lbPwd.Location = new System.Drawing.Point(74, 106);
            this.lbPwd.Name = "lbPwd";
            this.lbPwd.Size = new System.Drawing.Size(40, 16);
            this.lbPwd.TabIndex = 3;
            this.lbPwd.Text = "密码";
            // 
            // tbPwd
            // 
            this.tbPwd.Font = new System.Drawing.Font("宋体", 12F);
            this.tbPwd.Location = new System.Drawing.Point(120, 103);
            this.tbPwd.Name = "tbPwd";
            this.tbPwd.PasswordChar = '*';
            this.tbPwd.Size = new System.Drawing.Size(207, 26);
            this.tbPwd.TabIndex = 4;
            // 
            // tbShowInfo
            // 
            this.tbShowInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbShowInfo.Location = new System.Drawing.Point(13, 146);
            this.tbShowInfo.MaxLength = 65535;
            this.tbShowInfo.Multiline = true;
            this.tbShowInfo.Name = "tbShowInfo";
            this.tbShowInfo.ReadOnly = true;
            this.tbShowInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbShowInfo.Size = new System.Drawing.Size(779, 419);
            this.tbShowInfo.TabIndex = 6;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 577);
            this.Controls.Add(this.tbShowInfo);
            this.Controls.Add(this.btnFolder);
            this.Controls.Add(this.lbPwd);
            this.Controls.Add(this.lbMsg);
            this.Controls.Add(this.tbPwd);
            this.Controls.Add(this.tbPath);
            this.Controls.Add(this.btnDecrypte);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "解密备份文件";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDecrypte;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.Label lbPwd;
        private System.Windows.Forms.TextBox tbPwd;
        private System.Windows.Forms.TextBox tbShowInfo;
    }
}

