using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("SAP Logon Lite")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]

class Entry
{
    public string Name = "";
    public string Desc = "";
    public string Url = "";
}

class MainForm : Form
{
    static readonly Color SapBlue = Color.FromArgb(10, 110, 209);
    static readonly Color SapDark = Color.FromArgb(10, 42, 77);

    ListView list;
    ToolStripStatusLabel status;
    List<Entry> entries = new List<Entry>();
    string cfgDir;
    string cfgFile;
    ImageList images;

    public MainForm()
    {
        cfgDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SAPLogonLite");
        cfgFile = Path.Combine(cfgDir, "systems.txt");

        Text = "SAP Logon";
        Width = 820;
        Height = 520;
        MinimumSize = new Size(640, 400);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
        BackColor = Color.White;
        KeyPreview = true;

        Bitmap small = MakeLogo(16);
        Icon = Icon.FromHandle(MakeLogo(32).GetHicon());
        images = new ImageList();
        images.ImageSize = new Size(16, 16);
        images.ColorDepth = ColorDepth.Depth32Bit;
        images.Images.Add(small);

        // toolbar (at the very top, like the old SAP Logon)
        ToolStrip tb = new ToolStrip();
        tb.GripStyle = ToolStripGripStyle.Hidden;
        tb.BackColor = Color.FromArgb(243, 245, 248);
        tb.Padding = new Padding(8, 4, 8, 4);
        ToolStripButton bLogon = MakeBtn("Log On", delegate { LogOn(); });
        bLogon.ForeColor = Color.White;
        bLogon.BackColor = SapBlue;
        ToolStripButton bNew = MakeBtn("New", delegate { AddEntry(); });
        ToolStripButton bEdit = MakeBtn("Edit", delegate { EditEntry(); });
        ToolStripButton bDel = MakeBtn("Delete", delegate { DeleteEntry(); });
        tb.Items.Add(bLogon);
        tb.Items.Add(new ToolStripSeparator());
        tb.Items.Add(bNew);
        tb.Items.Add(bEdit);
        tb.Items.Add(bDel);

        // list
        list = new ListView();
        list.Dock = DockStyle.Fill;
        list.View = View.Details;
        list.FullRowSelect = true;
        list.HideSelection = false;
        list.MultiSelect = false;
        list.BorderStyle = BorderStyle.None;
        list.SmallImageList = images;
        list.Columns.Add("Name", 200);
        list.Columns.Add("Description", 250);
        list.Columns.Add("URL", 340);
        list.DoubleClick += delegate { LogOn(); };

        // status bar
        StatusStrip ss = new StatusStrip();
        status = new ToolStripStatusLabel();
        ss.Items.Add(status);

        Controls.Add(list);
        Controls.Add(tb);
        Controls.Add(ss);
        list.BringToFront();

        KeyDown += delegate(object s, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { LogOn(); e.Handled = true; }
            else if (e.KeyCode == Keys.Delete) { DeleteEntry(); e.Handled = true; }
            else if (e.KeyCode == Keys.F2) { EditEntry(); e.Handled = true; }
        };

        LoadEntries();
        RefreshList();
    }

    ToolStripButton MakeBtn(string text, EventHandler onClick)
    {
        ToolStripButton b = new ToolStripButton(text);
        b.DisplayStyle = ToolStripItemDisplayStyle.Text;
        b.Padding = new Padding(10, 2, 10, 2);
        b.Click += onClick;
        return b;
    }

    static Bitmap MakeLogo(int size)
    {
        Bitmap bmp = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            using (Brush b = new LinearGradientBrush(new Rectangle(0, 0, size, size),
                Color.FromArgb(40, 150, 240), Color.FromArgb(8, 90, 180), 90f))
            {
                g.FillEllipse(b, 0, 0, size - 1, size - 1);
            }
            using (Font f = new Font("Segoe UI", size * 0.38f, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("SAP", f, Brushes.White, new RectangleF(0, 0, size, size), sf);
            }
        }
        return bmp;
    }

    void LoadEntries()
    {
        entries.Clear();
        try
        {
            if (!Directory.Exists(cfgDir)) Directory.CreateDirectory(cfgDir);
            if (!File.Exists(cfgFile))
            {
                File.WriteAllText(cfgFile,
                    "UPPCL ERP\tUPPCL SAP ERP (WebGUI)\thttps://webgui.erp.uppclonline.com/sap/bc/gui/sap/its/webgui\r\n",
                    Encoding.UTF8);
            }
            foreach (string line in File.ReadAllLines(cfgFile, Encoding.UTF8))
            {
                string[] p = line.Split('\t');
                if (p.Length >= 3 && p[0].Trim().Length > 0)
                {
                    Entry e = new Entry();
                    e.Name = p[0]; e.Desc = p[1]; e.Url = p[2];
                    entries.Add(e);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not read systems list: " + ex.Message);
        }
    }

    void SaveEntries()
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            foreach (Entry e in entries)
                sb.Append(Clean(e.Name) + "\t" + Clean(e.Desc) + "\t" + Clean(e.Url) + "\r\n");
            File.WriteAllText(cfgFile, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not save systems list: " + ex.Message);
        }
    }

    static string Clean(string s)
    {
        return (s ?? "").Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
    }

    void RefreshList()
    {
        list.Items.Clear();
        foreach (Entry e in entries)
        {
            ListViewItem it = new ListViewItem(e.Name, 0);
            it.SubItems.Add(e.Desc);
            it.SubItems.Add(e.Url);
            it.Tag = e;
            list.Items.Add(it);
        }
        if (list.Items.Count > 0) list.Items[0].Selected = true;
        status.Text = "  " + entries.Count + " system(s)";
    }

    Entry Selected()
    {
        if (list.SelectedItems.Count == 0) return null;
        return (Entry)list.SelectedItems[0].Tag;
    }

    void LogOn()
    {
        Entry e = Selected();
        if (e == null) return;
        string url = e.Url.Trim();
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) url = "https://" + url;
        try
        {
            // Edge in app mode: its own window, no address bar, looks like a desktop client
            Process.Start("msedge", "--app=" + url);
        }
        catch
        {
            try { Process.Start("chrome", "--app=" + url); }
            catch
            {
                try { Process.Start(url); }
                catch (Exception ex) { MessageBox.Show("Could not open: " + ex.Message); }
            }
        }
    }

    void AddEntry()
    {
        Entry e = new Entry();
        if (EditDialog(e, "New System"))
        {
            entries.Add(e);
            SaveEntries();
            RefreshList();
            list.Items[list.Items.Count - 1].Selected = true;
        }
    }

    void EditEntry()
    {
        Entry e = Selected();
        if (e == null) return;
        if (EditDialog(e, "Edit System"))
        {
            SaveEntries();
            RefreshList();
        }
    }

    void DeleteEntry()
    {
        Entry e = Selected();
        if (e == null) return;
        if (MessageBox.Show("Delete \"" + e.Name + "\"?", "SAP Logon",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            entries.Remove(e);
            SaveEntries();
            RefreshList();
        }
    }

    bool EditDialog(Entry e, string caption)
    {
        Form f = new Form();
        f.Text = caption;
        f.FormBorderStyle = FormBorderStyle.FixedDialog;
        f.StartPosition = FormStartPosition.CenterParent;
        f.MaximizeBox = false;
        f.MinimizeBox = false;
        f.ClientSize = new Size(480, 170);
        f.Font = Font;

        Label l1 = new Label(); l1.Text = "Name"; l1.SetBounds(12, 18, 80, 20);
        TextBox t1 = new TextBox(); t1.SetBounds(100, 15, 365, 24); t1.Text = e.Name;
        Label l2 = new Label(); l2.Text = "Description"; l2.SetBounds(12, 52, 80, 20);
        TextBox t2 = new TextBox(); t2.SetBounds(100, 49, 365, 24); t2.Text = e.Desc;
        Label l3 = new Label(); l3.Text = "URL"; l3.SetBounds(12, 86, 80, 20);
        TextBox t3 = new TextBox(); t3.SetBounds(100, 83, 365, 24); t3.Text = e.Url;

        Button ok = new Button(); ok.Text = "OK"; ok.SetBounds(305, 128, 76, 28);
        ok.DialogResult = DialogResult.OK;
        Button cancel = new Button(); cancel.Text = "Cancel"; cancel.SetBounds(389, 128, 76, 28);
        cancel.DialogResult = DialogResult.Cancel;
        f.AcceptButton = ok;
        f.CancelButton = cancel;
        f.Controls.AddRange(new Control[] { l1, t1, l2, t2, l3, t3, ok, cancel });

        if (f.ShowDialog(this) == DialogResult.OK
            && t1.Text.Trim().Length > 0 && t3.Text.Trim().Length > 0)
        {
            e.Name = t1.Text.Trim();
            e.Desc = t2.Text.Trim();
            e.Url = t3.Text.Trim();
            return true;
        }
        return false;
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
