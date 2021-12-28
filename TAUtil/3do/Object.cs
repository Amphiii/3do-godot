
namespace TAUtil._3do
{
    using System.Collections.Generic;
    using System.IO;
	using System.Text;

    public class _3doObject {
        public string name;
        public Vector offset;
        public List<Vector> vertices;
        public List<List<Vector>> primitives;
        public int selectionPrimitiveOffset;

        public _3doObject() {}

        public _3doObject(string name, Vector offset, List<List<Vector>> primitives) {
            this.name = name;
            this.offset = offset;
            this.primitives = primitives;
        }
    }
}