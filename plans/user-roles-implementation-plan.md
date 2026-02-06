# User Roles Implementation Plan

## Overview

Implement a role-based access control (RBAC) system in the Django backend, matching the C# project's role model.

### C# Role System (Source)
- **Developer**: Full access, can manage users
- **Admin**: Can manage users, products, attendance, restock
- **Moderator**: Can manage stock, attendance, restock (NOT users)
- **User**: Can only sell (use shop interface)

---

## Implementation Plan

### Phase 1: Backend - Models & Constants

#### 1.1 Create Role Constants (`backend/api/constants.py`)
```python
# Role constants matching C# project
ROLE_DEVELOPER = 'Developer'
ROLE_ADMIN = 'Admin'
ROLE_MODERATOR = 'Moderator'
ROLE_USER = 'User'

ROLE_CHOICES = [
    (ROLE_DEVELOPER, 'Developer'),
    (ROLE_ADMIN, 'Admin'),
    (ROLE_MODERATOR, 'Moderator'),
    (ROLE_USER, 'User'),
]

ALL_ROLES = [ROLE_DEVELOPER, ROLE_ADMIN, ROLE_MODERATOR, ROLE_USER]
```

#### 1.2 Create UserProfile Model (`backend/api/models.py`)
```python
class UserProfile(models.Model):
    """Extended user profile with role support"""
    user = models.OneToOneField(
        User,
        on_delete=models.CASCADE,
        related_name='profile'
    )
    role = models.CharField(
        max_length=20,
        choices=ROLE_CHOICES,
        default=ROLE_USER
    )
    # Additional fields from C# User model
    monthly_salary = models.DecimalField(max_digits=10, decimal_places=2, default=0)
    overtime_multiplier = models.DecimalField(max_digits=4, decimal_places=2, default=1.5)
    
    class Meta:
        db_table = 'api_userprofile'
```

#### 1.3 Add Role Properties to User Model
Add computed properties to User model via monkey-patch or custom manager:
- `is_developer` → role == 'Developer'
- `is_admin` → role == 'Admin'
- `is_moderator` → role == 'Moderator'
- `is_user` → role == 'User'

---

### Phase 2: Backend - Permissions

#### 2.1 Create Permission Classes (`backend/api/permissions.py`)
```python
class IsDeveloperOrReadOnly(permissions.BasePermission):
    """Allow read for all, write only for Developer"""
    def has_permission(self, request, view):
        if request.method in permissions.SAFE_METHODS:
            return True
        return request.user.is_authenticated and getattr(request.user, 'role', None) == ROLE_DEVELOPER

class IsAdminOrReadOnly(permissions.BasePermission):
    """Allow read for all, write only for Admin"""
    def has_permission(self, request, view):
        if request.method in permissions.SAFE_METHODS:
            return True
        return request.user.is_authenticated and getattr(request.user, 'role', None) == ROLE_ADMIN

class CanManageUsers(permissions.BasePermission):
    """Developer and Admin can manage users"""
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = getattr(request.user, 'role', None)
        return role in [ROLE_DEVELOPER, ROLE_ADMIN]

class CanManageStock(permissions.BasePermission):
    """Developer, Admin, and Moderator can manage stock"""
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = getattr(request.user, 'role', None)
        return role in [ROLE_DEVELOPER, ROLE_ADMIN, ROLE_MODERATOR]

class CanRestock(permissions.BasePermission):
    """Developer, Admin, and Moderator can restock"""
    def has_permission(self, request, view):
        if not request.user.is_authenticated:
            return False
        role = getattr(request.user, 'role', None)
        return role in [ROLE_DEVELOPER, ROLE_ADMIN, ROLE_MODERATOR]
```

---

### Phase 3: Backend - Serializers

#### 3.1 Create UserProfileSerializer (`backend/api/serializers.py`)
```python
class UserProfileSerializer(serializers.ModelSerializer):
    """Serializer for UserProfile model"""
    user_id = serializers.IntegerField(source='user.id', read_only=True)
    username = serializers.CharField(source='user.username', read_only=True)
    
    class Meta:
        model = UserProfile
        fields = ['id', 'user_id', 'username', 'role', 'monthly_salary', 'overtime_multiplier']
```

#### 3.2 Update UserViewSet to use role-based permissions
- Add endpoints for user management (list, create, update, delete)
- Restrict user management to Developer and Admin roles

---

### Phase 4: Backend - Views Update

#### 4.1 Update RestockViewSet
```python
class RestockViewSet(viewsets.ModelViewSet):
    permission_classes = [CanRestock]  # Replace IsStaffOrReadOnly
```

#### 4.2 Update ProductViewSet
Add `CanManageStock` permission for create/update/delete actions

#### 4.3 Update UserViewSet
- Add user management endpoints
- `permissions` endpoint returns full role-based permissions:
  ```python
  {
      'is_authenticated': True,
      'role': 'Admin',
      'permissions': {
          'can_manage_users': True,  # Developer, Admin
          'can_manage_stock': True,  # Developer, Admin, Moderator
          'can_use_attendance': True,  # Developer, Admin, Moderator
          'can_restock': True,  # Developer, Admin, Moderator
      }
  }
  ```

---

### Phase 5: Database Migration

1. Create migration for UserProfile model
2. Create migration to populate UserProfile for existing users:
   - Superusers → Developer role
   - Staff users (non-superuser) → Admin role
   - Regular users → User role

---

### Phase 6: Admin Interface

#### 6.1 Register UserProfile in admin.py
```python
@admin.register(UserProfile)
class UserProfileAdmin(admin.ModelAdmin):
    list_display = ['user', 'role', 'monthly_salary']
    list_filter = ['role']
```

---

## Files to Modify/Create

| File | Action | Description |
|------|--------|-------------|
| `backend/api/constants.py` | Create | Role constants |
| `backend/api/models.py` | Modify | Add UserProfile model |
| `backend/api/permissions.py` | Create | Custom permission classes |
| `backend/api/serializers.py` | Modify | Add UserProfileSerializer |
| `backend/api/views.py` | Modify | Update permissions and add user management |
| `backend/api/urls.py` | Modify | Add user management endpoints |
| `backend/api/admin.py` | Modify | Register UserProfile in admin |

---

## Testing Checklist

- [ ] Developer can access all endpoints
- [ ] Admin can manage users but not see Developer-only features
- [ ] Moderator can restock and manage stock but not users
- [ ] User can only create sales
- [ ] Permissions endpoint returns correct role and permissions
- [ ] Existing users retain access after migration
