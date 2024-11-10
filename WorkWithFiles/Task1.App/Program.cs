// -
using System;
using System.IO;


namespace WorkWithFiles
{
    namespace Task1
    {
        /*
        Напишите программу, которая чистит нужную нам папку от файлов  и папок,
         которые не использовались более 30 минут

        На вход программа принимает путь до папки. 

        При разработке постарайтесь предусмотреть возможные ошибки (нет прав доступа,
         папка по заданному адресу не существует, передан некорректный путь)
         и уведомить об этом пользователя.
        */


        internal class Program
        {
            /// <summary>
            /// Запрашиваем путь к папке и удаляем в ней подпапки и файлы,
            ///  последнее обращение к которым (LastAccessTime)
            ///  было ранее чем за 30 минут от текущего времени
            /// </summary>
            /// <param name="args"></param>
            static void Main(string[] args)
            {
                Console.WriteLine("Clean a Folder from content older than 30 minutes");
                Console.WriteLine("---");

                Console.WriteLine("Type in the folder path: ");
                string sFolderPath = Console.ReadLine() ?? string.Empty;

                Console.WriteLine($"selected folder:");
                Console.WriteLine($"  [{Path.GetFullPath(sFolderPath)}]");

                int result = CleanFolderByTimeModified(sFolderPath, 30);

                Console.WriteLine("---");
                Console.WriteLine("return: [" + result.ToString() + "]");
            }


            /// <summary>
            /// Проверяем для файлов и подпапок в заданной папке
            ///  время последнего доступа. Если оно отличается от текущего на больший,
            ///  чем указано, промежуток времени, удаляем такой файл или подпапку
            /// </summary>
            /// <param name="sFolderPath">путь до папки</param>
            /// <param name="minutesCount">время, откладываемое в минутах</param>
            /// <returns>количество ошибок удаления</returns>
            private static int CleanFolderByTimeModified(string sFolderPath, int minutesCount)
            {
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
                    // задаем метку времени, отложенную от текущего времени
                    //  на указанное количество минут
                    DateTime nowTime = DateTime.UtcNow;
                    DateTime pickTime = nowTime.Subtract(TimeSpan.FromMinutes(minutesCount));
                    
                    Console.WriteLine($"info: function started ...");

                    DirectoryInfo directory = new DirectoryInfo(sFolderPath);

                    errors = CleanFolderByTimeModifiedRoutine(directory, pickTime);
                    Console.WriteLine($"info: function finished with {errors} errors");
                }

                return errors == 0 ? 0 : 1;
            }


            /// <summary>
            /// Рекурсивно проходим подпапки для указанной в аргументе папки
            ///  удаляем файлы и подпапки, отметка времени доступа у которых была создана раньше,
            ///  чем указано в аргументе
            /// </summary>
            /// <param name="directory">объект папки</param>
            /// <param name="pickTime">метка времени для сравнения</param>
            /// <returns>количество ошибок удаления</returns>
            private static int CleanFolderByTimeModifiedRoutine(DirectoryInfo directory, DateTime pickTime)
            {
                int res = 0;

                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.LastAccessTimeUtc < pickTime)
                    {
                        Console.WriteLine($"info: delete non actual file: {file.Name}");
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {
                            Console.WriteLine("err: error when delete the file");
                            res++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"info: file {file.Name} is actual");
                    }
                }

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    res += CleanFolderByTimeModifiedRoutine(subDirectory, pickTime);

                    if ((subDirectory.GetFiles().Length == 0)
                        && (subDirectory.GetDirectories().Length == 0)
                        && (subDirectory.LastAccessTimeUtc < pickTime))
                    {
                        Console.WriteLine($"info: delete non actual folder: {subDirectory.Name}");
                        try
                        {
                            subDirectory.Delete(false);
                        }
                        catch
                        {
                            Console.WriteLine("err: error when delete the folder");
                            res++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"info: folder {subDirectory.Name} is actual");
                    }
                }

                return res;
            }
        }
    }
}
