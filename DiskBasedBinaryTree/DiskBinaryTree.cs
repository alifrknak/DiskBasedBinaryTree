using System.Collections.Concurrent;

public class DiskBinaryTree : IDisposable
{
    private readonly FileStream _stream;
    private readonly BinaryWriter _writer;
    private readonly BinaryReader _reader;

    /// <summary>
    /// size in bytes of a node
    /// </summary>
    private const int NodeSizeOnDisk = 20; // 4(int) + 8(long) + 8(long)

    /// <summary>
    /// list of free offsets 
    /// used to reuse the space
    /// </summary>
    private ConcurrentQueue<long> _freeOffsets = new();

    public DiskBinaryTree(string filePath)
    {
        _stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _writer = new BinaryWriter(_stream);
        _reader = new BinaryReader(_stream);
    }

    /// <summary>
    /// Check if the value is in the tree.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>return true if the value present in the tree otherwise false.</returns>
    public bool Contains(int value)
    {
        try
        {
            Seek(value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public long Seek(int value)
    {
        long offset = 0;
        _stream.Seek(offset, SeekOrigin.Begin);
        try
        {
            while (true)
            {
                Node node = ReadNode(offset);
                if (node.Value == value)
                {
                    return offset;
                }
                if (node.Value > value)
                {
                    offset = node.LeftOffset;
                }
                else
                {
                    offset = node.RightOffset;
                }
                if (offset == 0)
                {
                    throw new Exception("Value not found");
                }
            }
        }
        catch (EndOfStreamException)
        {
        }
        throw new Exception("Value not found");
    }

    public void Insert(int value)
    {
        if (_stream.Length == 0)
        {
            Write(Node.Create(value));
            _stream.Seek(0, SeekOrigin.Begin);
            return;
        }

        long offset = 0;

        while (true)
        {
            var node = ReadNode(offset);

            if (node.Value > value)
            {
                if (node.LeftOffset == 0)
                {
                    node.LeftOffset = Write(Node.Create(value));
                    Update(node);
                    return;
                }
                else if (ReadNode(node.LeftOffset).Value > value)
                {
                    offset = node.LeftOffset;
                }
                else
                {
                    Node newNode = Node.Create(value);
                    newNode.LeftOffset = ReadNode(node.LeftOffset).Offset;
                    newNode.Offset = Write(newNode);
                    node.LeftOffset = newNode.Offset;
                    Update(node);
                    return;

                }
            }
            else
            {
                if (node.RightOffset == 0)
                {
                    node.RightOffset = Write(Node.Create(value));
                    Update(node);
                    return;
                }
                else if (ReadNode(node.RightOffset).Value < value)
                {
                    offset = node.RightOffset;
                }
                else
                {
                    Node newNode = Node.Create(value);
                    newNode.RightOffset = ReadNode(node.RightOffset).Offset;
                    newNode.Offset = Write(newNode);
                    node.RightOffset = newNode.Offset;
                    Update(node);
                    return;
                }
            }
        }
    }

    public bool Delete(int value)
    {
        try
        {
            long offset = Seek(value);
            Node node = ReadNode(offset);
            if (node.LeftOffset == 0 && node.RightOffset == 0)
            {
                _freeOffsets.Enqueue(offset);
                return true;
            }

            throw new NotImplementedException();
        }
        catch (Exception)
        {
            return false;
        }
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

    private void Update(Node node)
    {
        _stream.Seek(node.Offset, SeekOrigin.Begin);
        _writer.Write(node.Value);
        _writer.Write(node.LeftOffset);
        _writer.Write(node.RightOffset);
        _writer.Flush();
    }

    private long Write(Node node)
    {
        if (_freeOffsets.TryDequeue(out long freeOffset))
        {
            _stream.Seek(freeOffset, SeekOrigin.Begin);
        }
        else
        {
            _stream.Seek(0, SeekOrigin.End);
        }

        long currentPosition = _stream.Position;
        _writer.Write(node.Value);
        _writer.Write(node.LeftOffset);
        _writer.Write(node.RightOffset);
        _writer.Flush();

        return currentPosition;
    }

    /// <summary>
    /// format: value, leftOffset, rightOffset
    /// </summary>
    /// <returns></returns>
    private Node ReadNode(long offset)
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