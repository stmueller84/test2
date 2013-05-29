using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace imageLinksWebService_Test
{
    public partial class FormBrowseContentServer : Form
    {
        public NodeTarget Destination { get; private set; }

        public FormBrowseContentServer()
        {
            InitializeComponent();
        }

        private const int WS_SYSMENU = 0x80000; 
        protected override CreateParams CreateParams { get { CreateParams cp = base.CreateParams; cp.Style &= ~WS_SYSMENU; return cp; } }

        private void FormBrowseContentServer_Load(object sender, EventArgs e)
        {
            TreeNode enterpriseWS = new TreeNode("Enterprise Workspace");
            TreeNode personalWS = new TreeNode("Personal Workspace");
            enterpriseWS.Tag = new NodeTarget(2000,-2000,"");
            personalWS.Tag = null;
            treeView1.Nodes.Add(enterpriseWS);
            treeView1.Nodes.Add(personalWS);

        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(e.Node.Tag != null)
            {
                object parent = this.Owner;

                ContentServerDocumentManagement.Node[] subNodes = ((Form1)parent).documentManagementClient.ListNodes(ref ((Form1)parent).documentManagementAuthentication, ((NodeTarget)e.Node.Tag).ParentID, false);
                //ContentServerDocumentManagement.GetNodesInContainerOptions op = new ContentServerDocumentManagement.GetNodesInContainerOptions();
                //op.MaxDepth = 1;
                //ContentServerDocumentManagement.Node[] subNodes = ((Form1)parent).documentManagementClient.GetNodesInContainer(
                //    ref ((Form1)parent).documentManagementAuthentication, 
                //    (int)e.Node.Tag, 
                //    op);
                
                e.Node.Nodes.Clear();
                if (subNodes != null)
                {
                    //filter only folder type nodes
                    foreach (ContentServerDocumentManagement.Node node in subNodes)
                    {
                        List<string> lVisibleSubtypes = new List<string>();
                        lVisibleSubtypes.Add("Folder");
                        lVisibleSubtypes.Add("Alias");
                        lVisibleSubtypes.Add("SawNodeMediaCollection");
                        lVisibleSubtypes.Add("SawNodeImageLinksVolume");

                        if (node.IsContainer || lVisibleSubtypes.Contains(node.Type))
                        {
                            TreeNode treeNode = new TreeNode(node.Name);

                            if (node.IsReference)
                                ((Form1)parent).documentManagementClient.GetNode(ref ((Form1)parent).documentManagementAuthentication, node.ID);

                            treeNode.Tag = new NodeTarget(node.ID, node.VolumeID, node.Type);
                            if (node.Type.Equals("SawNodeMediaCollection"))
                            {
                                treeNode.ImageIndex = 1;
                                treeNode.SelectedImageIndex = 1;
                            }
                            e.Node.Nodes.Add(treeNode);
                        }
                    }
                    e.Node.Expand();
                }
            }
        }


        delegate void SetControlValueCallback(Control oControl, string propName, object propValue);
        private void SetControlPropertyValue(Control oControl, string propName, object propValue)
        {
            if (oControl.InvokeRequired)
            {
                SetControlValueCallback d = new SetControlValueCallback(SetControlPropertyValue);
                oControl.Invoke(d, new object[] { oControl, propName, propValue });
            }
            else
            {
                Type t = oControl.GetType();
                System.Reflection.PropertyInfo[] props = t.GetProperties();
                foreach (System.Reflection.PropertyInfo p in props)
                {
                    if (p.Name.ToUpper() == propName.ToUpper())
                    {
                        p.SetValue(oControl, propValue, null);
                    }
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SetControlPropertyValue(txtNodeID, "Text", ((NodeTarget)e.Node.Tag).ParentID.ToString());
            SetControlPropertyValue(txtVolumeID, "Text", ((NodeTarget)e.Node.Tag).VolumeID.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Destination = (NodeTarget)treeView1.SelectedNode.Tag;
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Destination = null;
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
