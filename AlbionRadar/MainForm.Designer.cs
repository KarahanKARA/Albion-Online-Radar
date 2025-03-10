namespace AlbionRadar;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        rbLog = new System.Windows.Forms.RichTextBox();
        bSettings = new System.Windows.Forms.Button();
        SuspendLayout();
        // 
        // rbLog
        // 
        rbLog.Location = new System.Drawing.Point(10, 58);
        rbLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        rbLog.Name = "rbLog";
        rbLog.ReadOnly = true;
        rbLog.Size = new System.Drawing.Size(435, 365);
        rbLog.TabIndex = 4;
        rbLog.Text = "";
        // 
        // bSettings
        // 
        bSettings.Location = new System.Drawing.Point(330, 9);
        bSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        bSettings.Name = "bSettings";
        bSettings.Size = new System.Drawing.Size(115, 45);
        bSettings.TabIndex = 5;
        bSettings.Text = "Settings";
        bSettings.UseVisualStyleBackColor = true;
        bSettings.Click += bSettings_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(457, 434);
        Controls.Add(bSettings);
        Controls.Add(rbLog);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        MaximizeBox = false;
        Name = "MainForm";
        Text = "Radar";
        FormClosing += MainForm_FormClosing;
        ResumeLayout(false);
    }

    #endregion
    private System.Windows.Forms.RichTextBox rbLog;
    private System.Windows.Forms.Button bSettings;
}
