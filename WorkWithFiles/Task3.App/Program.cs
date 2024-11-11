// -
using System;
using System.IO;


namespace WorkWithFiles
{
    namespace Task3
    {
        /*
        Показать, сколько весит папка до очистки.
        Использовать метод из задания 2. 
        Выполнить очистку.
        Показать сколько файлов удалено и сколько места освобождено.
        Показать, сколько папка весит после очистки.
        */


        internal class Program
        {
            /// <summary>
            /// Запрашиваем путь к папке, вычисляем ее размер,
            ///  удаляем в ней подпапки и файлы,
            ///  последнее обращение к которым (LastAccessTime)
            ///  было ранее чем за 30 минут от текущего времени,
            ///  вычисляем полученный размер после операции очистки
            /// </summary>
            /// <param name="args"></param>
            static void Main(string[] args)
            {
                Console.WriteLine("Calculate and Clean");
                Console.WriteLine("---");

                Console.WriteLine("Type in the folder path: ");
                string sFolderPath = Console.ReadLine() ?? string.Empty;

                Console.WriteLine($"selected folder:");
                Console.WriteLine($"  [{Path.GetFullPath(sFolderPath)}]");

                int nResult = 0;
                int sizeResult = 0;
                int cleanResult = 0;

                // dry-run - 1, full-run - 0
                int nRun = 0;

                if (!PathChecker(sFolderPath))
                {
                    nResult = 1;
                    nRun += 2;
                }

                // будем хранить в дополнительной переменной размер освобождаемого места
                long size0 = 0;

                // вычисляем размер папки,
                //  затем выполняем очистку,
                //  затем вычисляем размер папки снова
                while (nRun < 2)
                {
                    Console.WriteLine($"calculate folder size");
                    (long size, int errors1) = GetDirectorySize(sFolderPath);
                    sizeResult = errors1;

                    if (size0 != 0)
                        size0 -= size;

                    Console.WriteLine($"calculated size:");
                    Console.WriteLine($"  [{PresentSizeValue(size)}]");

                    if (nRun == 0)
                    {
                        size0 = size;

                        Console.WriteLine($"clean files and subfolders");
                        (int count, int errors2) = CleanFolderByTimeModified(sFolderPath, 30);
                        cleanResult = errors2;

                        Console.WriteLine($"deleted files and subfolders:");
                        Console.WriteLine($"  [{count}]");
                    }
                    nRun++;
                }

                // если размер изменился, то показываем сообщение с результатом
                if (size0 != 0)
                {
                    Console.WriteLine($"cleaned space:");
                    Console.WriteLine($"  [{PresentSizeValue(size0)}]");
                }

                nResult = (nResult == 0 && sizeResult == 0 && cleanResult == 0) ? 0 : 1;

                Console.WriteLine("---");
                Console.WriteLine("return: [" + nResult.ToString() + "]");
            }


            /// <summary>
            /// Проверяем корректность введенного аргумента
            /// </summary>
            /// <param name="sFolderPath">путь до папки</param>
            /// <returns>false - если путь не корректен, или не существует</returns>
            private static bool PathChecker(string sFolderPath)
            {
                bool res = true;

                if (string.IsNullOrWhiteSpace(sFolderPath))
                {
                    Console.WriteLine("err: path is empty");
                    res = false;
                }
                else if (!Directory.Exists(sFolderPath))
                {
                    Console.WriteLine("err: path doesn't exist");
                    res = false;
                }

                return res;
            }


            /// <summary>
            /// Формируем строку отображения размера в килобайтах и мегабайтах
            /// </summary>
            /// <param name="size">размер в байтах</param>
            /// <returns>строка с размером, вычисленным в соответствующих единицах</returns>
            private static string PresentSizeValue(long size)
            {
                string res;
                double sizeCalculate;

                if (size < 1024)
                {
                    res = $"{size} byte";
                }
                else if (size < (1024 * 1024))
                {
                    sizeCalculate = size / 1024.0;
                    res = $"{sizeCalculate} kB";
                }
                else
                {
                    sizeCalculate = size / (1024.0 * 1024.0);
                    res = $"{sizeCalculate} MB";
                }

                return res;
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

                Console.WriteLine($"info: function started ...");

                DirectoryInfo directory = new DirectoryInfo(sFolderPath);

                size = GetDirectorySizeRoutine(directory, out int outErrors);
                errors = outErrors;

                Console.WriteLine($"info: function finished with {errors} errors");

                return (size: size, errors: (errors == 0 ? 0 : 1));
            }


            /// <summary>
            /// Рекурсивно проходим подпапки для указанной в аргументе папки,
            ///  вычисляем размер содержащихся в них файлах
            /// </summary>
            /// <param name="directory">объект папки</param>
            /// <param name="outErrors">возвращаем количество ошибок в ходе работы функции</param>
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
                        // Console.WriteLine($"info: file [{file.Name}] size [{size}] byte");
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


            /// <summary>
            /// Проверяем для файлов и подпапок в заданной папке
            ///  время последнего доступа. Если оно отличается от текущего на больший,
            ///  чем указано, промежуток времени, удаляем такой файл или подпапку
            /// </summary>
            /// <param name="sFolderPath">путь до папки</param>
            /// <param name="minutesCount">время, откладываемое в минутах</param>
            /// <returns>количество операций удаления, код завершения</returns>
            private static (int count, int errors) CleanFolderByTimeModified(string sFolderPath, int minutesCount)
            {
                int count = 0;
                int errors = 0;

                // задаем метку времени, отложенную от текущего времени
                //  на указанное количество минут
                DateTime nowTime = DateTime.UtcNow;
                DateTime pickTime = nowTime.Subtract(TimeSpan.FromMinutes(minutesCount));
                
                Console.WriteLine($"info: function started ...");

                DirectoryInfo directory = new DirectoryInfo(sFolderPath);

                count = CleanFolderByTimeModifiedRoutine(directory, pickTime, out int outErrors);
                errors = outErrors;

                Console.WriteLine($"info: function finished with {errors} errors");

                return (count: count, errors: (errors == 0 ? 0 : 1));
            }


            /// <summary>
            /// Рекурсивно проходим подпапки для указанной в аргументе папки
            ///  удаляем файлы и подпапки, отметка времени доступа у которых была создана раньше,
            ///  чем указано в аргументе
            /// </summary>
            /// <param name="directory">объект папки</param>
            /// <param name="pickTime">метка времени для сравнения</param>
            /// <param name="outErrors">возвращаем количество ошибок в ходе работы функции</param>
            /// <returns>возвращаем количество удаленных файлов и подпапок</returns>
            private static int CleanFolderByTimeModifiedRoutine(DirectoryInfo directory, DateTime pickTime, out int outErrors)
            {
                int err = 0;
                int res = 0;

                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.LastAccessTimeUtc < pickTime)
                    {
                        Console.WriteLine($"info: delete non actual file: {file.Name}");
                        try
                        {
                            file.Delete();
                            res++;
                        }
                        catch
                        {
                            Console.WriteLine("err: error when delete the file");
                            err++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"info: file {file.Name} is actual");
                    }
                }

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    res += CleanFolderByTimeModifiedRoutine(subDirectory, pickTime, out int outSubErrors);
                    err += outSubErrors;
                }

                if ((directory.GetFiles().Length == 0)
                    && (directory.GetDirectories().Length == 0)
                    && (directory.LastAccessTimeUtc < pickTime))
                {
                    Console.WriteLine($"info: delete non actual folder: {directory.Name}");
                    try
                    {
                        directory.Delete(false);
                        res++;
                    }
                    catch
                    {
                        Console.WriteLine("err: error when delete the folder");
                        err++;
                    }
                }
                else
                {
                    Console.WriteLine($"info: folder {directory.Name} is actual");
                }

                outErrors = err;
                return res;
            }

        }
    }
}
