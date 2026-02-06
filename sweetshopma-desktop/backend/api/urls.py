"""
URL configuration for API app.
"""

from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import (
    CategoryViewSet,
    ProductViewSet,
    CustomerViewSet,
    SaleViewSet,
    ExpenseViewSet,
    DashboardViewSet,
    RestockViewSet,
    UserViewSet,
    AttendanceRecordViewSet,
    AttendanceSummaryViewSet,
    AttendanceExpenseViewSet,
)

# Create router and register viewsets
router = DefaultRouter()
router.register(r'categories', CategoryViewSet, basename='category')
router.register(r'products', ProductViewSet, basename='product')
router.register(r'customers', CustomerViewSet, basename='customer')
router.register(r'sales', SaleViewSet, basename='sale')
router.register(r'expenses', ExpenseViewSet, basename='expense')
router.register(r'dashboard', DashboardViewSet, basename='dashboard')
router.register(r'restocks', RestockViewSet, basename='restock')
router.register(r'user', UserViewSet, basename='user')
router.register(r'attendance', AttendanceRecordViewSet, basename='attendance')
router.register(r'attendance-summary', AttendanceSummaryViewSet, basename='attendance-summary')
router.register(r'attendance-expenses', AttendanceExpenseViewSet, basename='attendance-expense')

urlpatterns = [
    path('', include(router.urls)),
]
