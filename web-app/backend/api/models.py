from django.db import models
from django.contrib.auth.models import AbstractUser
from django.core.validators import MinValueValidator
from decimal import Decimal


class User(AbstractUser):
    """Custom User model with role-based permissions."""
    ROLE_CHOICES = [
        ('Developer', 'Developer'),
        ('Admin', 'Admin'),
        ('Moderator', 'Moderator'),
        ('User', 'User'),
    ]
    
    role = models.CharField(max_length=20, choices=ROLE_CHOICES, default='User')
    name = models.CharField(max_length=255)
    monthly_salary = models.DecimalField(max_digits=10, decimal_places=2, default=Decimal('0.00'))
    is_enabled = models.BooleanField(default=True)
    created_date = models.DateTimeField(auto_now_add=True)
    
    @property
    def is_developer(self):
        return self.role == 'Developer'
    
    @property
    def is_admin(self):
        return self.role == 'Admin'
    
    @property
    def is_moderator(self):
        return self.role == 'Moderator'
    
    @property
    def is_user(self):
        return self.role == 'User'
    
    @property
    def can_manage_users(self):
        return self.is_developer or self.is_admin
    
    @property
    def can_manage_stock(self):
        return self.is_developer or self.is_admin or self.is_moderator
    
    @property
    def can_use_attendance_tracker(self):
        return self.is_developer or self.is_admin or self.is_moderator
    
    @property
    def can_restock(self):
        return self.is_developer or self.is_admin or self.is_moderator
    
    def __str__(self):
        return f"{self.username} ({self.role})"


class Product(models.Model):
    """Product model for inventory management."""
    name = models.CharField(max_length=255)
    emoji = models.CharField(max_length=10, default='🍬')
    barcode = models.CharField(max_length=100, blank=True, default='')
    price = models.DecimalField(max_digits=10, decimal_places=2, validators=[MinValueValidator(Decimal('0.01'))])
    stock = models.DecimalField(max_digits=10, decimal_places=3, default=Decimal('0.000'), validators=[MinValueValidator(Decimal('0'))])
    is_sold_by_weight = models.BooleanField(default=False)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        ordering = ['name']
    
    @property
    def unit_label(self):
        return "KGS" if self.is_sold_by_weight else "PCS"
    
    def __str__(self):
        return f"{self.emoji} {self.name}"


class CartItem(models.Model):
    """Shopping cart item model."""
    user = models.ForeignKey(User, on_delete=models.CASCADE, related_name='cart_items')
    product = models.ForeignKey(Product, on_delete=models.CASCADE)
    quantity = models.DecimalField(max_digits=10, decimal_places=3, validators=[MinValueValidator(Decimal('0.001'))])
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        unique_together = ['user', 'product']
    
    @property
    def item_total(self):
        return self.product.price * self.quantity
    
    def __str__(self):
        return f"{self.user.username} - {self.product.name} x {self.quantity}"


class Order(models.Model):
    """Order model for completed transactions."""
    STATUS_CHOICES = [
        ('Completed', 'Completed'),
        ('Cancelled', 'Cancelled'),
    ]
    
    user = models.ForeignKey(User, on_delete=models.SET_NULL, null=True, related_name='orders')
    user_name = models.CharField(max_length=255)  # Denormalized for history
    order_date = models.DateTimeField(auto_now_add=True)
    total = models.DecimalField(max_digits=10, decimal_places=2)
    item_count = models.IntegerField(default=0)
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='Completed')
    
    class Meta:
        ordering = ['-order_date']
    
    def __str__(self):
        return f"Order #{self.id} - {self.user_name} - ${self.total}"


class OrderItem(models.Model):
    """Order item model for line items in an order."""
    order = models.ForeignKey(Order, on_delete=models.CASCADE, related_name='items')
    product = models.ForeignKey(Product, on_delete=models.SET_NULL, null=True)
    product_name = models.CharField(max_length=255)  # Denormalized
    product_emoji = models.CharField(max_length=10)  # Denormalized
    price = models.DecimalField(max_digits=10, decimal_places=2)  # Price at time of order
    quantity = models.DecimalField(max_digits=10, decimal_places=3)
    is_sold_by_weight = models.BooleanField(default=False)
    
    @property
    def item_total(self):
        return self.price * self.quantity
    
    @property
    def unit_label(self):
        return "KGS" if self.is_sold_by_weight else "PCS"
    
    def __str__(self):
        return f"{self.product_emoji} {self.product_name} x {self.quantity}"


class AttendanceRecord(models.Model):
    """Employee attendance record model."""
    STATUS_CHOICES = [
        ('Present', 'Present'),
        ('Absent', 'Absent'),
    ]
    
    user = models.ForeignKey(User, on_delete=models.CASCADE, related_name='attendance_records')
    user_name = models.CharField(max_length=255)  # Denormalized
    date = models.DateField()
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='Present')
    is_present = models.BooleanField(default=True)
    regular_hours = models.DecimalField(max_digits=5, decimal_places=2, default=Decimal('0.00'))
    overtime_hours = models.DecimalField(max_digits=5, decimal_places=2, default=Decimal('0.00'))
    daily_pay = models.DecimalField(max_digits=10, decimal_places=2, default=Decimal('0.00'))
    check_in_time = models.TimeField(null=True, blank=True)
    check_out_time = models.TimeField(null=True, blank=True)
    notes = models.TextField(blank=True, default='')
    created_at = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        unique_together = ['user', 'date']
        ordering = ['-date']
    
    @property
    def total_hours(self):
        return self.regular_hours + self.overtime_hours
    
    def __str__(self):
        return f"{self.user_name} - {self.date} - {self.status}"


class RestockRecord(models.Model):
    """Restock record model for inventory audit trail."""
    product = models.ForeignKey(Product, on_delete=models.CASCADE, related_name='restock_records')
    product_name = models.CharField(max_length=255)  # Denormalized
    product_emoji = models.CharField(max_length=10)  # Denormalized
    quantity_added = models.DecimalField(max_digits=10, decimal_places=3, validators=[MinValueValidator(Decimal('0.001'))])
    stock_before = models.DecimalField(max_digits=10, decimal_places=3)
    stock_after = models.DecimalField(max_digits=10, decimal_places=3)
    user = models.ForeignKey(User, on_delete=models.SET_NULL, null=True, related_name='restock_records')
    user_name = models.CharField(max_length=255)  # Denormalized
    restock_date = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        ordering = ['-restock_date']
    
    def __str__(self):
        return f"{self.product_name} - +{self.quantity_added} by {self.user_name}"

