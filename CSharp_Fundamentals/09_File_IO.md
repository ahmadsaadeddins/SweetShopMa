# C# Fundamentals - File I/O

## Working with Files 📁

File I/O (Input/Output) allows your programs to read from and write to files on your computer. C# provides powerful classes in the `System.IO` namespace for file operations.

---

## System.IO Namespace

Import the namespace to work with files:

```csharp
using System.IO;
```

### Key Classes

| Class | Purpose |
|-------|---------|
| `File` | Static methods for file operations |
| `Directory` | Static methods for directory operations |
| `FileInfo` | Instance methods for file operations |
| `DirectoryInfo` | Instance methods for directory operations |
| `StreamReader` | Read text from files |
| `StreamWriter` | Write text to files |
| `FileStream` | Read/write binary data |

---

## Checking Files and Directories

### File.Exists
```csharp
string path = "data.txt";

if (File.Exists(path))
{
    Console.WriteLine("File exists!");
}
else
{
    Console.WriteLine("File does not exist!");
}
```

### Directory.Exists
```csharp
string folderPath = @"C:\MyFiles";

if (Directory.Exists(folderPath))
{
    Console.WriteLine("Directory exists!");
}
else
{
    Console.WriteLine("Directory does not exist!");
}
```

---

## Creating and Deleting Files

### Create a File
```csharp
string path = "test.txt";

// Create and write text
File.WriteAllText(path, "Hello, World!");
Console.WriteLine("File created!");
```

### Delete a File
```csharp
string path = "test.txt";

if (File.Exists(path))
{
    File.Delete(path);
    Console.WriteLine("File deleted!");
}
```

### Create a Directory
```csharp
string folderPath = @"C:\MyFiles\NewFolder";

if (!Directory.Exists(folderPath))
{
    Directory.CreateDirectory(folderPath);
    Console.WriteLine("Directory created!");
}
```

---

## Reading Text Files

### ReadAllText (Small Files)
```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string path = "data.txt";
        
        // Write some data first
        File.WriteAllText(path, "Line 1\nLine 2\nLine 3");
        
        // Read entire file
        string content = File.ReadAllText(path);
        Console.WriteLine("File content:");
        Console.WriteLine(content);
    }
}
```

### ReadAllLines (Array of Lines)
```csharp
string path = "data.txt";
string[] lines = File.ReadAllLines(path);

Console.WriteLine("File lines:");
foreach (string line in lines)
{
    Console.WriteLine(line);
}
```

### StreamReader (Large Files)
```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string path = "data.txt";
        
        // Create file with multiple lines
        File.WriteAllLines(path, new string[] { "Line 1", "Line 2", "Line 3" });
        
        // Read using StreamReader
        using (StreamReader reader = new StreamReader(path))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
```

---

## Writing Text Files

### WriteAllText (Overwrites)
```csharp
string path = "output.txt";
string content = "This is some text.";

File.WriteAllText(path, content);
Console.WriteLine("Text written to file!");
```

### AppendAllText (Adds to End)
```csharp
string path = "log.txt";

File.AppendAllText(path, "Log entry 1\n");
File.AppendAllText(path, "Log entry 2\n");
File.AppendAllText(path, "Log entry 3\n");

Console.WriteLine("Log entries appended!");
```

### WriteAllLines (Multiple Lines)
```csharp
string path = "names.txt";
string[] names = { "Alice", "Bob", "Charlie" };

File.WriteAllLines(path, names);
Console.WriteLine("Names written to file!");
```

### StreamWriter (More Control)
```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string path = "output.txt";
        
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Line 1");
            writer.WriteLine("Line 2");
            writer.WriteLine("Line 3");
        }
        
        Console.WriteLine("Lines written to file!");
    }
}
```

---

## Working with Paths

### Path Class Methods
```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string fullPath = @"C:\MyFiles\data.txt";
        
        Console.WriteLine($"Full Path: {fullPath}");
        Console.WriteLine($"Directory: {Path.GetDirectoryName(fullPath)}");
        Console.WriteLine($"File Name: {Path.GetFileName(fullPath)}");
        Console.WriteLine($"Extension: {Path.GetExtension(fullPath)}");
        Console.WriteLine($"File Name w/o Ext: {Path.GetFileNameWithoutExtension(fullPath)}");
        
        // Combine paths
        string folder = @"C:\MyFiles";
        string file = "data.txt";
        string combined = Path.Combine(folder, file);
        Console.WriteLine($"Combined: {combined}");
    }
}
```

---

## File and Directory Information

### FileInfo Class
```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string path = "data.txt";
        
        // Create file
        File.WriteAllText(path, "Some content");
        
        FileInfo fileInfo = new FileInfo(path);
        
        Console.WriteLine($"File Name: {fileInfo.Name}");
        Console.WriteLine($"Full Path: {fileInfo.FullName}");
        Console.WriteLine($"Size: {fileInfo.Length} bytes");
        Console.WriteLine($"Created: {fileInfo.CreationTime}");
        Console.WriteLine($"Modified: {fileInfo.LastWriteTime}");
    }
}
```

### DirectoryInfo Class
```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string folderPath = @"C:\MyFiles";
        
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        
        Console.WriteLine($"Directory: {dirInfo.Name}");
        Console.WriteLine($"Full Path: {dirInfo.FullName}");
        Console.WriteLine($"Parent: {dirInfo.Parent?.Name}");
        Console.WriteLine($"Created: {dirInfo.CreationTime}");
        
        // List files
        Console.WriteLine("\nFiles:");
        foreach (FileInfo file in dirInfo.GetFiles())
        {
            Console.WriteLine($"  {file.Name} ({file.Length} bytes)");
        }
    }
}
```

---

## Practical Examples

### Example 1: Simple Notepad

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "notes.txt";
        
        Console.WriteLine("=== Simple Notepad ===\n");
        
        // Load existing content
        if (File.Exists(filePath))
        {
            Console.WriteLine("Current content:");
            Console.WriteLine(File.ReadAllText(filePath));
            Console.WriteLine();
        }
        
        // Get new content
        Console.WriteLine("Enter your notes (press Enter twice to finish):");
        string notes = "";
        string line;
        
        while ((line = Console.ReadLine()) != "")
        {
            notes += line + Environment.NewLine;
        }
        
        // Save to file
        File.WriteAllText(filePath, notes);
        Console.WriteLine("\nNotes saved!");
    }
}
```

### Example 2: Contact Manager

```csharp
using System;
using System.IO;
using System.Collections.Generic;

class Contact
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    
    public override string ToString()
    {
        return $"{Name} | {Phone} | {Email}";
    }
    
    public static Contact FromString(string line)
    {
        string[] parts = line.Split('|');
        return new Contact
        {
            Name = parts[0].Trim(),
            Phone = parts[1].Trim(),
            Email = parts[2].Trim()
        };
    }
}

class Program
{
    static void Main(string[] args)
    {
        string filePath = "contacts.txt";
        List<Contact> contacts = new List<Contact>();
        
        // Load contacts from file
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    contacts.Add(Contact.FromString(line));
                }
            }
        }
        
        // Display contacts
        Console.WriteLine("=== Contact Manager ===\n");
        
        if (contacts.Count > 0)
        {
            Console.WriteLine("Existing Contacts:");
            foreach (Contact contact in contacts)
            {
                Console.WriteLine($"  - {contact}");
            }
            Console.WriteLine();
        }
        
        // Add new contact
        Console.WriteLine("Add new contact:");
        Console.Write("Name: ");
        string name = Console.ReadLine();
        Console.Write("Phone: ");
        string phone = Console.ReadLine();
        Console.Write("Email: ");
        string email = Console.ReadLine();
        
        Contact newContact = new Contact { Name = name, Phone = phone, Email = email };
        contacts.Add(newContact);
        
        // Save all contacts
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (Contact contact in contacts)
            {
                writer.WriteLine(contact.ToString());
            }
        }
        
        Console.WriteLine("\nContact saved!");
    }
}
```

### Example 3: Log File Writer

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string logFilePath = "application.log";
        
        Console.WriteLine("=== Application Logger ===\n");
        
        while (true)
        {
            Console.WriteLine("1. Write Info Log");
            Console.WriteLine("2. Write Warning Log");
            Console.WriteLine("3. Write Error Log");
            Console.WriteLine("4. View Logs");
            Console.WriteLine("5. Exit");
            Console.Write("Choose: ");
            
            string choice = Console.ReadLine();
            
            if (choice == "5")
            {
                break;
            }
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = "";
            
            switch (choice)
            {
                case "1":
                    Console.Write("Enter message: ");
                    logMessage = $"[{timestamp}] [INFO] {Console.ReadLine()}";
                    break;
                case "2":
                    Console.Write("Enter message: ");
                    logMessage = $"[{timestamp}] [WARNING] {Console.ReadLine()}";
                    break;
                case "3":
                    Console.Write("Enter message: ");
                    logMessage = $"[{timestamp}] [ERROR] {Console.ReadLine()}";
                    break;
                case "4":
                    if (File.Exists(logFilePath))
                    {
                        Console.WriteLine("\n=== Log Contents ===");
                        Console.WriteLine(File.ReadAllText(logFilePath));
                    }
                    else
                    {
                        Console.WriteLine("No logs found.");
                    }
                    continue;
                default:
                    Console.WriteLine("Invalid choice!");
                    continue;
            }
            
            // Append to log file
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            Console.WriteLine("Log entry saved!\n");
        }
        
        Console.WriteLine("Goodbye!");
    }
}
```

### Example 4: File Copy Utility

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== File Copy Utility ===\n");
        
        Console.Write("Enter source file path: ");
        string sourcePath = Console.ReadLine();
        
        Console.Write("Enter destination path: ");
        string destPath = Console.ReadLine();
        
        try
        {
            // Check if source exists
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine("Error: Source file does not exist!");
                return;
            }
            
            // Check if destination directory exists
            string destDirectory = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
                Console.WriteLine("Created destination directory.");
            }
            
            // Copy file
            File.Copy(sourcePath, destPath, overwrite: true);
            
            // Get file info
            FileInfo sourceInfo = new FileInfo(sourcePath);
            FileInfo destInfo = new FileInfo(destPath);
            
            Console.WriteLine("\nCopy successful!");
            Console.WriteLine($"Source: {sourceInfo.Length} bytes");
            Console.WriteLine($"Destination: {destInfo.Length} bytes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

---

## Practice Exercises

### Exercise 1: Word Counter
Write a program that reads a file and counts the words.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "sample.txt";
        
        // Create sample file
        File.WriteAllText(filePath, "This is a sample text file with several words in it.");
        
        // Read content
        string content = File.ReadAllText(filePath);
        
        // Count words
        string[] words = content.Split(new char[] { ' ', '\t', '\n', '\r' }, 
                                       StringSplitOptions.RemoveEmptyEntries);
        
        Console.WriteLine($"File: {filePath}");
        Console.WriteLine($"Total words: {words.Length}");
        Console.WriteLine($"Total characters: {content.Length}");
    }
}
```
</details>

### Exercise 2: High Score Tracker
Create a program that saves and loads high scores to a file.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "highscores.txt";
        
        Console.WriteLine("=== High Score Tracker ===\n");
        
        // Display current high scores
        if (File.Exists(filePath))
        {
            Console.WriteLine("Current High Scores:");
            Console.WriteLine(File.ReadAllText(filePath));
        }
        else
        {
            Console.WriteLine("No high scores yet!");
        }
        
        // Add new score
        Console.Write("\nEnter player name: ");
        string name = Console.ReadLine();
        
        Console.Write("Enter score: ");
        if (int.TryParse(Console.ReadLine(), out int score))
        {
            string entry = $"{name}: {score}";
            File.AppendAllText(filePath, entry + Environment.NewLine);
            Console.WriteLine("High score saved!");
        }
        else
        {
            Console.WriteLine("Invalid score!");
        }
    }
}
```
</details>

### Exercise 3: CSV Reader
Write a program that reads a CSV file and displays the data.

<details>
<summary>🔍 Show Solution</summary>

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "data.csv";
        
        // Create sample CSV file
        string csvContent = "Name,Age,City\nAlice,25,New York\nBob,30,Los Angeles\nCharlie,35,Chicago";
        File.WriteAllText(filePath, csvContent);
        
        // Read CSV
        string[] lines = File.ReadAllLines(filePath);
        
        Console.WriteLine("=== CSV Data ===\n");
        
        foreach (string line in lines)
        {
            string[] values = line.Split(',');
            Console.WriteLine(string.Join(" | ", values));
        }
    }
}
```
</details>

---

## Common Mistakes

❌ **Wrong:** Not using `using` statement
```csharp
StreamReader reader = new StreamReader("file.txt");
string content = reader.ReadToEnd();
// Forgot to close! File stays locked
```

✅ **Correct:**
```csharp
using (StreamReader reader = new StreamReader("file.txt"))
{
    string content = reader.ReadToEnd();
}  // Automatically closes and disposes
```

❌ **Wrong:** Not checking if file exists
```csharp
string content = File.ReadAllText("nonexistent.txt");  // Crashes!
```

✅ **Correct:**
```csharp
if (File.Exists("nonexistent.txt"))
{
    string content = File.ReadAllText("nonexistent.txt");
}
else
{
    Console.WriteLine("File not found!");
}
```

❌ **Wrong:** Wrong path separator
```csharp
string path = "C:\MyFiles\data.txt";  // Error! Escape characters
```

✅ **Correct:**
```csharp
string path = @"C:\MyFiles\data.txt";  // Verbatim string
// OR
string path = "C:\\MyFiles\\data.txt";  // Escaped backslashes
```

---

## Key Takeaways

### 📌 Read File
```csharp
string content = File.ReadAllText("file.txt");
string[] lines = File.ReadAllLines("file.txt");
```

### 📌 Write File
```csharp
File.WriteAllText("file.txt", "content");
File.AppendAllText("file.txt", "more content");
```

### 📌 Using Statement
```csharp
using (StreamReader reader = new StreamReader("file.txt"))
{
    // Read file
}
```

### 📌 Check Existence
```csharp
if (File.Exists("file.txt")) { }
if (Directory.Exists("folder")) { }
```

---

## Next Steps

Great job! You now understand file I/O.

**Next up:** [Namespaces and Access Modifiers](./10_Namespaces_and_Modifiers.md) 📦

---

**💡 Tip:** Always use the `using` statement with StreamReader and StreamWriter! It ensures that files are properly closed and resources are released, even if an exception occurs!

---

**⚠️ Remember:** Use verbatim strings (`@"path"`) for file paths to avoid issues with escape characters. Backslashes in regular strings need to be escaped (`\\`), but verbatim strings treat backslashes literally!
