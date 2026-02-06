"""
Django REST Framework Serializers for SweetShopMa Desktop Application.

Defines how models are converted to/from JSON for API responses.
"""

from rest_framework import serializers
from decimal import Decimal
from django.contrib.auth.models import User
from django.db import transaction
from .models import (
    Category,
    Product,
    Customer,
    Sale,
    SaleItem,
    Expense,
    SyncMetadata,
    RestockRecord,
    UserProfile,
    SalaryHistory,
    UserActivityLog,
    AttendanceRecord,
    AttendanceSummary,
    AttendanceExpense,
)
from .constants import ROLE_CHOICES


class CategorySerializer(serializers.ModelSerializer):
    """Serializer for Category model"""
    product_count = serializers.SerializerMethodField()
    
    class Meta:
        model = Category
        fields = [
            'id',
            'name',
            'description',
            'product_count',
            'created_at',
            'updated_at',
        ]
        read_only_fields = ['created_at', 'updated_at']
    
    def get_product_count(self, obj):
        """Get count of products in this category"""
        return obj.products.filter(is_active=True).count()


class ProductSerializer(serializers.ModelSerializer):
    """Serializer for Product model"""
    category_name = serializers.CharField(source='category.name', read_only=True, allow_null=True)
    profit_margin = serializers.ReadOnlyField()
    is_low_stock = serializers.ReadOnlyField()
    stock_value = serializers.ReadOnlyField()
    
    class Meta:
        model = Product
        fields = [
            'id',
            'name',
            'description',
            'category',
            'category_name',
            'price',
            'cost',
            'quantity',
            'unit',
            'low_stock_threshold',
            'barcode',
            'sku',
            'is_active',
            'profit_margin',
            'is_low_stock',
            'stock_value',
            'created_at',
            'updated_at',
        ]
        read_only_fields = ['created_at', 'updated_at']


class ProductListSerializer(serializers.ModelSerializer):
    """Lightweight serializer for product lists"""
    category_name = serializers.CharField(source='category.name', read_only=True, allow_null=True)
    is_low_stock = serializers.ReadOnlyField()
    
    class Meta:
        model = Product
        fields = [
            'id',
            'name',
            'category',
            'category_name',
            'price',
            'quantity',
            'unit',
            'barcode',
            'sku',
            'is_active',
            'is_low_stock',
        ]


class CustomerSerializer(serializers.ModelSerializer):
    """Serializer for Customer model"""
    sale_count = serializers.SerializerMethodField()
    total_purchases = serializers.SerializerMethodField()
    
    class Meta:
        model = Customer
        fields = [
            'id',
            'name',
            'phone',
            'email',
            'address',
            'notes',
            'sale_count',
            'total_purchases',
            'created_at',
            'updated_at',
        ]
        read_only_fields = ['created_at', 'updated_at']
    
    def get_sale_count(self, obj):
        """Get count of sales for this customer"""
        return obj.sales.count()
    
    def get_total_purchases(self, obj):
        """Get total purchase amount for this customer"""
        return obj.sales.aggregate(
            total=models.Sum('total')
        )['total'] or 0


class SaleItemSerializer(serializers.ModelSerializer):
    """Serializer for SaleItem model"""
    product_name = serializers.CharField(source='product.name', read_only=True)
    total = serializers.ReadOnlyField()
    profit = serializers.ReadOnlyField()
    
    class Meta:
        model = SaleItem
        fields = [
            'id',
            'sale',
            'product',
            'product_name',
            'quantity',
            'unit_price',
            'cost_price',
            'subtotal',
            'discount',
            'total',
            'profit',
            'created_at',
        ]
        read_only_fields = ['created_at']


class SaleSerializer(serializers.ModelSerializer):
    """Serializer for Sale model"""
    items = SaleItemSerializer(many=True, read_only=True)
    customer_name = serializers.CharField(source='customer.name', read_only=True, allow_null=True)
    staff_name = serializers.SerializerMethodField(read_only=True)
    item_count = serializers.ReadOnlyField()
    profit = serializers.ReadOnlyField()
    
    def get_staff_name(self, obj):
        """Get staff full name or username"""
        if obj.staff:
            # Try full name first, then username
            full_name = obj.staff.get_full_name()
            if full_name:
                return full_name
            return obj.staff.username
        return None
    
    class Meta:
        model = Sale
        fields = [
            'id',
            'items',  # Added missing 'items' field
            'total',
            'subtotal',
            'tax',
            'discount',
            'payment_method',
            'customer',
            'customer_name',
            'staff',
            'staff_name',
            'status',
            'notes',
            'item_count',
            'profit',
            'created_at',
            'updated_at',
            'synced',
            'synced_at',
        ]
        read_only_fields = ['created_at', 'updated_at', 'synced_at']


class SaleCreateSerializer(serializers.ModelSerializer):
    """Serializer for creating sales with items"""
    items = serializers.ListField(
        child=serializers.DictField(),
        write_only=True,
        required=True
    )
    
    class Meta:
        model = Sale
        fields = [
            'subtotal',
            'tax',
            'discount',
            'total',
            'payment_method',
            'customer',
            'notes',
            'items',
        ]
        read_only_fields = ['staff']
    
    def create(self, validated_data):
        """
        Create sale and associated items.

        CRITICAL: Uses row-level locking (select_for_update) to prevent race conditions
        when multiple concurrent sales attempt to modify the same product inventory.
        """
        items_data = validated_data.pop('items')

        # Convert numeric fields to Decimal for sqlcipher3 compatibility
        from decimal import Decimal
        validated_data['subtotal'] = Decimal(str(validated_data.get('subtotal', 0)))
        validated_data['tax'] = Decimal(str(validated_data.get('tax', 0)))
        validated_data['discount'] = Decimal(str(validated_data.get('discount', 0)))
        validated_data['total'] = Decimal(str(validated_data.get('total', 0)))

        # Automatically assign staff from authenticated user
        request = self.context.get('request')
        if request and request.user and request.user.is_authenticated:
            validated_data['staff'] = request.user
            import logging
            logger = logging.getLogger(__name__)
            logger.info(f'[DEBUG SALE] Creating sale with authenticated user: {request.user.username} (ID: {request.user.id})')
        else:
            import logging
            logger = logging.getLogger(__name__)
            logger.warning('[DEBUG SALE] No authenticated user - staff will be NULL!')

        # Use atomic transaction to ensure all-or-nothing consistency
        with transaction.atomic():
            # Create sale
            sale = Sale.objects.create(**validated_data)

            # Create sale items and update inventory with row locking
            for item_data in items_data:
                try:
                    # Lock the product row to prevent concurrent modifications
                    product = Product.objects.select_for_update().get(id=item_data['product_id'])
                except Product.DoesNotExist:
                    raise serializers.ValidationError(
                        f"Product with id {item_data['product_id']} does not exist"
                    )

                # Convert to Decimal for proper calculations
                quantity = Decimal(str(item_data['quantity']))
                unit_price = Decimal(str(item_data['price']))

                # Validate sufficient stock
                if product.quantity < quantity:
                    raise serializers.ValidationError(
                        f"Insufficient stock for product '{product.name}'. "
                        f"Available: {product.quantity}, Required: {quantity}"
                    )

                # Get cost price for profit calculation
                cost_price = product.cost if product.cost else Decimal('0.00')

                # Create sale item
                SaleItem.objects.create(
                    sale=sale,
                    product=product,
                    quantity=quantity,
                    unit_price=unit_price,
                    cost_price=cost_price,
                    subtotal=quantity * unit_price,
                    discount=Decimal(str(item_data.get('discount', 0)))
                )

                # Update product quantity (row is already locked)
                product.quantity -= quantity
                product.save()

        return sale


class ExpenseSerializer(serializers.ModelSerializer):
    """Serializer for Expense model"""
    class Meta:
        model = Expense
        fields = [
            'id',
            'description',
            'category',
            'amount',
            'date',
            'notes',
            'created_at',
            'updated_at',
            'synced',
            'synced_at',
        ]
        read_only_fields = ['created_at', 'updated_at', 'synced_at']


class SyncMetadataSerializer(serializers.ModelSerializer):
    """Serializer for SyncMetadata model"""
    class Meta:
        model = SyncMetadata
        fields = [
            'branch_id',
            'last_sync',
            'last_successful_sync',
            'sync_count',
            'last_error',
        ]


class RestockRecordSerializer(serializers.ModelSerializer):
    """Serializer for RestockRecord"""
    user_name = serializers.SerializerMethodField(read_only=True)
    
    def get_user_name(self, obj):
        """Get user full name or username"""
        if obj.user:
            full_name = obj.user.get_full_name()
            if full_name:
                return full_name
            return obj.user.username
        return obj.user_name or 'Unknown'
    
    class Meta:
        model = RestockRecord
        fields = [
            'id',
            'product',
            'product_name',
            'product_emoji',
            'quantity_added',
            'stock_before',
            'stock_after',
            'user',
            'user_name',
            'restock_date'
        ]
        read_only_fields = ['user', 'restock_date']


class RestockSerializer(serializers.Serializer):
    """Serializer for restock action"""
    product_id = serializers.IntegerField()
    quantity = serializers.DecimalField(
        max_digits=10,
        decimal_places=3,
        min_value=Decimal('0.01')
    )


# Statistics Serializers

class SalesStatsSerializer(serializers.Serializer):
    """Serializer for sales statistics"""
    total_sales = serializers.IntegerField()
    total_revenue = serializers.DecimalField(max_digits=12, decimal_places=2)
    total_profit = serializers.DecimalField(max_digits=12, decimal_places=2)
    average_sale = serializers.DecimalField(max_digits=12, decimal_places=2)
    total_items = serializers.IntegerField()


class InventoryStatsSerializer(serializers.Serializer):
    """Serializer for inventory statistics"""
    total_products = serializers.IntegerField()
    low_stock_products = serializers.IntegerField()
    out_of_stock_products = serializers.IntegerField()
    total_stock_value = serializers.DecimalField(max_digits=12, decimal_places=2)


class DashboardStatsSerializer(serializers.Serializer):
    """Serializer for dashboard statistics"""
    sales = SalesStatsSerializer()
    inventory = InventoryStatsSerializer()
    today_revenue = serializers.DecimalField(max_digits=12, decimal_places=2)
    week_revenue = serializers.DecimalField(max_digits=12, decimal_places=2)
    month_revenue = serializers.DecimalField(max_digits=12, decimal_places=2)


# Import Sum for aggregations
from django.db.models import Sum


# User Profile Serializers
class UserProfileSerializer(serializers.ModelSerializer):
    """Serializer for UserProfile model with role information."""
    user_id = serializers.IntegerField(source='user.id', read_only=True)
    username = serializers.CharField(source='user.username', read_only=True)
    email = serializers.EmailField(source='user.email', read_only=True)
    is_active = serializers.BooleanField(source='user.is_active', read_only=True)
    is_staff = serializers.BooleanField(source='user.is_staff', read_only=True)
    is_superuser = serializers.BooleanField(source='user.is_superuser', read_only=True)
    date_joined = serializers.DateTimeField(source='user.date_joined', read_only=True)
    
    class Meta:
        model = UserProfile
        fields = [
            'id',
            'user_id',
            'username',
            'email',
            'is_active',
            'is_staff',
            'is_superuser',
            'date_joined',
            'role',
            'monthly_salary',
            'overtime_multiplier',
            'created_at',
            'updated_at',
        ]
        read_only_fields = ['user_id', 'username', 'email', 'is_active', 'is_staff', 'is_superuser', 'date_joined', 'created_at', 'updated_at']


class UserProfileCreateSerializer(serializers.ModelSerializer):
    """Serializer for creating a new user with profile."""
    username = serializers.CharField(max_length=150)
    password = serializers.CharField(write_only=True, required=True, min_length=8)
    email = serializers.EmailField(required=False, allow_blank=True)
    role = serializers.ChoiceField(choices=ROLE_CHOICES, default='User')
    monthly_salary = serializers.DecimalField(max_digits=10, decimal_places=2, default=0)
    overtime_multiplier = serializers.DecimalField(max_digits=4, decimal_places=2, default=1.5)
    
    class Meta:
        model = UserProfile
        fields = ['username', 'password', 'email', 'role', 'monthly_salary', 'overtime_multiplier']
    
    def create(self, validated_data):
        # Create the User
        username = validated_data.pop('username')
        password = validated_data.pop('password')
        email = validated_data.pop('email', '')
        
        user = User.objects.create_user(
            username=username,
            password=password,
            email=email,
            is_staff=(validated_data.get('role') in ['Admin', 'Developer'])
        )
        
        # Create the UserProfile
        profile = UserProfile.objects.create(user=user, **validated_data)
        return profile


class UserProfileUpdateSerializer(serializers.ModelSerializer):
    """Serializer for updating user profile."""
    role = serializers.ChoiceField(choices=ROLE_CHOICES, required=False)
    monthly_salary = serializers.DecimalField(max_digits=10, decimal_places=2, required=False)
    overtime_multiplier = serializers.DecimalField(max_digits=4, decimal_places=2, required=False)
    change_reason = serializers.CharField(max_length=200, required=False, allow_blank=True)

    class Meta:
        model = UserProfile
        fields = ['role', 'monthly_salary', 'overtime_multiplier', 'change_reason']

    def update(self, instance, validated_data):
        from .models import SalaryHistory, UserActivityLog

        # Get the request from context
        request = self.context.get('request')

        # Track old values
        old_salary = instance.monthly_salary
        old_overtime = instance.overtime_multiplier
        old_role = instance.role

        # Get change reason (optional)
        change_reason = validated_data.pop('change_reason', '')

        # Update User flags based on role
        new_role = validated_data.get('role', old_role)

        # Update User is_staff flag
        if new_role in ['Admin', 'Developer'] and not instance.user.is_staff:
            instance.user.is_staff = True
            instance.user.save()
        elif new_role == 'User' and instance.user.is_staff:
            # Don't remove staff from existing staff users unless they were Admin
            if old_role == 'Admin':
                instance.user.is_staff = False
                instance.user.save()

        # Call the parent update to actually update the instance
        instance = super().update(instance, validated_data)

        # Get new values
        new_salary = instance.monthly_salary
        new_overtime = instance.overtime_multiplier
        new_role = instance.role

        # Check if salary changed
        salary_changed = old_salary != new_salary or old_overtime != new_overtime

        if salary_changed:
            # Create salary history record
            SalaryHistory.objects.create(
                user_profile=instance,
                old_salary=old_salary,
                new_salary=new_salary,
                old_overtime_multiplier=old_overtime,
                new_overtime_multiplier=new_overtime,
                change_reason=change_reason,
                changed_by=request.user if request and request.user.is_authenticated else None,
                changed_by_name=request.user.get_full_name() if request and request.user.is_authenticated else 'Unknown'
            )

            # Log activity
            if request and request.user.is_authenticated:
                UserActivityLog.objects.create(
                    user=request.user,
                    activity_type='salary_change',
                    description=f"Changed salary for {instance.user.username}: {old_salary} -> {new_salary}",
                    resource_type='User',
                    resource_id=instance.id
                )

        # Log role change
        if old_role != new_role:
            if request and request.user.is_authenticated:
                UserActivityLog.objects.create(
                    user=request.user,
                    activity_type='role_change',
                    description=f"Changed role for {instance.user.username}: {old_role} -> {new_role}",
                    resource_type='User',
                    resource_id=instance.id
                )

        return instance


class UserSerializer(serializers.ModelSerializer):
    """Serializer for Django's built-in User model."""
    class Meta:
        model = User
        fields = ['id', 'username', 'email', 'is_staff', 'is_active', 'date_joined']
        read_only_fields = ['id', 'date_joined']


class SalaryHistorySerializer(serializers.ModelSerializer):
    """Serializer for SalaryHistory model."""
    changed_by_name = serializers.ReadOnlyField()
    old_salary_display = serializers.SerializerMethodField()
    new_salary_display = serializers.SerializerMethodField()

    def get_old_salary_display(self, obj):
        return f"{obj.old_salary:,.2f}"

    def get_new_salary_display(self, obj):
        return f"{obj.new_salary:,.2f}"

    class Meta:
        model = SalaryHistory
        fields = [
            'id',
            'user_profile',
            'old_salary',
            'new_salary',
            'old_salary_display',
            'new_salary_display',
            'old_overtime_multiplier',
            'new_overtime_multiplier',
            'change_reason',
            'changed_by',
            'changed_by_name',
            'changed_at',
        ]
        read_only_fields = ['changed_at']


class UserActivityLogSerializer(serializers.ModelSerializer):
    """Serializer for UserActivityLog model."""
    timestamp_display = serializers.SerializerMethodField()

    def get_timestamp_display(self, obj):
        return obj.timestamp.strftime('%Y-%m-%d %H:%M:%S')

    class Meta:
        model = UserActivityLog
        fields = [
            'id',
            'user',
            'user_name',
            'activity_type',
            'description',
            'resource_type',
            'resource_id',
            'ip_address',
            'user_agent',
            'timestamp',
            'timestamp_display',
        ]
        read_only_fields = ['timestamp']


# ============================================
# ATTENDANCE SERIALIZERS (Ported from C# SweetShopMa)
# ============================================


class AttendanceRecordSerializer(serializers.ModelSerializer):
    """Serializer for AttendanceRecord model."""
    total_hours = serializers.ReadOnlyField()
    check_in_display = serializers.ReadOnlyField()
    check_out_display = serializers.ReadOnlyField()
    
    class Meta:
        model = AttendanceRecord
        fields = [
            'id',
            'user',
            'user_name',
            'date',
            'status',
            'is_present',
            'regular_hours',
            'overtime_hours',
            'total_hours',
            'daily_pay',
            'check_in_time',
            'check_out_time',
            'check_in_display',
            'check_out_display',
            'notes',
            'absence_permission_type',
            'created_at',
            'updated_at',
        ]
        read_only_fields = ['created_at', 'updated_at', 'user_name', 'total_hours', 'check_in_display', 'check_out_display']


class AttendanceSummarySerializer(serializers.ModelSerializer):
    """Serializer for AttendanceSummary model."""
    total_hours = serializers.ReadOnlyField()
    
    class Meta:
        model = AttendanceSummary
        fields = [
            'id',
            'user',
            'user_name',
            'month',
            'days_present',
            'days_absent',
            'total_regular_hours',
            'total_overtime_hours',
            'total_hours',
            'total_payroll',
            'expenses_total',
            'rest_day_payout',
            'absence_deductions',
            'created_at',
            'updated_at',
        ]
        read_only_fields = ['created_at', 'updated_at']


class AttendanceExpenseSerializer(serializers.ModelSerializer):
    """Serializer for AttendanceExpense model."""
    
    class Meta:
        model = AttendanceExpense
        fields = [
            'id',
            'user',
            'user_name',
            'expense_date',
            'amount',
            'category',
            'notes',
            'created_at',
            'updated_at',
        ]
        read_only_fields = ['created_at', 'updated_at']


class AttendanceExpenseCreateSerializer(serializers.ModelSerializer):
    """Serializer for creating AttendanceExpense."""
    
    class Meta:
        model = AttendanceExpense
        fields = [
            'user',
            'expense_date',
            'amount',
            'category',
            'notes',
        ]
