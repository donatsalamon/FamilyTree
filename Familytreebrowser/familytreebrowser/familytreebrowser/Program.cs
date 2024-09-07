using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;

namespace familytreebrowser
{
    internal class Program
    {
        static List<Person> oragnizedFamilyTreeList = new List<Person>();

        //Convert the file into a better version in order be easier to use it -> adding the anchestor properties in order to know the parenst and chil better
        public static void FamilyTreeProcess(Person person, string prevFirstname = "", string prevLastname = "")
        {
            oragnizedFamilyTreeList.Add(person);
            if (person.Children == null && prevLastname != "")
            {
                person.ancestorLastname = prevLastname;
                person.ancestorFirstname = prevFirstname;
            }
            else if (person.Children != null)
            {
                foreach (Person child in person.Children)
                {
                    child.ancestorFirstname = person.FirstName;
                    child.ancestorLastname = person.LastName;

                    FamilyTreeProcess(child, person.FirstName, person.LastName);
                }
            }
            else
            {
                person.ancestorFirstname = "";
                person.ancestorLastname = "";
            }
        }
        //General string converter to have the needed form
        public static string nameExtend(Person person)
        {
            if(person.Gender=="Female")
            {
                return ($"Ms {person.FirstName} {person.LastName} ({person.Gender}), Age: {person.Age}, Ancestor: {person.ancestorFirstname},{person.ancestorLastname}");
            }
            else
            {
                return ($"Mr {person.FirstName} {person.LastName} ({person.Gender}), Age: {person.Age}, Ancestor: {person.ancestorFirstname},{person.ancestorLastname}");
            }
            
        }
        static void Main(string[] args)
        {
            int counter = 0;
            string temp = Console.ReadLine();
            List<Person> familyTreeList = new List<Person>();
            List<Person> tempOrderByList;
            //using while loop in order to be able to continuesly use the program (type if you want to stop the program)
            while (temp != "exit")
            {
                args = temp.Split(' ');
                // these if-else are in use in order to prevent the program from crashing if the information what is written into the console would be incorrect
                if (args[0] == "-input")
                {
                    //string filePath = "C:\\Users";

                    if (familyTreeList.Count == 0)
                    {
                        //This is where the json file reading is happenning
                        //First it reads the whole file and with on of its class function it just "split" it and loads it to the list,
                        //which has a class model based on the json file except 2 other properties which are there in order to make the structure more easier to handle(in my opinion)
                        string json = File.ReadAllText(args[1]);

                        familyTreeList = JsonConvert.DeserializeObject<List<Person>>(json);
                        foreach (Person person in familyTreeList)
                        {
                            FamilyTreeProcess(person);
                        }

                    }
                    if (args.Length < 3)
                    {
                        // -input familytrees.json
                        string splitReadLine = "";
                        foreach (Person person in oragnizedFamilyTreeList)
                        {
                            if (counter == 0)
                            {
                               Console.WriteLine(nameExtend(person));
                                counter++;
                            }
                            else if (!(counter % 10 == 0))
                            {
                                Console.WriteLine(nameExtend(person));
                                counter++;
                            }
                            else
                            {
                                splitReadLine = Console.ReadLine();
                                if (splitReadLine == "")
                                {
                                    // Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Gender}), Age: {person.Age}, Ancestor: {person.ancestorFirstname},{person.ancestorLastname}");
                                    Console.WriteLine(nameExtend(person));
                                    counter++;
                                }
                            }
                        }
                    }

                    if (args.Length > 2)
                    {
                        // -input familytrees.json -sort age/lastname
                        if (args[2] == "-sort")
                        {
                            if (args.Length > 3)
                            {
                                if (args[3] == "age")
                                {
                                    Console.WriteLine("\n");
                                    //LINQ expression to organize the list by age
                                    tempOrderByList = oragnizedFamilyTreeList.OrderBy(person => person.Age).ToList();
                                    foreach (Person person in tempOrderByList)
                                    {
                                        Console.WriteLine(nameExtend(person));
                                    }
                                    //Clear the list if the command would be used multiple times the data shouldbt be duplicated
                                    tempOrderByList.Clear();
                                }
                                else if (args[3] == "lastname")
                                {
                                    Console.WriteLine("\n");
                                    //LINQ expression to organize the list by the lastname
                                    tempOrderByList = oragnizedFamilyTreeList.OrderBy(person => person.LastName).ToList();
                                    foreach (Person person in tempOrderByList)
                                    {
                                        Console.WriteLine(nameExtend(person));
                                    }
                                    tempOrderByList.Clear();
                                }
                                else
                                {
                                    Console.WriteLine("Invalid paramter");
                                }
                            }
                            else
                            {
                                Console.WriteLine("There wasnt any paramter to sort by");
                                Console.WriteLine("/n");
                            }
                        }
                        // -input familytrees.json -search (any part of the name)
                        else if (args[2] == "-search")
                        {
                            if (args.Length > 3)
                            {
                                string nameSnippet = "";
                                nameSnippet = args[3];
                                List<Person> nameSnippetContainsList = new List<Person>();
                                nameSnippetContainsList = oragnizedFamilyTreeList.Where(person => person.LastName.ToLower().Contains(nameSnippet.ToLower())
                                || person.FirstName.ToLower().Contains(nameSnippet.ToLower())).ToList();

                                foreach (Person person in nameSnippetContainsList)
                                {
                                    Console.WriteLine(nameExtend(person));
                                }
                                Console.WriteLine("\n");
                            }
                            else
                            {
                                Console.WriteLine("Forgot to write a search word");
                            }
                        }
                        // -input familytrees.json -findduplicateinfamilytree firstname
                        else if (args[2] == "-findduplicateinfamilytree")
                        {
                            if (args.Length > 3)
                            {
                                if (args[3] == "firstname")
                                {
                                    List<Person> resultFamilyHeads= new List<Person>();

                                    //Linq expression to first find the "heads" of the families and use their firstname
                                    //to see if anyone else who is NOT a family head has the same firstname
                                    resultFamilyHeads = oragnizedFamilyTreeList
                                    .Where(person => person.ancestorFirstname == null && person.ancestorFirstname == null&&
                                    oragnizedFamilyTreeList.Any(current => current.FirstName== person.FirstName && current.ancestorFirstname==person.FirstName))
                                    .ToList();

                                    foreach (Person person in resultFamilyHeads)
                                    {
                                        Console.WriteLine(nameExtend(person));
                                    }
                                    Console.WriteLine("/n");
                                }
                                else
                                {
                                    Console.WriteLine("Misstyped argument");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Forgot to write a firstname");
                            }
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Incorrect command");
                }
                temp = Console.ReadLine();
            }
            //  Console.ReadKey();
        }
    }
}

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Gender { get; set; }
    public int Age { get; set; }
    public List<Person> Children { get; set; }
    public string ancestorFirstname { get; set; }
    public string ancestorLastname { get; set; }
}