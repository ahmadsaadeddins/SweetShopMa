"""
Django REST Framework Views for SweetShopMa Desktop Application.

Defines API endpoints for products, sales, customers, and reports.
"""

from rest_framework import viewsets, status, permissions as rf_permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from django.db.models import Sum, Count, Q, F
from django.db import transaction
from django.utils import timezone
from django.core.exceptions import ValidationError
from django.contrib.auth.models import User
from datetime import datetime, timedelta
from decimal import Decimal
import logging

from .models import (
    Category,
    Product,
    Customer,
    Sale,
    SaleItem,
    Expense,
    RestockRecord,
    UserProfile,
    SalaryHistory,
    UserActivityLog,
    AttendanceRecord,
    AttendanceSummary,
    AttendanceExpense,
)
from .serializers import (
    CategorySerializer,
    ProductSerializer,
    ProductListSerializer,
    CustomerSerializer,
    SaleSerializer,
    SaleCreateSerializer,
    SaleItemSerializer,
    ExpenseSerializer,
    SalesStatsSerializer,
    InventoryStatsSerializer,
    DashboardStatsSerializer,
    RestockRecordSerializer,
    RestockSerializer,
    UserProfileSerializer,
    UserProfileCreateSerializer,
    UserProfileUpdateSerializer,
    UserSerializer,
    SalaryHistorySerializer,
    UserActivityLogSerializer,
    AttendanceRecordSerializer,
    AttendanceSummarySerializer,
    AttendanceExpenseSerializer,
    AttendanceExpenseCreateSerializer,
)
from .permissions import (
    CanManageUsers,
    CanManageStock,
    CanRestock,
    IsDeveloperOrReadOnly,
    IsAdminOrReadOnly,
    get_user_permissions_dict,
    get_user_role,
)
from .constants import (
    ROLE_SELLER,
    ROLE_ADMIN,
    ROLE_DEVELOPER,
    ROLE_MODERATOR,
    ROLE_EMPLOYEE,
)

logger = logging.getLogger(__name__)


class CategoryViewSet(viewsets.ModelViewSet):
    """ViewSet for Category model"""
    queryset = Category.objects.all()
    serializer_class = CategorySerializer
    
    def get_queryset(self):
        """Filter categories"""
        queryset = super().get_queryset()
        search = self.request.query_params.get('search')
        if search:
            queryset = queryset.filter(name__icontains=search)
        return queryset


class ProductViewSet(viewsets.ModelViewSet):
    """ViewSet for Product model"""
    queryset = Product.objects.select_related('category').all()
    
    def get_serializer_class(self):
        """Use different serializer for list vs detail"""
        if self.action == 'list':
            return ProductListSerializer
        return ProductSerializer
    
    def get_queryset(self):
        """Filter products"""
        queryset = super().get_queryset()
        
        # Filter by category
        category = self.request.query_params.get('category')
        if category:
            queryset = queryset.filter(category_id=category)
        
        # Filter by active status
        is_active = self.request.query_params.get('is_active')
        if is_active is not None:
            queryset = queryset.filter(is_active=is_active.lower() == 'true')
        
        # Search
        search = self.request.query_params.get('search')
        if search:
            queryset = queryset.filter(
                Q(name__icontains=search) |
                Q(barcode__icontains=search) |
                Q(sku__icontains=search)
            )
        
        # Low stock filter
        low_stock = self.request.query_params.get('low_stock')
        if low_stock and low_stock.lower() == 'true':
            queryset = queryset.filter(quantity__lte=F('low_stock_threshold'))
        
        return queryset
    
    @action(detail=False, methods=['get'])
    def low_stock(self, request):
        """Get products with low stock"""
        products = self.get_queryset().filter(
            quantity__lte=F('low_stock_threshold'),
            is_active=True
        )
        serializer = ProductListSerializer(products, many=True)
        return Response({
            'count': products.count(),
            'results': serializer.data
        })
    
    @action(detail=False, methods=['get'])
    def out_of_stock(self, request):
        """Get products that are out of stock"""
        products = self.get_queryset().filter(quantity=0, is_active=True)
        serializer = ProductListSerializer(products, many=True)
        return Response({
            'count': products.count(),
            'results': serializer.data
        })
    
    @action(detail=False, methods=['post'])
    def bulk_update_quantity(self, request):
        """Bulk update product quantities"""
        updates = request.data.get('updates', [])
        updated_count = 0
        
        for update in updates:
            try:
                product = Product.objects.get(id=update['id'])
                product.quantity = update['quantity']
                product.save()
                updated_count += 1
            except Product.DoesNotExist:
                continue
        
        return Response({
            'updated': updated_count,
            'message': f'Updated {updated_count} products'
        })


class CustomerViewSet(viewsets.ModelViewSet):
    """ViewSet for Customer model"""
    queryset = Customer.objects.all()
    serializer_class = CustomerSerializer
    
    def get_queryset(self):
        """Filter customers"""
        queryset = super().get_queryset()
        search = self.request.query_params.get('search')
        if search:
            queryset = queryset.filter(
                Q(name__icontains=search) |
                Q(phone__icontains=search) |
                Q(email__icontains=search)
            )
        return queryset


class SaleViewSet(viewsets.ModelViewSet):
    """ViewSet for Sale model"""
    queryset = Sale.objects.select_related('customer', 'staff').prefetch_related('items__product').all()
    
    def get_serializer_class(self):
        """Use different serializer for create"""
        if self.action == 'create':
            return SaleCreateSerializer
        return SaleSerializer
    
    def get_queryset(self):
        """Filter sales"""
        queryset = super().get_queryset()
        
        # Filter by date range
        start_date = self.request.query_params.get('start_date')
        end_date = self.request.query_params.get('end_date')
        
        if start_date:
            queryset = queryset.filter(created_at__gte=start_date)
        if end_date:
            queryset = queryset.filter(created_at__lte=end_date)
        
        # Filter by status
        sale_status = self.request.query_params.get('status')
        if sale_status:
            queryset = queryset.filter(status=sale_status)
        
        # Filter by customer
        customer = self.request.query_params.get('customer')
        if customer:
            queryset = queryset.filter(customer_id=customer)
        
        return queryset
    
    def create(self, request, *args, **kwargs):
        """Create a new sale with items"""
        serializer = self.get_serializer(data=request.data)
        serializer.is_valid(raise_exception=True)
        
        # perform_create returns the created sale instance
        sale = self.perform_create(serializer)
        
        # Return detailed sale information
        response_serializer = SaleSerializer(sale)
        
        return Response(
            response_serializer.data,
            status=status.HTTP_201_CREATED
        )
    
    def perform_create(self, serializer):
        """Override to return the created instance"""
        return serializer.save()
    
    @action(detail=False, methods=['get'])
    def today_stats(self, request):
        """Get today's sales statistics"""
        today = timezone.now().date()
        today_sales = self.get_queryset().filter(
            created_at__date=today,
            status='completed'
        )
        
        stats = {
            'total_sales': today_sales.count(),
            'total_revenue': today_sales.aggregate(total=Sum('total'))['total'] or Decimal('0.00'),
            'total_profit': sum(sale.profit for sale in today_sales),
            'average_sale': today_sales.aggregate(
                avg=Sum('total') / Count('id')
            )['avg'] or Decimal('0.00'),
            'total_items': SaleItem.objects.filter(
                sale__in=today_sales
            ).aggregate(total=Sum('quantity'))['total'] or 0,
        }
        
        serializer = SalesStatsSerializer(stats)
        return Response(serializer.data)
    
    @action(detail=False, methods=['get'])
    def week_stats(self, request):
        """Get this week's sales statistics"""
        today = timezone.now().date()
        week_start = today - timedelta(days=today.weekday())
        week_sales = self.get_queryset().filter(
            created_at__date__gte=week_start,
            status='completed'
        )
        
        stats = {
            'total_sales': week_sales.count(),
            'total_revenue': week_sales.aggregate(total=Sum('total'))['total'] or Decimal('0.00'),
            'total_profit': sum(sale.profit for sale in week_sales),
            'average_sale': week_sales.aggregate(
                avg=Sum('total') / Count('id')
            )['avg'] or Decimal('0.00'),
        }
        
        serializer = SalesStatsSerializer(stats)
        return Response(serializer.data)
    
    @action(detail=False, methods=['get'])
    def month_stats(self, request):
        """Get this month's sales statistics"""
        today = timezone.now().date()
        month_start = today.replace(day=1)
        month_sales = self.get_queryset().filter(
            created_at__date__gte=month_start,
            status='completed'
        )
        
        stats = {
            'total_sales': month_sales.count(),
            'total_revenue': month_sales.aggregate(total=Sum('total'))['total'] or Decimal('0.00'),
            'total_profit': sum(sale.profit for sale in month_sales),
            'average_sale': month_sales.aggregate(
                avg=Sum('total') / Count('id')
            )['avg'] or Decimal('0.00'),
        }
        
        serializer = SalesStatsSerializer(stats)
        return Response(serializer.data)
    
    @action(detail=False, methods=['get'])
    def recent(self, request):
        """Get recent sales"""
        import logging
        logger = logging.getLogger(__name__)
        logger.info('[DEBUG API] SaleViewSet.recent() called')
        
        # DEBUG: Log authenticated user
        logger.info(f'[DEBUG API] Authenticated user: {request.user}')
        logger.info(f'[DEBUG API] Is authenticated: {request.user.is_authenticated}')

        limit = int(request.query_params.get('limit', 10))
        logger.info(f'[DEBUG API] Limit parameter: {limit}')

        recent_sales = self.get_queryset().order_by('-created_at')[:limit]
        logger.info(f'[DEBUG API] Recent sales count: {len(recent_sales)}')
        
        # DEBUG: Log staff for each sale
        for sale in recent_sales:
            logger.info(f'[DEBUG API] Sale #{sale.id} - staff: {sale.staff}, staff.username: {sale.staff.username if sale.staff else None}')

        serializer = SaleSerializer(recent_sales, many=True)
        logger.info(f'[DEBUG API] Serialized recent sales: {serializer.data}')

        return Response(serializer.data)
    
    @action(detail=True, methods=['post'])
    def refund(self, request, pk=None):
        """
        Refund a sale.

        CRITICAL: Uses row-level locking (select_for_update) to prevent race conditions
        when multiple concurrent refunds attempt to modify inventory.
        """
        sale = self.get_object()

        try:
            with transaction.atomic():
                # Re-fetch the sale with locked items to prevent concurrent modifications
                sale = Sale.objects.select_for_update().get(pk=pk)

                # Return items to inventory with row locking to prevent race conditions
                for item in sale.items.select_for_update().all():
                    # Lock the product row before updating
                    product = Product.objects.select_for_update().get(id=item.product.id)
                    product.quantity += item.quantity
                    product.save()

                # Update sale status
                sale.status = 'refunded'
                sale.notes = f"REFUNDED: {request.data.get('reason', 'No reason provided')}\n{sale.notes}"
                sale.save()

            serializer = SaleSerializer(sale)
            return Response(serializer.data)

        except Exception as e:
            logger.exception(f'Error during refund of sale {pk}: {str(e)}')
            return Response({
                'error': 'Failed to process refund',
                'code': 'REFUND_ERROR'
            }, status=500)
    
    @action(detail=True, methods=['get'])
    def receipt(self, request, pk=None):
        """Get sale details for receipt printing"""
        logger.info(f'Receipt requested for sale ID={pk} by user {request.user.username}')
        
        try:
            sale = self.get_object()
            items = sale.items.all()
            
            logger.debug(f'Generating receipt for sale {pk}: {items.count()} items, total={sale.total}')
            
            serializer = SaleItemSerializer(items, many=True)
            return Response({
                'sale': SaleSerializer(sale).data,
                'items': serializer.data
            })
            
        except Sale.DoesNotExist:
            logger.error(f'Sale not found for receipt: ID={pk}')
            return Response({'error': 'Sale not found'}, status=404)


class ExpenseViewSet(viewsets.ModelViewSet):
    """ViewSet for Expense model"""
    queryset = Expense.objects.all()
    serializer_class = ExpenseSerializer
    
    def get_queryset(self):
        """Filter expenses"""
        queryset = super().get_queryset()
        
        # Filter by date range
        start_date = self.request.query_params.get('start_date')
        end_date = self.request.query_params.get('end_date')
        
        if start_date:
            queryset = queryset.filter(date__gte=start_date)
        if end_date:
            queryset = queryset.filter(date__lte=end_date)
        
        # Filter by category
        category = self.request.query_params.get('category')
        if category:
            queryset = queryset.filter(category=category)
        
        return queryset
    
    @action(detail=False, methods=['get'])
    def summary(self, request):
        """Get expense summary by category"""
        start_date = request.query_params.get('start_date')
        end_date = request.query_params.get('end_date')
        
        queryset = self.get_queryset()
        
        summary = queryset.values('category').annotate(
            total=Sum('amount'),
            count=Count('id')
        ).order_by('-total')
        
        return Response(summary)


class DashboardViewSet(viewsets.ViewSet):
    """ViewSet for dashboard statistics"""

    def list(self, request):
        """Get dashboard statistics"""
        import logging
        logger = logging.getLogger(__name__)
        logger.info('[DEBUG API] DashboardViewSet.list() called')

        # Sales stats
        today = timezone.now().date()
        logger.info(f'[DEBUG API] Today\'s date: {today}')

        today_sales = Sale.objects.filter(
            created_at__date=today,
            status='completed'
        )
        logger.info(f'[DEBUG API] Today\'s completed sales count: {today_sales.count()}')

        week_start = today - timedelta(days=today.weekday())
        week_sales = Sale.objects.filter(
            created_at__date__gte=week_start,
            status='completed'
        )
        logger.info(f'[DEBUG API] Week start: {week_start}, week sales count: {week_sales.count()}')

        month_start = today.replace(day=1)
        month_sales = Sale.objects.filter(
            created_at__date__gte=month_start,
            status='completed'
        )
        logger.info(f'[DEBUG API] Month start: {month_start}, month sales count: {month_sales.count()}')

        # Inventory stats
        # Fetch all active products once to calculate stats in Python
        # This avoids potential SQLite driver issues with binding Decimal parameters (e.g. quantity=0)
        products = list(Product.objects.filter(is_active=True))
        total_products = len(products)
        logger.info(f'[DEBUG API] Total active products: {total_products}')

        low_stock_count = sum(1 for p in products if p.quantity <= p.low_stock_threshold)
        logger.info(f'[DEBUG API] Low stock products count: {low_stock_count}')

        out_of_stock_count = sum(1 for p in products if p.quantity == 0)
        logger.info(f'[DEBUG API] Out of stock products count: {out_of_stock_count}')

        # Build response
        data = {
            'sales': {
                'total_sales': today_sales.count(),
                'total_revenue': today_sales.aggregate(total=Sum('total'))['total'] or Decimal('0.00'),
                'total_profit': sum(sale.profit for sale in today_sales),
                'average_sale': today_sales.aggregate(
                    avg=Sum('total') / Count('id')
                )['avg'] or Decimal('0.00'),
                'total_items': SaleItem.objects.filter(
                    sale__in=today_sales
                ).aggregate(total=Sum('quantity'))['total'] or 0,
            },
            'inventory': {
                'total_products': total_products,
                'low_stock_products': low_stock_count,
                'out_of_stock_products': out_of_stock_count,
                'total_stock_value': sum(
                    (Decimal(str(p.quantity)) * Decimal(str(p.price)))
                    for p in products
                ),
            },
            'today_revenue': today_sales.aggregate(total=Sum('total'))['total'] or Decimal('0.00'),
            'week_revenue': week_sales.aggregate(total=Sum('total'))['total'] or Decimal('0.00'),
            'month_revenue': month_sales.aggregate(total=Sum('total'))['total'] or Decimal('0.00'),
        }

        logger.info(f'[DEBUG API] Dashboard data prepared: {data}')
        serializer = DashboardStatsSerializer(data)
        logger.info(f'[DEBUG API] Serialized data: {serializer.data}')
        logger.info('[DEBUG API] Returning dashboard response')
        return Response(serializer.data)


class RestockViewSet(viewsets.ModelViewSet):
    """ViewSet for RestockRecord with permission checks"""
    queryset = RestockRecord.objects.select_related('user', 'product').all()
    serializer_class = RestockRecordSerializer
    permission_classes = [CanRestock]  # Use new role-based permission
    
    def get_queryset(self):
        """Filter by product if specified"""
        queryset = super().get_queryset()
        product_id = self.request.query_params.get('product_id')
        if product_id:
            queryset = queryset.filter(product_id=product_id)
        return queryset
    
    @action(detail=False, methods=['post'], permission_classes=[rf_permissions.IsAuthenticated])
    def restock(self, request):
        """
        Restock a product and create audit record.
        
        Permission: Only users with restock permission (Developer, Admin, Moderator) can restock.
        
        Request body:
        {
            "product_id": 1,
            "quantity": 10.5
        }
        """
        user = request.user
        user_role = get_user_role(user)
        
        # Log attempt
        logger.info(f'Restock attempt by user {user.username} (role={user_role})')
        
        # Permission check: CanRestock permission already verified by CanRestock class
        # This additional check is for explicit logging and safety
        if not CanRestock().has_permission(request, self):
            logger.warning(f'Permission denied: User {user.username} attempted to restock without privileges')
            return Response(
                {'error': 'You do not have permission to restock products', 'code': 'PERMISSION_DENIED'},
                status=403
            )
        
        # Validate input
        serializer = RestockSerializer(data=request.data)
        if not serializer.is_valid():
            logger.warning(f'Validation failed for restock by user {user.username}: {serializer.errors}')
            return Response({
                'error': 'Invalid input data',
                'details': serializer.errors,
                'code': 'VALIDATION_ERROR'
            }, status=400)
        
        product_id = serializer.validated_data['product_id']
        quantity = serializer.validated_data['quantity']
        
        try:
            with transaction.atomic():
                # Get product with row lock
                product = Product.objects.select_for_update().get(id=product_id)
                stock_before = product.quantity
                
                logger.info(f'Processing restock: Product {product.name} (ID={product_id}), Quantity={quantity}, Stock Before={stock_before}')
                
                # Business rule: Max restock quantity
                if quantity > 10000:
                    logger.warning(f'Quantity exceeded: User {user.username} attempted to restock {quantity} units (max=10000)')
                    return Response({
                        'error': 'Quantity exceeds maximum limit',
                        'code': 'QUANTITY_EXCEEDED',
                        'max_allowed': 10000
                    }, status=400)
                
                # Update product quantity
                product.quantity += quantity
                product.full_clean()  # Validate model constraints
                product.save()
                
                # Create restock record
                restock_record = RestockRecord.objects.create(
                    product=product,
                    product_name=product.name,
                    product_emoji=getattr(product, 'emoji', ''),
                    quantity_added=quantity,
                    stock_before=stock_before,
                    stock_after=product.quantity,
                    user=request.user,
                    user_name=request.user.get_full_name() or request.user.username
                )
            
            logger.info(f'Restock successful: Record ID={restock_record.id}, Product={product.name}, New Stock={product.quantity}')
            
            # Serialize and return
            result_serializer = RestockRecordSerializer(restock_record)
            return Response(result_serializer.data, status=201)
            
        except Product.DoesNotExist:
            logger.error(f'Product not found: ID={product_id} requested by user {user.username}')
            return Response({
                'error': 'Product not found',
                'code': 'PRODUCT_NOT_FOUND',
                'product_id': product_id
            }, status=404)
            
        except ValidationError as e:
            logger.warning(f'Validation error during restock by user {user.username}: {e.message_dict}')
            return Response({
                'error': 'Data validation failed',
                'details': e.message_dict,
                'code': 'VALIDATION_ERROR'
            }, status=400)
            
        except Exception as e:
            logger.exception(f'Unexpected error during restock by user {user.username}: {str(e)}')
            return Response({
                'error': 'Internal server error',
                'code': 'INTERNAL_ERROR',
                'detail': str(e)
            }, status=500)


class UserViewSet(viewsets.ViewSet):
    """ViewSet for user-related operations including authentication and user management"""
    permission_classes = []  # Allow unauthenticated access for login
    
    def list(self, request):
        """
        List all users.
        
        Permission: Only users with can_manage_users permission (Developer, Admin) can access.
        """
        # Check permission
        if not CanManageUsers().has_permission(request, self):
            return Response(
                {'error': 'You do not have permission to list users', 'code': 'PERMISSION_DENIED'},
                status=403
            )
        
        # Get all user profiles
        profiles = UserProfile.objects.select_related('user').all()
        serializer = UserProfileSerializer(profiles, many=True)
        return Response(serializer.data)
    
    def retrieve(self, request, pk=None):
        """
        Get a specific user by ID.
        
        Permission: Only users with can_manage_users permission can access.
        """
        # Check permission
        if not CanManageUsers().has_permission(request, self):
            return Response(
                {'error': 'You do not have permission to view user details', 'code': 'PERMISSION_DENIED'},
                status=403
            )
        
        try:
            profile = UserProfile.objects.select_related('user').get(pk=pk)
            serializer = UserProfileSerializer(profile)
            return Response(serializer.data)
        except UserProfile.DoesNotExist:
            return Response({'error': 'User not found'}, status=404)
    
    def create(self, request):
        """
        Create a new user.
        
        Permission: Only users with can_manage_users permission can access.
        
        Request body:
        {
            "username": "newuser",
            "password": "password123",
            "email": "user@example.com",
            "role": "User",
            "monthly_salary": 5000,
            "overtime_multiplier": 1.5
        }
        """
        # Check permission
        if not CanManageUsers().has_permission(request, self):
            return Response(
                {'error': 'You do not have permission to create users', 'code': 'PERMISSION_DENIED'},
                status=403
            )
        
        serializer = UserProfileCreateSerializer(data=request.data)
        if serializer.is_valid():
            profile = serializer.save()
            logger.info(f'User {profile.user.username} created with role {profile.role}')
            return Response(UserProfileSerializer(profile).data, status=201)
        return Response(serializer.errors, status=400)
    
    def update(self, request, pk=None):
        """
        Update a user's profile.
        
        Permission: Only users with can_manage_users permission can access.
        """
        # Check permission
        if not CanManageUsers().has_permission(request, self):
            return Response(
                {'error': 'You do not have permission to update users', 'code': 'PERMISSION_DENIED'},
                status=403
            )
        
        try:
            profile = UserProfile.objects.get(pk=pk)
        except UserProfile.DoesNotExist:
            return Response({'error': 'User not found'}, status=404)
        
        serializer = UserProfileUpdateSerializer(profile, data=request.data)
        if serializer.is_valid():
            profile = serializer.save()
            logger.info(f'User {profile.user.username} updated with role {profile.role}')
            return Response(UserProfileSerializer(profile).data)
        return Response(serializer.errors, status=400)
    
    def destroy(self, request, pk=None):
        """
        Delete a user.
        
        Permission: Only users with can_manage_users permission can access.
        """
        # Check permission
        if not CanManageUsers().has_permission(request, self):
            return Response(
                {'error': 'You do not have permission to delete users', 'code': 'PERMISSION_DENIED'},
                status=403
            )
        
        try:
            profile = UserProfile.objects.get(pk=pk)
        except UserProfile.DoesNotExist:
            return Response({'error': 'User not found'}, status=404)
        
        username = profile.user.username
        profile.user.delete()  # This will cascade delete the profile
        logger.info(f'User {username} deleted')
        return Response(status=204)
    
    @action(detail=False, methods=['post'])
    def authenticate(self, request):
        """
        Authenticate user and return token.
        
        Request body:
        {
            "username": "admin",
            "password": "password"
        }
        
        Returns:
        {
            "token": "abc123...",
            "user_id": 1,
            "username": "admin",
            "role": "Admin",
            "permissions": {...}
        }
        """
        from django.contrib.auth import authenticate
        from rest_framework.authtoken.models import Token
        
        username = request.data.get('username')
        password = request.data.get('password')
        
        if not username or not password:
            return Response({
                'error': 'Username and password are required',
                'code': 'MISSING_CREDENTIALS'
            }, status=400)
        
        # Authenticate user
        user = authenticate(username=username, password=password)
        
        if not user:
            logger.warning(f'Failed authentication attempt for username: {username}')
            return Response({
                'error': 'Invalid username or password',
                'code': 'INVALID_CREDENTIALS'
            }, status=401)
        
        if not user.is_active:
            return Response({
                'error': 'User account is disabled',
                'code': 'ACCOUNT_DISABLED'
            }, status=403)
        
        # Get or create token
        token, created = Token.objects.get_or_create(user=user)
        
        logger.info(f'User {user.username} authenticated successfully')
        
        # Get user role and permissions
        role = get_user_role(user)
        permissions_dict = get_user_permissions_dict(user)
        
        return Response({
            'token': token.key,
            'user_id': user.id,
            'username': user.username,
            'is_staff': user.is_staff,
            'is_superuser': user.is_superuser,
            'role': role,
            'permissions': permissions_dict['permissions'],
            'full_name': user.get_full_name()
        })
    
    @action(detail=False, methods=['get'], permission_classes=[rf_permissions.IsAuthenticated])
    def me(self, request):
        """
        Get current authenticated user's profile.
        
        Returns the current user's profile with their role and permissions.
        """
        try:
            profile = UserProfile.objects.select_related('user').get(user=request.user)
            return Response(UserProfileSerializer(profile).data)
        except UserProfile.DoesNotExist:
            # User doesn't have a profile, return basic user info
            return Response({
                'user_id': request.user.id,
                'username': request.user.username,
                'email': request.user.email,
                'role': None,
                'message': 'User profile not found'
            })
    
    @action(detail=False, methods=['get'], permission_classes=[rf_permissions.IsAuthenticated])
    def permissions(self, request):
        """
        Get current user permissions.
        
        Returns a complete permissions object with role and all permission flags.
        
        Returns:
        {
            "is_authenticated": true,
            "is_superuser": false,
            "is_staff": true,
            "role": "Admin",
            "permissions": {
                "can_manage_users": true,
                "can_manage_stock": true,
                "can_use_attendance": true,
                "can_restock": true,
                "can_manage_settings": true,
                "can_view_all_reports": true
            },
            "username": "admin",
            "full_name": "Admin User"
        }
        """
        return Response(get_user_permissions_dict(request.user))

    @action(detail=False, methods=['get'], url_path='list-for-attendance', permission_classes=[rf_permissions.IsAuthenticated])
    def list_for_attendance(self, request):
        """
        List users for attendance tracking purposes.
        
        Permission: Any authenticated user with can_use_attendance permission can access.
        This allows Moderators and other roles that manage attendance to see the list of users.
        """
        # Check attendance permission
        permissions_dict = get_user_permissions_dict(request.user)
        if not permissions_dict['permissions'].get('can_use_attendance', False):
            return Response(
                {'error': 'You do not have permission to view users for attendance', 'code': 'PERMISSION_DENIED'},
                status=403
            )
        
        # Get all user profiles
        profiles = UserProfile.objects.select_related('user').all()
        serializer = UserProfileSerializer(profiles, many=True)
        return Response(serializer.data)

    @action(detail=True, methods=['get'], permission_classes=[rf_permissions.IsAuthenticated])
    def salary_history(self, request, pk=None):
        """
        Get salary history for a specific user.
        
        Permission: Only users with can_manage_users permission can access,
        or Moderators with can_view_all_reports permission.
        """
        # Check permission
        permissions_dict = get_user_permissions_dict(request.user)
        can_view = permissions_dict['permissions'].get('can_manage_users', False) or \
                   permissions_dict['permissions'].get('can_view_all_reports', False)

        if not can_view:
            return Response(
                {'error': 'You do not have permission to view salary history', 'code': 'PERMISSION_DENIED'},
                status=403
            )

        try:
            profile = UserProfile.objects.get(pk=pk)
        except UserProfile.DoesNotExist:
            return Response({'error': 'User not found'}, status=404)

        history = profile.salary_history.all()
        serializer = SalaryHistorySerializer(history, many=True)
        return Response(serializer.data)

    @action(detail=False, methods=['get'], permission_classes=[rf_permissions.IsAuthenticated])
    def all_salary_history(self, request):
        """
        Get salary history for all users.
        
        Permission: Only users with can_manage_users permission can access,
        or Moderators with can_view_all_reports permission.
        """
        # Check permission
        permissions_dict = get_user_permissions_dict(request.user)
        can_view = permissions_dict['permissions'].get('can_manage_users', False) or \
                   permissions_dict['permissions'].get('can_view_all_reports', False)

        if not can_view:
            return Response(
                {'error': 'You do not have permission to view salary history', 'code': 'PERMISSION_DENIED'},
                status=403
            )

        # Get optional filters
        user_id = request.query_params.get('user_id')

        queryset = SalaryHistory.objects.select_related('user_profile', 'user_profile__user', 'changed_by').all()

        if user_id:
            queryset = queryset.filter(user_profile_id=user_id)

        serializer = SalaryHistorySerializer(queryset, many=True)
        return Response(serializer.data)

    @action(detail=True, methods=['get'], permission_classes=[rf_permissions.IsAuthenticated])
    def activities(self, request, pk=None):
        """
        Get activity log for a specific user.
        
        Permission: Any authenticated user can view their own activities,
        Moderators+ can view any user's activities.
        """
        permissions_dict = get_user_permissions_dict(request.user)
        is_moderator_or_above = permissions_dict['permissions'].get('can_manage_stock', False) or \
                               permissions_dict['permissions'].get('can_manage_users', False)

        try:
            profile = UserProfile.objects.get(pk=pk)
        except UserProfile.DoesNotExist:
            return Response({'error': 'User not found'}, status=404)

        # Users can only view their own activities, unless they're moderators
        if str(pk) != str(request.user.id) and not is_moderator_or_above:
            return Response(
                {'error': 'You do not have permission to view this user\'s activities', 'code': 'PERMISSION_DENIED'},
                status=403
            )

        activities = profile.user.activity_logs.all()
        serializer = UserActivityLogSerializer(activities, many=True)
        return Response(serializer.data)

    @action(detail=False, methods=['get'], permission_classes=[rf_permissions.IsAuthenticated])
    def all_activities(self, request):
        """
        Get all activity logs.
        
        Permission: Only users with can_manage_users or can_view_all_reports permission can access.
        """
        # Check permission
        permissions_dict = get_user_permissions_dict(request.user)
        can_view = permissions_dict['permissions'].get('can_manage_users', False) or \
                   permissions_dict['permissions'].get('can_view_all_reports', False)

        if not can_view:
            return Response(
                {'error': 'You do not have permission to view activity logs', 'code': 'PERMISSION_DENIED'},
                status=403
            )

        # Get optional filters
        user_id = request.query_params.get('user_id')
        activity_type = request.query_params.get('activity_type')
        start_date = request.query_params.get('start_date')
        end_date = request.query_params.get('end_date')

        queryset = UserActivityLog.objects.select_related('user').all()

        if user_id:
            queryset = queryset.filter(user_id=user_id)

        if activity_type:
            queryset = queryset.filter(activity_type=activity_type)

        if start_date:
            queryset = queryset.filter(timestamp__gte=start_date)

        if end_date:
            queryset = queryset.filter(timestamp__lte=end_date)

        # Limit results
        limit = int(request.query_params.get('limit', 100))
        queryset = queryset[:limit]

        serializer = UserActivityLogSerializer(queryset, many=True)
        return Response(serializer.data)


# ============================================
# ATTENDANCE VIEWSETS (Ported from C# SweetShopMa)
# ============================================


class AttendanceRecordViewSet(viewsets.ModelViewSet):
    """ViewSet for AttendanceRecord model."""
    queryset = AttendanceRecord.objects.select_related('user').all()
    serializer_class = AttendanceRecordSerializer
    
    def get_queryset(self):
        """Filter attendance records by query parameters."""
        queryset = super().get_queryset()
        
        # Filter by user
        user_id = self.request.query_params.get('user')
        if user_id:
            queryset = queryset.filter(user_id=user_id)
        
        # Filter by date range
        start_date = self.request.query_params.get('start_date')
        end_date = self.request.query_params.get('end_date')
        
        if start_date:
            queryset = queryset.filter(date__gte=start_date)
        if end_date:
            queryset = queryset.filter(date__lte=end_date)
        
        # Filter by status
        status = self.request.query_params.get('status')
        if status:
            queryset = queryset.filter(status=status)
        
        # Filter by is_present
        is_present = self.request.query_params.get('is_present')
        if is_present is not None:
            queryset = queryset.filter(is_present=is_present.lower() == 'true')
        
        # Search by user name
        search = self.request.query_params.get('search')
        if search:
            queryset = queryset.filter(user_name__icontains=search)
        
        return queryset
    
    def perform_create(self, serializer):
        """Set user_name when creating attendance record."""
        instance = serializer.save()
        # Update user_name from related user
        if instance.user and not instance.user_name:
            full_name = instance.user.get_full_name()
            instance.user_name = full_name or instance.user.username
            instance.save(update_fields=['user_name'])
    
    @action(detail=False, methods=['get'])
    def summary(self, request):
        """
        Get attendance summary statistics for a date range.
        
        Query params:
        - start_date: Start date (YYYY-MM-DD)
        - end_date: End date (YYYY-MM-DD)
        - user_id: Optional user filter
        """
        queryset = self.get_queryset()
        
        # Apply user filter if provided
        user_id = request.query_params.get('user_id')
        if user_id:
            queryset = queryset.filter(user_id=user_id)
        
        # Get summary stats
        stats = {
            'total_records': queryset.count(),
            'present_count': queryset.filter(is_present=True).count(),
            'absent_count': queryset.filter(is_present=False).count(),
            'total_regular_hours': queryset.aggregate(
                total=Sum('regular_hours')
            )['total'] or Decimal('0.00'),
            'total_overtime_hours': queryset.aggregate(
                total=Sum('overtime_hours')
            )['total'] or Decimal('0.00'),
            'total_payroll': queryset.aggregate(
                total=Sum('daily_pay')
            )['total'] or Decimal('0.00'),
        }
        
        return Response(stats)
    
    @action(detail=False, methods=['post'])
    def bulk_delete(self, request):
        """
        Delete multiple attendance records by IDs.
        
        Request body:
        {
            "ids": [1, 2, 3]
        }
        """
        ids = request.data.get('ids', [])
        
        if not ids:
            return Response({'error': 'No IDs provided'}, status=400)
        
        deleted_count, _ = AttendanceRecord.objects.filter(id__in=ids).delete()
        
        return Response({
            'deleted': deleted_count,
            'message': f'Deleted {deleted_count} attendance records'
        })


class AttendanceSummaryViewSet(viewsets.ReadOnlyModelViewSet):
    """ViewSet for AttendanceSummary model (read-only)."""
    queryset = AttendanceSummary.objects.select_related('user').all()
    serializer_class = AttendanceSummarySerializer
    
    def get_queryset(self):
        """Filter by month and optionally by user."""
        queryset = super().get_queryset()
        
        # Filter by month (YYYY-MM format)
        month = self.request.query_params.get('month')
        if month:
            try:
                month_date = datetime.strptime(month, '%Y-%m').date().replace(day=1)
                queryset = queryset.filter(month=month_date)
            except ValueError:
                pass
        
        # Filter by user
        user_id = self.request.query_params.get('user')
        if user_id:
            queryset = queryset.filter(user_id=user_id)
        
        return queryset
    
    @action(detail=False, methods=['get'])
    def totals(self, request):
        """
        Get totals across all users for a month.
        
        Query params:
        - month: Month in YYYY-MM format
        """
        queryset = self.get_queryset()
        
        totals = {
            'total_present_days': queryset.aggregate(
                total=Sum('days_present')
            )['total'] or 0,
            'total_absent_days': queryset.aggregate(
                total=Sum('days_absent')
            )['total'] or 0,
            'total_regular_hours': queryset.aggregate(
                total=Sum('total_regular_hours')
            )['total'] or Decimal('0.00'),
            'total_overtime_hours': queryset.aggregate(
                total=Sum('total_overtime_hours')
            )['total'] or Decimal('0.00'),
            'total_payroll': queryset.aggregate(
                total=Sum('total_payroll')
            )['total'] or Decimal('0.00'),
            'total_expenses': queryset.aggregate(
                total=Sum('expenses_total')
            )['total'] or Decimal('0.00'),
        }
        
        return Response(totals)


class AttendanceExpenseViewSet(viewsets.ModelViewSet):
    """ViewSet for AttendanceExpense model."""
    queryset = AttendanceExpense.objects.select_related('user').all()
    
    def get_serializer_class(self):
        """Use different serializer for create vs retrieve/list."""
        if self.action == 'create':
            return AttendanceExpenseCreateSerializer
        return AttendanceExpenseSerializer
    
    def get_queryset(self):
        """Filter expenses by query parameters."""
        queryset = super().get_queryset()
        
        # Filter by user
        user_id = self.request.query_params.get('user')
        if user_id:
            queryset = queryset.filter(user_id=user_id)
        
        # Filter by date range
        start_date = self.request.query_params.get('start_date')
        end_date = self.request.query_params.get('end_date')
        
        if start_date:
            queryset = queryset.filter(expense_date__gte=start_date)
        if end_date:
            queryset = queryset.filter(expense_date__lte=end_date)
        
        # Filter by category
        category = self.request.query_params.get('category')
        if category:
            queryset = queryset.filter(category=category)
        
        return queryset
    
    def perform_create(self, serializer):
        """Set user_name when creating expense."""
        instance = serializer.save()
        # Update user_name from related user
        if instance.user and not instance.user_name:
            full_name = instance.user.get_full_name()
            instance.user_name = full_name or instance.user.username
            instance.save(update_fields=['user_name'])
    
    @action(detail=False, methods=['get'])
    def by_user(self, request):
        """
        Get expenses grouped by user for a date range.
        
        Query params:
        - start_date: Start date
        - end_date: End date
        """
        queryset = self.get_queryset()
        
        # Get totals by user
        from django.db.models import Sum
        by_user = queryset.values('user', 'user_name').annotate(
            total_amount=Sum('amount'),
            count=Count('id')
        ).order_by('user_name')
        
        return Response(list(by_user))
