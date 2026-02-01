# C# Fundamentals - Methods

## What are Methods? 🎯

A **method** (also called a function) is a reusable block of code that performs a specific task. Methods help you organize your code and avoid repetition.

---

## Why Use Methods?

### Without Methods (Repetitive Code)
```csharp
// Calculate area of rectangle 1
double length1 = 10.5;
double width1 = 5.2;
double area1 = length1 * width1;
Console.WriteLine($"Area 1: {area1}");

// Calculate area of rectangle 2
double length2 = 7.3;
double width2 = 3.8;
double area2 = length2 * width2;
Console.WriteLine($"Area 2: {area2}");

// Calculate area of rectangle 3
double length3 = 12.1;
double width3 = 6.4;
double area3 = length3 * width3;
Console.WriteLine($"Area 3: {area3}");
```

### With Methods (Clean Code)
```csharp
double area1 = CalculateArea(10.5, 5.2);
Console.WriteLine($"Area 1: {area1}");

double area2 = CalculateArea(7.3, 3.8);
Console.WriteLine($"Area 2: {area2}");

double area3 = CalculateArea(12.1, 6.4);
Console.WriteLine($"Area 3: {area3}");

// Method definition
double CalculateArea(double length, double width)
{
    return length * width;
}
```

---

## Method Syntax

### Basic Structure
```csharp
returnType MethodName(parameterType parameterName)
{
    // Method body
    return value;  // Optional
}
```

### Example
```csharp
int Add(int a, int b)
{
    int sum = a + b;
    return sum;
}
```

### Method Components

| Component | Description | Example |
|-----------|-------------|---------|
| Return Type | Type of value returned | `int`, `string`, `void` |
| Method Name | Name of the method | `Add`, `CalculateArea` |
| Parameters | Input values (optional) | `(int a, int b)` |
| Method Body | Code to execute | `{ return a + b; }` |
| Return Statement | Returns a value | `return sum;` |

---

## Return Types

### Void (No Return Value)
```csharp
void SayHello()
{
    Console.WriteLine("Hello, World!");
}
```

### Returning a Value
```csharp
int GetNumber()
{
    return 42;
}

string GetName()
{
    return "John";
}

double CalculateAverage(int a, int b, int c)
{
    return (a + b + c) / 3.0;
}
```

### Multiple Return Paths
```csharp
string GetGrade(int score)
{
    if (score >= 90)
    {
        return "A";
    }
    else if (score >= 80)
    {
        return "B";
    }
    else if (score >= 70)
    {
        return "C";
    }
    else
    {
        return "F";
    }
}
```

---

## Parameters

### Single Parameter
```csharp
void PrintNumber(int number)
{
    Console.WriteLine($"Number: {number}");
}

// Usage
PrintNumber(42);
```

### Multiple Parameters
```csharp
int Add(int a, int b)
{
    return a + b;
}

// Usage
int result = Add(5, 3);
```

### No Parameters
```csharp
void Greet()
{
    Console.WriteLine("Hello!");
}

// Usage
Greet();
```

---

## Parameter Types

### Value Parameters (Default)
```csharp
void Increment(int number)
{
    number++;  // Changes local copy only
}

int x = 5;
Increment(x);
Console.WriteLine(x);  // Output: 5 (unchanged)
```

### Reference Parameters (ref)
```csharp
void IncrementRef(ref int number)
{
    number++;  // Changes original variable
}

int x = 5;
IncrementRef(ref x);
Console.WriteLine(x);  // Output: 6 (changed!)
```

### Out Parameters
```csharp
bool TryParse(string text, out int number)
{
    return int.TryParse(text, out number);
}

int result;
if (TryParse("123", out result))
{
    Console.WriteLine($"Parsed: {result}");
}
```

### Optional Parameters
```csharp
void Greet(string name = "Guest")
{
    Console.WriteLine($"Hello, {name}!");
}

Greet();           // Output: Hello, Guest!
Greet("John");     // Output: Hello, John!
```

### Named Arguments
```csharp
void CreatePerson(string name, int age, string city)
{
    Console.WriteLine($"{name}, {age}, {city}");
}

CreatePerson("John", 25, "NYC");
CreatePerson(age: 30, name: "Jane", city: "LA");
```

---

## Method Overloading

You can have multiple methods with the same name but different parameters.

```csharp
int Add(int a, int b)
{
    return a + b;
}

double Add(double a, double b)
{
    return a + b;
}

int Add(int a, int b, int c)
{
    return a + b + c;
}

// Usage
int result1 = Add(5, 3);          // Uses Add(int, int)
double result2 = Add(5.5, 3.3);   // Uses Add(double, double)
int result3 = Add(1, 2, 3);       // Uses Add(int, int, int)
```

---

## Practical Examples

### Example 1: Calculator
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Simple Calculator ===");
        
        double num1 = 10;
        double num2 = 5;
        
        Console.WriteLine($"Addition: {num1} + {num2} = {Add(num1, num2)}");
        Console.WriteLine($"Subtraction: {num1} - {num2} = {Subtract(num1, num2)}");
        Console.WriteLine($"Multiplication: {num1} * {num2} = {Multiply(num1, num2)}");
        Console.WriteLine($"Division: {num1} / {num2} = {Divide(num1, num2)}");
        
        Console.ReadLine();
    }
    
    static double Add(double a, double b)
    {
        return a + b;
    }
    
    static double Subtract(double a, double b)
    {
        return a - b;
    }
    
    static double Multiply(double a, double b)
    {
        return a * b;
    }
    
    static double Divide(double a, double b)
    {
        if (b == 0)
        {
            Console.WriteLine("Error: Cannot divide by zero!");
            return 0;
        }
        return a / b;
    }
}
```

### Example 2: Temperature Converter
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        double celsius = 25;
        
        double fahrenheit = CelsiusToFahrenheit(celsius);
        Console.WriteLine($"{celsius}°C = {fahrenheit}°F");
        
        double kelvin = CelsiusToKelvin(celsius);
        Console.WriteLine($"{celsius}°C = {kelvin}K");
        
        Console.ReadLine();
    }
    
    static double CelsiusToFahrenheit(double celsius)
    {
        return (celsius * 9 / 5) + 32;
    }
    
    static double CelsiusToKelvin(double celsius)
    {
        return celsius + 273.15;
    }
}
```

### Example 3: Student Grade System
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string studentName = "John Doe";
        int[] grades = { 85, 92, 78, 90, 88 };
        
        double average = CalculateAverage(grades);
        string letterGrade = GetLetterGrade(average);
        string status = GetPassStatus(average);
        
        DisplayStudentReport(studentName, grades, average, letterGrade, status);
        
        Console.ReadLine();
    }
    
    static double CalculateAverage(int[] grades)
    {
        int sum = 0;
        foreach (int grade in grades)
        {
            sum += grade;
        }
        return (double)sum / grades.Length;
    }
    
    static string GetLetterGrade(double average)
    {
        if (average >= 90) return "A";
        if (average >= 80) return "B";
        if (average >= 70) return "C";
        if (average >= 60) return "D";
        return "F";
    }
    
    static string GetPassStatus(double average)
    {
        return average >= 60 ? "Passed" : "Failed";
    }
    
    static void DisplayStudentReport(string name, int[] grades, 
                                     double average, string letter, string status)
    {
        Console.WriteLine("=== Student Report ===");
        Console.WriteLine($"Name: {name}");
        Console.WriteLine("Grades:");
        foreach (int grade in grades)
        {
            Console.WriteLine($"  - {grade}");
        }
        Console.WriteLine($"Average: {average:F2}");
        Console.WriteLine($"Letter Grade: {letter}");
        Console.WriteLine($"Status: {status}");
    }
}
```

---

## Static vs Instance Methods

### Static Methods
Belong to the class, not to an instance.

```csharp
class MathHelper
{
    public static int Add(int a, int b)
    {
        return a + b;
    }
}

// Usage
int result = MathHelper.Add(5, 3);
```

### Instance Methods
Belong to an instance of the class.

```csharp
class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}

// Usage
Calculator calc = new Calculator();
int result = calc.Add(5, 3);
```

---

## Recursion

A method that calls itself.

### Example: Factorial
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int number = 5;
        long factorial = CalculateFactorial(number);
        Console.WriteLine($"Factorial of {number} is {factorial}");
        Console.ReadLine();
    }
    
    static long CalculateFactorial(int n)
    {
        if (n <= 1)
        {
            return 1;  // Base case
        }
        return n * CalculateFactorial(n - 1);  // Recursive call
    }
}
```

### Example: Fibonacci
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int n = 10;
        Console.WriteLine($"Fibonacci sequence ({n} numbers):");
        
        for (int i = 0; i < n; i++)
        {
            Console.Write(Fibonacci(i) + " ");
        }
        
        Console.ReadLine();
    }
    
    static int Fibonacci(int n)
    {
        if (n <= 1)
        {
            return n;
        }
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }
}
```

---

## Practice Exercises

### Exercise 1: Power Function
Write a method that calculates the power of a number.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        double number = 2;
        int power = 8;
        double result = Power(number, power);
        Console.WriteLine($"{number}^{power} = {result}");
        Console.ReadLine();
    }
    
    static double Power(double baseNumber, int exponent)
    {
        double result = 1;
        for (int i = 0; i < exponent; i++)
        {
            result *= baseNumber;
        }
        return result;
    }
}
```
</details>

### Exercise 2: Palindrome Checker
Write a method that checks if a string is a palindrome.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string word = "racecar";
        bool isPalindrome = IsPalindrome(word);
        Console.WriteLine($"Is '{word}' a palindrome? {isPalindrome}");
        Console.ReadLine();
    }
    
    static bool IsPalindrome(string text)
    {
        int left = 0;
        int right = text.Length - 1;
        
        while (left < right)
        {
            if (text[left] != text[right])
            {
                return false;
            }
            left++;
            right--;
        }
        return true;
    }
}
```
</details>

### Exercise 3: Array Statistics
Write methods to find min, max, and average of an array.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int[] numbers = { 5, 2, 8, 1, 9, 3, 7 };
        
        Console.WriteLine($"Array: {string.Join(", ", numbers)}");
        Console.WriteLine($"Min: {FindMin(numbers)}");
        Console.WriteLine($"Max: {FindMax(numbers)}");
        Console.WriteLine($"Average: {FindAverage(numbers):F2}");
        Console.ReadLine();
    }
    
    static int FindMin(int[] arr)
    {
        int min = arr[0];
        foreach (int num in arr)
        {
            if (num < min)
            {
                min = num;
            }
        }
        return min;
    }
    
    static int FindMax(int[] arr)
    {
        int max = arr[0];
        foreach (int num in arr)
        {
            if (num > max)
            {
                max = num;
            }
        }
        return max;
    }
    
    static double FindAverage(int[] arr)
    {
        int sum = 0;
        foreach (int num in arr)
        {
            sum += num;
        }
        return (double)sum / arr.Length;
    }
}
```
</details>

### Exercise 4: Prime Number Checker
Write a method that checks if a number is prime.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int number = 17;
        bool isPrime = IsPrime(number);
        Console.WriteLine($"Is {number} prime? {isPrime}");
        Console.ReadLine();
    }
    
    static bool IsPrime(int n)
    {
        if (n < 2)
        {
            return false;
        }
        
        for (int i = 2; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0)
            {
                return false;
            }
        }
        return true;
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Not returning a value
```csharp
int Add(int a, int b)
{
    int sum = a + b;
    // Forgot return statement!
}
```

✅ **Correct:**
```csharp
int Add(int a, int b)
{
    int sum = a + b;
    return sum;
}
```

❌ **Wrong:** Wrong return type
```csharp
int GetNumber()
{
    return "42";  // Error! Can't return string from int method
}
```

✅ **Correct:**
```csharp
int GetNumber()
{
    return 42;  // OK!
}
```

❌ **Wrong:** Missing parameter type
```csharp
void Print(number)  // Error! Missing type
{
    Console.WriteLine(number);
}
```

✅ **Correct:**
```csharp
void Print(int number)  // OK!
{
    Console.WriteLine(number);
}
```

---

## Key Takeaways

### 📌 Method Syntax
```csharp
returnType MethodName(parameters)
{
    // code
    return value;
}
```

### 📌 Void Method
```csharp
void SayHello()
{
    Console.WriteLine("Hello!");
}
```

### 📌 Method with Return Value
```csharp
int Add(int a, int b)
{
    return a + b;
}
```

### 📌 Method Overloading
```csharp
int Add(int a, int b) { }
double Add(double a, double b) { }
```

---

## Next Steps

Excellent! You now understand methods.

**Next up:** [Arrays and Collections](./05_Arrays_and_Collections.md) 📊

---

**💡 Tip:** Keep methods small and focused. Each method should do one thing well. If a method is getting too long, consider breaking it into smaller methods!

---

**⚠️ Remember:** Always match your return type with the actual value you're returning. You can't return a string from a method that's declared to return an int!
