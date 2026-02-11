"""
Role constants for SweetShopMa Desktop Application.

Role-based access control (RBAC) system matching the C# project implementation.

Roles Hierarchy:
1. Developer: Full access, can manage users
2. Admin: Can manage users, products, attendance, restock
3. Moderator: Can manage stock, attendance, restock (NOT users)
4. Employee: Can only use attendance tracker (NOT POS, NOT management)
5. Seller: Can only sell (use POS interface) - nothing else
"""

# Role constants
ROLE_DEVELOPER = 'Developer'
ROLE_ADMIN = 'Admin'
ROLE_MODERATOR = 'Moderator'
ROLE_EMPLOYEE = 'Employee'
ROLE_SELLER = 'Seller'

# Role choices for model fields
ROLE_CHOICES = [
    (ROLE_DEVELOPER, 'Developer'),
    (ROLE_ADMIN, 'Admin'),
    (ROLE_MODERATOR, 'Moderator'),
    (ROLE_EMPLOYEE, 'Employee'),
    (ROLE_SELLER, 'Seller'),
]

# List of all roles
ALL_ROLES = [ROLE_DEVELOPER, ROLE_ADMIN, ROLE_MODERATOR, ROLE_EMPLOYEE, ROLE_SELLER]

# Role permissions mapping
ROLE_PERMISSIONS = {
    ROLE_DEVELOPER: {
        'can_manage_users': True,
        'can_view_employees': True,
        'can_manage_stock': True,
        'can_use_attendance': True,
        'can_restock': True,
        'can_manage_settings': True,
        'can_view_all_reports': True,
        'can_sell': True,
    },
    ROLE_ADMIN: {
        'can_manage_users': True,
        'can_view_employees': True,
        'can_manage_stock': True,
        'can_use_attendance': True,
        'can_restock': True,
        'can_manage_settings': True,
        'can_view_all_reports': True,
        'can_sell': True,
    },
    ROLE_MODERATOR: {
        'can_manage_users': False,
        'can_view_employees': True,  # Can view employees for attendance tracking
        'can_manage_stock': True,
        'can_use_attendance': True,
        'can_restock': True,
        'can_manage_settings': True,
        'can_view_all_reports': False,
        'can_sell': True,
    },
    ROLE_SELLER: {
        'can_manage_users': False,
        'can_manage_stock': False,
        'can_use_attendance': False,
        'can_restock': False,
        'can_manage_settings': False,
        'can_view_all_reports': False,
        'can_sell': True,  # Seller can ONLY sell
    },
    ROLE_EMPLOYEE: {
        'can_manage_users': False,
        'can_view_employees': True,  # Can view employees for attendance tracking
        'can_manage_stock': False,
        'can_use_attendance': True,  # Employee can ONLY track attendance
        'can_restock': False,
        'can_manage_settings': False,
        'can_view_all_reports': False,
        'can_sell': False,
    },
}


def get_role_permissions(role):
    """Get permissions dictionary for a given role."""
    return ROLE_PERMISSIONS.get(role, ROLE_PERMISSIONS[ROLE_SELLER])


def has_permission(role, permission):
    """Check if a role has a specific permission."""
    permissions = get_role_permissions(role)
    return permissions.get(permission, False)


def can_manage_users(role):
    """Check if role can manage users."""
    return has_permission(role, 'can_manage_users')


def can_manage_stock(role):
    """Check if role can manage stock."""
    return has_permission(role, 'can_manage_stock')


def can_use_attendance(role):
    """Check if role can use attendance tracker."""
    return has_permission(role, 'can_use_attendance')


def can_restock(role):
    """Check if role can restock products."""
    return has_permission(role, 'can_restock')


def can_manage_settings(role):
    """Check if role can manage settings."""
    return has_permission(role, 'can_manage_settings')


def can_view_all_reports(role):
    """Check if role can view all reports."""
    return has_permission(role, 'can_view_all_reports')


def can_sell(role):
    """Check if role can use POS/sales functionality."""
    return has_permission(role, 'can_sell')
