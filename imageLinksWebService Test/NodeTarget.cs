namespace imageLinksWebService_Test
{
    public class NodeTarget
    {
        public int ParentID { get; private set; }
        public int VolumeID { get; private set; }
        public string NodeType { get; private set; }
        public NodeTarget(int parentID, int volumeID, string nodeType)
        {
            this.ParentID = parentID;
            this.VolumeID = volumeID;
            this.NodeType = nodeType;
        }
    }
}
