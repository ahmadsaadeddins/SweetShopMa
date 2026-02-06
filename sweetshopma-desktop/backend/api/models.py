"""
Django Models for SweetShopMa Desktop Application.

Defines the database schema for products, sales, and related entities.
"""

from django.db import models
from django.contrib.auth.models import User
from django.core.validators import MinValueValidator
from decimal import Decimal

# Import role constants
from .constants import ROLE_SELLER, ROLE_CHOICES


class Category(models.Model):
    """Product category for organization"""
    name = models.CharField(max_length=100, unique=True)
    description = models.TextField(blank=True)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        db_table = 'api_category'
        verbose_name_plural = 'Categories'
        ordering = ['name']
    
    def __str__(self):
        return self.name


class Product(models.Model):
    """Product inventory model"""
    
    UNIT_CHOICES = [
        ('piece', 'Piece'),
        ('kg', 'Kilogram'),
        ('g', 'Gram'),
        ('lb', 'Pound'),
        ('oz', 'Ounce'),
        ('l', 'Liter'),
        ('ml', 'Milliliter'),
        ('box', 'Box'),
        ('pack', 'Pack'),
    ]
    
    name = models.CharField(max_length=200)
    description = models.TextField(blank=True)
    category = models.ForeignKey(
        Category,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='products'
    )
    
    # Pricing
    price = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    cost = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))],
        null=True,
        blank=True
    )
    
    # Inventory
    quantity = models.DecimalField(
        max_digits=10,
        decimal_places=3,
        default=Decimal('0.000'),
        validators=[MinValueValidator(Decimal('0.000'))]
    )
    unit = models.CharField(
        max_length=20,
        choices=UNIT_CHOICES,
        default='piece'
    )
    low_stock_threshold = models.IntegerField(
        default=10,
        validators=[MinValueValidator(0)],
        help_text="Alert when quantity falls below this level"
    )
    
    # Identification
    barcode = models.CharField(max_length=50, blank=True, unique=True, null=True)
    sku = models.CharField(max_length=50, blank=True, unique=True, null=True)
    
    # Status
    is_active = models.BooleanField(default=True)
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        db_table = 'api_product'
        ordering = ['name']
        indexes = [
            models.Index(fields=['barcode']),
            models.Index(fields=['sku']),
            models.Index(fields=['category']),
            models.Index(fields=['is_active']),
            # PERFORMANCE: Composite index for low stock queries (dashboard)
            models.Index(fields=['is_active', 'quantity']),
        ]
    
    def __str__(self):
        return self.name
    
    @property
    def profit_margin(self):
        """Calculate profit margin percentage"""
        if self.cost and self.cost > 0:
            return ((self.price - self.cost) / self.price) * 100
        return 0
    
    @property
    def is_low_stock(self):
        """Check if product is low on stock"""
        return self.quantity <= self.low_stock_threshold
    
    @property
    def stock_value(self):
        """Calculate total stock value"""
        return self.quantity * self.price


class Customer(models.Model):
    """Customer information for sales tracking"""
    name = models.CharField(max_length=200)
    phone = models.CharField(max_length=20, blank=True)
    email = models.EmailField(blank=True)
    address = models.TextField(blank=True)
    notes = models.TextField(blank=True)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        db_table = 'api_customer'
        ordering = ['name']
    
    def __str__(self):
        return self.name


class Sale(models.Model):
    """Sales transaction model"""
    
    PAYMENT_METHODS = [
        ('cash', 'Cash'),
        ('card', 'Card'),
        ('mobile', 'Mobile Payment'),
        ('credit', 'Credit/Account'),
    ]
    
    STATUS_CHOICES = [
        ('completed', 'Completed'),
        ('refunded', 'Refunded'),
        ('cancelled', 'Cancelled'),
    ]
    
    # Transaction details
    total = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    subtotal = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    tax = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00'),
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    discount = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00'),
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    
    # Payment
    payment_method = models.CharField(
        max_length=20,
        choices=PAYMENT_METHODS,
        default='cash'
    )
    
    # Customer (optional)
    customer = models.ForeignKey(
        Customer,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='sales'
    )
    
    # Staff
    staff = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='sales'
    )
    
    # Status
    status = models.CharField(
        max_length=20,
        choices=STATUS_CHOICES,
        default='completed'
    )
    
    # Notes
    notes = models.TextField(blank=True)
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    # Sync status
    synced = models.BooleanField(default=False)
    synced_at = models.DateTimeField(null=True, blank=True)
    
    class Meta:
        db_table = 'api_sale'
        ordering = ['-created_at']
        indexes = [
            models.Index(fields=['created_at']),
            models.Index(fields=['status']),
            models.Index(fields=['synced']),
            # PERFORMANCE: Composite indexes for dashboard queries (today/week/month stats)
            models.Index(fields=['created_at', 'status']),
            models.Index(fields=['customer']),
            models.Index(fields=['staff']),
        ]
    
    def __str__(self):
        return f"Sale #{self.id} - {self.total}"
    
    @property
    def item_count(self):
        """Get total number of items in sale"""
        return sum(item.quantity for item in self.items.all())
    
    @property
    def profit(self):
        """Calculate total profit from sale"""
        return sum(item.profit for item in self.items.all())


class SaleItem(models.Model):
    """Individual items in a sale"""
    sale = models.ForeignKey(
        Sale,
        on_delete=models.CASCADE,
        related_name='items'
    )
    product = models.ForeignKey(
        Product,
        on_delete=models.PROTECT,  # Prevent deleting products that are in sales
        related_name='sale_items'
    )
    
    # Quantity and pricing
    quantity = models.DecimalField(
        max_digits=10,
        decimal_places=3,
        validators=[MinValueValidator(Decimal('0.001'))]
    )
    unit_price = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    cost_price = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))],
        null=True,
        blank=True
    )
    
    # Calculated fields
    subtotal = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    discount = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00'),
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        db_table = 'api_saleitem'
        verbose_name = 'Sale Item'
        ordering = ['id']
    
    def __str__(self):
        return f"{self.product.name} x {self.quantity}"
    
    @property
    def total(self):
        """Calculate total for this item"""
        return self.subtotal - self.discount
    
    @property
    def profit(self):
        """Calculate profit for this item"""
        if self.cost_price:
            return (self.unit_price - self.cost_price) * self.quantity
        return Decimal('0.00')
    
    def save(self, *args, **kwargs):
        """Calculate subtotal before saving"""
        if not self.subtotal:
            self.subtotal = self.quantity * self.unit_price
        super().save(*args, **kwargs)


class Expense(models.Model):
    """Business expenses tracking"""
    
    CATEGORIES = [
        ('rent', 'Rent'),
        ('utilities', 'Utilities'),
        ('supplies', 'Supplies'),
        ('maintenance', 'Maintenance'),
        ('salary', 'Salary'),
        ('marketing', 'Marketing'),
        ('other', 'Other'),
    ]
    
    description = models.CharField(max_length=200)
    category = models.CharField(
        max_length=20,
        choices=CATEGORIES,
        default='other'
    )
    amount = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.00'))]
    )
    date = models.DateField()
    notes = models.TextField(blank=True)
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    # Sync status
    synced = models.BooleanField(default=False)
    synced_at = models.DateTimeField(null=True, blank=True)
    
    class Meta:
        db_table = 'api_expense'
        ordering = ['-date', '-created_at']
    
    def __str__(self):
        return f"{self.description} - {self.amount}"


class SyncMetadata(models.Model):
    """Track synchronization status with central server"""
    branch_id = models.CharField(max_length=50, unique=True)
    last_sync = models.DateTimeField()
    last_successful_sync = models.DateTimeField(null=True, blank=True)
    sync_count = models.IntegerField(default=0)
    last_error = models.TextField(blank=True)
    
    class Meta:
        db_table = 'sync_metadata'
        verbose_name = 'Sync Metadata'
    
    def __str__(self):
        return f"Sync metadata for {self.branch_id}"


class RestockRecord(models.Model):
    """Audit trail for inventory restocking operations"""
    
    product = models.ForeignKey(
        Product,
        on_delete=models.CASCADE,
        related_name='restock_records'
    )
    
    # Denormalized fields for historical accuracy
    product_name = models.CharField(max_length=200)
    product_emoji = models.CharField(max_length=10, blank=True)
    
    # Restock details
    quantity_added = models.DecimalField(
        max_digits=10,
        decimal_places=3,
        validators=[MinValueValidator(Decimal('0.01'))]
    )
    stock_before = models.DecimalField(
        max_digits=10,
        decimal_places=3
    )
    stock_after = models.DecimalField(
        max_digits=10,
        decimal_places=3
    )
    
    # User tracking
    user = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        related_name='restock_records'
    )
    user_name = models.CharField(max_length=100)
    
    # Timestamps
    restock_date = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        db_table = 'api_restock_record'
        ordering = ['-restock_date']
        indexes = [
            models.Index(fields=['product']),
            models.Index(fields=['restock_date']),
            models.Index(fields=['user']),
        ]
    
    def __str__(self):
        return f"Restock {self.product_name}: +{self.quantity_added}"


class UserProfile(models.Model):
    """
    Extended user profile with role-based access control.
    
    This model extends Django's built-in User model with:
    - Role-based permissions (Developer, Admin, Moderator, User)
    - Payroll-related fields (monthly_salary, overtime_multiplier)
    
    The role field determines what actions a user can perform:
    - Developer: Full access to all features
    - Admin: Can manage users, products, and operations
    - Moderator: Can manage stock and operations (not users)
    - User: Can only create sales
    """
    
    user = models.OneToOneField(
        User,
        on_delete=models.CASCADE,
        related_name='profile'
    )
    
    # Role-based access control
    role = models.CharField(
        max_length=20,
        choices=ROLE_CHOICES,
        default=ROLE_SELLER,
        help_text="User role for access control"
    )
    
    # Payroll-related fields (from C# User model)
    monthly_salary = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00'),
        validators=[MinValueValidator(Decimal('0.00'))],
        help_text="Employee's monthly salary for payroll calculations"
    )
    
    overtime_multiplier = models.DecimalField(
        max_digits=4,
        decimal_places=2,
        default=Decimal('1.50'),
        validators=[MinValueValidator(Decimal('1.00'))],
        help_text="Overtime multiplier for calculating overtime pay (e.g., 1.5 for time and a half)"
    )
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        db_table = 'api_userprofile'
        verbose_name = 'User Profile'
        verbose_name_plural = 'User Profiles'
        ordering = ['user__username']
    
    def __str__(self):
        return f"{self.user.username} ({self.role})"
    
    # Role check properties (matching C# User model)
    @property
    def is_developer(self):
        """Returns True if user has Developer role."""
        return self.role == 'Developer'
    
    @property
    def is_admin(self):
        """Returns True if user has Admin role."""
        return self.role == 'Admin'
    
    @property
    def is_moderator(self):
        """Returns True if user has Moderator role."""
        return self.role == 'Moderator'
    
    @property
    def is_user(self):
        """Returns True if user has User role."""
        return self.role == 'User'
    
    # Permission check properties (matching C# User model)
    @property
    def can_manage_users(self):
        """Developer and Admin can manage users."""
        return self.is_developer or self.is_admin
    
    @property
    def can_manage_stock(self):
        """Developer, Admin, and Moderator can manage stock."""
        return self.is_developer or self.is_admin or self.is_moderator
    
    @property
    def can_use_attendance_tracker(self):
        """Developer, Admin, and Moderator can use attendance tracker."""
        return self.is_developer or self.is_admin or self.is_moderator
    
    @property
    def can_restock(self):
        """Developer, Admin, and Moderator can restock."""
        return self.is_developer or self.is_admin or self.is_moderator


class SalaryHistory(models.Model):
    """
    Audit trail for salary changes.
    Tracks when, why, and by whom salaries were changed.
    """
    user_profile = models.ForeignKey(
        UserProfile,
        on_delete=models.CASCADE,
        related_name='salary_history'
    )

    # Salary values at time of change
    old_salary = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00')
    )
    new_salary = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00')
    )
    old_overtime_multiplier = models.DecimalField(
        max_digits=4,
        decimal_places=2,
        default=Decimal('1.50')
    )
    new_overtime_multiplier = models.DecimalField(
        max_digits=4,
        decimal_places=2,
        default=Decimal('1.50')
    )

    # Change metadata
    change_reason = models.CharField(max_length=200, blank=True)
    changed_by = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        related_name='salary_changes_made'
    )
    changed_by_name = models.CharField(max_length=100, blank=True)

    # Timestamps
    changed_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        db_table = 'api_salary_history'
        ordering = ['-changed_at']
        indexes = [
            models.Index(fields=['user_profile', 'changed_at']),
        ]

    def __str__(self):
        return f"{self.user_profile.user.username}: {self.old_salary} -> {self.new_salary}"


class UserActivityLog(models.Model):
    """
    Audit trail for user activities.
    Tracks logins, actions, and important events.
    """

    ACTIVITY_TYPES = [
        ('login', 'User Login'),
        ('logout', 'User Logout'),
        ('create', 'Create Record'),
        ('update', 'Update Record'),
        ('delete', 'Delete Record'),
        ('sale', 'Create Sale'),
        ('restock', 'Restock Product'),
        ('attendance', 'Attendance Action'),
        ('expense', 'Expense Action'),
        ('password_change', 'Password Change'),
        ('role_change', 'Role Change'),
        ('salary_change', 'Salary Change'),
        ('other', 'Other'),
    ]

    # User who performed the action
    user = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        related_name='activity_logs'
    )
    user_name = models.CharField(max_length=100, blank=True)

    # Activity details
    activity_type = models.CharField(max_length=30, choices=ACTIVITY_TYPES)
    description = models.TextField(blank=True)

    # Optional reference to related object
    resource_type = models.CharField(max_length=50, blank=True)
    resource_id = models.IntegerField(null=True, blank=True)

    # Metadata
    ip_address = models.GenericIPAddressField(null=True, blank=True)
    user_agent = models.TextField(blank=True)

    # Timestamps
    timestamp = models.DateTimeField(auto_now_add=True)

    class Meta:
        db_table = 'api_user_activity_log'
        ordering = ['-timestamp']
        indexes = [
            models.Index(fields=['user', 'timestamp']),
            models.Index(fields=['activity_type', 'timestamp']),
            models.Index(fields=['resource_type', 'resource_id']),
        ]

    def __str__(self):
        return f"{self.user_name} - {self.activity_type} - {self.timestamp}"

    def save(self, *args, **kwargs):
        # Automatically set user_name from the related user
        if self.user and not self.user_name:
            full_name = self.user.get_full_name()
            self.user_name = full_name or self.user.username
        super().save(*args, **kwargs)


def create_user_profile(sender, instance, created, **kwargs):
    """
    Signal handler to create UserProfile when a new User is created.
    """
    if created:
        # Determine initial role based on user flags
        if instance.is_superuser:
            role = 'Developer'
        elif instance.is_staff:
            role = 'Admin'
        else:
            role = 'Seller'  # Default to Seller role
        
        UserProfile.objects.create(user=instance, role=role)


# Connect the signal
from django.db.models.signals import post_save
post_save.connect(create_user_profile, sender=User)


# ============================================
# ATTENDANCE MODELS (Ported from C# SweetShopMa)
# ============================================


class AttendanceRecord(models.Model):
    """
    Represents an employee attendance record for a specific date.
    
    Tracks whether an employee was present or absent on a given date,
    along with their working hours, pay, and check-in/check-out times.
    """
    
    ATTENDANCE_STATUS = [
        ('Present', 'Present'),
        ('Reset', 'Reset'),
        ('AbsentWithPermission', 'Absent With Permission'),
        ('AbsentWithoutPermission', 'Absent Without Permission'),
    ]
    
    ABSENCE_PERMISSION_TYPES = [
        ('None', 'None'),
        ('WithPermission', 'With Permission'),
        ('WithoutPermission', 'Without Permission'),
        ('Reset', 'Reset'),
    ]
    
    # User reference
    user = models.ForeignKey(
        User,
        on_delete=models.CASCADE,
        related_name='attendance_records'
    )
    
    # Denormalized user name for display and historical accuracy
    user_name = models.CharField(max_length=200)
    
    # Date of the attendance record
    date = models.DateField()
    
    # Attendance status
    status = models.CharField(
        max_length=30,
        choices=ATTENDANCE_STATUS,
        default='Present'
    )
    
    # Boolean attendance status (true = Present, false = Absent)
    is_present = models.BooleanField(default=True)
    
    # Working hours
    regular_hours = models.DecimalField(
        max_digits=5,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    overtime_hours = models.DecimalField(
        max_digits=5,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    # Calculated daily pay
    daily_pay = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    # Check-in and check-out times
    check_in_time = models.DateTimeField(null=True, blank=True)
    check_out_time = models.DateTimeField(null=True, blank=True)
    
    # Optional notes
    notes = models.TextField(blank=True, default='')
    
    # Absence permission type for payroll deductions
    absence_permission_type = models.CharField(
        max_length=20,
        choices=ABSENCE_PERMISSION_TYPES,
        default='None'
    )
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        db_table = 'api_attendance_record'
        ordering = ['-date', '-created_at']
        unique_together = ['user', 'date']
        indexes = [
            models.Index(fields=['user', 'date']),
            models.Index(fields=['date']),
            models.Index(fields=['status']),
        ]
    
    def __str__(self):
        return f"{self.user_name} - {self.date} - {self.status}"
    
    @property
    def total_hours(self):
        """Total hours worked: RegularHours + OvertimeHours"""
        return self.regular_hours + self.overtime_hours
    
    @property
    def check_in_display(self):
        """Formatted check-in time for display (e.g., '08:00' or '--')"""
        if self.check_in_time:
            return self.check_in_time.strftime('%H:%M')
        return '--'
    
    @property
    def check_out_display(self):
        """Formatted check-out time for display (e.g., '17:00' or '--')"""
        if self.check_out_time:
            return self.check_out_time.strftime('%H:%M')
        return '--'
    
    def save(self, *args, **kwargs):
        # Automatically set user_name from the related user
        if self.user and not self.user_name:
            full_name = self.user.get_full_name()
            self.user_name = full_name or self.user.username
        super().save(*args, **kwargs)


class AttendanceSummary(models.Model):
    """
    Summary statistics for attendance records within a date range.
    Typically stored monthly for each employee.
    """
    
    user = models.ForeignKey(
        User,
        on_delete=models.CASCADE,
        related_name='attendance_summaries'
    )
    
    # Denormalized user name
    user_name = models.CharField(max_length=200)
    
    # Month this summary represents (stored as first day of month)
    month = models.DateField()
    
    # Basic counts
    days_present = models.IntegerField(default=0)
    days_absent = models.IntegerField(default=0)
    
    # Hours
    total_regular_hours = models.DecimalField(
        max_digits=8,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    total_overtime_hours = models.DecimalField(
        max_digits=8,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    # Payroll
    total_payroll = models.DecimalField(
        max_digits=12,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    # Expenses (deducted from payroll)
    expenses_total = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    # Additional calculations
    rest_day_payout = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    absence_deductions = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=Decimal('0.00')
    )
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        db_table = 'api_attendance_summary'
        ordering = ['-month', 'user_name']
        unique_together = ['user', 'month']
        indexes = [
            models.Index(fields=['user', 'month']),
            models.Index(fields=['month']),
        ]
    
    def __str__(self):
        return f"{self.user_name} - {self.month} - Present: {self.days_present}"
    
    @property
    def total_hours(self):
        """Total hours worked in the period"""
        return self.total_regular_hours + self.total_overtime_hours


class AttendanceExpense(models.Model):
    """
    Employee expenses that can be deducted from payroll.
    These are expenses specific to an employee for tracking purposes.
    """
    
    EXPENSE_CATEGORIES = [
        ('General', 'General'),
        ('Supplies', 'Supplies'),
        ('Travel', 'Travel'),
        ('Meals', 'Meals'),
        ('Equipment', 'Equipment'),
        ('Other', 'Other'),
    ]
    
    # User reference
    user = models.ForeignKey(
        User,
        on_delete=models.CASCADE,
        related_name='attendance_expenses'
    )
    
    # Denormalized user name
    user_name = models.CharField(max_length=200)
    
    # Expense details
    expense_date = models.DateField()
    amount = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        validators=[MinValueValidator(Decimal('0.01'))]
    )
    category = models.CharField(
        max_length=20,
        choices=EXPENSE_CATEGORIES,
        default='General'
    )
    notes = models.TextField(blank=True, default='')
    
    # Timestamps
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)
    
    class Meta:
        db_table = 'api_attendance_expense'
        ordering = ['-expense_date', '-created_at']
        indexes = [
            models.Index(fields=['user', 'expense_date']),
            models.Index(fields=['expense_date']),
        ]
    
    def __str__(self):
        return f"{self.user_name} - {self.expense_date} - {self.amount}"
    
    def save(self, *args, **kwargs):
        # Automatically set user_name from the related user
        if self.user and not self.user_name:
            full_name = self.user.get_full_name()
            self.user_name = full_name or self.user.username
        super().save(*args, **kwargs)
