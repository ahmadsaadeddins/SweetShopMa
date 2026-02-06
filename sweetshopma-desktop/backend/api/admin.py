"""
Django Admin configuration for SweetShopMa Desktop Application.
"""

from django.contrib import admin
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
)


@admin.register(Category)
class CategoryAdmin(admin.ModelAdmin):
    """Admin interface for Category"""
    list_display = ['name', 'product_count', 'created_at', 'updated_at']
    search_fields = ['name', 'description']
    readonly_fields = ['created_at', 'updated_at']
    
    def product_count(self, obj):
        """Display count of products in category"""
        return obj.products.filter(is_active=True).count()
    product_count.short_description = 'Products'


@admin.register(Product)
class ProductAdmin(admin.ModelAdmin):
    """Admin interface for Product"""
    list_display = [
        'name', 'category', 'price', 'quantity', 'unit',
        'is_active', 'is_low_stock', 'created_at'
    ]
    list_filter = ['category', 'is_active', 'unit']
    search_fields = ['name', 'barcode', 'sku', 'description']
    readonly_fields = ['created_at', 'updated_at']
    fieldsets = (
        ('Basic Information', {
            'fields': ('name', 'description', 'category')
        }),
        ('Pricing', {
            'fields': ('price', 'cost')
        }),
        ('Inventory', {
            'fields': ('quantity', 'unit', 'low_stock_threshold', 'is_active')
        }),
        ('Identification', {
            'fields': ('barcode', 'sku')
        }),
        ('Timestamps', {
            'fields': ('created_at', 'updated_at'),
            'classes': ('collapse',)
        }),
    )
    
    def is_low_stock(self, obj):
        """Display if product is low on stock"""
        return obj.is_low_stock
    is_low_stock.boolean = True
    is_low_stock.short_description = 'Low Stock'


class SaleItemInline(admin.TabularInline):
    """Inline admin for sale items"""
    model = SaleItem
    extra = 0
    readonly_fields = ['created_at']
    fields = ['product', 'quantity', 'unit_price', 'cost_price', 'subtotal', 'discount']


@admin.register(Sale)
class SaleAdmin(admin.ModelAdmin):
    """Admin interface for Sale"""
    list_display = [
        'id', 'total', 'payment_method', 'customer', 'status',
        'item_count', 'created_at', 'synced'
    ]
    list_filter = ['status', 'payment_method', 'synced', 'created_at']
    search_fields = ['customer__name', 'notes']
    readonly_fields = ['created_at', 'updated_at', 'synced_at']
    inlines = [SaleItemInline]
    date_hierarchy = 'created_at'
    
    fieldsets = (
        ('Transaction Details', {
            'fields': ('customer', 'staff', 'status', 'payment_method')
        }),
        ('Amounts', {
            'fields': ('subtotal', 'tax', 'discount', 'total')
        }),
        ('Additional', {
            'fields': ('notes',)
        }),
        ('Sync Status', {
            'fields': ('synced', 'synced_at')
        }),
        ('Timestamps', {
            'fields': ('created_at', 'updated_at'),
            'classes': ('collapse',)
        }),
    )
    
    def item_count(self, obj):
        """Display number of items in sale"""
        return obj.item_count
    item_count.short_description = 'Items'


@admin.register(Customer)
class CustomerAdmin(admin.ModelAdmin):
    """Admin interface for Customer"""
    list_display = ['name', 'phone', 'email', 'sale_count', 'created_at']
    search_fields = ['name', 'phone', 'email', 'address']
    readonly_fields = ['created_at', 'updated_at']
    
    def sale_count(self, obj):
        """Display number of sales for customer"""
        return obj.sales.count()
    sale_count.short_description = 'Sales'


@admin.register(Expense)
class ExpenseAdmin(admin.ModelAdmin):
    """Admin interface for Expense"""
    list_display = ['description', 'category', 'amount', 'date', 'synced', 'created_at']
    list_filter = ['category', 'synced', 'date']
    search_fields = ['description', 'notes']
    readonly_fields = ['created_at', 'updated_at', 'synced_at']
    date_hierarchy = 'date'


@admin.register(SyncMetadata)
class SyncMetadataAdmin(admin.ModelAdmin):
    """Admin interface for SyncMetadata"""
    list_display = ['branch_id', 'last_sync', 'last_successful_sync', 'sync_count', 'last_error']
    readonly_fields = ['branch_id', 'last_sync', 'last_successful_sync', 'sync_count', 'last_error']


@admin.register(RestockRecord)
class RestockRecordAdmin(admin.ModelAdmin):
    """Admin interface for RestockRecord"""
    list_display = [
        'id', 'product_name', 'quantity_added', 'stock_before',
        'stock_after', 'user_name', 'restock_date'
    ]
    list_filter = ['restock_date', 'user_name']
    search_fields = ['product_name', 'user_name']
    readonly_fields = [
        'product', 'product_name', 'product_emoji',
        'quantity_added', 'stock_before', 'stock_after',
        'user', 'user_name', 'restock_date'
    ]
    date_hierarchy = 'restock_date'
    ordering = ['-restock_date']
    
    fieldsets = (
        ('Product Info', {
            'fields': ('product', 'product_name', 'product_emoji')
        }),
        ('Stock Changes', {
            'fields': ('quantity_added', 'stock_before', 'stock_after')
        }),
        ('User Info', {
            'fields': ('user', 'user_name')
        }),
        ('Timestamp', {
            'fields': ('restock_date',),
        }),
    )


@admin.register(UserProfile)
class UserProfileAdmin(admin.ModelAdmin):
    """Admin interface for UserProfile with role-based access control"""
    list_display = ['user', 'role', 'monthly_salary', 'overtime_multiplier', 'is_user_active']
    list_filter = ['role']
    search_fields = ['user__username', 'user__email', 'user__first_name', 'user__last_name']
    readonly_fields = ['created_at', 'updated_at']
    
    def is_user_active(self, obj):
        """Display if user is active"""
        return obj.user.is_active
    is_user_active.boolean = True
    is_user_active.short_description = 'User Active'
    
    fieldsets = (
        ('User Information', {
            'fields': ('user', 'role')
        }),
        ('Payroll', {
            'fields': ('monthly_salary', 'overtime_multiplier')
        }),
        ('Timestamps', {
            'fields': ('created_at', 'updated_at'),
            'classes': ('collapse',)
        }),
    )
