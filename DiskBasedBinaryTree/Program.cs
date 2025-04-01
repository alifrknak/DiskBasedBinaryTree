public class Program
{
    private static void Main(string[] args)
    {
        const string path = "binaryTree.dat";

        // it is adding to make  development easier
        File.Delete(path);

        using DiskBinaryTree tree = new DiskBinaryTree(path);

        tree.Insert(10);
        Console.WriteLine(tree.Delete(10));

        tree.Insert(20);

        tree.Print();

    }
}
