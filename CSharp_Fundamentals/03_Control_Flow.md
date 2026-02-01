# C# Fundamentals - Control Flow

## What is Control Flow? 🔄

Control flow determines the order in which code statements are executed. It allows your program to make decisions and repeat actions based on conditions.

---

## If Statements

The `if` statement executes code only when a condition is true.

### Basic If Statement

```csharp
int age = 18;

if (age >= 18)
{
    Console.WriteLine("You are an adult");
}
```

### If-Else Statement

```csharp
int age = 15;

if (age >= 18)
{
    Console.WriteLine("You are an adult");
}
else
{
    Console.WriteLine("You are a minor");
}
```

### If-Else If-Else Statement

```csharp
int score = 85;

if (score >= 90)
{
    Console.WriteLine("Grade: A");
}
else if (score >= 80)
{
    Console.WriteLine("Grade: B");
}
else if (score >= 70)
{
    Console.WriteLine("Grade: C");
}
else if (score >= 60)
{
    Console.WriteLine("Grade: D");
}
else
{
    Console.WriteLine("Grade: F");
}
```

### Practical Example: Login System

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string username = "admin";
        string password = "12345";
        
        Console.Write("Username: ");
        string inputUser = Console.ReadLine();
        
        Console.Write("Password: ");
        string inputPass = Console.ReadLine();
        
        if (username == inputUser && password == inputPass)
        {
            Console.WriteLine("Login successful!");
        }
        else
        {
            Console.WriteLine("Invalid username or password!");
        }
        
        Console.ReadLine();
    }
}
```

---

## Switch Statement

The `switch` statement is a cleaner way to handle multiple conditions.

### Basic Switch

```csharp
int dayOfWeek = 3;

switch (dayOfWeek)
{
    case 1:
        Console.WriteLine("Monday");
        break;
    case 2:
        Console.WriteLine("Tuesday");
        break;
    case 3:
        Console.WriteLine("Wednesday");
        break;
    case 4:
        Console.WriteLine("Thursday");
        break;
    case 5:
        Console.WriteLine("Friday");
        break;
    case 6:
        Console.WriteLine("Saturday");
        break;
    case 7:
        Console.WriteLine("Sunday");
        break;
    default:
        Console.WriteLine("Invalid day");
        break;
}
```

### Switch with String

```csharp
string grade = "A";

switch (grade)
{
    case "A":
        Console.WriteLine("Excellent!");
        break;
    case "B":
        Console.WriteLine("Good!");
        break;
    case "C":
        Console.WriteLine("Average");
        break;
    case "D":
        Console.WriteLine("Below average");
        break;
    case "F":
        Console.WriteLine("Fail");
        break;
    default:
        Console.WriteLine("Invalid grade");
        break;
}
```

### Switch Expression (C# 8.0+)

```csharp
int dayOfWeek = 3;
string dayName = dayOfWeek switch
{
    1 => "Monday",
    2 => "Tuesday",
    3 => "Wednesday",
    4 => "Thursday",
    5 => "Friday",
    6 => "Saturday",
    7 => "Sunday",
    _ => "Invalid day"
};

Console.WriteLine(dayName);
```

### Practical Example: Menu System

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Main Menu ===");
        Console.WriteLine("1. View Profile");
        Console.WriteLine("2. Settings");
        Console.WriteLine("3. Logout");
        Console.Write("Choose an option: ");
        
        string choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                Console.WriteLine("Opening profile...");
                break;
            case "2":
                Console.WriteLine("Opening settings...");
                break;
            case "3":
                Console.WriteLine("Logging out...");
                break;
            default:
                Console.WriteLine("Invalid option!");
                break;
        }
        
        Console.ReadLine();
    }
}
```

---

## Loops

Loops allow you to execute code repeatedly.

### For Loop

Use when you know how many times to iterate.

```csharp
for (int i = 1; i <= 5; i++)
{
    Console.WriteLine($"Count: {i}");
}
```

**Output:**
```
Count: 1
Count: 2
Count: 3
Count: 4
Count: 5
```

### For Loop Components

```csharp
for (initialization; condition; increment)
{
    // code to execute
}
```

| Component | Purpose | Example |
|-----------|---------|---------|
| Initialization | Set starting value | `int i = 1` |
| Condition | Check before each iteration | `i <= 5` |
| Increment | Update after each iteration | `i++` |

### Practical Example: Multiplication Table

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int number = 5;
        
        Console.WriteLine($"Multiplication Table of {number}:");
        
        for (int i = 1; i <= 10; i++)
        {
            int result = number * i;
            Console.WriteLine($"{number} x {i} = {result}");
        }
        
        Console.ReadLine();
    }
}
```

### While Loop

Use when you don't know how many times to iterate.

```csharp
int count = 1;

while (count <= 5)
{
    Console.WriteLine($"Count: {count}");
    count++;
}
```

### Practical Example: Password Checker

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string correctPassword = "12345";
        string inputPassword = "";
        int attempts = 0;
        int maxAttempts = 3;
        
        while (inputPassword != correctPassword && attempts < maxAttempts)
        {
            Console.Write("Enter password: ");
            inputPassword = Console.ReadLine();
            attempts++;
            
            if (inputPassword != correctPassword)
            {
                Console.WriteLine($"Incorrect! Attempts left: {maxAttempts - attempts}");
            }
        }
        
        if (inputPassword == correctPassword)
        {
            Console.WriteLine("Access granted!");
        }
        else
        {
            Console.WriteLine("Access denied! Too many attempts.");
        }
        
        Console.ReadLine();
    }
}
```

### Do-While Loop

Always executes at least once.

```csharp
int count = 1;

do
{
    Console.WriteLine($"Count: {count}");
    count++;
} while (count <= 5);
```

### Practical Example: Menu System

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        bool running = true;
        
        do
        {
            Console.WriteLine("\n=== Main Menu ===");
            Console.WriteLine("1. Option 1");
            Console.WriteLine("2. Option 2");
            Console.WriteLine("3. Exit");
            Console.Write("Choose: ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    Console.WriteLine("You chose Option 1");
                    break;
                case "2":
                    Console.WriteLine("You chose Option 2");
                    break;
                case "3":
                    Console.WriteLine("Exiting...");
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option!");
                    break;
            }
        } while (running);
    }
}
```

### Foreach Loop

Used to iterate through collections.

```csharp
string[] fruits = { "Apple", "Banana", "Orange" };

foreach (string fruit in fruits)
{
    Console.WriteLine(fruit);
}
```

### Practical Example: Shopping Cart

```csharp
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        List<string> cart = new List<string>
        {
            "Laptop - $999",
            "Mouse - $25",
            "Keyboard - $75"
        };
        
        Console.WriteLine("=== Shopping Cart ===");
        Console.WriteLine();
        
        double total = 0;
        int itemCount = 0;
        
        foreach (string item in cart)
        {
            Console.WriteLine(item);
            itemCount++;
        }
        
        Console.WriteLine();
        Console.WriteLine($"Total items: {itemCount}");
        Console.ReadLine();
    }
}
```

---

## Break and Continue

### Break

Exits the loop immediately.

```csharp
for (int i = 1; i <= 10; i++)
{
    if (i == 5)
    {
        break;  // Exit loop when i is 5
    }
    Console.WriteLine(i);
}
```

**Output:**
```
1
2
3
4
```

### Continue

Skips the current iteration and continues with the next.

```csharp
for (int i = 1; i <= 10; i++)
{
    if (i % 2 == 0)
    {
        continue;  // Skip even numbers
    }
    Console.WriteLine(i);
}
```

**Output:**
```
1
3
5
7
9
```

### Practical Example: Number Filter

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Printing odd numbers from 1 to 20:");
        
        for (int i = 1; i <= 20; i++)
        {
            if (i % 2 == 0)
            {
                continue;  // Skip even numbers
            }
            
            if (i > 15)
            {
                break;  // Stop after 15
            }
            
            Console.WriteLine(i);
        }
        
        Console.ReadLine();
    }
}
```

---

## Nested Loops

Loops inside loops.

### Example: Multiplication Table

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        for (int i = 1; i <= 5; i++)
        {
            for (int j = 1; j <= 5; j++)
            {
                Console.Write($"{i * j}\t");
            }
            Console.WriteLine();
        }
        Console.ReadLine();
    }
}
```

**Output:**
```
1	2	3	4	5	
2	4	6	8	10	
3	6	9	12	15	
4	8	12	16	20	
5	10	15	20	25	
```

### Example: Pattern Printing

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int rows = 5;
        
        for (int i = 1; i <= rows; i++)
        {
            for (int j = 1; j <= i; j++)
            {
                Console.Write("* ");
            }
            Console.WriteLine();
        }
        
        Console.ReadLine();
    }
}
```

**Output:**
```
* 
* * 
* * * 
* * * * 
* * * * * 
```

---

## Practice Exercises

### Exercise 1: Number Guessing Game
Write a program that:
1. Generates a random number (1-100)
2. Asks user to guess
3. Gives hints (too high/too low)
4. Counts attempts

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Random random = new Random();
        int secretNumber = random.Next(1, 101);
        int attempts = 0;
        int guess = 0;
        
        Console.WriteLine("=== Number Guessing Game ===");
        Console.WriteLine("Guess a number between 1 and 100");
        
        while (guess != secretNumber)
        {
            Console.Write("Enter your guess: ");
            guess = int.Parse(Console.ReadLine());
            attempts++;
            
            if (guess < secretNumber)
            {
                Console.WriteLine("Too low! Try again.");
            }
            else if (guess > secretNumber)
            {
                Console.WriteLine("Too high! Try again.");
            }
        }
        
        Console.WriteLine($"Congratulations! You guessed it in {attempts} attempts!");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 2: Factorial Calculator
Write a program that calculates the factorial of a number.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int number = 5;
        int factorial = 1;
        
        for (int i = 1; i <= number; i++)
        {
            factorial *= i;
        }
        
        Console.WriteLine($"Factorial of {number} is {factorial}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 3: Prime Number Checker
Write a program that checks if a number is prime.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int number = 17;
        bool isPrime = true;
        
        if (number < 2)
        {
            isPrime = false;
        }
        else
        {
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0)
                {
                    isPrime = false;
                    break;
                }
            }
        }
        
        Console.WriteLine($"{number} is {(isPrime ? "prime" : "not prime")}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 4: Fibonacci Sequence
Write a program that prints the first N numbers of the Fibonacci sequence.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int n = 10;
        int first = 0, second = 1;
        
        Console.WriteLine($"First {n} Fibonacci numbers:");
        
        for (int i = 0; i < n; i++)
        {
            Console.Write(first + " ");
            
            int temp = first + second;
            first = second;
            second = temp;
        }
        
        Console.ReadLine();
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Infinite loop
```csharp
int i = 0;
while (i < 10)
{
    Console.WriteLine(i);
    // Forgot to increment i!
}
```

✅ **Correct:**
```csharp
int i = 0;
while (i < 10)
{
    Console.WriteLine(i);
    i++;  // Don't forget to increment!
}
```

❌ **Wrong:** Off-by-one error
```csharp
for (int i = 0; i <= 10; i++)  // This runs 11 times!
```

✅ **Correct:**
```csharp
for (int i = 0; i < 10; i++)  // This runs 10 times
```

❌ **Wrong:** Missing break in switch
```csharp
case 1:
    Console.WriteLine("One");
    // Forgot break! Falls through to case 2
case 2:
    Console.WriteLine("Two");
    break;
```

✅ **Correct:**
```csharp
case 1:
    Console.WriteLine("One");
    break;  // Always use break!
case 2:
    Console.WriteLine("Two");
    break;
```

---

## Key Takeaways

### 📌 If-Else
```csharp
if (condition)
{
    // code
}
else if (condition)
{
    // code
}
else
{
    // code
}
```

### 📌 Switch
```csharp
switch (value)
{
    case 1:
        // code
        break;
    default:
        // code
        break;
}
```

### 📌 For Loop
```csharp
for (int i = 0; i < 10; i++)
{
    // code
}
```

### 📌 While Loop
```csharp
while (condition)
{
    // code
}
```

### 📌 Foreach Loop
```csharp
foreach (var item in collection)
{
    // code
}
```

---

## Next Steps

Great job! You now understand control flow.

**Next up:** [Methods](./04_Methods.md) 🎯

---

**💡 Tip:** Use `foreach` when you don't need to modify the collection or track the index. It's cleaner and less error-prone!

---

**⚠️ Remember:** Always include `break` statements in your `switch` cases (unless you intentionally want fall-through behavior)!
