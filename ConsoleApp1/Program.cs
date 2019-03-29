using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections;

namespace ConsoleApp1
{
    class Person
    {
        public string name;
        public int age;

        public Person() {
            name = "Сергей";
            age = 26;
        }
        public Person(string n)
        {
            name = n;
            age = 26;
        }
        public Person (string n, int m)
        {
            name = n;
            age = m;
        }

        public void GetInfo()
        {
            Console.WriteLine($"Имя :{name} Возраст : {age} ");
            Console.ReadKey();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            
            for (int i = 0; i<1000;i++)
            {

                string input = Console.ReadLine();


                if (File.Exists(input))
                {

                using (StreamReader TextReader = File.OpenText(input))
                    {
                    string text = "";
                    while ((text = TextReader.ReadLine()) != null)
                    Console.WriteLine(text);
                    Console.ReadKey();
                        break;
                    }
                }
                else if(input == Convert.ToString(""))
                       {
                using (StreamReader TextReader = File.OpenText(@"C:\telegram\tportable.1.2.17\Telegram\log.txt"))
                    {
                    string text = "";
                    while ((text = TextReader.ReadLine()) != null)
                    Console.WriteLine(text);
                    Console.ReadKey();
                        break;
                    }
                       }
            else 
                    {
                    Console.WriteLine("Данный файл отсутствует. Введите ещё раз");
                    continue;
                    }

            }




        }
    }
}
