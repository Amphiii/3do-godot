namespace TAUtil._3do
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// Class for reading 3DO format models.
    /// </summary>
    public class ModelReader
    {
        private readonly IModelReaderAdapter adapter;

        private readonly BinaryReader reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelReader"/> class.
        /// </summary>
        /// <param name="s">The stream from which to read.</param>
        /// <param name="adapter">The object to pass read data to.</param>
        public ModelReader(Stream s, IModelReaderAdapter adapter)
            : this(new BinaryReader(s), adapter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelReader"/> class.
        /// </summary>
        /// <param name="r">The reader from which to read.</param>
        /// <param name="adapter">The object to pass read data to.</param>
        public ModelReader(BinaryReader r, IModelReaderAdapter adapter)
        {
            this.adapter = adapter;
            this.reader = r;
        }

        /// <summary>
        /// Reads the 3DO model from the stream.
        /// </summary>
        public void Read()
        {
            var header = new ObjectHeader();

            ObjectHeader.Read(this.reader, ref header);

            this.reader.BaseStream.Seek(header.PtrObjectName, SeekOrigin.Begin);
            var name = this.ReadString();

            this.adapter.CreateChild(name, header.Position);

            this.reader.BaseStream.Seek(header.PtrVertexArray, SeekOrigin.Begin);

            for (int i = 0; i < header.VertexCount; i++)
            {
                var v = new Vector();
                Vector.Read(this.reader, ref v);
                this.adapter.AddVertex(v);
            }

            for (int i = 0; i < header.PrimitiveCount; i++)
            {
                int offset = i * PrimitiveHeader.Length;
                this.reader.BaseStream.Seek(
                    header.PtrPrimitiveArray + offset,
                    SeekOrigin.Begin);
                this.ReadPrimitive(i == header.PtrSelectionPrimitive);
            }

            if (header.PtrChildObject != 0)
            {
                this.reader.BaseStream.Seek(header.PtrChildObject, SeekOrigin.Begin);
                this.Read();
            }

            this.adapter.BackToParent();

            if (header.PtrSiblingObject != 0)
            {
                this.reader.BaseStream.Seek(header.PtrSiblingObject, SeekOrigin.Begin);
                this.Read();
            }
        }

        private void ReadPrimitive(bool isSelectionPrimitive)
        {
            var header = new PrimitiveHeader();
            PrimitiveHeader.Read(this.reader, ref header);

            var vertices = new int[header.VertexCount];

            this.reader.BaseStream.Seek(header.PtrVertexIndexArray, SeekOrigin.Begin);

            for (int i = 0; i < header.VertexCount; i++)
            {
                vertices[i] = this.reader.ReadUInt16();
            }

            this.reader.BaseStream.Seek(header.PtrTextureName, SeekOrigin.Begin);

            string texture = this.ReadString();

            this.adapter.AddPrimitive(header.ColorIndex, texture, vertices, isSelectionPrimitive);
        }

        private string ReadString()
        {
            StringBuilder nameBuilder = new StringBuilder();
            byte c;
            while ((c = this.reader.ReadByte()) != 0)
            {
                nameBuilder.Append((char)c);
            }

            return nameBuilder.ToString();
        }
    }
}
