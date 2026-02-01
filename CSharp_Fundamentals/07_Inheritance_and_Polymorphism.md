# C# Fundamentals - Inheritance and Polymorphism

## Advanced OOP Concepts 🔄

Inheritance and polymorphism are powerful features that allow you to create reusable and flexible code.

---

## Inheritance

**Inheritance** allows a class to acquire the properties and methods of another class.

### Key Terms

| Term | Description |
|------|-------------|
| **Base Class** (Parent) | The class being inherited from |
| **Derived Class** (Child) | The class that inherits |
| `: baseClass` | Syntax to inherit from a class |

### Basic Inheritance Example

```csharp
// Base class
class Animal
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    public void Eat()
    {
        Console.WriteLine($"{Name} is eating.");
    }
    
    public void Sleep()
    {
        Console.WriteLine($"{Name} is sleeping.");
    }
}

// Derived class
class Dog : Animal
{
    public void Bark()
    {
        Console.WriteLine($"{Name} is barking!");
    }
}

// Another derived class
class Cat : Animal
{
    public void Meow()
    {
        Console.WriteLine($"{Name} is meowing!");
    }
}

// Usage
class Program
{
    static void Main(string[] args)
    {
        Dog dog = new Dog();
        dog.Name = "Buddy";
        dog.Age = 3;
        
        dog.Eat();    // Inherited from Animal
        dog.Bark();   // Dog's own method
        
        Cat cat = new Cat();
        cat.Name = "Whiskers";
        cat.Age = 2;
        
        cat.Sleep();  // Inherited from Animal
        cat.Meow();   // Cat's own method
    }
}
```

---

## Protected Access Modifier

`protected` members are accessible within the class and derived classes.

```csharp
class Vehicle
{
    protected string brand;
    protected int year;
    
    public Vehicle(string brand, int year)
    {
        this.brand = brand;
        this.year = year;
    }
    
    public void DisplayInfo()
    {
        Console.WriteLine($"Brand: {brand}, Year: {year}");
    }
}

class Car : Vehicle
{
    public int doors;
    
    public Car(string brand, int year, int doors) : base(brand, year)
    {
        this.doors = doors;
    }
    
    public void DisplayDetails()
    {
        Console.WriteLine($"Brand: {brand}");  // Can access protected member
        Console.WriteLine($"Year: {year}");    // Can access protected member
        Console.WriteLine($"Doors: {doors}");
    }
}
```

---

## base Keyword

The `base` keyword is used to access members of the base class.

```csharp
class Animal
{
    public string Name { get; set; }
    
    public Animal(string name)
    {
        Name = name;
    }
    
    public virtual void MakeSound()
    {
        Console.WriteLine("Some generic animal sound");
    }
}

class Dog : Animal
{
    public string Breed { get; set; }
    
    public Dog(string name, string breed) : base(name)
    {
        Breed = breed;
    }
    
    public void DisplayInfo()
    {
        Console.WriteLine($"Name: {Name}");  // Inherited property
        Console.WriteLine($"Breed: {Breed}"); // Own property
    }
    
    public override void MakeSound()
    {
        base.MakeSound();  // Call base class method
        Console.WriteLine("Woof! Woof!");
    }
}
```

---

## Method Hiding with new

Use the `new` keyword to hide a base class method.

```csharp
class Animal
{
    public void Eat()
    {
        Console.WriteLine("Animal is eating.");
    }
}

class Dog : Animal
{
    public new void Eat()
    {
        Console.WriteLine("Dog is eating dog food.");
    }
}

// Usage
Animal animal = new Dog();
animal.Eat();  // Output: Animal is eating.

Dog dog = new Dog();
dog.Eat();  // Output: Dog is eating dog food.
```

---

## Polymorphism

**Polymorphism** allows objects to be treated as instances of their base class while maintaining their specific behavior.

### Virtual and Override Methods

```csharp
class Shape
{
    public virtual void Draw()
    {
        Console.WriteLine("Drawing a shape");
    }
    
    public virtual double CalculateArea()
    {
        return 0;
    }
}

class Circle : Shape
{
    public double Radius { get; set; }
    
    public Circle(double radius)
    {
        Radius = radius;
    }
    
    public override void Draw()
    {
        Console.WriteLine("Drawing a circle");
    }
    
    public override double CalculateArea()
    {
        return Math.PI * Radius * Radius;
    }
}

class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    
    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }
    
    public override void Draw()
    {
        Console.WriteLine("Drawing a rectangle");
    }
    
    public override double CalculateArea()
    {
        return Width * Height;
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Polymorphism in action
        Shape[] shapes = new Shape[3];
        shapes[0] = new Circle(5);
        shapes[1] = new Rectangle(4, 6);
        shapes[2] = new Circle(3);
        
        foreach (Shape shape in shapes)
        {
            shape.Draw();
            Console.WriteLine($"Area: {shape.CalculateArea():F2}\n");
        }
    }
}
```

---

## Abstract Classes and Methods

**Abstract classes** cannot be instantiated and are meant to be inherited from.

```csharp
// Abstract class
abstract class Animal
{
    public string Name { get; set; }
    
    public Animal(string name)
    {
        Name = name;
    }
    
    // Regular method
    public void Sleep()
    {
        Console.WriteLine($"{Name} is sleeping.");
    }
    
    // Abstract method (must be implemented by derived classes)
    public abstract void MakeSound();
}

class Dog : Animal
{
    public string Breed { get; set; }
    
    public Dog(string name, string breed) : base(name)
    {
        Breed = breed;
    }
    
    // Must implement abstract method
    public override void MakeSound()
    {
        Console.WriteLine($"{Name} barks: Woof! Woof!");
    }
}

class Cat : Animal
{
    public Cat(string name) : base(name)
    {
    }
    
    public override void MakeSound()
    {
        Console.WriteLine($"{Name} meows: Meow!");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Cannot instantiate abstract class
        // Animal animal = new Animal("Generic");  // Error!
        
        Dog dog = new Dog("Buddy", "Golden Retriever");
        Cat cat = new Cat("Whiskers");
        
        dog.MakeSound();
        dog.Sleep();
        
        cat.MakeSound();
        cat.Sleep();
    }
}
```

---

## Sealed Classes and Methods

**Sealed** prevents a class from being inherited or a method from being overridden.

```csharp
// Sealed class - cannot be inherited
sealed class FinalClass
{
    public void Method()
    {
        Console.WriteLine("This class cannot be inherited");
    }
}

// Error! Cannot inherit from sealed class
// class DerivedClass : FinalClass { }

class BaseClass
{
    public virtual void Method()
    {
        Console.WriteLine("Base method");
    }
}

class DerivedClass : BaseClass
{
    // Sealed method - cannot be overridden further
    public sealed override void Method()
    {
        Console.WriteLine("Derived method (cannot be overridden)");
    }
}
```

---

## Interfaces

An **interface** defines a contract that classes must implement.

### Defining an Interface

```csharp
interface IPlayable
{
    void Play();
    void Pause();
    void Stop();
}

interface IRecordable
{
    void Record();
    void StopRecording();
}
```

### Implementing Interfaces

```csharp
class MusicPlayer : IPlayable, IRecordable
{
    public void Play()
    {
        Console.WriteLine("Playing music...");
    }
    
    public void Pause()
    {
        Console.WriteLine("Music paused.");
    }
    
    public void Stop()
    {
        Console.WriteLine("Music stopped.");
    }
    
    public void Record()
    {
        Console.WriteLine("Recording...");
    }
    
    public void StopRecording()
    {
        Console.WriteLine("Recording stopped.");
    }
}

class VideoPlayer : IPlayable
{
    public void Play()
    {
        Console.WriteLine("Playing video...");
    }
    
    public void Pause()
    {
        Console.WriteLine("Video paused.");
    }
    
    public void Stop()
    {
        Console.WriteLine("Video stopped.");
    }
}
```

### Interface as Parameter

```csharp
class Program
{
    static void TestPlayer(IPlayable player)
    {
        player.Play();
        player.Pause();
        player.Stop();
    }
    
    static void Main(string[] args)
    {
        MusicPlayer musicPlayer = new MusicPlayer();
        VideoPlayer videoPlayer = new VideoPlayer();
        
        Console.WriteLine("=== Music Player ===");
        TestPlayer(musicPlayer);
        
        Console.WriteLine("\n=== Video Player ===");
        TestPlayer(videoPlayer);
    }
}
```

---

## Practical Example: Employee Management

```csharp
using System;
using System.Collections.Generic;

// Base class
abstract class Employee
{
    public string Name { get; set; }
    public int Id { get; set; }
    protected double BaseSalary { get; set; }
    
    public Employee(string name, int id, double baseSalary)
    {
        Name = name;
        Id = id;
        BaseSalary = baseSalary;
    }
    
    // Abstract method - must be implemented by derived classes
    public abstract double CalculateSalary();
    
    // Virtual method - can be overridden
    public virtual void DisplayInfo()
    {
        Console.WriteLine($"ID: {Id}, Name: {Name}");
    }
}

// Derived class
class FullTimeEmployee : Employee
{
    public double Bonus { get; set; }
    
    public FullTimeEmployee(string name, int id, double baseSalary, double bonus) 
        : base(name, id, baseSalary)
    {
        Bonus = bonus;
    }
    
    public override double CalculateSalary()
    {
        return BaseSalary + Bonus;
    }
    
    public override void DisplayInfo()
    {
        base.DisplayInfo();
        Console.WriteLine($"Type: Full-Time, Salary: ${CalculateSalary():F2}");
    }
}

// Another derived class
class PartTimeEmployee : Employee
{
    public double HourlyRate { get; set; }
    public int HoursWorked { get; set; }
    
    public PartTimeEmployee(string name, int id, double hourlyRate, int hoursWorked) 
        : base(name, id, 0)
    {
        HourlyRate = hourlyRate;
        HoursWorked = hoursWorked;
    }
    
    public override double CalculateSalary()
    {
        return HourlyRate * HoursWorked;
    }
    
    public override void DisplayInfo()
    {
        base.DisplayInfo();
        Console.WriteLine($"Type: Part-Time, Hours: {HoursWorked}, Salary: ${CalculateSalary():F2}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        List<Employee> employees = new List<Employee>();
        
        employees.Add(new FullTimeEmployee("Alice Johnson", 1, 5000, 1000));
        employees.Add(new PartTimeEmployee("Bob Smith", 2, 20, 80));
        employees.Add(new FullTimeEmployee("Charlie Brown", 3, 6000, 1500));
        
        Console.WriteLine("=== Employee Report ===\n");
        
        double totalPayroll = 0;
        
        foreach (Employee emp in employees)
        {
            emp.DisplayInfo();
            totalPayroll += emp.CalculateSalary();
            Console.WriteLine();
        }
        
        Console.WriteLine($"Total Payroll: ${totalPayroll:F2}");
    }
}
```

---

## Practical Example: Shape Hierarchy

```csharp
using System;

// Abstract base class
abstract class Shape
{
    public string Color { get; set; }
    
    public Shape(string color)
    {
        Color = color;
    }
    
    // Abstract methods
    public abstract double CalculateArea();
    public abstract double CalculatePerimeter();
    
    // Virtual method
    public virtual void DisplayInfo()
    {
        Console.WriteLine($"Color: {Color}");
    }
}

// Interface for resizable shapes
interface IResizable
{
    void Resize(double factor);
}

// Derived class
class Circle : Shape, IResizable
{
    public double Radius { get; set; }
    
    public Circle(double radius, string color) : base(color)
    {
        Radius = radius;
    }
    
    public override double CalculateArea()
    {
        return Math.PI * Radius * Radius;
    }
    
    public override double CalculatePerimeter()
    {
        return 2 * Math.PI * Radius;
    }
    
    public void Resize(double factor)
    {
        Radius *= factor;
    }
    
    public override void DisplayInfo()
    {
        base.DisplayInfo();
        Console.WriteLine($"Type: Circle");
        Console.WriteLine($"Radius: {Radius:F2}");
        Console.WriteLine($"Area: {CalculateArea():F2}");
        Console.WriteLine($"Perimeter: {CalculatePerimeter():F2}");
    }
}

// Another derived class
class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    
    public Rectangle(double width, double height, string color) : base(color)
    {
        Width = width;
        Height = height;
    }
    
    public override double CalculateArea()
    {
        return Width * Height;
    }
    
    public override double CalculatePerimeter()
    {
        return 2 * (Width + Height);
    }
    
    public override void DisplayInfo()
    {
        base.DisplayInfo();
        Console.WriteLine($"Type: Rectangle");
        Console.WriteLine($"Width: {Width:F2}, Height: {Height:F2}");
        Console.WriteLine($"Area: {CalculateArea():F2}");
        Console.WriteLine($"Perimeter: {CalculatePerimeter():F2}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Circle circle = new Circle(5, "Red");
        Rectangle rectangle = new Rectangle(4, 6, "Blue");
        
        Console.WriteLine("=== Circle ===");
        circle.DisplayInfo();
        
        Console.WriteLine("\n=== Rectangle ===");
        rectangle.DisplayInfo();
        
        // Resize the circle
        Console.WriteLine("\n=== Resizing Circle ===");
        circle.Resize(1.5);
        circle.DisplayInfo();
    }
}
```

---

## Practice Exercises

### Exercise 1: Vehicle Hierarchy
Create a base Vehicle class and derived Car and Motorcycle classes.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

abstract class Vehicle
{
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    
    public Vehicle(string brand, string model, int year)
    {
        Brand = brand;
        Model = model;
        Year = year;
    }
    
    public abstract void Start();
    public abstract void Stop();
    
    public void DisplayInfo()
    {
        Console.WriteLine($"{Year} {Brand} {Model}");
    }
}

class Car : Vehicle
{
    public int Doors { get; set; }
    
    public Car(string brand, string model, int year, int doors) 
        : base(brand, model, year)
    {
        Doors = doors;
    }
    
    public override void Start()
    {
        Console.WriteLine($"{Brand} {Model} car is starting with key ignition.");
    }
    
    public override void Stop()
    {
        Console.WriteLine($"{Brand} {Model} car is stopping.");
    }
}

class Motorcycle : Vehicle
{
    public bool HasSidecar { get; set; }
    
    public Motorcycle(string brand, string model, int year, bool hasSidecar) 
        : base(brand, model, year)
    {
        HasSidecar = hasSidecar;
    }
    
    public override void Start()
    {
        Console.WriteLine($"{Brand} {Model} motorcycle is starting with kick start.");
    }
    
    public override void Stop()
    {
        Console.WriteLine($"{Brand} {Model} motorcycle is stopping.");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Car car = new Car("Toyota", "Camry", 2020, 4);
        Motorcycle moto = new Motorcycle("Harley-Davidson", "Street 750", 2019, false);
        
        car.DisplayInfo();
        car.Start();
        car.Stop();
        
        Console.WriteLine();
        
        moto.DisplayInfo();
        moto.Start();
        moto.Stop();
    }
}
```
</details>

### Exercise 2: Bank Account Hierarchy
Create abstract BankAccount class and Savings/Checking derived classes.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

abstract class BankAccount
{
    public string AccountNumber { get; set; }
    public string OwnerName { get; set; }
    protected double Balance { get; set; }
    
    public BankAccount(string accountNumber, string ownerName, double initialBalance)
    {
        AccountNumber = accountNumber;
        OwnerName = ownerName;
        Balance = initialBalance;
    }
    
    public abstract void CalculateInterest();
    
    public void Deposit(double amount)
    {
        Balance += amount;
        Console.WriteLine($"Deposited ${amount}. New balance: ${Balance:F2}");
    }
    
    public void Withdraw(double amount)
    {
        if (amount <= Balance)
        {
            Balance -= amount;
            Console.WriteLine($"Withdrew ${amount}. New balance: ${Balance:F2}");
        }
        else
        {
            Console.WriteLine("Insufficient funds.");
        }
    }
    
    public void DisplayBalance()
    {
        Console.WriteLine($"Account: {AccountNumber}, Balance: ${Balance:F2}");
    }
}

class SavingsAccount : BankAccount
{
    public double InterestRate { get; set; }
    
    public SavingsAccount(string accountNumber, string ownerName, double initialBalance, double interestRate) 
        : base(accountNumber, ownerName, initialBalance)
    {
        InterestRate = interestRate;
    }
    
    public override void CalculateInterest()
    {
        double interest = Balance * InterestRate / 100;
        Balance += interest;
        Console.WriteLine($"Interest calculated: ${interest:F2}. New balance: ${Balance:F2}");
    }
}

class CheckingAccount : BankAccount
{
    public double OverdraftLimit { get; set; }
    
    public CheckingAccount(string accountNumber, string ownerName, double initialBalance, double overdraftLimit) 
        : base(accountNumber, ownerName, initialBalance)
    {
        OverdraftLimit = overdraftLimit;
    }
    
    public override void CalculateInterest()
    {
        Console.WriteLine("Checking accounts don't earn interest.");
    }
    
    public new void Withdraw(double amount)
    {
        if (amount <= Balance + OverdraftLimit)
        {
            Balance -= amount;
            Console.WriteLine($"Withdrew ${amount}. New balance: ${Balance:F2}");
        }
        else
        {
            Console.WriteLine("Exceeds overdraft limit.");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        SavingsAccount savings = new SavingsAccount("SAV001", "John Doe", 1000, 2.5);
        CheckingAccount checking = new CheckingAccount("CHK001", "John Doe", 500, 200);
        
        Console.WriteLine("=== Savings Account ===");
        savings.DisplayBalance();
        savings.CalculateInterest();
        
        Console.WriteLine("\n=== Checking Account ===");
        checking.DisplayBalance();
        checking.CalculateInterest();
        checking.Withdraw(600);  // Uses overdraft
    }
}
```
</details>

### Exercise 3: Interface Implementation
Create an IPrintable interface and implement it in multiple classes.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

interface IPrintable
{
    void Print();
}

interface IScannable
{
    void Scan();
}

class Document : IPrintable, IScannable
{
    public string Title { get; set; }
    public string Content { get; set; }
    
    public Document(string title, string content)
    {
        Title = title;
        Content = content;
    }
    
    public void Print()
    {
        Console.WriteLine($"Printing document: {Title}");
        Console.WriteLine(Content);
    }
    
    public void Scan()
    {
        Console.WriteLine($"Scanning document: {Title}");
    }
}

class Photo : IPrintable
{
    public string FileName { get; set; }
    public int Resolution { get; set; }
    
    public Photo(string fileName, int resolution)
    {
        FileName = fileName;
        Resolution = resolution;
    }
    
    public void Print()
    {
        Console.WriteLine($"Printing photo: {FileName} at {Resolution} DPI");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Document doc = new Document("My Document", "This is the content.");
        Photo photo = new Photo("vacation.jpg", 300);
        
        Console.WriteLine("=== Document ===");
        doc.Print();
        doc.Scan();
        
        Console.WriteLine("\n=== Photo ===");
        photo.Print();
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Forgetting to implement abstract methods
```csharp
abstract class Animal
{
    public abstract void MakeSound();
}

class Dog : Animal
{
    // Error! Must implement abstract method
}
```

✅ **Correct:**
```csharp
class Dog : Animal
{
    public override void MakeSound()
    {
        Console.WriteLine("Woof!");
    }
}
```

❌ **Wrong:** Wrong signature when overriding
```csharp
class Base
{
    public virtual void Method(int x) { }
}

class Derived : Base
{
    public override void Method(string x)  // Error! Wrong signature
    {
    }
}
```

✅ **Correct:**
```csharp
class Derived : Base
{
    public override void Method(int x)  // OK! Same signature
    {
    }
}
```

---

## Key Takeaways

### 📌 Inheritance
```csharp
class DerivedClass : BaseClass
{
    // Additional members
}
```

### 📌 Abstract Classes
```csharp
abstract class ClassName
{
    public abstract void Method();  // Must be implemented
}
```

### 📌 Virtual/Override
```csharp
public virtual void Method() { }
public override void Method() { }
```

### 📌 Interfaces
```csharp
interface IInterface
{
    void Method();  // Must be implemented
}
```

---

## Next Steps

Great job! You now understand inheritance and polymorphism.

**Next up:** [Exception Handling](./08_Exception_Handling.md) ⚠️

---

**💡 Tip:** Use abstract classes when you want to provide common implementation but force derived classes to implement certain methods. Use interfaces when you want to define a contract without any implementation!

---

**⚠️ Remember:** A class can inherit from only ONE base class, but can implement MULTIPLE interfaces!
