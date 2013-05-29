using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace imageLinksWebService_Test
{
    public partial class Form1 : Form
    {

        String contentServerToken;
        String host = "192.168.29.190";
        int port = 8080;
        String imagelinksUri;
        String authenticationUri;
        String documentManagementUri;
        String contentServiceUri;
        String authenticationUserName = "Admin";
        String authenticationPassword = "livelink";

        public ContentServerDocumentManagement.DocumentManagementClient documentManagementClient;
        public ContentServerDocumentManagement.OTAuthentication documentManagementAuthentication;

        public Form1()
        {
            InitializeComponent();

            imagelinksUri = String.Format(@"http://{0}:{1}/ImageLinks/services/ImageLinks?wsdl", host, port.ToString());
            authenticationUri = String.Format(@"http://{0}:{1}/les-services/services/Authentication?wsdl", host, port.ToString());
            documentManagementUri = String.Format(@"http://{0}:{1}/les-services/services/DocumentManagement?wsdl", host, port.ToString());
            contentServiceUri = String.Format(@"http://{0}:{1}/les-services/services/ContentService?wsdl", host, port.ToString());

            try
            {
                ContentServerAuthentication.AuthenticationClient authClient = new ContentServerAuthentication.AuthenticationClient();
                authClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(authenticationUri);
                contentServerToken = authClient.AuthenticateUser(authenticationUserName, authenticationPassword);

                txtLoginName.Text = authenticationUserName;


            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        #region btn click
        private void btnCreateNode(object sender, EventArgs e)
        {
            TestCreateNode();
        }
        private void btnProcessNodes(object sender, EventArgs e)
        {
            if (txtProcessNodesNodeId.Text.Length > 0 )
            {
                TestProcessNodes(Convert.ToInt32(txtProcessNodesNodeId.Text));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Content Server node id");
            }
        }
        private void btnProcessAndUpload_Click(object sender, EventArgs e)
        {
            if (txtProcessNodesNodeId.Text.Length > 0)
            {
                TestProcessNodesWithUpload(Convert.ToInt32(txtProcessNodesNodeId.Text));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Content Server node id");
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (txtProcessNodesNodeId.Text.Length > 0)
            {
                TestAddVersion(Convert.ToInt32(txtProcessNodesNodeId.Text));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Content Server node id");
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (txtProcessNodesNodeId.Text.Length > 0)
            {
                TestResizeOriginal(Convert.ToInt32(txtProcessNodesNodeId.Text));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please enter a valid Content Server node id");
            }
        }
        #endregion

        #region test methods
        private void TestAddVersion(int p)
        {
            ContentServerContentService.ContentServiceClient contentServiceClient;
            ContentServerContentService.OTAuthentication contentServiceAuthentication;

            Stream content = null;

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = @"C:\Users\Public\Documents\LEADTOOLS Images\ImageProcessingDemo";

            if (openFile.ShowDialog() == DialogResult.OK)
            {

                //connect
                try
                {
                    documentManagementClient = new ContentServerDocumentManagement.DocumentManagementClient();
                    documentManagementClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(documentManagementUri);
                    documentManagementAuthentication = new ContentServerDocumentManagement.OTAuthentication();
                    documentManagementAuthentication.AuthenticationToken = contentServerToken;
                }
                catch (Exception ex)
                {
                }

                try
                {
                    documentManagementAuthentication.AuthenticationToken = contentServerToken;
                    ContentServerDocumentManagement.Node testNodeResult = documentManagementClient.GetNode(ref documentManagementAuthentication, p);

                    ContentServerDocumentManagement.Metadata testMetadata = new ContentServerDocumentManagement.Metadata();

                    documentManagementAuthentication.AuthenticationToken = contentServerToken;
                    String addVersionContext = documentManagementClient.AddVersionContext(ref documentManagementAuthentication, testNodeResult.ID, testMetadata);

                    ContentServerContentService.FileAtts testFileAtts = new ContentServerContentService.FileAtts();
                    FileInfo testFileInfo = new FileInfo(openFile.FileName);
                    testFileAtts.CreatedDate = testFileInfo.CreationTime;
                    testFileAtts.FileName = testFileInfo.Name;
                    testFileAtts.FileSize = testFileInfo.Length;
                    testFileAtts.ModifiedDate = testFileInfo.LastWriteTime;

                    content = File.Open(openFile.FileName, FileMode.Open);

                    contentServiceClient = new ContentServerContentService.ContentServiceClient();
                    contentServiceClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(contentServiceUri);
                    contentServiceAuthentication = new ContentServerContentService.OTAuthentication();

                    contentServiceAuthentication.AuthenticationToken = contentServerToken;
                    contentServiceClient.UploadContent(ref contentServiceAuthentication, addVersionContext, testFileAtts, content);

                    System.Windows.Forms.MessageBox.Show(String.Format("Version to {0} succesfully added!", testNodeResult.ID.ToString()));
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (content != null)
                    {
                        content.Close();
                        content.Dispose();
                    }
                }
            }
        }
        private void TestCreateNode()
        {
            ContentServerContentService.ContentServiceClient contentServiceClient;
            ContentServerContentService.OTAuthentication contentServiceAuthentication;

            Stream content = null;

            OpenFileDialog openFile = new OpenFileDialog();

            if (openFile.ShowDialog() == DialogResult.OK)
            {

                //connect
                try
                {
                    documentManagementClient = new ContentServerDocumentManagement.DocumentManagementClient();
                    documentManagementClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(documentManagementUri);
                    documentManagementAuthentication = new ContentServerDocumentManagement.OTAuthentication();
                    documentManagementAuthentication.AuthenticationToken = contentServerToken;
                }
                catch (Exception ex)
                {
                }

                try
                {
                    //BrowseDialog
                    FormBrowseContentServer formBrowse;
                    using (formBrowse = new FormBrowseContentServer())
                    {
                        formBrowse.StartPosition = FormStartPosition.CenterParent;
                        if (formBrowse.ShowDialog(this) == DialogResult.OK)
                        {
                            ContentServerDocumentManagement.Node testNode = new ContentServerDocumentManagement.Node();

                            if (formBrowse.Destination.NodeType.Equals("SawNodeMediaCollection"))
                                testNode.Type = "SawNodeImage"; //30024
                            else
                                testNode.Type = "Document"; //144

                            testNode.Comment = "imageLinks node";
                            testNode.Name = "webservice " + DateTime.Now.ToString().Replace(":", "");
                            testNode.VolumeID = formBrowse.Destination.VolumeID;
                            testNode.ParentID = formBrowse.Destination.ParentID;

                            documentManagementAuthentication.AuthenticationToken = contentServerToken;
                            ContentServerDocumentManagement.Node testNodeResult = documentManagementClient.CreateNode(ref documentManagementAuthentication, testNode);

                            ContentServerDocumentManagement.Metadata testMetadata = new ContentServerDocumentManagement.Metadata();

                            documentManagementAuthentication.AuthenticationToken = contentServerToken;
                            String addVersionContext = documentManagementClient.AddVersionContext(ref documentManagementAuthentication, testNodeResult.ID, testMetadata);

                            ContentServerContentService.FileAtts testFileAtts = new ContentServerContentService.FileAtts();
                            FileInfo testFileInfo = new FileInfo(openFile.FileName);
                            testFileAtts.CreatedDate = testFileInfo.CreationTime;
                            testFileAtts.FileName = testFileInfo.Name;
                            testFileAtts.FileSize = testFileInfo.Length;
                            testFileAtts.ModifiedDate = testFileInfo.LastWriteTime;

                            content = File.Open(openFile.FileName, FileMode.Open);

                            contentServiceClient = new ContentServerContentService.ContentServiceClient();
                            contentServiceClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(contentServiceUri);
                            contentServiceAuthentication = new ContentServerContentService.OTAuthentication();

                            contentServiceAuthentication.AuthenticationToken = contentServerToken;
                            contentServiceClient.UploadContent(ref contentServiceAuthentication, addVersionContext, testFileAtts, content);

                            System.Windows.Forms.MessageBox.Show(String.Format("Node {0} was created!", testNodeResult.ID.ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (content != null)
                    {
                        content.Close();
                        content.Dispose();
                    }
                }
            }
        }
        private void TestProcessNodes(int nodeId)
        {
            imagelinks.ImageLinksClient imageLinksClient = new imagelinks.ImageLinksClient();
            imageLinksClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(imagelinksUri);

            imagelinks.OTAuthentication imagelinksAuthentication = new imagelinks.OTAuthentication();

            try
            {
                imagelinks.Size originalSize = new imagelinks.Size();
                originalSize.Height = 768;
                originalSize.Width = 1024;

                imagelinks.Size thumbSize = new imagelinks.Size();
                thumbSize.Height = 120;
                thumbSize.Width = 160;

                imagelinks.Size previewSize = new imagelinks.Size();
                previewSize.Height = 720;
                previewSize.Width = 1280;

                imagelinks.Size splashSize = new imagelinks.Size();
                splashSize.Height = 480;
                splashSize.Width = 640;

                imagelinks.ProcessNodesParams testImagelinksParams = new imagelinks.ProcessNodesParams();
                testImagelinksParams.OriginalNodeId = nodeId;
                testImagelinksParams.OriginalSize = originalSize;
                testImagelinksParams.PreviewSize = previewSize;
                testImagelinksParams.SplashSize = null;
                testImagelinksParams.ThumbSize = thumbSize;

                imagelinksAuthentication.AuthenticationToken = contentServerToken;
                imagelinks.ProcessNodesResult testProcessNodesResult = imageLinksClient.ProcessNodes(ref imagelinksAuthentication, testImagelinksParams);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {

            }
        }
        private void TestProcessNodesWithUpload(int nodeId)
        {
            imagelinks.ImageLinksClient imageLinksClient = new imagelinks.ImageLinksClient();
            imageLinksClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(imagelinksUri);

            imagelinks.OTAuthentication imagelinksAuthentication = new imagelinks.OTAuthentication();

            String[,] iptc = new String[30, 2];
            String[,] exif = new String[50, 2];
            String[,] gps = new String[5, 2];
            String[,] icc = null; //new String[1, 2];

            gps[0, 0] = "x-achse";
            gps[1, 0] = "y-achse";
            gps[2, 0] = "datum";
            gps[3, 0] = "z-achse";
            gps[4, 0] = "real";
            gps[0, 1] = "15";
            gps[1, 1] = "28";
            gps[2, 1] = "13.06.2012";
            gps[3, 1] = "56";
            gps[4, 1] = "ja";
            //Dictionary<String, String> t = new Dictionary<string, string>();
            //t.Count

            try
            {
                imagelinks.Size originalSize = new imagelinks.Size();
                originalSize.Height = 768;
                originalSize.Width = 1024;

                imagelinks.Size thumbSize = new imagelinks.Size();
                thumbSize.Height = 120;
                thumbSize.Width = 160;

                imagelinks.Size previewSize = new imagelinks.Size();
                previewSize.Height = 720;
                previewSize.Width = 1280;

                imagelinks.Size splashSize = new imagelinks.Size();
                splashSize.Height = 480;
                splashSize.Width = 640;

                imagelinks.ProcessNodesParams testImagelinksParams = new imagelinks.ProcessNodesParams();
                testImagelinksParams.OriginalNodeId = nodeId;
                testImagelinksParams.OriginalSize = originalSize;
                testImagelinksParams.PreviewSize = previewSize;
                testImagelinksParams.SplashSize = null;
                testImagelinksParams.ThumbSize = thumbSize;


                imagelinks.ArrayValues[] gpsValues2 = new imagelinks.ArrayValues[5];
                gpsValues2[0] = getArrayValue(gps[0, 0], gps[0, 1]);
                gpsValues2[1] = getArrayValue(gps[1, 0], gps[1, 1]);
                gpsValues2[2] = getArrayValue(gps[2, 0], gps[2, 1]);
                gpsValues2[3] = getArrayValue(gps[3, 0], gps[3, 1]);
                gpsValues2[4] = getArrayValue(gps[4, 0], gps[4, 1]);

                List<imagelinks.ArrayValues> gpsValues = new List<imagelinks.ArrayValues>();
                gpsValues.Add(getArrayValue(gps[0, 0], gps[0, 1]));
                gpsValues.Add(getArrayValue(gps[1, 0], gps[1, 1]));
                gpsValues.Add(getArrayValue(gps[2, 0], gps[2, 1]));
                gpsValues.Add(getArrayValue(gps[3, 0], gps[3, 1]));
                gpsValues.Add(getArrayValue(gps[4, 0], gps[4, 1]));

                testImagelinksParams.GPS = gpsValues2;

                imagelinks.Metadata previewMetadata = new imagelinks.Metadata();
                imagelinks.Metadata thumbnailMetadata = new imagelinks.Metadata();

                imagelinksAuthentication.AuthenticationToken = contentServerToken;
                imagelinks.ProcessNodesResult testProcessNodesResult = imageLinksClient.ProcessNodesWithUpload(
                    ref imagelinksAuthentication, 
                    testImagelinksParams,
                    previewMetadata,
                    thumbnailMetadata,
                    null);

                ContentServerContentService.ContentServiceClient contentServiceClient = new ContentServerContentService.ContentServiceClient();
                contentServiceClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(contentServiceUri);
                ContentServerContentService.OTAuthentication contentServiceAuthentication = new ContentServerContentService.OTAuthentication();


                String fileName = @"C:\Users\Public\Pictures\Sample Pictures\preview.jpg";
                ContentServerContentService.FileAtts testFileAtts = new ContentServerContentService.FileAtts();
                FileInfo testFileInfo = new FileInfo(fileName);
                testFileAtts.CreatedDate = testFileInfo.CreationTime;
                testFileAtts.FileName = testFileInfo.Name;
                testFileAtts.FileSize = testFileInfo.Length;
                testFileAtts.ModifiedDate = testFileInfo.LastWriteTime;

                Stream content = File.Open(fileName, FileMode.Open);

                contentServiceAuthentication.AuthenticationToken = contentServerToken;
                contentServiceClient.UploadContent(ref contentServiceAuthentication, testProcessNodesResult.PreviewContext, testFileAtts, content);
                content.Close(); content.Dispose();

                fileName = @"C:\Users\Public\Pictures\Sample Pictures\thumbnail.jpg";
                testFileAtts = new ContentServerContentService.FileAtts();
                testFileInfo = new FileInfo(fileName);
                testFileAtts.CreatedDate = testFileInfo.CreationTime;
                testFileAtts.FileName = testFileInfo.Name;
                testFileAtts.FileSize = testFileInfo.Length;
                testFileAtts.ModifiedDate = testFileInfo.LastWriteTime;

                content = File.Open(fileName, FileMode.Open);

                contentServiceAuthentication.AuthenticationToken = contentServerToken;
                contentServiceClient.UploadContent(ref contentServiceAuthentication, testProcessNodesResult.ThumbnailContext, testFileAtts, content);
                content.Close(); content.Dispose();

                /*
                fileName = @"C:\Users\Public\Pictures\Sample Pictures\splash.jpg";
                testFileAtts = new ContentServerContentService.FileAtts();
                testFileInfo = new FileInfo(fileName);
                testFileAtts.CreatedDate = testFileInfo.CreationTime;
                testFileAtts.FileName = testFileInfo.Name;
                testFileAtts.FileSize = testFileInfo.Length;
                testFileAtts.ModifiedDate = testFileInfo.LastWriteTime;

                content = File.Open(fileName, FileMode.Open);

                contentServiceAuthentication.AuthenticationToken = contentServerToken;
                contentServiceClient.UploadContent(ref contentServiceAuthentication, testProcessNodesResult.SplashContext, testFileAtts, content);
                content.Close(); content.Dispose();
                */
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {

            }
        }
        private void TestResizeOriginal(int nodeId)
        {
            try
            {
                imagelinks.ImageLinksClient imageLinksClient = new imagelinks.ImageLinksClient();
                imageLinksClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(imagelinksUri);

                imagelinks.OTAuthentication imagelinksAuthentication = new imagelinks.OTAuthentication();

                String fileName = @"C:\Users\Public\Pictures\Sample Pictures\preview.jpg";
                ContentServerContentService.FileAtts testFileAtts = new ContentServerContentService.FileAtts();
                FileInfo testFileInfo = new FileInfo(fileName);
                testFileAtts.CreatedDate = testFileInfo.CreationTime;
                testFileAtts.FileName = testFileInfo.Name;
                testFileAtts.FileSize = testFileInfo.Length;
                testFileAtts.ModifiedDate = testFileInfo.LastWriteTime;

                documentManagementClient = new ContentServerDocumentManagement.DocumentManagementClient();
                documentManagementClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(documentManagementUri);
                documentManagementAuthentication = new ContentServerDocumentManagement.OTAuthentication();
                documentManagementAuthentication.AuthenticationToken = contentServerToken;

                ContentServerDocumentManagement.Node testNodeResult = documentManagementClient.GetNode(ref documentManagementAuthentication, nodeId);
                imagelinksAuthentication.AuthenticationToken = contentServerToken;
                imagelinks.ResizeOriginalResult resizeOriginalResult = imageLinksClient.ResizeOriginal(ref imagelinksAuthentication, nodeId, new imagelinks.Metadata());

                Stream content = File.Open(fileName, FileMode.Open);

                ContentServerContentService.ContentServiceClient contentServiceClient = new ContentServerContentService.ContentServiceClient();
                contentServiceClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(contentServiceUri);
                ContentServerContentService.OTAuthentication contentServiceAuthentication = new ContentServerContentService.OTAuthentication();

                contentServiceAuthentication.AuthenticationToken = contentServerToken;
                string s = contentServiceClient.UploadContent(ref contentServiceAuthentication, resizeOriginalResult.OriginalContext, testFileAtts, content);
                content.Close(); content.Dispose();
            }
            catch (Exception ex)
            {

            }
        }

        private imagelinks.ArrayValues getArrayValue(string key, string value)
        {
            imagelinks.ArrayValues gpsValue = new imagelinks.ArrayValues();
            gpsValue.Key = key;
            gpsValue.Value = value;
            return gpsValue;
        }
        #endregion

    }
}