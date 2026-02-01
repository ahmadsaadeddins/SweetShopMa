# C# Fundamentals - Classes and Objects

## Object-Oriented Programming (OOP) 🏗️

Object-Oriented Programming is a programming paradigm based on the concept of "objects", which can contain data and code.

### Key OOP Concepts
- **Class**: A blueprint or template for creating objects
- **Object**: An instance of a class
- **Encapsulation**: Bundling data and methods together
- **Inheritance**: Creating new classes from existing ones
- **Polymorphism**: Objects can take many forms

---

## What is a Class?

A **class** is a user-defined data type that serves as a blueprint for creating objects. It defines:

1. **Fields** (variables) - store data
2. **Properties** - control access to fields
3. **Methods** - define behavior
4. **Constructors** - initialize objects

### Basic Class Structure

```csharp
class Person
{
    // Fields
    private string name;
    private int age;
    
    // Constructor
    public Person(string name, int age)
    {
        this.name = name;
        this.age = age;
    }
    
    // Properties
    public string Name 
    { 
        get { return name; } 
        set { name = value; }
    }
    
    public int Age 
    { 
        get { return age; } 
        set { age = value; }
    }
    
    // Methods
    public void Introduce()
    {
        Console.WriteLine($"Hi, I'm {name} and I'm {age} years old.");
    }
}
```

---

## Creating Objects

An **object** is an instance of a class.

```csharp
// Create an object (instance of Person class)
Person person1 = new Person("Alice", 25);
Person person2 = new Person("Bob", 30);

// Use the object
person1.Introduce();  // Output: Hi, I'm Alice and I'm 25 years old.
person2.Introduce();  // Output: Hi, I'm Bob and I'm 30 years old.
```

---

## Fields

Fields are variables declared in a class.

```csharp
class Car
{
    // Public field (accessible from outside)
    public string brand;
    
    // Private field (only accessible within the class)
    private int mileage;
    
    // Public field with default value
    public int year = 2020;
}
```

### Access Modifiers

| Modifier | Description |
|----------|-------------|
| `public` | Accessible from anywhere |
| `private` | Only accessible within the class |
| `protected` | Accessible within the class and derived classes |
| `internal` | Accessible within the same assembly |

---

## Properties

Properties provide controlled access to private fields.

### Auto-Implemented Properties

```csharp
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Usage
Person person = new Person();
person.Name = "Alice";
person.Age = 25;
```

### Properties with Backing Fields

```csharp
class Person
{
    private string name;
    
    public string Name
    {
        get { return name; }
        set 
        { 
            if (!string.IsNullOrEmpty(value))
            {
                name = value;
            }
        }
    }
}
```

### Read-Only and Write-Only Properties

```csharp
class Person
{
    private string name;
    private int age;
    
    // Read-only property (only get)
    public string Name
    {
        get { return name; }
    }
    
    // Write-only property (only set)
    public int Age
    {
        set { age = value; }
    }
}
```

---

## Constructors

Constructors initialize objects when they're created.

### Default Constructor

```csharp
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    // Default constructor
    public Person()
    {
        Name = "Unknown";
        Age = 0;
    }
}

// Usage
Person person = new Person();
```

### Parameterized Constructor

```csharp
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    // Parameterized constructor
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

// Usage
Person person = new Person("Alice", 25);
```

### Constructor Overloading

```csharp
class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    // Default constructor
    public Person()
    {
        Name = "Unknown";
        Age = 0;
    }
    
    // Parameterized constructor
    public Person(string name)
    {
        Name = name;
        Age = 0;
    }
    
    // Full parameterized constructor
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

// Usage
Person p1 = new Person();
Person p2 = new Person("Alice");
Person p3 = new Person("Bob", 30);
```

---

## Methods

Methods define the behavior of a class.

```csharp
class Calculator
{
    // Public method
    public int Add(int a, int b)
    {
        return a + b;
    }
    
    // Private method (only accessible within the class)
    private int Subtract(int a, int b)
    {
        return a - b;
    }
    
    // Public method that uses private method
    public int Calculate(int a, int b)
    {
        return Subtract(a, b);
    }
}
```

---

## Practical Example: Bank Account

```csharp
using System;

class BankAccount
{
    // Private fields
    private string accountNumber;
    private string ownerName;
    private double balance;
    
    // Constructor
    public BankAccount(string accountNumber, string ownerName, double initialBalance)
    {
        this.accountNumber = accountNumber;
        this.ownerName = ownerName;
        this.balance = initialBalance;
    }
    
    // Properties
    public string AccountNumber 
    { 
        get { return accountNumber; } 
    }
    
    public string OwnerName 
    { 
        get { return ownerName; } 
    }
    
    public double Balance 
    { 
        get { return balance; } 
    }
    
    // Methods
    public void Deposit(double amount)
    {
        if (amount > 0)
        {
            balance += amount;
            Console.WriteLine($"Deposited ${amount}. New balance: ${balance}");
        }
        else
        {
            Console.WriteLine("Invalid deposit amount.");
        }
    }
    
    public void Withdraw(double amount)
    {
        if (amount > 0 && amount <= balance)
        {
            balance -= amount;
            Console.WriteLine($"Withdrew ${amount}. New balance: ${balance}");
        }
        else if (amount > balance)
        {
            Console.WriteLine("Insufficient funds.");
        }
        else
        {
            Console.WriteLine("Invalid withdrawal amount.");
        }
    }
    
    public void DisplayAccountInfo()
    {
        Console.WriteLine("\n=== Account Information ===");
        Console.WriteLine($"Account Number: {accountNumber}");
        Console.WriteLine($"Owner: {ownerName}");
        Console.WriteLine($"Balance: ${balance:F2}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create a bank account
        BankAccount myAccount = new BankAccount("123456789", "John Doe", 1000.00);
        
        // Display account info
        myAccount.DisplayAccountInfo();
        
        // Make transactions
        myAccount.Deposit(500.00);
        myAccount.Withdraw(200.00);
        myAccount.Withdraw(2000.00);  // Insufficient funds
        
        // Display updated info
        myAccount.DisplayAccountInfo();
        
        Console.ReadLine();
    }
}
```

---

## Practical Example: Student Management

```csharp
using System;
using System.Collections.Generic;

class Student
{
    // Auto-implemented properties
    public int StudentId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Major { get; set; }
    
    private List<double> grades;
    
    // Constructor
    public Student(int studentId, string name, int age, string major)
    {
        StudentId = studentId;
        Name = name;
        Age = age;
        Major = major;
        grades = new List<double>();
    }
    
    // Methods
    public void AddGrade(double grade)
    {
        if (grade >= 0 && grade <= 100)
        {
            grades.Add(grade);
            Console.WriteLine($"Added grade {grade} for {Name}");
        }
        else
        {
            Console.WriteLine("Invalid grade. Must be between 0 and 100.");
        }
    }
    
    public double CalculateGPA()
    {
        if (grades.Count == 0)
        {
            return 0.0;
        }
        
        double sum = 0;
        foreach (double grade in grades)
        {
            sum += grade;
        }
        return sum / grades.Count;
    }
    
    public void DisplayStudentInfo()
    {
        Console.WriteLine("\n=== Student Information ===");
        Console.WriteLine($"ID: {StudentId}");
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Age: {Age}");
        Console.WriteLine($"Major: {Major}");
        Console.WriteLine($"GPA: {CalculateGPA():F2}");
        Console.WriteLine($"Number of grades: {grades.Count}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create students
        Student student1 = new Student(1, "Alice Johnson", 20, "Computer Science");
        Student student2 = new Student(2, "Bob Smith", 21, "Mathematics");
        
        // Add grades
        student1.AddGrade(90);
        student1.AddGrade(85);
        student1.AddGrade(92);
        
        student2.AddGrade(78);
        student2.AddGrade(82);
        student2.AddGrade(80);
        
        // Display student information
        student1.DisplayStudentInfo();
        student2.DisplayStudentInfo();
        
        Console.ReadLine();
    }
}
```

---

## Static Members

Static members belong to the class itself, not to instances.

### Static Fields

```csharp
class Counter
{
    // Static field - shared by all instances
    public static int count = 0;
    
    public Counter()
    {
        count++;  // Increment count when object is created
    }
}

// Usage
Counter c1 = new Counter();
Counter c2 = new Counter();
Counter c3 = new Counter();

Console.WriteLine(Counter.count);  // Output: 3
```

### Static Methods

```csharp
class MathHelper
{
    public static int Add(int a, int b)
    {
        return a + b;
    }
    
    public static int Multiply(int a, int b)
    {
        return a * b;
    }
}

// Usage - call on class, not instance
int sum = MathHelper.Add(5, 3);
int product = MathHelper.Multiply(4, 7);
```

### Static Classes

```csharp
static class ConfigurationManager
{
    public static string AppName { get; set; } = "My App";
    public static string Version { get; set; } = "1.0";
    
    public static void DisplayInfo()
    {
        Console.WriteLine($"App: {AppName}, Version: {Version}");
    }
}

// Usage
ConfigurationManager.DisplayInfo();
```

---

## this Keyword

The `this` keyword refers to the current instance of the class.

```csharp
class Person
{
    private string name;
    private int age;
    
    public Person(string name, int age)
    {
        // 'this.name' refers to the field
        // 'name' refers to the parameter
        this.name = name;
        this.age = age;
    }
    
    public void DisplayInfo()
    {
        // 'this' is optional but can make code clearer
        Console.WriteLine($"Name: {this.name}, Age: {this.age}");
    }
}
```

---

## Practice Exercises

### Exercise 1: Rectangle Class
Create a Rectangle class with length, width, and methods to calculate area and perimeter.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Rectangle
{
    public double Length { get; set; }
    public double Width { get; set; }
    
    public Rectangle(double length, double width)
    {
        Length = length;
        Width = width;
    }
    
    public double CalculateArea()
    {
        return Length * Width;
    }
    
    public double CalculatePerimeter()
    {
        return 2 * (Length + Width);
    }
    
    public void DisplayInfo()
    {
        Console.WriteLine($"Rectangle: {Length} x {Width}");
        Console.WriteLine($"Area: {CalculateArea()}");
        Console.WriteLine($"Perimeter: {CalculatePerimeter()}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Rectangle rect = new Rectangle(10, 5);
        rect.DisplayInfo();
        Console.ReadLine();
    }
}
```
</details>

### Exercise 2: Book Class
Create a Book class with title, author, pages, and methods to display info and check if it's a long book.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int Pages { get; set; }
    
    public Book(string title, string author, int pages)
    {
        Title = title;
        Author = author;
        Pages = pages;
    }
    
    public void DisplayInfo()
    {
        Console.WriteLine($"Title: {Title}");
        Console.WriteLine($"Author: {Author}");
        Console.WriteLine($"Pages: {Pages}");
    }
    
    public bool IsLongBook()
    {
        return Pages > 500;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Book book = new Book("The Great Novel", "Jane Doe", 600);
        book.DisplayInfo();
        Console.WriteLine($"Is long book: {book.IsLongBook()}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 3: Temperature Class
Create a Temperature class that stores temperature in Celsius and has methods to convert to Fahrenheit and Kelvin.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Temperature
{
    private double celsius;
    
    public double Celsius 
    { 
        get { return celsius; }
        set 
        { 
            celsius = value;
        }
    }
    
    public Temperature(double celsius)
    {
        this.celsius = celsius;
    }
    
    public double ToFahrenheit()
    {
        return (celsius * 9 / 5) + 32;
    }
    
    public double ToKelvin()
    {
        return celsius + 273.15;
    }
    
    public void DisplayAll()
    {
        Console.WriteLine($"Celsius: {celsius:F2}°C");
        Console.WriteLine($"Fahrenheit: {ToFahrenheit():F2}°F");
        Console.WriteLine($"Kelvin: {ToKelvin():F2}K");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Temperature temp = new Temperature(25);
        temp.DisplayAll();
        Console.ReadLine();
    }
}
```
</details>

### Exercise 4: ShoppingCart Class
Create a ShoppingCart class that manages items and calculates total.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.Collections.Generic;

class ShoppingCart
{
    private Dictionary<string, double> items;
    
    public ShoppingCart()
    {
        items = new Dictionary<string, double>();
    }
    
    public void AddItem(string name, double price)
    {
        if (items.ContainsKey(name))
        {
            items[name] += price;
        }
        else
        {
            items[name] = price;
        }
        Console.WriteLine($"Added {name} (${price})");
    }
    
    public void RemoveItem(string name)
    {
        if (items.Remove(name))
        {
            Console.WriteLine($"Removed {name}");
        }
        else
        {
            Console.WriteLine($"{name} not found in cart");
        }
    }
    
    public double CalculateTotal()
    {
        double total = 0;
        foreach (var item in items)
        {
            total += item.Value;
        }
        return total;
    }
    
    public void DisplayCart()
    {
        Console.WriteLine("\n=== Shopping Cart ===");
        foreach (var item in items)
        {
            Console.WriteLine($"{item.Key}: ${item.Value:F2}");
        }
        Console.WriteLine($"\nTotal: ${CalculateTotal():F2}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        ShoppingCart cart = new ShoppingCart();
        
        cart.AddItem("Laptop", 999.99);
        cart.AddItem("Mouse", 25.50);
        cart.AddItem("Keyboard", 75.00);
        
        cart.DisplayCart();
        
        cart.RemoveItem("Mouse");
        
        cart.DisplayCart();
        
        Console.ReadLine();
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Forgetting to use `new` keyword
```csharp
Person person;  // Only declared, not initialized
person.Name = "Alice";  // Error! Null reference
```

✅ **Correct:**
```csharp
Person person = new Person();  // Properly initialized
person.Name = "Alice";
```

❌ **Wrong:** Accessing private field directly
```csharp
class Person
{
    private string name;
}

Person p = new Person();
p.name = "Alice";  // Error! Can't access private field
```

✅ **Correct:**
```csharp
class Person
{
    private string name;
    public string Name { get { return name; } set { name = value; } }
}

Person p = new Person();
p.Name = "Alice";  // OK!
```

❌ **Wrong:** Not using `this` with parameter name conflict
```csharp
class Person
{
    private string name;
    
    public Person(string name)
    {
        name = name;  // Error! Assigns parameter to itself
    }
}
```

✅ **Correct:**
```csharp
class Person
{
    private string name;
    
    public Person(string name)
    {
        this.name = name;  // OK!
    }
}
```

---

## Key Takeaways

### 📌 Class Definition
```csharp
class ClassName
{
    // Fields, properties, methods
}
```

### 📌 Creating Objects
```csharp
ClassName obj = new ClassName();
```

### 📌 Properties
```csharp
public string Name { get; set; }
```

### 📌 Constructors
```csharp
public ClassName(parameters)
{
    // Initialization code
}
```

### 📌 Methods
```csharp
public ReturnType MethodName(parameters)
{
    // Method body
}
```

---

## Next Steps

Excellent! You now understand classes and objects.

**Next up:** [Inheritance and Polymorphism](./07_Inheritance_and_Polymorphism.md) 🔄

---

**💡 Tip:** Use properties instead of public fields. Properties give you control over how data is accessed and modified, which is essential for encapsulation!

---

**⚠️ Remember:** Always initialize your objects with the `new` keyword! Uninitialized objects will cause null reference exceptions when you try to use them!
