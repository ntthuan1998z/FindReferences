using System;


namespace GetReferences
{
    class Program
    {
        static void Main(string[] args)
        {
            MainProgram pg = new MainProgram();
            pg.Execute();

            Console.Write($"{Environment.NewLine}Press any key to exit...");
            Console.ReadKey(true);
        }
    }

}
