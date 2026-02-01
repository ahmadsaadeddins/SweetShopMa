# C# Fundamentals - Operators

## What are Operators? 🔧

Operators are symbols that tell the compiler to perform specific mathematical or logical manipulations. In C#, operators are used to manipulate variables and values.

---

## Arithmetic Operators

Used to perform mathematical calculations.

| Operator | Name | Example | Result |
|----------|------|---------|--------|
| `+` | Addition | `5 + 3` | `8` |
| `-` | Subtraction | `5 - 3` | `2` |
| `*` | Multiplication | `5 * 3` | `15` |
| `/` | Division | `6 / 3` | `2` |
| `%` | Modulus (Remainder) | `5 % 3` | `2` |

### Examples

```csharp
int a = 10;
int b = 3;

int sum = a + b;        // 13
int difference = a - b;  // 7
int product = a * b;     // 30
int quotient = a / b;    // 3 (integer division)
int remainder = a % b;   // 1
```

### ⚠️ Integer Division Warning

```csharp
int result1 = 5 / 2;     // 2 (not 2.5!)
double result2 = 5.0 / 2; // 2.5
double result3 = 5 / 2.0; // 2.5
double result4 = 5.0 / 2.0; // 2.5
```

### Practical Example: Shopping Calculator

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        double itemPrice = 19.99;
        int quantity = 3;
        double taxRate = 0.08;  // 8%
        
        double subtotal = itemPrice * quantity;
        double tax = subtotal * taxRate;
        double total = subtotal + tax;
        
        Console.WriteLine($"Item Price: ${itemPrice}");
        Console.WriteLine($"Quantity: {quantity}");
        Console.WriteLine($"Subtotal: ${subtotal:F2}");
        Console.WriteLine($"Tax: ${tax:F2}");
        Console.WriteLine($"Total: ${total:F2}");
        Console.ReadLine();
    }
}
```

---

## Increment and Decrement Operators

Used to increase or decrease a value by 1.

| Operator | Name | Description |
|----------|------|-------------|
| `++` | Increment | Increases value by 1 |
| `--` | Decrement | Decreases value by 1 |

### Prefix vs Postfix

```csharp
int x = 5;

// Prefix: Increment first, then use
int a = ++x;  // x becomes 6, a becomes 6

// Reset
x = 5;

// Postfix: Use first, then increment
int b = x++;  // b becomes 5, x becomes 6
```

### Example

```csharp
int count = 0;
count++;        // count is now 1
count++;        // count is now 2
count--;        // count is now 1

Console.WriteLine(count);  // Output: 1
```

---

## Assignment Operators

Used to assign values to variables.

| Operator | Example | Same As |
|----------|---------|---------|
| `=` | `x = 5` | `x = 5` |
| `+=` | `x += 3` | `x = x + 3` |
| `-=` | `x -= 3` | `x = x - 3` |
| `*=` | `x *= 3` | `x = x * 3` |
| `/=` | `x /= 3` | `x = x / 3` |
| `%=` | `x %= 3` | `x = x % 3` |

### Examples

```csharp
int score = 10;

score += 5;   // score = 15
score -= 3;   // score = 12
score *= 2;   // score = 24
score /= 4;   // score = 6
score %= 4;   // score = 2
```

### Practical Example: Score Tracker

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int score = 0;
        
        Console.WriteLine($"Initial score: {score}");
        
        score += 100;  // Bonus points
        Console.WriteLine($"After bonus: {score}");
        
        score -= 20;   // Penalty
        Console.WriteLine($"After penalty: {score}");
        
        score *= 2;    // Double score
        Console.WriteLine($"After doubling: {score}");
        
        Console.ReadLine();
    }
}
```

---

## Comparison Operators

Used to compare two values. Returns `true` or `false`.

| Operator | Name | Example | Result |
|----------|------|---------|--------|
| `==` | Equal to | `5 == 5` | `true` |
| `!=` | Not equal to | `5 != 3` | `true` |
| `>` | Greater than | `5 > 3` | `true` |
| `<` | Less than | `5 < 3` | `false` |
| `>=` | Greater than or equal | `5 >= 5` | `true` |
| `<=` | Less than or equal | `5 <= 3` | `false` |

### Examples

```csharp
int age = 18;
bool canVote = age >= 18;  // true

int price = 50;
int budget = 100;
bool canAfford = price <= budget;  // true

string password = "12345";
string input = "12345";
bool isCorrect = password == input;  // true
```

### Practical Example: Age Checker

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int age = 20;
        int votingAge = 18;
        int seniorAge = 65;
        
        bool canVote = age >= votingAge;
        bool isSenior = age >= seniorAge;
        bool isTeenager = age >= 13 && age <= 19;
        
        Console.WriteLine($"Age: {age}");
        Console.WriteLine($"Can vote: {canVote}");
        Console.WriteLine($"Is senior: {isSenior}");
        Console.WriteLine($"Is teenager: {isTeenager}");
        Console.ReadLine();
    }
}
```

---

## Logical Operators

Used to combine multiple conditions.

| Operator | Name | Description | Example |
|----------|------|-------------|---------|
| `&&` | Logical AND | True if both conditions are true | `age >= 18 && hasLicense` |
| `\|\|` | Logical OR | True if at least one condition is true | `isWeekend \|\| isHoliday` |
| `!` | Logical NOT | Reverses the result | `!isRaining` |

### Truth Tables

#### AND (&&)
| A | B | A && B |
|---|---|--------|
| true | true | **true** |
| true | false | false |
| false | true | false |
| false | false | false |

#### OR (||)
| A | B | A \|\| B |
|---|---|----------|
| true | true | **true** |
| true | false | **true** |
| false | true | **true** |
| false | false | false |

#### NOT (!)
| A | !A |
|---|-----|
| true | **false** |
| false | **true** |

### Examples

```csharp
int age = 25;
bool hasLicense = true;
bool hasInsurance = false;

bool canDrive = age >= 18 && hasLicense;  // true
bool canRentCar = age >= 25 && hasLicense && hasInsurance;  // false
bool canGetDiscount = age < 18 || age >= 65;  // false
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
        bool isActive = true;
        bool isAdmin = true;
        
        string inputUser = "admin";
        string inputPass = "12345";
        
        bool isValidUser = username == inputUser && password == inputPass;
        bool canLogin = isValidUser && isActive;
        bool hasFullAccess = canLogin && isAdmin;
        
        Console.WriteLine($"Valid user: {isValidUser}");
        Console.WriteLine($"Can login: {canLogin}");
        Console.WriteLine($"Full access: {hasFullAccess}");
        Console.ReadLine();
    }
}
```

---

## Operator Precedence

Operators are evaluated in a specific order:

| Precedence | Operator |
|------------|----------|
| Highest | `()`, `[]`, `.` |
| | `++`, `--`, `!` |
| | `*`, `/`, `%` |
| | `+`, `-` |
| | `<`, `<=`, `>`, `>=` |
| | `==`, `!=` |
| | `&&` |
| Lowest | `\|\|` |

### Example

```csharp
int result = 5 + 3 * 2;  // 11 (multiplication first)
int result2 = (5 + 3) * 2;  // 16 (parentheses first)

bool check = 5 > 3 && 2 < 4;  // true
bool check2 = 5 > (3 && 2) < 4;  // Error! Can't use && with numbers
```

### Practical Example: Complex Condition

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int age = 25;
        int income = 50000;
        bool hasJob = true;
        int creditScore = 750;
        
        // Complex condition: Good loan candidate
        bool isGoodCandidate = (age >= 21 && age <= 65) && 
                              (income >= 30000 || hasJob) && 
                              creditScore >= 700;
        
        Console.WriteLine($"Age: {age}");
        Console.WriteLine($"Income: ${income}");
        Console.WriteLine($"Has job: {hasJob}");
        Console.WriteLine($"Credit score: {creditScore}");
        Console.WriteLine($"Good loan candidate: {isGoodCandidate}");
        Console.ReadLine();
    }
}
```

---

## Ternary Operator

A shorthand for `if-else` statement.

### Syntax
```csharp
condition ? valueIfTrue : valueIfFalse;
```

### Examples

```csharp
int age = 20;
string status = age >= 18 ? "Adult" : "Minor";
Console.WriteLine(status);  // Output: Adult

int score = 85;
string grade = score >= 60 ? "Pass" : "Fail";
Console.WriteLine(grade);  // Output: Pass

int temperature = 25;
string weather = temperature > 20 ? "Hot" : "Cold";
Console.WriteLine(weather);  // Output: Hot
```

### Nested Ternary (Not Recommended for Beginners)

```csharp
int score = 75;
string grade = score >= 90 ? "A" : 
               score >= 80 ? "B" : 
               score >= 70 ? "C" : 
               score >= 60 ? "D" : "F";
Console.WriteLine(grade);  // Output: C
```

---

## Null-Coalescing Operator

Used with nullable types and reference types.

### `??` Operator
```csharp
string name = null;
string displayName = name ?? "Guest";
Console.WriteLine(displayName);  // Output: Guest

int? age = null;
int personAge = age ?? 0;  // Use 0 if age is null
Console.WriteLine(personAge);  // Output: 0
```

### `??=` Operator (C# 8.0+)
```csharp
string name = null;
name ??= "Guest";  // Assign "Guest" only if name is null
Console.WriteLine(name);  // Output: Guest
```

---

## Practice Exercises

### Exercise 1: Simple Calculator
Write a program that:
1. Takes two numbers
2. Performs all arithmetic operations
3. Displays results

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int a = 15;
        int b = 4;
        
        Console.WriteLine($"a = {a}, b = {b}");
        Console.WriteLine($"Addition: {a} + {b} = {a + b}");
        Console.WriteLine($"Subtraction: {a} - {b} = {a - b}");
        Console.WriteLine($"Multiplication: {a} * {b} = {a * b}");
        Console.WriteLine($"Division: {a} / {b} = {a / b}");
        Console.WriteLine($"Modulus: {a} % {b} = {a % b}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 2: Even or Odd
Write a program that checks if a number is even or odd.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int number = 17;
        bool isEven = number % 2 == 0;
        
        Console.WriteLine($"Number: {number}");
        Console.WriteLine($"Is even: {isEven}");
        Console.WriteLine($"Is odd: {!isEven}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 3: Grade Calculator
Write a program that:
1. Takes a score (0-100)
2. Determines the letter grade
3. Uses ternary operators

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int score = 85;
        
        string grade = score >= 90 ? "A" :
                       score >= 80 ? "B" :
                       score >= 70 ? "C" :
                       score >= 60 ? "D" : "F";
        
        Console.WriteLine($"Score: {score}");
        Console.WriteLine($"Grade: {grade}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 4: Leap Year Checker
Write a program that checks if a year is a leap year.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int year = 2024;
        
        // Leap year rules:
        // - Divisible by 4 AND not divisible by 100
        // - OR divisible by 400
        bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || 
                          (year % 400 == 0);
        
        Console.WriteLine($"Year: {year}");
        Console.WriteLine($"Is leap year: {isLeapYear}");
        Console.ReadLine();
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Assignment instead of comparison
```csharp
if (age = 18)  // Error! This assigns 18 to age
```

✅ **Correct:**
```csharp
if (age == 18)  // This compares age to 18
```

❌ **Wrong:** Integer division
```csharp
double result = 5 / 2;  // Result is 2.0, not 2.5!
```

✅ **Correct:**
```csharp
double result = 5.0 / 2;  // Result is 2.5
```

❌ **Wrong:** Operator precedence confusion
```csharp
bool check = true || false && false;  // true (&& evaluated first)
```

✅ **Correct:**
```csharp
bool check = (true || false) && false;  // false (parentheses first)
```

---

## Key Takeaways

### 📌 Arithmetic Operators
```csharp
+  -  *  /  %
```

### 📌 Comparison Operators
```csharp
==  !=  >  <  >=  <=
```

### 📌 Logical Operators
```csharp
&&  ||  !
```

### 📌 Ternary Operator
```csharp
condition ? valueIfTrue : valueIfFalse
```

### 📌 Assignment Operators
```csharp
=  +=  -=  *=  /=  %=
```

---

## Next Steps

Excellent! You now understand operators.

**Next up:** [Control Flow](./03_Control_Flow.md) 🔀

---

**💡 Tip:** Always use parentheses when you have complex conditions to make your code more readable and avoid precedence issues!

---

**⚠️ Remember:** `=` is for assignment, `==` is for comparison. Mixing them up is a common beginner mistake!
