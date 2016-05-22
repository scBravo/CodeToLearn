using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.Serialization;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Practice
{
    /*
     * If someone has a comment about this code, please send comment to me. Thanks!
     *
     * Реализовать класс Employee, который будет иметь открытые свойства:
     *  Last Name,
     *   First Name,
     *    Age,
     *     Department,
     *      Address
     * и закрытое поле EmployeeID.
     * Класс должен сериализоваться в XML формат. 
     * Рутовая нода при сериализации/десериализации Employees должна называться Employees (а не ArrayOfEmployee например)
Написать Console Application, которое должно выполнить следующую последовательность действий:
1.	Десериализовать из заранее подготовленного файла коллекцию объектов Employee, 
при этом поле EmployeeID должно содержать в себе значение Last Name + First Name.
2.	Вывести на экран информацию о каждом Employee в таком виде: № п/п, Last Name: value, First Name: value, и т.д.
Значения EmployeeID и Address взять из закрытых полей используя рефлексию.
3.	Отобрать Employee в возрасте от 25 до 35 лет и упорядочить их по EmployeeID. 
Полученную коллекцию сериализовать в новый файл.
    */
    [DataContract]
    public class Employee:IComparer<Employee>,IComparable<Employee>
    {
        [DataMember (Order =0)]
        public string Name { get; set; }
        [DataMember (Order = 1)]
        public string LastName { get; set; }
        [DataMember (Order = 2)]
        public int Age { get; set; }
        [DataMember (Order = 3)]
        public string Department { get; set; }
        [DataMember (Order = 4)]
        public string Address { get; set; }    
        [DataMember (Order = 5)]

        private string EmployeeID;
        
        public Employee() { }
        public Employee(string firstName,string lastName,int age,string department,string address)
        {
            Name = firstName;
            LastName = lastName;
            Age = age;
            Department = department;
            Address = address;
       
            EmployeeID = firstName + " " + lastName;
        }

        public override string ToString()
        {
            return string.Format("Employee subject is: \n Name: {0} \n Lastname: {1} \n Age: {2}\n Department: {3}\n Adress: {4} \n EmployeeID: {5} \n",Name,LastName,Age,Department,Address,Utils.GetEmpIDByReflection(this));
        }

        [OnSerialized]
        private void OnSerializing(StreamingContext context)
        {
            EmployeeID = Name + " " + LastName;
        }
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            EmployeeID = Name + " " + LastName;
        }

        public int Compare(Employee x, Employee y)
        {
            int result = x.EmployeeID.ToString().CompareTo(y.EmployeeID.ToString());
            if (result == -1)
                return -1;
            else if (result == 1)
                return 1;
            else
                return 0;                         
        }

        public int CompareTo(Employee other)
        {
            return Compare(this, other);
        }
    }
    
    interface IContainer
    {
        void PrintEmployees();//{  }

        void RestoreList(string path);//Deserialization file by filename       

        void SaveList(string path);//Serealize in string argument filename

        void GetYoungEmployeers();//Serealize Employeers which age between 25 and 35        
    }

    [DataContract]
    class Container<T> : IContainer,IList<T>/*Just want IList interaface, it is not matter is this case*/ where T:Employee
    {
        [DataMember]
        List<Employee> content = new List<Employee>();

        public T this[int index]
        {
            get
            {
                return ((IList<T>)content)[index];
            }

            set
            {
                ((IList<T>)content)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<T>)content).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<T>)content).IsReadOnly;
            }
        }

        public void Add(T item)
        {
            ((IList<T>)content).Add(item);
        }

        public void Clear()
        {
            ((IList<T>)content).Clear();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)content).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>)content).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)content).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)content).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)content).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((IList<T>)content).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)content).RemoveAt(index);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)content).GetEnumerator();
        }

        public void PrintEmployees()
        {
            foreach(T elem in content)
            {
                Console.WriteLine(elem.ToString());
            }
        }

        public void RestoreList(string path)//Deserialization file by filename
        {
            using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                DataContractSerializer ds = new DataContractSerializer(typeof(List<Employee>), rootName: "Employees", rootNamespace: "");
                if (content != null)
                    content.Clear();
                content = (List<Employee>)ds.ReadObject(fs);                
            }
        }

        public void SaveList(string path)//Serealize in string argument filename
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                DataContractSerializer ds = new DataContractSerializer(typeof(List<Employee>),rootName:"Employees",rootNamespace:"");                    
                ds.WriteObject(fs, content);
            }
        }

        public void Sort()
        {
            content.Sort();
        }
        
        public void GetYoungEmployeers()//Serealize Employeers which age between 25 and 35
        {
            Container<Employee> resultCollection = new Container<Employee>();
            foreach (Employee elem in this)
            {
                if (elem.Age > 25 && elem.Age < 35)
                    resultCollection.Add(elem);
            }
            resultCollection.Sort();
            resultCollection.SaveList(@"D:\temp\Result.xml");       
        }
    }

    class Utils
    {
        public static string GetEmpIDByReflection(Employee obj)//Very very strange spike, but it is demand, so it is...
        {
            FieldInfo f = typeof(Employee).GetField("EmployeeID", BindingFlags.NonPublic | BindingFlags.Instance);
            return f.GetValue(obj).ToString();
        }
    }
}
