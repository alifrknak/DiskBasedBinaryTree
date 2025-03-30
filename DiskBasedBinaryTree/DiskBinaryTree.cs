public class DiskBinaryTree : IDisposable
{
    private readonly FileStream _stream;
    private readonly BinaryWriter _writer;
    private readonly BinaryReader _reader;

    /// <summary>
    /// size in bytes of a node
    /// </summary>
    private const int NodeSizeOnDisk = 20; // 4(int) + 8(long) + 8(long)

    public DiskBinaryTree(string filePath)
    {
        _stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _writer = new BinaryWriter(_stream);
        _reader = new BinaryReader(_stream);
    }

    public void Insert(int value)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        InsertRecursive(value);
    }

    private void InsertRecursive(int value)
    {
        if (_stream.Length == 0)
        {
            Write(Node.Create(value));
            _stream.Seek(0, SeekOrigin.Begin);
            return;
        }

        var node = ReadNode(_stream.Position);

        if (node.Value > value)
        {
            if (node.LeftOffset == 0)
            {
                node.LeftOffset = Write(Node.Create(value));
                Update(node);
            }
            else if (ReadNode(node.LeftOffset).Value > value)
            {
                _stream.Seek(node.LeftOffset, SeekOrigin.Begin);
                InsertRecursive(value);
            }
            else
            {
                Node newNode = Node.Create(value);
                newNode.LeftOffset = ReadNode(node.LeftOffset).Offset;
                newNode.Offset = Write(newNode);
                node.LeftOffset = newNode.Offset;
                Update(node);
            }
        }
        else
        {
            if (node.RightOffset == 0)
            {
                node.RightOffset = Write(Node.Create(value));
                Update(node);
            }
            else if (ReadNode(node.RightOffset).Value < value)
            {
                _stream.Seek(node.RightOffset, SeekOrigin.Begin);
                InsertRecursive(value);
            }
            else
            {
                Node newNode = Node.Create(value);
                newNode.RightOffset = ReadNode(node.RightOffset).Offset;
                newNode.Offset = Write(newNode);
                node.RightOffset = newNode.Offset;
                Update(node);
            }
        }

    }

    public void Update(Node node)
    {
        _stream.Seek(node.Offset, SeekOrigin.Begin);
        _writer.Write(node.Value);
        _writer.Write(node.LeftOffset);
        _writer.Write(node.RightOffset);
        _writer.Flush();
    }

    public long Write(Node node)
    {
        _stream.Lock(0, NodeSizeOnDisk);

        _stream.Seek(0, SeekOrigin.End);
        long currentPosition = _stream.Position;
        _writer.Write(node.Value);
        _writer.Write(node.LeftOffset);
        _writer.Write(node.RightOffset);
        _writer.Flush();

        _stream.Unlock(0, NodeSizeOnDisk);
        return currentPosition;
    }


    public void Print()
    {
        long offset = 0;
        _stream.Seek(offset, SeekOrigin.Begin);
        try
        {
            while (true)
            {
                Node node = ReadNode(offset);
                Console.Write($" Value: {node.Value}, offset: {node.Offset}, LeftOffset: {node.LeftOffset}, RightOffset: {node.RightOffset}");
                Console.WriteLine(node.Offset == 0 ? " {ROOT}" : "");
                offset += NodeSizeOnDisk;
            }
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("-----------");
        }
    }


    /// <summary>
    /// format: value, leftOffset, rightOffset
    /// </summary>
    /// <returns></returns>
    public Node ReadNode(long offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        return new Node(
            value: _reader.ReadInt32(),
            offset: offset,
            leftOffset: _reader.ReadInt64(),
            rightOffset: _reader.ReadInt64());
    }


    public void Dispose()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _stream?.Dispose();
    }
}