# C# Fundamentals - Namespaces and Access Modifiers

## Organizing Your Code 📦

Namespaces and access modifiers help you organize code and control access to your classes and members.

---

## Namespaces

A **namespace** is a container that organizes related classes, interfaces, and other types. They help prevent naming conflicts and organize code logically.

### What are Namespaces?

Think of namespaces as folders for your code. Just as folders organize files on your computer, namespaces organize types in your programs.

### Basic Namespace Syntax

```csharp
namespace MyApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello from MyApplication!");
        }
    }
}
```

### Nested Namespaces

```csharp
namespace MyApplication
{
    namespace Models
    {
        class Person
        {
            public string Name { get; set; }
        }
    }
    
    namespace Services
    {
        class EmailService
        {
            public void SendEmail(string to, string message)
            {
                Console.WriteLine($"Sending email to {to}");
            }
        }
    }
}
```

### Alternative Syntax (Dot Notation)

```csharp
namespace MyApplication.Models
{
    class Person
    {
        public string Name { get; set; }
    }
}

namespace MyApplication.Services
{
    class EmailService
    {
        public void SendEmail(string to, string message)
        {
            Console.WriteLine($"Sending email to {to}");
        }
    }
}
```

---

## Using Namespaces

### Fully Qualified Names

```csharp
namespace MyApplication.Models
{
    class Person
    {
        public string Name { get; set; }
    }
}

namespace MyApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Fully qualified name
            MyApplication.Models.Person person = new MyApplication.Models.Person();
            person.Name = "John";
        }
    }
}
```

### Using Directive

```csharp
using System;
using MyApplication.Models;  // Import namespace

namespace MyApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Now we can use Person directly
            Person person = new Person();
            person.Name = "John";
        }
    }
}
```

### Using Static

Import static members of a class.

```csharp
using static System.Console;  // Import static members
using static System.Math;     // Import Math members

class Program
{
    static void Main(string[] args)
    {
        WriteLine("Hello!");  // No need for Console.WriteLine
        double result = Sqrt(25);  // No need for Math.Sqrt
        WriteLine($"Square root: {result}");
    }
}
```

### Using Alias

Create an alias for a namespace or type to resolve conflicts.

```csharp
using System;
using MyAlias = MyApplication.Models.Person;

class Program
{
    static void Main(string[] args)
    {
        MyAlias person = new MyAlias();
        person.Name = "John";
    }
}
```

---

## Common Namespaces

| Namespace | Purpose |
|-----------|---------|
| `System` | Fundamental classes and base types |
| `System.IO` | File and directory operations |
| `System.Collections.Generic` | Generic collections |
| `System.Linq` | LINQ query operations |
| `System.Text` | String manipulation |
| `System.Threading.Tasks` | Async operations |
| `System.Data` | Database operations |

### Example

```csharp
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Use System
        Console.WriteLine("Hello!");
        
        // Use System.IO
        File.WriteAllText("test.txt", "Content");
        
        // Use System.Collections.Generic
        List<int> numbers = new List<int> { 1, 2, 3 };
        
        // Use System.Linq
        var evenNumbers = numbers.Where(n => n % 2 == 0);
    }
}
```

---

## Access Modifiers

Access modifiers control the visibility and accessibility of types and members.

### Types of Access Modifiers

| Modifier | Description | Accessibility |
|----------|-------------|---------------|
| `public` | Accessible from anywhere | Any code |
| `private` | Only within the containing type | Same class only |
| `protected` | Within containing class or derived classes | Same class + derived |
| `internal` | Within the same assembly | Same assembly |
| `protected internal` | Within same assembly or derived classes | Same assembly + derived |
| `private protected` | Within same assembly and derived classes | Same assembly + derived |

---

## Access Modifiers in Classes

### Public Class
```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    public void DisplayInfo()
    {
        Console.WriteLine($"Name: {Name}, Age: {Age}");
    }
}

// Can be accessed from anywhere
class Program
{
    static void Main(string[] args)
    {
        Person person = new Person();
        person.Name = "John";
        person.Age = 25;
        person.DisplayInfo();
    }
}
```

### Internal Class (Default)
```csharp
class Person  // Same as 'internal class Person'
{
    public string Name { get; set; }
}

// Can only be accessed within the same assembly
```

---

## Access Modifiers in Members

### Public Members
```csharp
class BankAccount
{
    public string AccountNumber { get; set; }
    public double Balance { get; set; }
    
    public void Deposit(double amount)
    {
        Balance += amount;
    }
}

// Accessible from anywhere
class Program
{
    static void Main(string[] args)
    {
        BankAccount account = new BankAccount();
        account.AccountNumber = "12345";  // OK
        account.Balance = 1000;           // OK
        account.Deposit(500);             // OK
    }
}
```

### Private Members
```csharp
class BankAccount
{
    private string accountNumber;
    private double balance;
    
    public void Deposit(double amount)
    {
        balance += amount;  // Can access private member
    }
    
    public double GetBalance()
    {
        return balance;  // Can access private member
    }
}

class Program
{
    static void Main(string[] args)
    {
        BankAccount account = new BankAccount();
        account.accountNumber = "12345";  // ERROR! Cannot access
        account.balance = 1000;           // ERROR! Cannot access
        account.Deposit(500);             // OK
    }
}
```

### Protected Members
```csharp
class Animal
{
    protected string name;
    
    public Animal(string name)
    {
        this.name = name;
    }
    
    protected void Sleep()
    {
        Console.WriteLine($"{name} is sleeping.");
    }
}

class Dog : Animal
{
    public Dog(string name) : base(name)
    {
    }
    
    public void Bark()
    {
        Console.WriteLine($"{name} is barking!");  // Can access protected
        Sleep();  // Can access protected method
    }
}

class Program
{
    static void Main(string[] args)
    {
        Dog dog = new Dog("Buddy");
        dog.Bark();
        // dog.name = "Buddy";  // ERROR! Cannot access protected
    }
}
```

---

## Properties with Access Modifiers

Different access levels for get and set.

```csharp
class Person
{
    private string name;
    
    public string Name
    {
        get { return name; }  // Public read
        private set { name = value; }  // Private write
    }
    
    public Person(string name)
    {
        Name = name;  // Can set within the class
    }
}

class Program
{
    static void Main(string[] args)
    {
        Person person = new Person("John");
        Console.WriteLine(person.Name);  // OK - can read
        
        // person.Name = "Jane";  // ERROR! Cannot write
    }
}
```

---

## Practical Examples

### Example 1: Organized Application Structure

```csharp
using System;
using MyApplication.Models;
using MyApplication.Services;

namespace MyApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        private string Password { get; set; }
        
        public User(int id, string username, string password)
        {
            Id = id;
            Username = username;
            Password = password;
        }
        
        public bool ValidatePassword(string password)
        {
            return Password == password;
        }
    }
}

namespace MyApplication.Services
{
    public class UserService
    {
        public void CreateUser(User user)
        {
            Console.WriteLine($"Creating user: {user.Username}");
        }
        
        public void DeleteUser(int userId)
        {
            Console.WriteLine($"Deleting user: {userId}");
        }
    }
}

namespace MyApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            User user = new User(1, "john_doe", "secret123");
            UserService userService = new UserService();
            
            userService.CreateUser(user);
            
            Console.WriteLine($"User created: {user.Username}");
            Console.WriteLine($"Password valid: {user.ValidatePassword("secret123")}");
        }
    }
}
```

### Example 2: Access Control in Banking

```csharp
using System;

namespace BankingSystem
{
    namespace Models
    {
        public class BankAccount
        {
            private string accountNumber;
            private double balance;
            private string ownerName;
            
            public BankAccount(string accountNumber, string ownerName, double initialBalance)
            {
                this.accountNumber = accountNumber;
                this.ownerName = ownerName;
                this.balance = initialBalance;
            }
            
            // Public read-only properties
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
            
            // Public methods for controlled access
            public void Deposit(double amount)
            {
                if (amount <= 0)
                {
                    throw new ArgumentException("Amount must be positive");
                }
                balance += amount;
                Console.WriteLine($"Deposited ${amount}. New balance: ${balance}");
            }
            
            public bool Withdraw(double amount)
            {
                if (amount <= 0)
                {
                    Console.WriteLine("Amount must be positive");
                    return false;
                }
                
                if (amount > balance)
                {
                    Console.WriteLine("Insufficient funds");
                    return false;
                }
                
                balance -= amount;
                Console.WriteLine($"Withdrew ${amount}. New balance: ${balance}");
                return true;
            }
            
            // Private helper method
            private bool ValidateAmount(double amount)
            {
                return amount > 0;
            }
        }
    }
    
    namespace Services
    {
        public class TransactionService
        {
            public void TransferMoney(BankingSystem.Models.BankAccount fromAccount,
                                     BankingSystem.Models.BankAccount toAccount,
                                     double amount)
            {
                if (fromAccount.Withdraw(amount))
                {
                    toAccount.Deposit(amount);
                    Console.WriteLine("Transfer successful!");
                }
                else
                {
                    Console.WriteLine("Transfer failed!");
                }
            }
        }
    }
}

namespace BankingSystem
{
    using BankingSystem.Models;
    using BankingSystem.Services;
    
    class Program
    {
        static void Main(string[] args)
        {
            BankAccount account1 = new BankAccount("123456", "Alice", 1000);
            BankAccount account2 = new BankAccount("789012", "Bob", 500);
            
            Console.WriteLine("=== Initial Balances ===");
            Console.WriteLine($"Alice: ${account1.Balance}");
            Console.WriteLine($"Bob: ${account2.Balance}");
            
            Console.WriteLine("\n=== Transfer ===");
            TransactionService service = new TransactionService();
            service.TransferMoney(account1, account2, 200);
            
            Console.WriteLine("\n=== Final Balances ===");
            Console.WriteLine($"Alice: ${account1.Balance}");
            Console.WriteLine($"Bob: ${account2.Balance}");
            
            // account1.balance = 10000;  // ERROR! Cannot access private member
        }
    }
}
```

---

## Best Practices

### ✅ DO
- Use namespaces to organize related classes
- Use meaningful namespace names
- Keep namespaces consistent with folder structure
- Use `private` by default, expose only what's necessary
- Use properties instead of public fields
- Use `protected` for members needed by derived classes

### ❌ DON'T
- Put everything in one namespace
- Use `public` for everything
- Expose fields directly (use properties)
- Create deeply nested namespaces (more than 3 levels)

---

## Practice Exercises

### Exercise 1: Create Organized Structure
Create a simple application with proper namespace organization.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using SchoolApp.Models;
using SchoolApp.Services;

namespace SchoolApp.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private int grade;
        
        public Student(int id, string name, int grade)
        {
            Id = id;
            Name = name;
            this.grade = grade;
        }
        
        public int GetGrade()
        {
            return grade;
        }
        
        public void SetGrade(int newGrade)
        {
            if (newGrade >= 0 && newGrade <= 100)
            {
                grade = newGrade;
            }
        }
    }
}

namespace SchoolApp.Services
{
    public class StudentService
    {
        public void DisplayStudentInfo(Student student)
        {
            Console.WriteLine($"ID: {student.Id}");
            Console.WriteLine($"Name: {student.Name}");
            Console.WriteLine($"Grade: {student.GetGrade()}");
        }
    }
}

namespace SchoolApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Student student = new Student(1, "Alice", 95);
            StudentService service = new StudentService();
            
            service.DisplayStudentInfo(student);
        }
    }
}
```
</details>

### Exercise 2: Access Control
Create a class with proper access modifiers for a shopping cart.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.Collections.Generic;

namespace ShoppingApp
{
    public class ShoppingCart
    {
        private List<string> items;
        private List<double> prices;
        
        public ShoppingCart()
        {
            items = new List<string>();
            prices = new List<double>();
        }
        
        public void AddItem(string item, double price)
        {
            items.Add(item);
            prices.Add(price);
            Console.WriteLine($"Added {item} for ${price}");
        }
        
        public double GetTotal()
        {
            double total = 0;
            foreach (double price in prices)
            {
                total += price;
            }
            return total;
        }
        
        public void DisplayCart()
        {
            Console.WriteLine("\n=== Shopping Cart ===");
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"{items[i]}: ${prices[i]}");
            }
            Console.WriteLine($"Total: ${GetTotal()}");
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
            
            // cart.items.Clear();  // ERROR! Cannot access private member
        }
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Not using namespaces
```csharp
// Everything in global namespace
class User { }
class Product { }
class Order { }
```

✅ **Correct:**
```csharp
namespace MyApp.Models
{
    class User { }
    class Product { }
}

namespace MyApp.Services
{
    class Order { }
}
```

❌ **Wrong:** Everything public
```csharp
class BankAccount
{
    public double balance;  // Should be private
    public string accountNumber;  // Should be private
}
```

✅ **Correct:**
```csharp
class BankAccount
{
    private double balance;
    private string accountNumber;
    
    public double Balance { get { return balance; } }
}
```

❌ **Wrong:** Forgetting to import namespace
```csharp
class Program
{
    static void Main(string[] args)
    {
        List<int> numbers = new List<int>();  // ERROR! Missing using
    }
}
```

✅ **Correct:**
```csharp
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        List<int> numbers = new List<int>();  // OK!
    }
}
```

---

## Key Takeaways

### 📌 Namespace Declaration
```csharp
namespace MyNamespace
{
    class MyClass { }
}
```

### 📌 Using Directive
```csharp
using System;
using MyNamespace;
```

### 📌 Access Modifiers
```csharp
public class MyClass
{
    private int field;
    public int Property { get; set; }
    protected void Method() { }
}
```

### 📌 Property Access Levels
```csharp
public string Name { get; private set; }
```

---

## Congratulations! 🎉

You've completed the C# Fundamentals course! You now have a solid foundation in:

✅ Variables and Data Types  
✅ Operators  
✅ Control Flow  
✅ Methods  
✅ Arrays and Collections  
✅ Classes and Objects  
✅ Inheritance and Polymorphism  
✅ Exception Handling  
✅ File I/O  
✅ Namespaces and Access Modifiers  

### What's Next?

Continue your journey with:
- **LINQ** - Language Integrated Query
- **Async/Await** - Asynchronous programming
- **Entity Framework** - Database operations
- **ASP.NET Core** - Web development
- **.NET MAUI** - Cross-platform mobile apps

---

**💡 Tip:** Keep practicing! The best way to learn programming is by building real projects. Try creating a simple application that uses all the concepts you've learned!

---

**⚠️ Remember:** Good code organization and proper access control are essential for building maintainable and scalable applications. Always think about who needs access to what when designing your classes!
