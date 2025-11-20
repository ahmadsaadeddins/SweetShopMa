from django.urls import path, include
from rest_framework.routers import DefaultRouter
from rest_framework_simplejwt.views import TokenRefreshView
from .views import (
    AuthView, UserViewSet, ProductViewSet, CartViewSet, OrderViewSet,
    AttendanceRecordViewSet, RestockRecordViewSet, ReportsView
)

router = DefaultRouter()
router.register(r'users', UserViewSet, basename='user')
router.register(r'products', ProductViewSet, basename='product')
router.register(r'cart', CartViewSet, basename='cart')
router.register(r'orders', OrderViewSet, basename='order')
router.register(r'attendance', AttendanceRecordViewSet, basename='attendance')
router.register(r'restock-records', RestockRecordViewSet, basename='restock-record')

urlpatterns = [
    path('auth/login/', AuthView.as_view(), name='login'),
    path('auth/refresh/', TokenRefreshView.as_view(), name='token_refresh'),
    path('auth/me/', AuthView.as_view(), name='me'),  # Will need custom view
    path('reports/', ReportsView.as_view(), name='reports'),
    path('', include(router.urls)),
]

