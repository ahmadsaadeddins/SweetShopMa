# MVVM Basics 02: Understanding the Model

## 🎯 What You'll Learn

By the end of this lesson, you will:
- Understand what the Model is
- Learn how to create Model classes
- Know what belongs in a Model
- See real-world Model examples

---

## 📖 What is the Model?

The **Model** represents your **data** and **business logic**. It's the "what" of your application - the actual information you're working with.

### Think of the Model as:
- 📦 **A container** for your data
- 📋 **A blueprint** for information
- 🔐 **The rules** for your data

### What the Model Does:
✅ Holds data (properties)
✅ Validates data (rules)
✅ Contains business logic
❌ **NOT** UI code
❌ **NOT** user interaction

---

## 🏗️ Model Structure

A Model is a simple C# class with **properties** that describe your data.

### Basic Model Example

```csharp
// A simple Person model
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
```

### What This Does:
- `Id`: Unique identifier for the person
- `FirstName`, `LastName`: Person's name
- `Age`: How old they are
- `Email`: Contact information

---

## 📝 Real-World Model Examples

### Example 1: Product Model (E-commerce)

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}
```

**Use Case**: Shopping app showing products for sale

---

### Example 2: Todo Item Model

```csharp
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime DueDate { get; set; }
    public Priority Priority { get; set; }
}

public enum Priority
{
    Low,
    Medium,
    High
}
```

**Use Case**: Task management app

---

### Example 3: User Model (Authentication)

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // Never store plain passwords!
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; }
}
```

**Use Case**: Login and user management

---

### Example 4: Order Model (Shopping)

```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public string ShippingAddress { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

**Use Case**: Online shopping system

---

## 🔐 Adding Validation to Models

Models can also contain **validation logic** to ensure data is correct.

### Simple Validation Example

```csharp
public class Student
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty");
            if (value.Length < 2)
                throw new ArgumentException("Name must be at least 2 characters");
            _name = value;
        }
    }
    
    private int _age;
    public int Age
    {
        get => _age;
        set
        {
            if (value < 0 || value > 120)
                throw new ArgumentException("Age must be between 0 and 120");
            _age = value;
        }
    }
    
    public string Email { get; set; }
    
    // Method to validate email format
    public bool IsValidEmail()
    {
        return Email.Contains("@") && Email.Contains(".");
    }
}
```

---

## 🎯 Best Practices for Models

### ✅ DO:
- Keep models **simple** and **focused**
- Use **meaningful** property names
- Include **validation** when needed
- Make properties **public** with { get; set; }
- Use **appropriate data types** (int, string, decimal, etc.)

### ❌ DON'T:
- Put UI code in models (no Colors, Fonts, etc.)
- Put database code in models (no SQL queries)
- Put user interaction in models (no button clicks)
- Make models too complex
- Mix concerns (keep data separate from logic)

---

## 🔄 Model Relationships

Models can reference other models:

```csharp
public class Author
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Book> Books { get; set; }  // One author has many books
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public Author Author { get; set; }  // Each book has one author
    public DateTime PublishDate { get; set; }
    public decimal Price { get; set; }
}
```

---

## 📊 Model vs. ViewModel

**Important**: Don't confuse Model with ViewModel!

| Feature | Model | ViewModel |
|---------|-------|-----------|
| **Purpose** | Holds data | Connects Model to View |
| **UI Code** | Never | Sometimes (for display) |
| **Commands** | Never | Yes (handles actions) |
| **Notifications** | No | Yes (INotifyPropertyChanged) |
| **Example** | `User`, `Product` | `UserViewModel`, `ProductViewModel` |

---

## 🛠️ Creating Your First Model

### Step 1: Create a Models Folder

```
MyApp/
├── Models/
│   └── TodoItem.cs
├── ViewModels/
├── Views/
```

### Step 2: Create the Model Class

```csharp
// Models/TodoItem.cs
namespace MyApp.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
```

### Step 3: Use the Model

```csharp
// In your ViewModel
var todo = new TodoItem
{
    Id = 1,
    Title = "Learn MVVM",
    IsCompleted = false
};
```

---

## ✅ Quick Check

**Question**: What is the main purpose of a Model?

<details>
<summary>Answer</summary>

To hold **data** and **business logic**. Models represent the information your app works with.

</details>

---

**Question**: Should a Model have UI code like Colors or Fonts?

<details>
<summary>Answer</summary>

**No!** Models should never contain UI code. That belongs in the View.

</details>

---

**Question**: Can a Model reference another Model?

<details>
<summary>Answer</summary>

**Yes!** Models can reference other models to show relationships (like an Order having OrderItems).

</details>

---

## 🎓 Key Takeaways

1. **Models = Data**: They hold your application's information
2. **Keep It Simple**: Models should be straightforward classes
3. **No UI Code**: Never put user interface code in models
4. **Validation is OK**: Models can validate their own data
5. **Use Good Names**: Property names should be clear and descriptive

---

## 📚 Next Steps

Now that you understand Models, let's look at Views:

- **Next**: [MVVM Basics 03: Understanding the View](MVVM_Basics_03_View.md)
- **Then**: [MVVM Basics 04: Understanding the ViewModel](MVVM_Basics_04_ViewModel.md)

---

## 💡 Practice Exercise

**Try This**: Create a Model for a simple contact book app

**Requirements**:
- Contact's name
- Phone number
- Email address
- Whether they're a favorite contact

<details>
<summary>See Solution</summary>

```csharp
public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
```

</details>

---

**Remember**: Models are the foundation of your app. Get them right, and everything else becomes easier!
