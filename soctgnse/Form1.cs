using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace soctgnse
{
    public partial class Form1 : Form
    {

        public List<TextBox> inputControls = new List<TextBox>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            nameTextBox.TextChanged += t_TextChanged;

            treeView1.ContextMenuStrip = contextMenuStrip1;

            DialogResult result = openFileDialog1.ShowDialog();
            if(result != DialogResult.Cancel)
            {
                LoadDefinition(openFileDialog1.FileName);
            }
        }

        private void LoadDefinition(string p)
        {
            if(File.Exists(p) && p.ToLower().EndsWith(".xml"))
            {
                TableLayoutPanel dynamicTableLayoutPanel = new TableLayoutPanel();
                dynamicTableLayoutPanel.ColumnCount = 2;
                dynamicTableLayoutPanel.Dock = DockStyle.Fill;

                XmlDocument doc = new XmlDocument();
                doc.Load(p);
                XmlNodeList list = doc.SelectNodes("//card/property");
                dynamicTableLayoutPanel.RowCount = list.Count;
                int i = 0;

                foreach(XmlNode node in list)
                {
                    string name = node.Attributes["name"].Value;
                    Label l = new Label();
                    l.Text = name;

                    TextBox t = new TextBox();
                    t.Tag = name;
                    t.TextChanged += t_TextChanged;

                    inputControls.Add(t);

                    dynamicTableLayoutPanel.Controls.Add(l, 0, i);
                    dynamicTableLayoutPanel.Controls.Add(t, 1, i);

                    i++;
                }

                splitContainer2.Panel2.Controls.Add(dynamicTableLayoutPanel);
                //tabPage1.Controls.Add(dynamicTableLayoutPanel);
            }
        }

        void t_TextChanged(object sender, EventArgs e)
        {
            CreateTextNode();
        }

        private void CreateTextNode()
        {
            xmlBox.Clear();
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = doc.CreateNode("element", "card", null);
            doc.AppendChild(rootNode);
            XmlAttribute nameAttribute = doc.CreateAttribute("name");
            nameAttribute.Value = nameTextBox.Text.Trim();

            XmlAttribute guidAttribute = doc.CreateAttribute("id");
            guidAttribute.Value = GuidTextBox.Text.Trim();

            rootNode.Attributes.Append(nameAttribute);
            rootNode.Attributes.Append(guidAttribute);
            foreach(TextBox t in inputControls)
            {
                if (t.Text.Trim().Length > 0)
                {
                    XmlNode a = doc.CreateNode("element", "property", null);
                    XmlAttribute firstAttribute = doc.CreateAttribute("name");
                    firstAttribute.Value = t.Tag.ToString();

                    XmlAttribute secondAttribute = doc.CreateAttribute("value");
                    secondAttribute.Value = t.Text.Trim();

                    a.Attributes.Append(firstAttribute);
                    a.Attributes.Append(secondAttribute);
                    rootNode.AppendChild(a);
                }
            }
            xmlBox.Text = doc.OuterXml;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GuidTextBox.Text = Guid.NewGuid().ToString();
            t_TextChanged(null, EventArgs.Empty);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode node = new TreeNode();
            node.Tag = xmlBox.Text;
            node.Text = string.Format("Card: {0}", nameTextBox.Text.Trim());
            treeView1.Nodes.Add(node);
            Reset();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            nameTextBox.Clear();
            foreach(TextBox t in inputControls)
            {
                t.Clear();
            }
            button2_Click(null, EventArgs.Empty);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TreeViewToCards();
        }

        private void TreeViewToCards()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode rootElement = doc.CreateNode("element", "cards", null);
            doc.AppendChild(rootElement);
            foreach(TreeNode treeNode in treeView1.Nodes)
            {
                XmlDocumentFragment fragment = doc.CreateDocumentFragment();
                fragment.InnerXml = treeNode.Tag.ToString();
                rootElement.AppendChild(fragment);
            }
            xmlBox.Text = Beautify(doc);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode != null)
            {
                TreeNode node = treeView1.SelectedNode;
                treeView1.Nodes.Remove(node);
            }
        }

        public string Beautify(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }
    }
}
