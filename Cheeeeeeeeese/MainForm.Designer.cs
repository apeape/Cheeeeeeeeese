namespace Cheeeeeeeeese
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
            this.UsernameTxt = new System.Windows.Forms.TextBox();
            this.PasswordTxt = new System.Windows.Forms.TextBox();
            this.LoginButton = new System.Windows.Forms.Button();
            this.ServerComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.VersionTxt = new System.Windows.Forms.TextBox();
            this.RoomTxt = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ProxyCheckBox = new System.Windows.Forms.CheckBox();
            this.ProxyTxt = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ProxyTypeComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // UsernameTxt
            // 
            this.UsernameTxt.Location = new System.Drawing.Point(59, 27);
            this.UsernameTxt.Name = "UsernameTxt";
            this.UsernameTxt.Size = new System.Drawing.Size(114, 21);
            this.UsernameTxt.TabIndex = 1;
            this.UsernameTxt.TextChanged += new System.EventHandler(this.UsernameTxt_TextChanged);
            this.UsernameTxt.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.UsernameTxt_KeyPress);
            // 
            // PasswordTxt
            // 
            this.PasswordTxt.Location = new System.Drawing.Point(59, 50);
            this.PasswordTxt.Name = "PasswordTxt";
            this.PasswordTxt.Size = new System.Drawing.Size(114, 21);
            this.PasswordTxt.TabIndex = 2;
            this.PasswordTxt.UseSystemPasswordChar = true;
            // 
            // LoginButton
            // 
            this.LoginButton.Image = global::Cheeeeeeeeese.Properties.Resources.mouse;
            this.LoginButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LoginButton.Location = new System.Drawing.Point(97, 96);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Padding = new System.Windows.Forms.Padding(4, 0, 8, 0);
            this.LoginButton.Size = new System.Drawing.Size(77, 23);
            this.LoginButton.TabIndex = 10;
            this.LoginButton.Text = "Log In";
            this.LoginButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // ServerComboBox
            // 
            this.ServerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ServerComboBox.FormattingEnabled = true;
            this.ServerComboBox.Location = new System.Drawing.Point(43, 3);
            this.ServerComboBox.Name = "ServerComboBox";
            this.ServerComboBox.Size = new System.Drawing.Size(131, 21);
            this.ServerComboBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Server";
            // 
            // VersionTxt
            // 
            this.VersionTxt.Location = new System.Drawing.Point(59, 96);
            this.VersionTxt.Name = "VersionTxt";
            this.VersionTxt.Size = new System.Drawing.Size(35, 21);
            this.VersionTxt.TabIndex = 4;
            // 
            // RoomTxt
            // 
            this.RoomTxt.Location = new System.Drawing.Point(59, 73);
            this.RoomTxt.Name = "RoomTxt";
            this.RoomTxt.Size = new System.Drawing.Size(114, 21);
            this.RoomTxt.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Version";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Room";
            // 
            // ProxyCheckBox
            // 
            this.ProxyCheckBox.AutoSize = true;
            this.ProxyCheckBox.Location = new System.Drawing.Point(6, 123);
            this.ProxyCheckBox.Name = "ProxyCheckBox";
            this.ProxyCheckBox.Size = new System.Drawing.Size(54, 17);
            this.ProxyCheckBox.TabIndex = 11;
            this.ProxyCheckBox.Text = "Proxy";
            this.ProxyCheckBox.UseVisualStyleBackColor = true;
            // 
            // ProxyTxt
            // 
            this.ProxyTxt.Location = new System.Drawing.Point(60, 121);
            this.ProxyTxt.Name = "ProxyTxt";
            this.ProxyTxt.Size = new System.Drawing.Size(114, 21);
            this.ProxyTxt.TabIndex = 3;
            this.ProxyTxt.Text = "127.0.0.1:7070";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 148);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Type";
            // 
            // ProxyTypeComboBox
            // 
            this.ProxyTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProxyTypeComboBox.FormattingEnabled = true;
            this.ProxyTypeComboBox.Location = new System.Drawing.Point(60, 145);
            this.ProxyTypeComboBox.Name = "ProxyTypeComboBox";
            this.ProxyTypeComboBox.Size = new System.Drawing.Size(114, 21);
            this.ProxyTypeComboBox.TabIndex = 13;
            // 
            // MainForm
            // 
            this.AcceptButton = this.LoginButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(178, 172);
            this.Controls.Add(this.ProxyTypeComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ProxyCheckBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.VersionTxt);
            this.Controls.Add(this.ProxyTxt);
            this.Controls.Add(this.RoomTxt);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ServerComboBox);
            this.Controls.Add(this.UsernameTxt);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PasswordTxt);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Bot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox UsernameTxt;
        private System.Windows.Forms.TextBox PasswordTxt;
        private System.Windows.Forms.Button LoginButton;
        private System.Windows.Forms.ComboBox ServerComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox RoomTxt;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox VersionTxt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox ProxyCheckBox;
        private System.Windows.Forms.TextBox ProxyTxt;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox ProxyTypeComboBox;
    }
}

