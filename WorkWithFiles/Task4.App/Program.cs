// -
using System;
using System.IO;
using System.Collections.Generic;


namespace WorkWithFiles
{
    namespace Task4
    {
        /*
        Написать программу-загрузчик данных из бинарного формата в текст.

        На вход программа получает бинарный файл, предположительно, это база данных студентов.

        Свойства сущности Student:

            Имя — Name (string);
            Группа — Group (string);
            Дата рождения — DateOfBirth (DateTime).
            Средний балл — (decimal).

        Ваша программа должна:

            Cчитать данные о студентах из файла;
            Создать на рабочем столе директорию Students.
            Внутри раскидать всех студентов из файла по группам
             (каждая группа-отдельный текстовый файл),
             в файле группы студенты перечислены построчно в формате "Имя, дата рождения, средний балл".
        */


        internal class Program
        {
            /// <summary>
            /// Запрашиваем путь к файлу с бинарными данными о студентах
            ///  и путь к папке вывода данных, создаем по указанному пути папку Students,
            ///  сохраняем прочитанные данные в файлы отдельно по группам
            /// </summary>
            /// <param name="args"></param>
            static void Main(string[] args)
            {
                Console.WriteLine("Read Binary");
                Console.WriteLine("---");

                Console.WriteLine("Type in the input file path: ");
                string sFilePath = Console.ReadLine() ?? string.Empty;

                Console.WriteLine("Type in the output folder path: ");
                string sFolderPath = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(sFolderPath))
                {
                    sFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                int nResult = 0;

                if (!(PathChecker(sFilePath) && PathChecker(sFolderPath)))
                {
                    nResult = 1;
                }

                if (nResult == 0)
                {
                    Console.WriteLine($"selected input file:");
                    Console.WriteLine($"  [{Path.GetFullPath(sFilePath)}]");

                    Console.WriteLine($"selected output folder:");
                    Console.WriteLine($"  [{Path.GetFullPath(sFolderPath)}]");

                    sFolderPath = Path.Combine(sFolderPath, "Students");
                    if (!Directory.Exists(sFolderPath))
                        Directory.CreateDirectory(sFolderPath);

                    // students.dat
                    List<Student> studentsToRead = ReadStudentsFromBinFile(sFilePath);

                    string[] line;
                    foreach (Student studentProp in studentsToRead)
                    {
                        Console.WriteLine($"group: [{studentProp.Group}]");
                        string sGroupFilePath = Path.Combine(sFolderPath, studentProp.Group + ".txt");

                        if (!File.Exists(sGroupFilePath))
                        {
                            line = [
                                "Имя",
                                "Дата рождения",
                                "Средний балл"
                            ];
                            File.AppendAllText(sGroupFilePath, string.Join(", ", line) + Environment.NewLine);
                        }

                        Console.WriteLine($"student: [{studentProp.Name}]");

                        line = [
                            studentProp.Name,
                            studentProp.DateOfBirth.ToLongDateString(),
                            studentProp.AverageScore.ToString("0.0")
                        ];

                        File.AppendAllText(sGroupFilePath, string.Join(", ", line) + Environment.NewLine);
                    }
                }

                Console.WriteLine("---");
                Console.WriteLine("return: [" + nResult.ToString() + "]");


                /*
                List<Student> studentsToWrite = new List<Student>
                {
                    new Student { Name = "Жульен", Group = "G1", DateOfBirth = new DateTime(2001, 10, 22), AverageScore = 3.3M },
                    new Student { Name = "Боб", Group = "G1", DateOfBirth = new DateTime(1999, 5, 25), AverageScore = 4.5M},
                    new Student { Name = "Лилия", Group = "F2", DateOfBirth = new DateTime(1999, 1, 11), AverageScore = 5M},
                    new Student { Name = "Роза", Group = "F2", DateOfBirth = new DateTime(1989, 9, 19), AverageScore = 3.7M}
                };
                WriteStudentsToBinFile(studentsToWrite, "students.dat");
                */
            }


            /// <summary>
            /// Проверяем корректность введенного аргумента
            /// </summary>
            /// <param name="sPath">путь до папки или файла</param>
            /// <returns>false - если путь не корректен, или не существует</returns>
            private static bool PathChecker(string sPath)
            {
                bool res = true;

                if (string.IsNullOrWhiteSpace(sPath))
                {
                    Console.WriteLine("err: path is empty");
                    res = false;
                }
                else if (!(Directory.Exists(sPath) || File.Exists(sPath)))
                {
                    Console.WriteLine("err: path doesn't exist");
                    res = false;
                }

                return res;
            }


            /// <summary>
            /// Оригинальная функция записи списка студентов в бинарный файл
            /// </summary>
            /// <param name="students"></param>
            /// <param name="fileName"></param>
            static void WriteStudentsToBinFile(List<Student> students, string fileName)
            {
                using FileStream fs = new FileStream(fileName, FileMode.Create);
                using BinaryWriter bw = new BinaryWriter(fs);

                foreach (Student student in students)
                {
                    bw.Write(student.Name);
                    bw.Write(student.Group);
                    bw.Write(student.DateOfBirth.ToBinary());
                    bw.Write(student.AverageScore);
                }
                bw.Flush();

                bw.Close();
                fs.Close();
            }


            /// <summary>
            /// Оригинальная функция чтения списка студентов из бинарного файла 
            /// </summary>
            /// <param name="fileName"></param>
            /// <returns></returns>
            static List<Student> ReadStudentsFromBinFile(string fileName)
            {
                List<Student> result = new();

                using FileStream fs = new FileStream(fileName, FileMode.Open);

                // using StreamReader sr = new StreamReader(fs);
                // Console.WriteLine(sr.ReadToEnd());
                // fs.Position = 0;

                BinaryReader br = new BinaryReader(fs);

                while (fs.Position < fs.Length)
                {
                    Student student = new Student();

                    student.Name = br.ReadString();
                    student.Group = br.ReadString();
                    long dt = br.ReadInt64();
                    student.DateOfBirth = DateTime.FromBinary(dt);
                    student.AverageScore = br.ReadDecimal();

                    result.Add(student);
                }

                br.Close();
                fs.Close();
                return result;
            }
        }


        /// <summary>
        /// Описание объекта, представляющего запись об одном студенте
        /// </summary>
        internal class Student
        {
            public string Name { get; set; } = string.Empty;
            public string Group { get; set; } = string.Empty;
            public DateTime DateOfBirth { get; set; }
            public decimal AverageScore { get; set; }
        }

    }
}
