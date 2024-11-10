// -
using System;
using System.IO;


namespace WorkWithFiles
{
    namespace Task2
    {
        /*
        Напишите программу, которая считает размер папки на диске
         (вместе со всеми вложенными папками и файлами).
        На вход метод принимает URL директории, в ответ — размер в байтах.
        */


        internal class Program
        {
            /// <summary>
            /// Запрашиваем путь к папке и вычисляем размер ее содержимого
            /// </summary>
            /// <param name="args"></param>
            static void Main(string[] args)
            {
                Console.WriteLine("Calculate folder size");
                Console.WriteLine("---");

                Console.WriteLine("Type in the folder path: ");
                string sFolderPath = Console.ReadLine() ?? string.Empty;

                Console.WriteLine($"selected folder:");
                Console.WriteLine($"  [{Path.GetFullPath(sFolderPath)}]");

                (long size, int errors) = GetDirectorySize(sFolderPath);

                double sizeCalculate;
                Console.WriteLine($"calculated size:");
                if (size < 1024)
                {
                    Console.WriteLine($"  [{size} byte]");
                }
                else if (size < (1024 * 1024))
                {
                    sizeCalculate = size / 1024.0;
                    Console.WriteLine($"  [{sizeCalculate} kB]");
                }
                else
                {
                    sizeCalculate = size / (1024.0 * 1024.0);
                    Console.WriteLine($"  [{sizeCalculate} MB]");
                }
                Console.WriteLine("---");
                Console.WriteLine("return: [" + errors.ToString() + "]");
            }


            /// <summary>
            /// Вычисляем размер содержимого папки
            /// </summary>
            /// <param name="sFolderPath">путь до папки</param>
            /// <returns>размер содержимого, код завершения</returns>
            private static (long size, int errors) GetDirectorySize(string sFolderPath)
            {
                long size = 0;
                int errors = 0;

                if (string.IsNullOrWhiteSpace(sFolderPath))
                {
                    Console.WriteLine("err: path is empty");
                    errors++;
                }
                else if (!Directory.Exists(sFolderPath))
                {
                    Console.WriteLine("err: path doesn't exist");
                    errors++;
                }
                else
                {
                    Console.WriteLine($"info: function started ...");

                    DirectoryInfo directory = new DirectoryInfo(sFolderPath);

                    size = GetDirectorySizeRoutine(directory, out int outErrors);
                    errors = outErrors;

                    Console.WriteLine($"info: function finished with {errors} errors");
                }

                return (size: size, errors: (errors == 0 ? 0 : 1));
            }


            /// <summary>
            /// Рекурсивно проходим подпапки для указанной в аргументе папки,
            ///  вычисляем размер содержащихся в них файлах
            /// </summary>
            /// <param name="directory">объект папки</param>
            /// <param name="outErrors">количество ошибок</param>
            /// <returns>размер содержимого папки</returns>
            private static long GetDirectorySizeRoutine(DirectoryInfo directory, out int outErrors)
            {
                int err = 0;
                long res = 0;
                long size;

                FileInfo[] files = [];
                DirectoryInfo[] subDirectories = [];
                try
                {
                    files = directory.GetFiles();
                    subDirectories = directory.GetDirectories();
                }
                catch
                {
                    Console.WriteLine("err: error getting directory contents");
                    err++;
                }

                foreach (FileInfo file in files)
                {
                    size = 0;
                    try
                    {
                        size = file.Length;
                        Console.WriteLine($"info: file [{file.Name}] size [{size}] byte");
                    }
                    catch
                    {
                        Console.WriteLine("err: error when get the file size");
                        err++;
                    }
                    res += size;
                }

                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    res += GetDirectorySizeRoutine(subDirectory, out int outSubErrors);
                    err += outSubErrors;
                }

                outErrors = err;
                return res;
            }
        }
    }
}
