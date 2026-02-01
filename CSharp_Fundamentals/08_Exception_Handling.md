# C# Fundamentals - Exception Handling

## What are Exceptions? ⚠️

An **exception** is an error that occurs during program execution. Exception handling allows your program to gracefully handle errors instead of crashing.

---

## Why Handle Exceptions?

### Without Exception Handling
```csharp
int[] numbers = { 1, 2, 3 };
Console.WriteLine(numbers[10]);  // CRASH! Index out of range
```

### With Exception Handling
```csharp
try
{
    int[] numbers = { 1, 2, 3 };
    Console.WriteLine(numbers[10]);
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred: " + ex.Message);
}
// Program continues running
```

---

## Try-Catch Block

The basic structure for handling exceptions.

### Syntax
```csharp
try
{
    // Code that might cause an exception
}
catch (ExceptionType ex)
{
    // Code to handle the exception
}
```

### Example
```csharp
try
{
    int result = 10 / 0;  // Division by zero
}
catch (DivideByZeroException ex)
{
    Console.WriteLine("Cannot divide by zero!");
    Console.WriteLine($"Error: {ex.Message}");
}
```

---

## Common Exception Types

| Exception | When It Occurs |
|-----------|----------------|
| `DivideByZeroException` | Division by zero |
| `IndexOutOfRangeException` | Array index out of range |
| `NullReferenceException` | Accessing null object |
| `FormatException` | Invalid format conversion |
| `FileNotFoundException` | File not found |
| `InvalidOperationException` | Invalid operation |
| `ArgumentException` | Invalid argument |

### Examples

```csharp
// DivideByZeroException
try
{
    int result = 10 / 0;
}
catch (DivideByZeroException ex)
{
    Console.WriteLine("Division by zero!");
}

// IndexOutOfRangeException
try
{
    int[] numbers = { 1, 2, 3 };
    int value = numbers[10];
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine("Index out of range!");
}

// NullReferenceException
try
{
    string text = null;
    int length = text.Length;  // Error!
}
catch (NullReferenceException ex)
{
    Console.WriteLine("Object is null!");
}

// FormatException
try
{
    string number = "abc";
    int value = int.Parse(number);
}
catch (FormatException ex)
{
    Console.WriteLine("Invalid number format!");
}
```

---

## Multiple Catch Blocks

Handle different exceptions differently.

```csharp
try
{
    Console.Write("Enter a number: ");
    string input = Console.ReadLine();
    int number = int.Parse(input);
    int result = 100 / number;
    Console.WriteLine($"Result: {result}");
}
catch (DivideByZeroException ex)
{
    Console.WriteLine("Error: Cannot divide by zero!");
}
catch (FormatException ex)
{
    Console.WriteLine("Error: Please enter a valid number!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Order Matters
```csharp
try
{
    // Some code
}
catch (DivideByZeroException ex)
{
    // More specific exception first
    Console.WriteLine("Division by zero");
}
catch (Exception ex)
{
    // General exception last
    Console.WriteLine("General error");
}
```

---

## Finally Block

Code in `finally` always executes, whether an exception occurs or not.

```csharp
try
{
    Console.WriteLine("Opening file...");
    // File operations
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    Console.WriteLine("Closing file...");  // Always executes
}
```

### Practical Example: File Handling

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        StreamReader reader = null;
        
        try
        {
            reader = new StreamReader("data.txt");
            string content = reader.ReadToEnd();
            Console.WriteLine(content);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("File not found!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Always close the file
            if (reader != null)
            {
                reader.Close();
                Console.WriteLine("File closed.");
            }
        }
    }
}
```

---

## Throwing Exceptions

You can throw your own exceptions.

### Throw Statement
```csharp
int age = -5;

if (age < 0)
{
    throw new ArgumentException("Age cannot be negative!");
}
```

### Custom Exception Messages
```csharp
void SetAge(int age)
{
    if (age < 0)
    {
        throw new ArgumentException("Age cannot be negative!", nameof(age));
    }
    if (age > 150)
    {
        throw new ArgumentException("Age is unrealistic!", nameof(age));
    }
    // Set age...
}
```

---

## Custom Exceptions

Create your own exception types.

```csharp
// Custom exception class
class InvalidAgeException : Exception
{
    public InvalidAgeException() : base("Invalid age provided.")
    {
    }
    
    public InvalidAgeException(string message) : base(message)
    {
    }
    
    public InvalidAgeException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

// Using custom exception
class Person
{
    private int age;
    
    public int Age
    {
        get { return age; }
        set
        {
            if (value < 0 || value > 150)
            {
                throw new InvalidAgeException($"Age {value} is invalid.");
            }
            age = value;
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Person person = new Person();
            person.Age = 200;  // Invalid!
        }
        catch (InvalidAgeException ex)
        {
            Console.WriteLine($"Custom Error: {ex.Message}");
        }
    }
}
```

---

## Exception Properties

### Message
```csharp
try
{
    int[] numbers = { 1, 2, 3 };
    int value = numbers[10];
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine(ex.Message);
    // Output: Index was outside the bounds of the array.
}
```

### StackTrace
```csharp
try
{
    int result = 10 / 0;
}
catch (DivideByZeroException ex)
{
    Console.WriteLine(ex.StackTrace);
    // Shows the sequence of calls that led to the exception
}
```

### InnerException
```csharp
try
{
    try
    {
        int result = 10 / 0;
    }
    catch (DivideByZeroException innerEx)
    {
        throw new Exception("Calculation failed", innerEx);
    }
}
catch (Exception outerEx)
{
    Console.WriteLine($"Outer: {outerEx.Message}");
    Console.WriteLine($"Inner: {outerEx.InnerException.Message}");
}
```

---

## Practical Examples

### Example 1: Safe User Input

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Safe Number Input ===\n");
        
        int number = GetValidNumber("Enter a number: ");
        Console.WriteLine($"You entered: {number}");
        
        Console.ReadLine();
    }
    
    static int GetValidNumber(string prompt)
    {
        while (true)
        {
            try
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                int number = int.Parse(input);
                
                if (number < 0)
                {
                    Console.WriteLine("Please enter a positive number.");
                    continue;
                }
                
                return number;
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input! Please enter a valid number.");
            }
            catch (OverflowException)
            {
                Console.WriteLine("Number too large or too small!");
            }
        }
    }
}
```

### Example 2: Bank Account with Validation

```csharp
using System;

class InsufficientFundsException : Exception
{
    public double Balance { get; }
    public double Amount { get; }
    
    public InsufficientFundsException(double balance, double amount)
        : base($"Insufficient funds. Balance: ${balance}, Attempted: ${amount}")
    {
        Balance = balance;
        Amount = amount;
    }
}

class BankAccount
{
    public string AccountNumber { get; set; }
    public double Balance { get; private set; }
    
    public BankAccount(string accountNumber, double initialBalance)
    {
        if (initialBalance < 0)
        {
            throw new ArgumentException("Initial balance cannot be negative.");
        }
        
        AccountNumber = accountNumber;
        Balance = initialBalance;
    }
    
    public void Deposit(double amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive.");
        }
        
        Balance += amount;
        Console.WriteLine($"Deposited ${amount}. New balance: ${Balance:F2}");
    }
    
    public void Withdraw(double amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be positive.");
        }
        
        if (amount > Balance)
        {
            throw new InsufficientFundsException(Balance, amount);
        }
        
        Balance -= amount;
        Console.WriteLine($"Withdrew ${amount}. New balance: ${Balance:F2}");
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            BankAccount account = new BankAccount("123456", 1000);
            
            account.Deposit(500);
            account.Withdraw(200);
            
            // This will throw an exception
            account.Withdraw(2000);
        }
        catch (InsufficientFundsException ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nUnexpected error: {ex.Message}");
        }
        
        Console.ReadLine();
    }
}
```

### Example 3: File Operations

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "notes.txt";
        
        try
        {
            // Write to file
            WriteToFile(filePath, "Hello, World!");
            
            // Read from file
            string content = ReadFromFile(filePath);
            Console.WriteLine($"File content: {content}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"File I/O error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        
        Console.ReadLine();
    }
    
    static void WriteToFile(string path, string content)
    {
        try
        {
            File.WriteAllText(path, content);
            Console.WriteLine($"Successfully wrote to {path}");
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Directory not found!");
            throw;
        }
        catch (PathTooLongException)
        {
            Console.WriteLine("Path too long!");
            throw;
        }
    }
    
    static string ReadFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File {path} not found!");
            }
            
            return File.ReadAllText(path);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Access denied!");
            throw;
        }
    }
}
```

---

## Best Practices

### ✅ DO
- Use specific exception types
- Provide meaningful error messages
- Use `finally` for cleanup
- Validate input before processing
- Log exceptions for debugging

### ❌ DON'T
- Catch all exceptions with `catch (Exception)` unless necessary
- Swallow exceptions (empty catch blocks)
- Use exceptions for normal control flow
- Throw exceptions in constructors for invalid state

---

## Practice Exercises

### Exercise 1: Safe Division
Write a program that safely divides two numbers with proper exception handling.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.Write("Enter first number: ");
            double num1 = double.Parse(Console.ReadLine());
            
            Console.Write("Enter second number: ");
            double num2 = double.Parse(Console.ReadLine());
            
            if (num2 == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero!");
            }
            
            double result = num1 / num2;
            Console.WriteLine($"Result: {result}");
        }
        catch (FormatException)
        {
            Console.WriteLine("Please enter valid numbers!");
        }
        catch (DivideByZeroException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.ReadLine();
    }
}
```
</details>

### Exercise 2: Age Validator
Create a program that validates age input with custom exceptions.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class InvalidAgeException : Exception
{
    public InvalidAgeException(string message) : base(message)
    {
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.Write("Enter your age: ");
            string input = Console.ReadLine();
            
            if (!int.TryParse(input, out int age))
            {
                throw new FormatException("Age must be a number!");
            }
            
            if (age < 0)
            {
                throw new InvalidAgeException("Age cannot be negative!");
            }
            
            if (age > 150)
            {
                throw new InvalidAgeException("Age is unrealistic!");
            }
            
            Console.WriteLine($"Valid age: {age}");
        }
        catch (InvalidAgeException ex)
        {
            Console.WriteLine($"Validation Error: {ex.Message}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Format Error: {ex.Message}");
        }
        
        Console.ReadLine();
    }
}
```
</details>

### Exercise 3: Array Access Handler
Write a program that safely accesses array elements with exception handling.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int[] numbers = { 10, 20, 30, 40, 50 };
        
        Console.WriteLine("Array: " + string.Join(", ", numbers));
        
        while (true)
        {
            try
            {
                Console.Write("\nEnter index (or -1 to exit): ");
                string input = Console.ReadLine();
                
                if (input == "-1")
                {
                    break;
                }
                
                int index = int.Parse(input);
                int value = numbers[index];
                
                Console.WriteLine($"Value at index {index}: {value}");
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Error: Index out of range!");
            }
            catch (FormatException)
            {
                Console.WriteLine("Error: Please enter a valid number!");
            }
        }
        
        Console.WriteLine("Goodbye!");
        Console.ReadLine();
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Empty catch block
```csharp
try
{
    int result = 10 / 0;
}
catch (DivideByZeroException)
{
    // Error! Silently ignores the exception
}
```

✅ **Correct:**
```csharp
try
{
    int result = 10 / 0;
}
catch (DivideByZeroException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    // Log the error or handle it appropriately
}
```

❌ **Wrong:** Catching general exception first
```csharp
try
{
    // Some code
}
catch (Exception ex)  // Too general!
{
    Console.WriteLine("Error");
}
catch (DivideByZeroException ex)  // Never reached!
{
    Console.WriteLine("Division by zero");
}
```

✅ **Correct:**
```csharp
try
{
    // Some code
}
catch (DivideByZeroException ex)  // Specific first
{
    Console.WriteLine("Division by zero");
}
catch (Exception ex)  // General last
{
    Console.WriteLine("Error");
}
```

---

## Key Takeaways

### 📌 Try-Catch
```csharp
try
{
    // Code that might throw
}
catch (ExceptionType ex)
{
    // Handle exception
}
```

### 📌 Finally
```csharp
finally
{
    // Always executes
}
```

### 📌 Throw
```csharp
throw new ExceptionType("message");
```

### 📌 Custom Exception
```csharp
class MyException : Exception
{
    // Custom exception class
}
```

---

## Next Steps

Excellent! You now understand exception handling.

**Next up:** [File I/O](./09_File_IO.md) 📁

---

**💡 Tip:** Always provide meaningful error messages! A good error message should explain what went wrong and, if possible, how to fix it!

---

**⚠️ Remember:** Don't use exceptions for normal program flow! Exceptions are for exceptional circumstances, not for expected conditions that you can check with if statements!
