# C# Fundamentals - Getting Started

## Welcome to C# Programming! 🎯

C# (pronounced "C-Sharp") is a modern, object-oriented programming language developed by Microsoft. It's widely used for:
- Desktop applications (Windows)
- Web applications (ASP.NET)
- Mobile apps (Xamarin, .NET MAUI)
- Game development (Unity)
- Cloud services (Azure)

---

## What You'll Learn

In this course, you'll master:
- ✅ Variables and data types
- ✅ Control flow and loops
- ✅ Methods and functions
- ✅ Object-oriented programming
- ✅ Error handling
- ✅ File operations
- ✅ And much more!

---

## Setting Up Your Environment

### Option 1: Visual Studio (Recommended for Windows)
1. Download Visual Studio Community (Free)
2. Install ".NET desktop development" workload
3. Create a new "Console App" project

### Option 2: Visual Studio Code (Cross-platform)
1. Install Visual Studio Code
2. Install the C# extension
3. Install .NET SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com)

### Option 3: Online Compiler (Quick Start)
Use [dotnetfiddle.net](https://dotnetfiddle.net) or [replit.com](https://replit.com) to run C# code in your browser!

---

## Your First C# Program

Let's write the classic "Hello, World!" program:

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        // This prints text to the console
        Console.WriteLine("Hello, World!");
        
        // This waits for user input before closing
        Console.ReadLine();
    }
}
```

### Breaking It Down:

| Line | Explanation |
|------|-------------|
| `using System;` | Imports the System namespace (gives access to Console) |
| `class Program` | Defines a class named "Program" |
| `static void Main` | The entry point of the program |
| `Console.WriteLine()` | Prints output to the console |
| `Console.ReadLine()` | Reads input from the user |

---

## Understanding the Structure

### Namespaces
```csharp
using System;           // Basic functionality
using System.Collections.Generic;  // Lists, dictionaries
using System.Linq;      // Query operations
```

### Classes
```csharp
class Program
{
    // Class members go here
}
```

### Methods
```csharp
static void Main(string[] args)
{
    // Method body
}
```

---

## Try It Yourself!

### Exercise 1: Personal Greeting
Write a program that:
1. Prints "What is your name?"
2. Reads the user's name
3. Prints "Hello, [name]!"

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("What is your name? ");
        string name = Console.ReadLine();
        Console.WriteLine($"Hello, {name}!");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 2: Simple Calculator
Write a program that:
1. Asks for two numbers
2. Adds them together
3. Displays the result

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter first number: ");
        string input1 = Console.ReadLine();
        int num1 = int.Parse(input1);
        
        Console.Write("Enter second number: ");
        string input2 = Console.ReadLine();
        int num2 = int.Parse(input2);
        
        int sum = num1 + num2;
        Console.WriteLine($"The sum is: {sum}");
        Console.ReadLine();
    }
}
```
</details>

---

## Key Concepts Recap

### 📌 Console Methods
- `Console.WriteLine()` - Print and move to next line
- `Console.Write()` - Print without moving to next line
- `Console.ReadLine()` - Read a line of text input

### 📌 Comments
```csharp
// Single-line comment

/* Multi-line
   comment
*/

/// XML documentation comment
```

### 📌 Statements
Every statement in C# must end with a semicolon (`;`).

---

## Common Beginner Mistakes

❌ **Wrong:** Missing semicolon
```csharp
Console.WriteLine("Hello")  // Error!
```

✅ **Correct:**
```csharp
Console.WriteLine("Hello");  // Correct!
```

❌ **Wrong:** Case sensitivity
```csharp
console.writeline("Hello");  // Error!
```

✅ **Correct:**
```csharp
Console.WriteLine("Hello");  // Correct!
```

---

## Next Steps

Great job! You've written your first C# program. 

**Next up:** [Variables and Data Types](./01_Variables_and_Data_Types.md) 📊

---

## Quick Reference

```csharp
// Basic template
using System;

class Program
{
    static void Main(string[] args)
    {
        // Your code here
    }
}
```

---

**💡 Tip:** Practice makes perfect! Try modifying the examples and see what happens.

---

**⚠️ Remember:** C# is case-sensitive. `Console` is not the same as `console`!
