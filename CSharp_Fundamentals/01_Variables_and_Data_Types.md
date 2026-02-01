# C# Fundamentals - Variables and Data Types

## Understanding Variables 📦

A **variable** is a named storage location in memory that holds a value. Think of it as a labeled box where you can store information.

---

## Declaring Variables

### Syntax
```csharp
dataType variableName = value;
```

### Example
```csharp
int age = 25;           // Store an integer
string name = "John";   // Store text
double price = 19.99;   // Store a decimal number
bool isStudent = true;  // Store true/false
```

---

## Built-in Data Types

### Integer Types (Whole Numbers)

| Type | Size | Range | Example |
|------|------|-------|---------|
| `byte` | 1 byte | 0 to 255 | `byte age = 25;` |
| `short` | 2 bytes | -32,768 to 32,767 | `short count = 1000;` |
| `int` | 4 bytes | -2 billion to 2 billion | `int population = 1000000;` |
| `long` | 8 bytes | Huge numbers | `long stars = 7000000000L;` |

### Floating-Point Types (Decimal Numbers)

| Type | Size | Precision | Example |
|------|------|-----------|---------|
| `float` | 4 bytes | 6-7 digits | `float pi = 3.14f;` |
| `double` | 8 bytes | 15-16 digits | `double pi = 3.14159265359;` |
| `decimal` | 16 bytes | 28-29 digits | `decimal money = 99.99m;` |

**💡 Use `decimal` for financial calculations!**

### Text Types

| Type | Description | Example |
|------|-------------|---------|
| `char` | Single character | `char grade = 'A';` |
| `string` | Sequence of characters | `string name = "Hello";` |

### Other Types

| Type | Description | Example |
|------|-------------|---------|
| `bool` | True or false | `bool isActive = true;` |

---

## Variable Naming Rules

### ✅ Valid Names
```csharp
int age;
int _age;
int age2;
int Age;
int myAge;
```

### ❌ Invalid Names
```csharp
int 2age;        // Can't start with number
int my-age;      // Can't use hyphen
int class;       // Can't use reserved keyword
int my age;      // Can't contain spaces
```

### 📋 Naming Conventions (Best Practices)

```csharp
// camelCase for local variables
int studentAge;
string firstName;
double totalPrice;

// PascalCase for constants
const int MaxAge = 100;
const double Pi = 3.14159;
```

---

## Type Inference (var)

You can let C# figure out the type:

```csharp
// Explicit type
int age = 25;

// Type inference (var)
var age = 25;           // Compiler knows it's int
var name = "John";      // Compiler knows it's string
var price = 19.99;      // Compiler knows it's double
```

**⚠️ Rule:** `var` must be initialized with a value!

```csharp
var age;  // ❌ Error! Can't use var without initialization
```

---

## Constants

Constants are variables that cannot be changed after initialization:

```csharp
const int DaysInWeek = 7;
const double Pi = 3.14159;
const string CompanyName = "My Company";

// ❌ This will cause an error
// DaysInWeek = 8;  // Can't change a constant!
```

---

## Default Values

Every variable has a default value:

| Type | Default Value |
|------|---------------|
| `int`, `long`, `short`, `byte` | `0` |
| `float`, `double`, `decimal` | `0.0` |
| `bool` | `false` |
| `char` | `'\0'` (null character) |
| `string` | `null` |

---

## Practical Examples

### Example 1: Shopping Cart
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string productName = "Laptop";
        int quantity = 2;
        double price = 999.99;
        bool inStock = true;
        
        double total = quantity * price;
        
        Console.WriteLine($"Product: {productName}");
        Console.WriteLine($"Quantity: {quantity}");
        Console.WriteLine($"Price: ${price}");
        Console.WriteLine($"In Stock: {inStock}");
        Console.WriteLine($"Total: ${total}");
        Console.ReadLine();
    }
}
```

**Output:**
```
Product: Laptop
Quantity: 2
Price: $999.99
In Stock: True
Total: $1999.98
```

### Example 2: User Profile
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string firstName = "John";
        string lastName = "Doe";
        int age = 30;
        string city = "New York";
        bool isPremiumMember = true;
        
        string fullName = firstName + " " + lastName;
        
        Console.WriteLine($"Name: {fullName}");
        Console.WriteLine($"Age: {age}");
        Console.WriteLine($"City: {city}");
        Console.WriteLine($"Premium Member: {isPremiumMember}");
        Console.ReadLine();
    }
}
```

---

## String Operations

### Concatenation
```csharp
string firstName = "John";
string lastName = "Doe";

// Method 1: + operator
string fullName1 = firstName + " " + lastName;

// Method 2: String interpolation (Recommended)
string fullName2 = $"{firstName} {lastName}";

// Method 3: String.Format
string fullName3 = string.Format("{0} {1}", firstName, lastName);
```

### String Properties
```csharp
string text = "Hello World";

int length = text.Length;           // 11
bool isEmpty = string.IsNullOrEmpty(text);  // false
string upper = text.ToUpper();      // "HELLO WORLD"
string lower = text.ToLower();      // "hello world"
```

---

## Type Conversion

### Implicit Conversion (Automatic)
```csharp
int intValue = 100;
long longValue = intValue;  // Automatically converts

double doubleValue = 123.45;
int intValue2 = (int)doubleValue;  // 123 (loses decimal part)
```

### Explicit Conversion (Casting)
```csharp
double pi = 3.14;
int intPi = (int)pi;  // 3 (truncates decimal)

// ⚠️ Be careful with data loss!
double bigNumber = 999.99;
int smallNumber = (int)bigNumber;  // 999 (lost .99)
```

### Safe Conversion Methods
```csharp
string numberString = "123";

// Parse (throws exception if invalid)
int number1 = int.Parse(numberString);

// TryParse (returns false if invalid)
bool success = int.TryParse(numberString, out int number2);
if (success)
{
    Console.WriteLine($"Converted: {number2}");
}

// Convert class
int number3 = Convert.ToInt32(numberString);
```

---

## Nullable Types

By default, value types cannot be `null`. Use nullable types when you need to represent "no value":

```csharp
// Regular int cannot be null
int age = 25;
// age = null;  // ❌ Error!

// Nullable int (can be null)
int? age2 = 25;
age2 = null;  // ✅ OK!

// Check if has value
if (age2.HasValue)
{
    Console.WriteLine($"Age: {age2.Value}");
}

// Use null-coalescing operator
int age3 = age2 ?? 0;  // Use 0 if age2 is null
```

---

## Practice Exercises

### Exercise 1: Temperature Converter
Write a program that:
1. Stores a temperature in Celsius
2. Converts it to Fahrenheit
3. Displays both values

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        double celsius = 25.5;
        double fahrenheit = (celsius * 9 / 5) + 32;
        
        Console.WriteLine($"Celsius: {celsius}°C");
        Console.WriteLine($"Fahrenheit: {fahrenheit}°F");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 2: Rectangle Calculator
Write a program that:
1. Stores length and width of a rectangle
2. Calculates area and perimeter
3. Displays results

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        double length = 10.5;
        double width = 5.2;
        
        double area = length * width;
        double perimeter = 2 * (length + width);
        
        Console.WriteLine($"Length: {length}");
        Console.WriteLine($"Width: {width}");
        Console.WriteLine($"Area: {area}");
        Console.WriteLine($"Perimeter: {perimeter}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 3: Student Grade
Write a program that:
1. Stores student name and three grades
2. Calculates the average
3. Displays the result

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string studentName = "Alice";
        double grade1 = 85.5;
        double grade2 = 92.0;
        double grade3 = 78.5;
        
        double average = (grade1 + grade2 + grade3) / 3;
        
        Console.WriteLine($"Student: {studentName}");
        Console.WriteLine($"Grade 1: {grade1}");
        Console.WriteLine($"Grade 2: {grade2}");
        Console.WriteLine($"Grade 3: {grade3}");
        Console.WriteLine($"Average: {average:F2}");  // F2 = 2 decimal places
        Console.ReadLine();
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Using uninitialized variable
```csharp
int age;
Console.WriteLine(age);  // Error! Variable not initialized
```

✅ **Correct:**
```csharp
int age = 25;
Console.WriteLine(age);  // OK!
```

❌ **Wrong:** Wrong type for value
```csharp
int age = "twenty-five";  // Error! Can't assign string to int
```

✅ **Correct:**
```csharp
int age = 25;
string ageText = "twenty-five";
```

❌ **Wrong:** Integer division
```csharp
int result = 5 / 2;  // Result is 2, not 2.5!
```

✅ **Correct:**
```csharp
double result = 5.0 / 2;  // Result is 2.5
```

---

## Key Takeaways

### 📌 Variable Declaration
```csharp
int age = 25;
string name = "John";
double price = 19.99;
bool isActive = true;
```

### 📌 Type Inference
```csharp
var age = 25;  // Compiler infers int
```

### 📌 Constants
```csharp
const int MaxAge = 100;
```

### 📌 String Interpolation
```csharp
Console.WriteLine($"Name: {name}, Age: {age}");
```

---

## Next Steps

Great work! You now understand variables and data types.

**Next up:** [Operators](./02_Operators.md) ➕➖✖️➗

---

**💡 Tip:** Always choose the most appropriate data type for your data. Use `decimal` for money, `int` for counting, and `string` for text!

---

**⚠️ Remember:** C# is strongly typed. You can't assign a string to an int variable without proper conversion!
