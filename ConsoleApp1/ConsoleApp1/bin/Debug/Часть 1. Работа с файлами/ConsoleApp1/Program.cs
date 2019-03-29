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
                using (StreamReader TextReader = File.OpenText(@"C:\Users\pevnev_sa\source\repos\ConsoleApp1\ConsoleApp1\bin\Debug\Новый текстовый документ (3).txt"))
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
