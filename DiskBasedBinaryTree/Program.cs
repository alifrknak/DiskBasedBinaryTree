public class Program
{
    private static void Main(string[] args)
    {
        const string path = "binaryTree.dat";

        // it is adding to make  development easier
        File.Delete(path);

        using DiskBinaryTree tree = new DiskBinaryTree(path);

        tree.Insert(10);
        tree.Insert(30);
        tree.Insert(1);
        tree.Insert(11);
        tree.Insert(100);
        tree.Insert(5);
        tree.Insert(9);

        tree.Print();

        //TODO: add search method
        //TODO:  add delete method
        //TODO:  add unit tests

        //TODO:  add concurrency support



    }
}
