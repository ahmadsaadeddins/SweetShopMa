"""
Custom permission classes for role-based access control.

These permissions complement Django's built-in permissions and implement
the role-based access control (RBAC) system matching the C# project.
"""

from rest_framework import permissions
from .constants import (
    ROLE_DEVELOPER,
    ROLE_ADMIN,
    ROLE_MODERATOR,
    ROLE_EMPLOYEE,
    ROLE_SELLER,
    get_role_permissions,
)


def get_user_role(user):
    """Get the role of a user from their profile."""
    if not user.is_authenticated:
        return None
    # Try to get role from profile first
    if hasattr(user, 'profile') and user.profile:
        return getattr(user.profile, 'role', ROLE_SELLER)
    # Fallback: use is_staff and is_superuser to determine role
    if user.is_superuser:
        return ROLE_DEVELOPER
    if user.is_staff:
        return ROLE_ADMIN
    return ROLE_SELLER


class IsDeveloper(permissions.BasePermission):
    """Only Developer role can access."""
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        return get_user_role(request.user) == ROLE_DEVELOPER


class IsDeveloperOrReadOnly(permissions.BasePermission):
    """Allow read for all, write only for Developer."""
    def has_permission(self, request, view):
        if request.method in permissions.SAFE_METHODS:
            return True
        if not request.user.is_authenticated:
            return False
        return get_user_role(request.user) == ROLE_DEVELOPER


class IsAdmin(permissions.BasePermission):
    """Only Admin role can access."""
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        return role in [ROLE_DEVELOPER, ROLE_ADMIN]


class IsAdminOrReadOnly(permissions.BasePermission):
    """Allow read for all, write only for Admin."""
    def has_permission(self, request, view):
        if request.method in permissions.SAFE_METHODS:
            return True
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        return role in [ROLE_DEVELOPER, ROLE_ADMIN]


class IsAdminOrDeveloper(permissions.BasePermission):
    """Allow access for Admin and Developer roles."""
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        return role in [ROLE_DEVELOPER, ROLE_ADMIN]


class CanManageUsers(permissions.BasePermission):
    """
    Permission check for user management.
    Developer and Admin can manage users.
    """
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        permissions = get_role_permissions(role)
        return permissions.get('can_manage_users', False)

    def has_object_permission(self, request, view, obj):
        return self.has_permission(request, view)


class CanViewEmployees(permissions.BasePermission):
    """
    Permission check for viewing employees list.
    Developer, Admin, Moderator, and Employee can view employees for attendance tracking.
    """
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        permissions = get_role_permissions(role)
        return permissions.get('can_view_employees', False)

    def has_object_permission(self, request, view, obj):
        return self.has_permission(request, view)


class CanManageStock(permissions.BasePermission):
    """
    Permission check for stock management.
    Developer, Admin, and Moderator can manage stock.
    """
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        permissions = get_role_permissions(role)
        return permissions.get('can_manage_stock', False)


class CanRestock(permissions.BasePermission):
    """
    Permission check for restock operations.
    Developer, Admin, and Moderator can restock.
    """
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        permissions = get_role_permissions(role)
        return permissions.get('can_restock', False)


class CanUseAttendance(permissions.BasePermission):
    """
    Permission check for attendance tracker.
    Developer, Admin, and Moderator can use attendance.
    """
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        permissions = get_role_permissions(role)
        return permissions.get('can_use_attendance', False)


class CanManageSettings(permissions.BasePermission):
    """
    Permission check for settings management.
    Only Developer and Admin can manage settings.
    """
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        permissions = get_role_permissions(role)
        return permissions.get('can_manage_settings', False)


class CanViewAllReports(permissions.BasePermission):
    """
    Permission check for viewing all reports.
    Only Developer and Admin can view all reports.
    """
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        permissions = get_role_permissions(role)
        return permissions.get('can_view_all_reports', False)


class IsStaffOrReadOnly(permissions.BasePermission):
    """
    Legacy permission class for backward compatibility.
    Only staff users (Admin, Moderator, Developer) can write.
    """
    def has_permission(self, request, view):
        if request.method in permissions.SAFE_METHODS:
            return True
        if not request.user.is_authenticated:
            return False
        role = get_user_role(request.user)
        return role in [ROLE_DEVELOPER, ROLE_ADMIN, ROLE_MODERATOR]


class IsAuthenticated(permissions.BasePermission):
    """Only authenticated users can access."""
    def has_permission(self, request, view):
        return request.user and request.user.is_authenticated


class IsAuthenticatedOrReadOnly(permissions.BasePermission):
    """Allow read for all, write only for authenticated users."""
    def has_permission(self, request, view):
        if request.method in permissions.SAFE_METHODS:
            return True
        return request.user and request.user.is_authenticated


def get_user_permissions_dict(user):
    """
    Get a dictionary of all permissions for a user.
    Useful for returning in API responses.
    """
    if not user.is_authenticated:
        return {
            'is_authenticated': False,
            'role': None,
            'permissions': {
                'can_manage_users': False,
                'can_manage_stock': False,
                'can_use_attendance': False,
                'can_restock': False,
                'can_manage_settings': False,
                'can_view_all_reports': False,
            }
        }

    role = get_user_role(user)
    permissions = get_role_permissions(role)

    return {
        'is_authenticated': True,
        'is_superuser': user.is_superuser,
        'is_staff': user.is_staff,
        'role': role,
        'permissions': permissions,
        'username': user.username,
        'full_name': user.get_full_name() or user.username,
    }
