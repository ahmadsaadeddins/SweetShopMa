from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework.views import APIView
from rest_framework_simplejwt.tokens import RefreshToken
from django.contrib.auth import authenticate
from django.db.models import Sum, Count, Q, F
from django.db import IntegrityError
from django.utils import timezone
from datetime import timedelta
from decimal import Decimal

from .models import User, Product, CartItem, Order, OrderItem, AttendanceRecord, RestockRecord
from .serializers import (
    UserSerializer, ProductSerializer, CartItemSerializer, OrderSerializer,
    AttendanceRecordSerializer, RestockRecordSerializer
)


class IsDeveloperOrAdmin(permissions.BasePermission):
    """Permission for Developer or Admin only."""
    def has_permission(self, request, view):
        return request.user and (request.user.is_developer or request.user.is_admin)


class IsDeveloperAdminOrModerator(permissions.BasePermission):
    """Permission for Developer, Admin, or Moderator."""
    def has_permission(self, request, view):
        return request.user and (request.user.is_developer or request.user.is_admin or request.user.is_moderator)


class AuthView(APIView):
    """Authentication views."""
    permission_classes = [permissions.AllowAny]
    
    def post(self, request):
        """Login endpoint."""
        username = request.data.get('username')
        password = request.data.get('password')
        
        if not username or not password:
            return Response({'error': 'Username and password required'}, status=status.HTTP_400_BAD_REQUEST)
        
        user = authenticate(username=username, password=password)
        
        if user and user.is_enabled:
            refresh = RefreshToken.for_user(user)
            serializer = UserSerializer(user)
            return Response({
                'refresh': str(refresh),
                'access': str(refresh.access_token),
                'user': serializer.data
            })
        
        return Response({'error': 'Invalid credentials or account disabled'}, status=status.HTTP_401_UNAUTHORIZED)
    
    def get(self, request):
        """Get current user info."""
        if not request.user.is_authenticated:
            return Response({'error': 'Not authenticated'}, status=status.HTTP_401_UNAUTHORIZED)
        serializer = UserSerializer(request.user)
        return Response({'user': serializer.data})


class UserViewSet(viewsets.ModelViewSet):
    """User management viewset."""
    queryset = User.objects.all()
    serializer_class = UserSerializer
    permission_classes = [IsDeveloperOrAdmin]
    
    def get_queryset(self):
        """Filter out Developer users from list."""
        qs = super().get_queryset()
        if not self.request.user.is_developer:
            qs = qs.exclude(role='Developer')
        return qs
    
    @action(detail=True, methods=['post'])
    def toggle_status(self, request, pk=None):
        """Enable/disable user."""
        user = self.get_object()
        if user == request.user:
            return Response({'error': 'Cannot disable your own account'}, status=status.HTTP_400_BAD_REQUEST)
        user.is_enabled = not user.is_enabled
        user.save()
        return Response({'is_enabled': user.is_enabled})


class ProductViewSet(viewsets.ModelViewSet):
    """Product management viewset."""
    queryset = Product.objects.all()
    serializer_class = ProductSerializer
    permission_classes = [IsDeveloperAdminOrModerator]
    
    def get_queryset(self):
        """Order products by sales."""
        qs = super().get_queryset()
        search = self.request.query_params.get('search', None)
        if search:
            qs = qs.filter(Q(name__icontains=search) | Q(barcode__icontains=search))
        return qs.order_by('name')
    
    @action(detail=True, methods=['post'])
    def restock(self, request, pk=None):
        """Restock a product."""
        product = self.get_object()
        quantity = Decimal(str(request.data.get('quantity', 0)))
        
        if quantity <= 0:
            return Response({'error': 'Quantity must be greater than 0'}, status=status.HTTP_400_BAD_REQUEST)
        
        stock_before = product.stock
        product.stock += quantity
        product.save()
        
        # Create restock record
        RestockRecord.objects.create(
            product=product,
            product_name=product.name,
            product_emoji=product.emoji,
            quantity_added=quantity,
            stock_before=stock_before,
            stock_after=product.stock,
            user=request.user,
            user_name=request.user.name or request.user.username
        )
        
        return Response({'stock': product.stock, 'quantity_added': quantity})


class CartViewSet(viewsets.ModelViewSet):
    """Shopping cart viewset."""
    serializer_class = CartItemSerializer
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """Get current user's cart items."""
        return CartItem.objects.filter(user=self.request.user)
    
    def create(self, request, *args, **kwargs):
        """Add item to cart."""
        product_id = request.data.get('product_id')
        quantity = Decimal(str(request.data.get('quantity', 1)))
        
        try:
            product = Product.objects.get(id=product_id)
        except Product.DoesNotExist:
            return Response({'error': 'Product not found'}, status=status.HTTP_404_NOT_FOUND)
        
        # Check stock availability
        cart_item, created = CartItem.objects.get_or_create(
            user=request.user,
            product=product,
            defaults={'quantity': quantity}
        )
        
        if not created:
            new_quantity = cart_item.quantity + quantity
            if new_quantity > product.stock:
                return Response({'error': 'Insufficient stock'}, status=status.HTTP_400_BAD_REQUEST)
            cart_item.quantity = new_quantity
            cart_item.save()
        
        serializer = self.get_serializer(cart_item)
        return Response(serializer.data, status=status.HTTP_201_CREATED)
    
    @action(detail=False, methods=['get'])
    def total(self, request):
        """Get cart total."""
        total = sum(item.item_total for item in self.get_queryset())
        return Response({'total': total})


class OrderViewSet(viewsets.ReadOnlyModelViewSet):
    """Order viewset (read-only, create via checkout)."""
    serializer_class = OrderSerializer
    permission_classes = [permissions.IsAuthenticated]
    
    def get_queryset(self):
        """Get orders based on user role."""
        if self.request.user.can_manage_stock:
            return Order.objects.all()
        return Order.objects.filter(user=self.request.user)
    
    @action(detail=False, methods=['post'])
    def checkout(self, request):
        """Checkout - create order from cart."""
        cart_items = CartItem.objects.filter(user=request.user)
        
        if not cart_items.exists():
            return Response({'error': 'Cart is empty'}, status=status.HTTP_400_BAD_REQUEST)
        
        # Calculate total and item count
        total = sum(item.item_total for item in cart_items)
        item_count = sum(int(item.quantity) for item in cart_items)
        
        # Create order
        order = Order.objects.create(
            user=request.user,
            user_name=request.user.name or request.user.username,
            total=total,
            item_count=item_count,
            status='Completed'
        )
        
        # Create order items and update inventory
        for cart_item in cart_items:
            OrderItem.objects.create(
                order=order,
                product=cart_item.product,
                product_name=cart_item.product.name,
                product_emoji=cart_item.product.emoji,
                price=cart_item.product.price,
                quantity=cart_item.quantity,
                is_sold_by_weight=cart_item.product.is_sold_by_weight
            )
            
            # Update product stock
            cart_item.product.stock -= cart_item.quantity
            cart_item.product.save()
        
        # Clear cart
        cart_items.delete()
        
        serializer = self.get_serializer(order)
        return Response(serializer.data, status=status.HTTP_201_CREATED)


class AttendanceRecordViewSet(viewsets.ModelViewSet):
    """Attendance record viewset."""
    serializer_class = AttendanceRecordSerializer
    permission_classes = [IsDeveloperAdminOrModerator]
    
    def get_queryset(self):
        """Get attendance records."""
        qs = AttendanceRecord.objects.all()
        user_id = self.request.query_params.get('user_id')
        if user_id:
            qs = qs.filter(user_id=user_id)
        return qs
    
    def create(self, request, *args, **kwargs):
        """Create attendance record with duplicate check."""
        import logging
        logger = logging.getLogger(__name__)
        
        logger.info(f"Attendance create called with data: {request.data}")
        user_id = request.data.get('user_id')
        date = request.data.get('date')
        
        # Check for duplicate FIRST, before serializer validation
        if user_id and date:
            try:
                existing = AttendanceRecord.objects.filter(user_id=user_id, date=date).first()
                if existing:
                    error_msg = f'Attendance record already exists for this employee on {date}'
                    logger.warning(f"Duplicate attendance record: {error_msg}")
                    # Use 409 Conflict for duplicate records (more semantically correct)
                    return Response({
                        'error': error_msg
                    }, status=status.HTTP_409_CONFLICT)
            except Exception as e:
                logger.error(f"Error checking duplicate: {e}")
                pass  # If query fails, let serializer handle it
        
        # Validate serializer
        serializer = self.get_serializer(data=request.data)
        if not serializer.is_valid():
            # Return validation errors in a consistent format
            errors = serializer.errors
            logger.warning(f"Serializer validation errors: {errors}")
            error_message = 'Validation error'
            if isinstance(errors, dict):
                # Get first error message
                for field, field_errors in errors.items():
                    if isinstance(field_errors, list) and len(field_errors) > 0:
                        error_message = f"{field}: {field_errors[0]}"
                        break
                    elif isinstance(field_errors, str):
                        error_message = f"{field}: {field_errors}"
                        break
            elif isinstance(errors, list) and len(errors) > 0:
                error_message = errors[0]
            
            logger.warning(f"Returning validation error: {error_message}")
            return Response({
                'error': error_message
            }, status=status.HTTP_400_BAD_REQUEST)
        
        # Try to create the record
        try:
            serializer.save()
            logger.info(f"Attendance record created successfully")
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        except IntegrityError as e:
            # Handle database constraint violations
            logger.error(f"IntegrityError: {e}")
            if 'unique' in str(e).lower() or 'duplicate' in str(e).lower():
                error_msg = f'Attendance record already exists for this employee on {date}'
                logger.warning(f"Duplicate detected via IntegrityError: {error_msg}")
                # Use 409 Conflict for duplicate records
                return Response({
                    'error': error_msg
                }, status=status.HTTP_409_CONFLICT)
            return Response({
                'error': 'Database error occurred while creating attendance record'
            }, status=status.HTTP_400_BAD_REQUEST)
        except Exception as e:
            logger.error(f"Exception creating attendance record: {e}", exc_info=True)
            return Response({
                'error': f'Error creating attendance record: {str(e)}'
            }, status=status.HTTP_400_BAD_REQUEST)
    
    @action(detail=False, methods=['get'])
    def monthly_summary(self, request):
        """Get monthly attendance summary."""
        month = request.query_params.get('month')
        if month:
            year, month = map(int, month.split('-'))
        else:
            now = timezone.now()
            year, month = now.year, now.month
        
        # Get all users
        users = User.objects.filter(is_enabled=True)
        summaries = []
        
        for user in users:
            records = AttendanceRecord.objects.filter(
                user=user,
                date__year=year,
                date__month=month
            )
            
            present_days = records.filter(is_present=True).count()
            absent_days = records.filter(is_present=False).count()
            total_hours = sum(record.total_hours for record in records)
            total_pay = sum(record.daily_pay for record in records)
            
            summaries.append({
                'user_id': user.id,
                'user_name': user.name or user.username,
                'present_days': present_days,
                'absent_days': absent_days,
                'total_hours': total_hours,
                'total_pay': total_pay
            })
        
        return Response(summaries)


class RestockRecordViewSet(viewsets.ReadOnlyModelViewSet):
    """Restock record viewset."""
    queryset = RestockRecord.objects.all()
    serializer_class = RestockRecordSerializer
    permission_classes = [IsDeveloperAdminOrModerator]


class ReportsView(APIView):
    """Reports and analytics endpoint."""
    permission_classes = [IsDeveloperAdminOrModerator]
    
    def get(self, request):
        """Get sales reports."""
        # Total sales
        total_sales = Order.objects.aggregate(total=Sum('total'))['total'] or Decimal('0.00')
        
        # Total orders
        total_orders = Order.objects.count()
        
        # Average order value
        avg_order_value = total_sales / total_orders if total_orders > 0 else Decimal('0.00')
        
        # Total items sold
        total_items_sold = OrderItem.objects.aggregate(total=Sum('quantity'))['total'] or Decimal('0.00')
        
        # Last 7 days sales
        seven_days_ago = timezone.now() - timedelta(days=7)
        last_7_days_sales = Order.objects.filter(
            order_date__gte=seven_days_ago
        ).aggregate(total=Sum('total'))['total'] or Decimal('0.00')
        
        # Top products
        top_products = OrderItem.objects.values(
            'product_name', 'product_emoji'
        ).annotate(
            total_sold=Sum('quantity')
        ).order_by('-total_sold')[:10]
        
        return Response({
            'total_sales': total_sales,
            'total_orders': total_orders,
            'average_order_value': avg_order_value,
            'total_items_sold': total_items_sold,
            'last_7_days_sales': last_7_days_sales,
            'top_products': list(top_products)
        })

