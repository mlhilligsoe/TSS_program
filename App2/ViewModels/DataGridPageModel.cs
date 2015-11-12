using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using MyToolkit.Mvvm;

namespace App2.ViewModels
{
    public class DataGridPageModel : ViewModelBase
    {
        private Person[] _people;

        public DataGridPageModel()
        {
            var list = new List<Person>
            {
                new Person{FirstName = "John", LastName = "Doe", Category = "A"},
                new Person{FirstName = "Max", LastName = "Muster", Category = "B"},
            };

            var random = new Random();
            for (int i = 0; i < 3; i++)
            {
                list.Add(new Person
                {
                    FirstName = "Foo" + random.Next(0, 99999),
                    LastName = "Bar" + random.Next(0, 99999),
                    Category = "C" + i
                });
            }

            People = list.ToArray();
        }

        /// <summary>Gets or sets the people. </summary>
        public Person[] People
        {
            get { return _people; }
            set { Set(ref _people, value); }
        }
    }

    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Category { get; set; }
    }
}
