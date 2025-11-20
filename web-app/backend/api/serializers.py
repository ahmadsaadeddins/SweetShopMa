from rest_framework import serializers
from django.contrib.auth.password_validation import validate_password
from django.contrib.auth import get_user_model
from .models import Product, CartItem, Order, OrderItem, AttendanceRecord, RestockRecord

User = get_user_model()


class UserSerializer(serializers.ModelSerializer):
    """User serializer with role and permission properties."""
    password = serializers.CharField(write_only=True, required=False)
    is_developer = serializers.ReadOnlyField()
    is_admin = serializers.ReadOnlyField()
    is_moderator = serializers.ReadOnlyField()
    is_user = serializers.ReadOnlyField()
    can_manage_users = serializers.ReadOnlyField()
    can_manage_stock = serializers.ReadOnlyField()
    can_use_attendance_tracker = serializers.ReadOnlyField()
    can_restock = serializers.ReadOnlyField()
    
    class Meta:
        model = User
        fields = [
            'id', 'username', 'name', 'role', 'monthly_salary', 'is_enabled',
            'created_date', 'is_developer', 'is_admin', 'is_moderator', 'is_user',
            'can_manage_users', 'can_manage_stock', 'can_use_attendance_tracker', 'can_restock',
            'password'
        ]
        read_only_fields = ['id', 'created_date']
    
    def create(self, validated_data):
        password = validated_data.pop('password', None)
        user = User.objects.create_user(**validated_data)
        if password:
            user.set_password(password)
            user.save()
        return user
    
    def update(self, instance, validated_data):
        password = validated_data.pop('password', None)
        for attr, value in validated_data.items():
            setattr(instance, attr, value)
        if password:
            instance.set_password(password)
        instance.save()
        return instance


class ProductSerializer(serializers.ModelSerializer):
    """Product serializer."""
    unit_label = serializers.ReadOnlyField()
    
    class Meta:
        model = Product
        fields = ['id', 'name', 'emoji', 'barcode', 'price', 'stock', 'is_sold_by_weight', 'unit_label']


class CartItemSerializer(serializers.ModelSerializer):
    """Cart item serializer."""
    product = ProductSerializer(read_only=True)
    product_id = serializers.PrimaryKeyRelatedField(queryset=Product.objects.all(), source='product', write_only=True)
    item_total = serializers.ReadOnlyField()
    
    class Meta:
        model = CartItem
        fields = ['id', 'product', 'product_id', 'quantity', 'item_total']


class OrderItemSerializer(serializers.ModelSerializer):
    """Order item serializer."""
    item_total = serializers.ReadOnlyField()
    unit_label = serializers.ReadOnlyField()
    
    class Meta:
        model = OrderItem
        fields = ['id', 'product_id', 'product_name', 'product_emoji', 'price', 'quantity', 
                 'is_sold_by_weight', 'item_total', 'unit_label']


class OrderSerializer(serializers.ModelSerializer):
    """Order serializer."""
    items = OrderItemSerializer(many=True, read_only=True)
    
    class Meta:
        model = Order
        fields = ['id', 'user_id', 'user_name', 'order_date', 'total', 'item_count', 'status', 'items']


class AttendanceRecordSerializer(serializers.ModelSerializer):
    """Attendance record serializer."""
    total_hours = serializers.ReadOnlyField()
    user_id = serializers.PrimaryKeyRelatedField(queryset=User.objects.all(), source='user', write_only=True)
    
    class Meta:
        model = AttendanceRecord
        fields = ['id', 'user_id', 'user_name', 'date', 'status', 'is_present', 
                 'regular_hours', 'overtime_hours', 'daily_pay', 'total_hours',
                 'check_in_time', 'check_out_time', 'notes']
    
    def create(self, validated_data):
        """Create attendance record with user."""
        user = validated_data.pop('user')
        user_name = validated_data.pop('user_name', user.name or user.username)
        return AttendanceRecord.objects.create(
            user=user,
            user_name=user_name,
            **validated_data
        )


class RestockRecordSerializer(serializers.ModelSerializer):
    """Restock record serializer."""
    
    class Meta:
        model = RestockRecord
        fields = ['id', 'product_id', 'product_name', 'product_emoji', 'quantity_added',
                 'stock_before', 'stock_after', 'user_id', 'user_name', 'restock_date']

