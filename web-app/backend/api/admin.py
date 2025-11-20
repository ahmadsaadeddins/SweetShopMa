from django.contrib import admin
from django.contrib.auth.admin import UserAdmin as BaseUserAdmin
from .models import User, Product, Order, OrderItem, CartItem, AttendanceRecord, RestockRecord


@admin.register(User)
class UserAdmin(BaseUserAdmin):
    list_display = ['username', 'name', 'role', 'is_enabled', 'created_date']
    list_filter = ['role', 'is_enabled', 'is_staff']
    fieldsets = BaseUserAdmin.fieldsets + (
        ('Additional Info', {'fields': ('name', 'role', 'monthly_salary', 'is_enabled')}),
    )


@admin.register(Product)
class ProductAdmin(admin.ModelAdmin):
    list_display = ['emoji', 'name', 'barcode', 'price', 'stock', 'is_sold_by_weight', 'unit_label']
    list_filter = ['is_sold_by_weight']
    search_fields = ['name', 'barcode']


@admin.register(Order)
class OrderAdmin(admin.ModelAdmin):
    list_display = ['id', 'user_name', 'order_date', 'total', 'item_count', 'status']
    list_filter = ['status', 'order_date']
    readonly_fields = ['order_date']


@admin.register(OrderItem)
class OrderItemAdmin(admin.ModelAdmin):
    list_display = ['order', 'product_name', 'quantity', 'price', 'item_total']
    list_filter = ['is_sold_by_weight']


@admin.register(CartItem)
class CartItemAdmin(admin.ModelAdmin):
    list_display = ['user', 'product', 'quantity', 'item_total']
    list_filter = ['created_at']


@admin.register(AttendanceRecord)
class AttendanceRecordAdmin(admin.ModelAdmin):
    list_display = ['user_name', 'date', 'status', 'regular_hours', 'overtime_hours', 'total_hours']
    list_filter = ['status', 'date']
    search_fields = ['user_name']


@admin.register(RestockRecord)
class RestockRecordAdmin(admin.ModelAdmin):
    list_display = ['product_name', 'quantity_added', 'stock_before', 'stock_after', 'user_name', 'restock_date']
    list_filter = ['restock_date']
    readonly_fields = ['restock_date']

