# C# Fundamentals - Arrays and Collections

## What are Arrays and Collections? 📊

Arrays and collections are used to store multiple values in a single variable. Instead of creating separate variables for each item, you can store them all together.

---

## Arrays

An **array** is a fixed-size collection of elements of the same type.

### Declaring Arrays

```csharp
// Method 1: Declare and initialize
int[] numbers = { 1, 2, 3, 4, 5 };

// Method 2: Declare with size
int[] numbers2 = new int[5];

// Method 3: Declare and initialize with new
int[] numbers3 = new int[] { 1, 2, 3, 4, 5 };
```

### Accessing Array Elements

Arrays are **zero-indexed**, meaning the first element is at index 0.

```csharp
string[] fruits = { "Apple", "Banana", "Orange" };

Console.WriteLine(fruits[0]);  // Apple
Console.WriteLine(fruits[1]);  // Banana
Console.WriteLine(fruits[2]);  // Orange
```

### Modifying Array Elements

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };

numbers[0] = 10;  // Change first element
numbers[4] = 50;  // Change last element

Console.WriteLine(numbers[0]);  // 10
Console.WriteLine(numbers[4]);  // 50
```

### Array Length

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };
Console.WriteLine($"Array length: {numbers.Length}");  // 5
```

### Iterating Through Arrays

```csharp
string[] fruits = { "Apple", "Banana", "Orange" };

// Using for loop
for (int i = 0; i < fruits.Length; i++)
{
    Console.WriteLine(fruits[i]);
}

// Using foreach loop
foreach (string fruit in fruits)
{
    Console.WriteLine(fruit);
}
```

### Practical Example: Student Grades

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        string[] studentNames = { "Alice", "Bob", "Charlie" };
        int[] grades = { 85, 92, 78 };
        
        Console.WriteLine("=== Student Grades ===\n");
        
        for (int i = 0; i < studentNames.Length; i++)
        {
            Console.WriteLine($"{studentNames[i]}: {grades[i]}");
        }
        
        // Calculate average
        int sum = 0;
        foreach (int grade in grades)
        {
            sum += grade;
        }
        double average = (double)sum / grades.Length;
        
        Console.WriteLine($"\nClass Average: {average:F2}");
        Console.ReadLine();
    }
}
```

---

## Multidimensional Arrays

### Two-Dimensional Arrays

```csharp
// Declare and initialize
int[,] matrix = 
{
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 }
};

// Access elements
Console.WriteLine(matrix[0, 0]);  // 1
Console.WriteLine(matrix[1, 2]);  // 6
Console.WriteLine(matrix[2, 1]);  // 8

// Get dimensions
Console.WriteLine($"Rows: {matrix.GetLength(0)}");  // 3
Console.WriteLine($"Columns: {matrix.GetLength(1)}");  // 3
```

### Iterating Through 2D Arrays

```csharp
int[,] matrix = 
{
    { 1, 2, 3 },
    { 4, 5, 6 }
};

for (int i = 0; i < matrix.GetLength(0); i++)
{
    for (int j = 0; j < matrix.GetLength(1); j++)
    {
        Console.Write(matrix[i, j] + " ");
    }
    Console.WriteLine();
}
```

### Practical Example: Multiplication Table

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int size = 5;
        int[,] multiplicationTable = new int[size, size];
        
        // Fill the table
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                multiplicationTable[i, j] = (i + 1) * (j + 1);
            }
        }
        
        // Display the table
        Console.WriteLine("Multiplication Table:\n");
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Console.Write($"{multiplicationTable[i, j],3}");
            }
            Console.WriteLine();
        }
        
        Console.ReadLine();
    }
}
```

---

## Jagged Arrays

A jagged array is an array of arrays.

```csharp
// Declare jagged array
int[][] jagged = new int[3][];

// Initialize each array
jagged[0] = new int[] { 1, 2 };
jagged[1] = new int[] { 3, 4, 5 };
jagged[2] = new int[] { 6, 7, 8, 9 };

// Access elements
Console.WriteLine(jagged[0][0]);  // 1
Console.WriteLine(jagged[1][2]);  // 5
Console.WriteLine(jagged[2][3]);  // 9
```

---

## Lists

A `List<T>` is a dynamic array that can grow or shrink in size.

### Creating Lists

```csharp
using System.Collections.Generic;

// Create an empty list
List<int> numbers = new List<int>();

// Create and initialize
List<string> fruits = new List<string> { "Apple", "Banana", "Orange" };
```

### Adding Elements

```csharp
List<int> numbers = new List<int>();

numbers.Add(1);
numbers.Add(2);
numbers.Add(3);

// Add multiple elements
numbers.AddRange(new int[] { 4, 5, 6 });
```

### Accessing Elements

```csharp
List<string> fruits = new List<string> { "Apple", "Banana", "Orange" };

Console.WriteLine(fruits[0]);  // Apple
Console.WriteLine(fruits[1]);  // Banana
```

### Modifying Elements

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

numbers[0] = 10;  // Change first element
numbers[4] = 50;  // Change last element
```

### Removing Elements

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

numbers.Remove(3);        // Remove value 3
numbers.RemoveAt(0);      // Remove element at index 0
numbers.RemoveAll(x => x > 3);  // Remove all elements > 3
```

### List Properties and Methods

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

Console.WriteLine($"Count: {numbers.Count}");  // 5
Console.WriteLine($"Contains 3: {numbers.Contains(3)}");  // True
Console.WriteLine($"Index of 4: {numbers.IndexOf(4)}");  // 3

numbers.Clear();  // Remove all elements
Console.WriteLine($"Count after clear: {numbers.Count}");  // 0
```

### Practical Example: Todo List

```csharp
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        List<string> todoList = new List<string>();
        
        // Add tasks
        todoList.Add("Buy groceries");
        todoList.Add("Finish homework");
        todoList.Add("Call mom");
        todoList.Add("Exercise");
        
        Console.WriteLine("=== Todo List ===\n");
        
        // Display all tasks
        for (int i = 0; i < todoList.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {todoList[i]}");
        }
        
        // Mark first task as complete
        Console.WriteLine($"\n✓ Completed: {todoList[0]}");
        todoList.RemoveAt(0);
        
        Console.WriteLine("\n=== Updated Todo List ===\n");
        
        foreach (string task in todoList)
        {
            Console.WriteLine($"• {task}");
        }
        
        Console.ReadLine();
    }
}
```

---

## Dictionaries

A `Dictionary<TKey, TValue>` stores key-value pairs.

### Creating Dictionaries

```csharp
using System.Collections.Generic;

// Create empty dictionary
Dictionary<string, int> ages = new Dictionary<string, int>();

// Create and initialize
Dictionary<string, string> capitals = new Dictionary<string, string>
{
    { "USA", "Washington D.C." },
    { "UK", "London" },
    { "France", "Paris" }
};
```

### Adding Elements

```csharp
Dictionary<string, int> ages = new Dictionary<string, int>();

ages.Add("Alice", 25);
ages.Add("Bob", 30);
ages.Add("Charlie", 35);

// Alternative syntax
ages["David"] = 28;
```

### Accessing Elements

```csharp
Dictionary<string, int> ages = new Dictionary<string, int>
{
    { "Alice", 25 },
    { "Bob", 30 }
};

Console.WriteLine(ages["Alice"]);  // 25
Console.WriteLine(ages["Bob"]);    // 30
```

### Checking if Key Exists

```csharp
Dictionary<string, int> ages = new Dictionary<string, int>
{
    { "Alice", 25 },
    { "Bob", 30 }
};

if (ages.ContainsKey("Alice"))
{
    Console.WriteLine($"Alice's age: {ages["Alice"]}");
}

if (!ages.ContainsKey("Charlie"))
{
    Console.WriteLine("Charlie not found");
}
```

### Removing Elements

```csharp
Dictionary<string, int> ages = new Dictionary<string, int>
{
    { "Alice", 25 },
    { "Bob", 30 },
    { "Charlie", 35 }
};

ages.Remove("Bob");  // Remove Bob
```

### Iterating Through Dictionaries

```csharp
Dictionary<string, int> ages = new Dictionary<string, int>
{
    { "Alice", 25 },
    { "Bob", 30 },
    { "Charlie", 35 }
};

foreach (KeyValuePair<string, int> pair in ages)
{
    Console.WriteLine($"{pair.Key}: {pair.Value}");
}

// Alternative syntax
foreach (var pair in ages)
{
    Console.WriteLine($"{pair.Key}: {pair.Value}");
}
```

### Practical Example: Phone Book

```csharp
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Dictionary<string, string> phoneBook = new Dictionary<string, string>();
        
        // Add contacts
        phoneBook.Add("Alice", "555-1234");
        phoneBook.Add("Bob", "555-5678");
        phoneBook.Add("Charlie", "555-9012");
        
        Console.WriteLine("=== Phone Book ===\n");
        
        // Search for contact
        string searchName = "Bob";
        
        if (phoneBook.ContainsKey(searchName))
        {
            Console.WriteLine($"{searchName}: {phoneBook[searchName]}");
        }
        else
        {
            Console.WriteLine($"{searchName} not found");
        }
        
        // Display all contacts
        Console.WriteLine("\nAll Contacts:");
        foreach (var contact in phoneBook)
        {
            Console.WriteLine($"  {contact.Key}: {contact.Value}");
        }
        
        Console.ReadLine();
    }
}
```

---

## Array vs List vs Dictionary

| Feature | Array | List | Dictionary |
|---------|-------|------|------------|
| Size | Fixed | Dynamic | Dynamic |
| Access | Index | Index | Key |
| Performance | Fast | Fast | Fast lookup by key |
| Use Case | Known size | Dynamic data | Key-value pairs |

---

## Common Array and List Methods

### Array Methods

```csharp
int[] numbers = { 5, 2, 8, 1, 9, 3 };

// Sort
Array.Sort(numbers);  // { 1, 2, 3, 5, 8, 9 }

// Reverse
Array.Reverse(numbers);  // { 9, 8, 5, 3, 2, 1 }

// Find
int index = Array.IndexOf(numbers, 5);  // 2

// Clear
Array.Clear(numbers, 0, numbers.Length);  // All zeros
```

### List Methods

```csharp
List<int> numbers = new List<int> { 5, 2, 8, 1, 9, 3 };

// Sort
numbers.Sort();  // { 1, 2, 3, 5, 8, 9 }

// Reverse
numbers.Reverse();  // { 9, 8, 5, 3, 2, 1 }

// Find
int first = numbers.Find(x => x > 5);  // 9
List<int> largeNumbers = numbers.FindAll(x => x > 5);  // { 9, 8 }

// Exists
bool exists = numbers.Exists(x => x == 5);  // true
```

---

## Practice Exercises

### Exercise 1: Array Statistics
Write a program that finds the min, max, and average of an array.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        int[] numbers = { 5, 2, 8, 1, 9, 3, 7 };
        
        int min = numbers[0];
        int max = numbers[0];
        int sum = 0;
        
        foreach (int num in numbers)
        {
            if (num < min) min = num;
            if (num > max) max = num;
            sum += num;
        }
        
        double average = (double)sum / numbers.Length;
        
        Console.WriteLine($"Array: {string.Join(", ", numbers)}");
        Console.WriteLine($"Min: {min}");
        Console.WriteLine($"Max: {max}");
        Console.WriteLine($"Average: {average:F2}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 2: Remove Duplicates
Write a program that removes duplicates from an array.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        int[] numbers = { 1, 2, 2, 3, 4, 4, 5, 5, 5 };
        
        List<int> uniqueNumbers = new List<int>();
        
        foreach (int num in numbers)
        {
            if (!uniqueNumbers.Contains(num))
            {
                uniqueNumbers.Add(num);
            }
        }
        
        Console.WriteLine($"Original: {string.Join(", ", numbers)}");
        Console.WriteLine($"Unique: {string.Join(", ", uniqueNumbers)}");
        Console.ReadLine();
    }
}
```
</details>

### Exercise 3: Word Frequency Counter
Write a program that counts word frequency in a sentence.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        string text = "the quick brown fox jumps over the lazy dog the the";
        string[] words = text.Split(' ');
        
        Dictionary<string, int> wordCount = new Dictionary<string, int>();
        
        foreach (string word in words)
        {
            if (wordCount.ContainsKey(word))
            {
                wordCount[word]++;
            }
            else
            {
                wordCount[word] = 1;
            }
        }
        
        Console.WriteLine("Word Frequency:");
        foreach (var pair in wordCount)
        {
            Console.WriteLine($"  {pair.Key}: {pair.Value}");
        }
        
        Console.ReadLine();
    }
}
```
</details>

### Exercise 4: Shopping Cart
Write a program that manages a shopping cart using a Dictionary.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Dictionary<string, double> cart = new Dictionary<string, double>();
        
        // Add items
        cart["Laptop"] = 999.99;
        cart["Mouse"] = 25.50;
        cart["Keyboard"] = 75.00;
        cart["Monitor"] = 299.99;
        
        Console.WriteLine("=== Shopping Cart ===\n");
        
        double total = 0;
        
        foreach (var item in cart)
        {
            Console.WriteLine($"{item.Key}: ${item.Value:F2}");
            total += item.Value;
        }
        
        Console.WriteLine($"\nTotal: ${total:F2}");
        Console.ReadLine();
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Array index out of bounds
```csharp
int[] numbers = { 1, 2, 3 };
Console.WriteLine(numbers[5]);  // Error! Index out of range
```

✅ **Correct:**
```csharp
int[] numbers = { 1, 2, 3 };
if (numbers.Length > 5)
{
    Console.WriteLine(numbers[5]);
}
```

❌ **Wrong:** Wrong type in array
```csharp
int[] numbers = { 1, 2, "three" };  // Error! Can't mix types
```

✅ **Correct:**
```csharp
int[] numbers = { 1, 2, 3 };  // Same type
```

❌ **Wrong:** Accessing non-existent key
```csharp
Dictionary<string, int> ages = new Dictionary<string, int>();
Console.WriteLine(ages["Bob"]);  // Error! Key not found
```

✅ **Correct:**
```csharp
Dictionary<string, int> ages = new Dictionary<string, int>();
if (ages.ContainsKey("Bob"))
{
    Console.WriteLine(ages["Bob"]);
}
```

---

## Key Takeaways

### 📌 Arrays
```csharp
int[] numbers = { 1, 2, 3 };
Console.WriteLine(numbers[0]);
```

### 📌 Lists
```csharp
List<int> numbers = new List<int> { 1, 2, 3 };
numbers.Add(4);
```

### 📌 Dictionaries
```csharp
Dictionary<string, int> ages = new Dictionary<string, int>();
ages["Alice"] = 25;
```

### 📌 Iterating
```csharp
foreach (int num in numbers)
{
    Console.WriteLine(num);
}
```

---

## Next Steps

Great job! You now understand arrays and collections.

**Next up:** [Classes and Objects](./06_Classes_and_Objects.md) 🏗️

---

**💡 Tip:** Use Lists when you need a dynamic collection that can grow or shrink. Use Arrays when you know the size won't change. Use Dictionaries when you need fast lookup by key!

---

**⚠️ Remember:** Arrays are zero-indexed! The first element is at index 0, not index 1. This is a common source of bugs for beginners!
