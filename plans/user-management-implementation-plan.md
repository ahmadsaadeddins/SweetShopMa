# User Management Implementation Plan

## Overview
Create a comprehensive User Management page for SweetShopMa Desktop that allows administrators to:
- Add new users
- Remove users
- Update user information (role, salary, etc.)
- View all users in a list
- **Track salary changes over time** ⭐
- **Track user activity (logins, actions)** ⭐⭐ NEW

---

## Current State Analysis

### Backend (Django) - Needs Model Extensions
- ✅ [`UserProfile`](sweetshopma-desktop/backend/api/models.py:442) model exists with `role`, `monthly_salary`, `overtime_multiplier` fields
- ✅ [`UserViewSet`](sweetshopma-desktop/backend/api/views.py:621) provides CRUD operations
- ⚠️ Need to add **`SalaryHistory`** model for tracking salary changes
- ⚠️ Need to add **`UserActivityLog`** model for tracking user activities

---

## ⭐ FEATURE 1: Salary History Tracking

### Model: SalaryHistory
```python
class SalaryHistory(models.Model):
    """
    Audit trail for salary changes.
    Tracks when, why, and by whom salaries were changed.
    """
    user_profile = models.ForeignKey(
        UserProfile,
        on_delete=models.CASCADE,
        related_name='salary_history'
    )
    
    # Salary values at time of change
    old_salary = models.DecimalField(max_digits=10, decimal_places=2)
    new_salary = models.DecimalField(max_digits=10, decimal_places=2)
    old_overtime_multiplier = models.DecimalField(max_digits=4, decimal_places=2)
    new_overtime_multiplier = models.DecimalField(max_digits=4, decimal_places=2)
    
    # Change metadata
    change_reason = models.CharField(max_length=200, blank=True)
    changed_by = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        related_name='salary_changes_made'
    )
    changed_by_name = models.CharField(max_length=100)
    
    # Timestamps
    changed_at = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        db_table = 'api_salary_history'
        ordering = ['-changed_at']
```

---

## ⭐⭐ FEATURE 2: User Activity Logging

### Model: UserActivityLog
```python
class UserActivityLog(models.Model):
    """
    Audit trail for user activities.
    Tracks logins, actions, and important events.
    """
    
    ACTIVITY_TYPES = [
        ('login', 'User Login'),
        ('logout', 'User Logout'),
        ('create', 'Create Record'),
        ('update', 'Update Record'),
        ('delete', 'Delete Record'),
        ('sale', 'Create Sale'),
        ('restock', 'Restock Product'),
        ('attendance', 'Attendance Action'),
        ('expense', 'Expense Action'),
        ('password_change', 'Password Change'),
        ('role_change', 'Role Change'),
        ('salary_change', 'Salary Change'),
        ('other', 'Other'),
    ]
    
    # User who performed the action
    user = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        related_name='activity_logs'
    )
    user_name = models.CharField(max_length=100)
    
    # Activity details
    activity_type = models.CharField(max_length=30, choices=ACTIVITY_TYPES)
    description = models.TextField()
    
    # Optional reference to related object
    resource_type = models.CharField(max_length=50, blank=True)  # e.g., 'Sale', 'Product'
    resource_id = models.IntegerField(null=True, blank=True)
    
    # Metadata
    ip_address = models.GenericIPAddressField(null=True, blank=True)
    user_agent = models.TextField(blank=True)
    
    # Timestamps
    timestamp = models.DateTimeField(auto_now_add=True)
    
    class Meta:
        db_table = 'api_user_activity_log'
        ordering = ['-timestamp']
        indexes = [
            models.Index(fields=['user', 'timestamp']),
            models.Index(fields=['activity_type', 'timestamp']),
            models.Index(fields=['resource_type', 'resource_id']),
        ]
```

### Auto-logging Middleware
Create middleware to automatically log activities:
- User login/logout
- CRUD operations on major models
- API calls for sensitive operations

---

## Implementation Tasks

### Phase 0: Backend - Database Models (NEW)
**File:** [`sweetshopma-desktop/backend/api/models.py`](sweetshopma-desktop/backend/api/models.py)

- [ ] Add `SalaryHistory` model
- [ ] Add `UserActivityLog` model
- [ ] Create migrations

### Phase 1: Backend - Serializers & Views
**Files:** [`serializers.py`](sweetshopma-desktop/backend/api/serializers.py), [`views.py`](sweetshopma-desktop/backend/api/views.py)

- [ ] Add `SalaryHistorySerializer`
- [ ] Add `UserActivityLogSerializer`
- [ ] Add salary history endpoints to `UserViewSet`
- [ ] Add activity log endpoints to `UserViewSet`
- [ ] Update `UserProfileUpdateSerializer` to create history record

### Phase 2: Backend Activity Middleware
**File:** `sweetshopma-desktop/backend/middleware/activity_log.py` (NEW)

- [ ] Create middleware to auto-log user activities
- [ ] Log login/logout events
- [ ] Log CRUD operations

### Phase 3: Backend API Bridge (Python)
**File:** [`sweetshopma-desktop/frontend/api.py`](sweetshopma-desktop/frontend/api.py)

- [ ] Add `get_users()` - List all users
- [ ] Add `get_user(user_id)` - Get single user
- [ ] Add `create_user(data)` - Create new user
- [ ] Add `update_user(user_id, data)` - Update user
- [ ] Add `delete_user(user_id)` - Delete user
- [ ] Add `get_user_salary_history(user_id)` - Get salary history
- [ ] Add `get_user_activities(user_id)` - Get user activities
- [ ] Add `get_all_activities()` - Get all activities

### Phase 4: API Service (JavaScript)
**File:** [`sweetshopma-desktop/frontend/src/services/apiService.js`](sweetshopma-desktop/frontend/src/services/apiService.js)

- [ ] Add API endpoint constants for users
- [ ] Add user service methods to `useApi()` hook

### Phase 5: Custom Hooks
**File:** [`sweetshopma-desktop/frontend/src/hooks/useApiData.js`](sweetshopma-desktop/frontend/src/hooks/useApiData.js)

- [ ] Add `useUsers()` - Hook for users list
- [ ] Add `useUser(id)` - Hook for single user
- [ ] Add `useUserSalaryHistory(userId)` - Hook for salary history
- [ ] Add `useUserActivities(userId)` - Hook for user activities

### Phase 6: User Page Component
**File:** [`sweetshopma-desktop/frontend/src/pages/UsersPage.jsx`](sweetshopma-desktop/frontend/src/pages/UsersPage.jsx)

#### Tab 1: User List
- [ ] User list table with search/filter
- [ ] Add/Edit/Delete user modals
- [ ] View Salary History button

#### Tab 2: User Activity Log
- [ ] Activity log table with filters (user, date range, activity type)
- [ ] Show who did what and when
- [ ] Export activity log

#### Tab 3: Salary History
- [ ] Salary change history for all users
- [ ] Filter by user, date range

### Phase 7: Navigation & Routing
**Files:** [`App.jsx`](sweetshopma-desktop/frontend/src/App.jsx), [`Sidebar.jsx`](sweetshopma-desktop/frontend/src/components/layout/Sidebar.jsx)

- [ ] Add `/users` route with tabs (Users, Activity Log, Salary History)
- [ ] Add "Users" menu item to Sidebar
- [ ] Add "Activity Log" menu item to Sidebar

---

## UI Components

### UsersPage with Tabs
```
┌─────────────────────────────────────────────────────────────────────┐
│  Users Management                                              [+ Add User] │
├─────────────────────────────────────────────────────────────────────┤
│  [ Users ]  [ Activity Log ]  [ Salary History ]                     │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐     │
│  │ Username  │ Role      │ Salary    │ Status  │ Actions    │     │
│  ├─────────────────────────────────────────────────────────────┤     │
│  │ admin     │ Developer │ $10,000  │ Active  │ [Edit][📜] │     │
│  │ john_doe  │ Seller    │ $5,000   │ Active  │ [Edit][📜] │     │
│  └─────────────────────────────────────────────────────────────┘     │
├─────────────────────────────────────────────────────────────────────┤
│  Showing 2 of 2 users                                               │
└─────────────────────────────────────────────────────────────────────┘
```

### Activity Log Tab
```
┌─────────────────────────────────────────────────────────────────────┐
│  User Activity Log                                              [+ Export] │
├─────────────────────────────────────────────────────────────────────┤
│  Filter: [User ▼] [Activity Type ▼] [Date Range ▼]                  │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐     │
│  │ Timestamp           │ User        │ Activity     │ Details │     │
│  ├─────────────────────────────────────────────────────────────┤     │
│  │ 2024-02-05 10:30   │ admin       │ Login        │ -       │     │
│  │ 2024-02-05 10:31   │ admin       │ Salary Change│ j_doe ↑ │     │
│  │ 2024-02-05 09:15   │ john_doe    │ Login        │ -       │     │
│  │ 2024-02-04 18:00   │ john_doe    │ Logout       │ -       │     │
│  │ 2024-02-04 14:22   │ admin       │ Create Sale  │ #1234   │     │
│  └─────────────────────────────────────────────────────────────┘     │
├─────────────────────────────────────────────────────────────────────┤
│  Showing 5 of 156 activities                                        │
└─────────────────────────────────────────────────────────────────────┘
```

### Salary History Tab
```
┌─────────────────────────────────────────────────────────────────────┐
│  Salary History                                              [+ Export] │
├─────────────────────────────────────────────────────────────────────┤
│  Filter: [User ▼] [Date Range ▼]                                    │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐     │
│  │ Date        │ User        │ Change              │ By       │     │
│  ├─────────────────────────────────────────────────────────────┤     │
│  │ 2024-02-05  │ john_doe    │ $4,500 → $5,000    │ admin    │     │
│  │             │             │ Reason: Annual raise           │     │
│  ├─────────────────────────────────────────────────────────────┤     │
│  │ 2024-01-15  │ john_doe    │ (Initial) $5,000   │ System   │     │
│  ├─────────────────────────────────────────────────────────────┤     │
│  │ 2024-02-01  │ jane        │ $7,000 → $8,000    │ admin    │     │
│  │             │             │ Reason: Promotion              │     │
│  └─────────────────────────────────────────────────────────────┘     │
├─────────────────────────────────────────────────────────────────────┤
│  Showing 3 of 15 salary changes                                    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## User Activity Types
| Activity | Description | Auto-logged |
|----------|-------------|-------------|
| login | User logged in | ✅ Yes |
| logout | User logged out | ✅ Yes |
| create | Created a record | ✅ Yes |
| update | Updated a record | ✅ Yes |
| delete | Deleted a record | ✅ Yes |
| sale | Created a sale | ✅ Yes |
| restock | Restocked a product | ✅ Yes |
| attendance | Attendance action | ✅ Yes |
| expense | Expense action | ✅ Yes |
| password_change | Changed password | ✅ Yes |
| role_change | Changed user role | ✅ Yes |
| salary_change | Changed user salary | ✅ Yes |
| other | Other actions | Manual |

---

## User Roles Reference
From [`constants.py`](sweetshopma-desktop/backend/api/constants.py:15):
| Role | Can Manage Users | Can View Logs | Description |
|------|-----------------|---------------|-------------|
| Developer | ✅ Yes | ✅ Yes | Full access to all features |
| Admin | ✅ Yes | ✅ Yes | Can manage users, products, attendance |
| Moderator | ❌ No | ✅ Yes | Can view logs, manage stock |
| Employee | ❌ No | ❌ No | Can only track attendance |
| Seller | ❌ No | ❌ No | Can only use POS interface |

---

## Permission Requirements
| Page/Action | Developer | Admin | Moderator | Employee | Seller |
|-------------|-----------|-------|-----------|----------|--------|
| Users List | ✅ | ✅ | ❌ | ❌ | ❌ |
| Add User | ✅ | ✅ | ❌ | ❌ | ❌ |
| Edit User | ✅ | ✅ | ❌ | ❌ | ❌ |
| Delete User | ✅ | ✅ | ❌ | ❌ | ❌ |
| Salary History | ✅ | ✅ | ✅ | ❌ | ❌ |
| Activity Log | ✅ | ✅ | ✅ | ❌ | ❌ |

---

## Files to Create/Modify

### New Files
1. `sweetshopma-desktop/frontend/src/pages/UsersPage.jsx` - Main user management page
2. `sweetshopma-desktop/backend/middleware/activity_log.py` - Activity logging middleware

### Modified Files (Backend)
1. `sweetshopma-desktop/backend/api/models.py` - Add SalaryHistory, UserActivityLog models
2. `sweetshopma-desktop/backend/api/serializers.py` - Add serializers
3. `sweetshopma-desktop/backend/api/views.py` - Add endpoints
4. `sweetshopma-desktop/backend/sweetshop/settings.py` - Add middleware

### Modified Files (Frontend)
1. `sweetshopma-desktop/frontend/api.py` - Add user API methods
2. `sweetshopma-desktop/frontend/src/services/apiService.js` - Add user endpoints
3. `sweetshopma-desktop/frontend/src/hooks/useApiData.js` - Add user hooks
4. `sweetshopma-desktop/frontend/src/App.jsx` - Add route
5. `sweetshopma-desktop/frontend/src/components/layout/Sidebar.jsx` - Add menu items

---

## Testing Checklist
### User Management
- [ ] User list displays correctly
- [ ] Add user creates new user
- [ ] Edit user updates information
- [ ] Delete user removes user
- [ ] Permission check prevents unauthorized access

### Salary History
- [ ] Salary change creates history record
- [ ] Salary history modal shows correct data
- [ ] Filter by user works

### Activity Log
- [ ] Login/logout events are logged
- [ ] CRUD operations are logged
- [ ] Activity log table displays correctly
- [ ] Filters work (user, date, activity type)
- [ ] Permission check works (Moderator can view but not manage users)

---

## Implementation Order
1. Create database models (SalaryHistory, UserActivityLog)
2. Add serializers and endpoints
3. Create activity logging middleware
4. Add API bridge methods
5. Add API service methods
6. Add custom hooks
7. Create UsersPage component with tabs
8. Add navigation and routes
9. Test in browser
10. Build for PyWebView
11. Test in PyWebView

---

## Estimated Complexity
| Feature | Complexity |
|---------|------------|
| Backend Models | 2-3 hours |
| Backend API | 2 hours |
| Activity Middleware | 2-3 hours |
| Frontend UsersPage | 8-10 hours |
| Frontend Activity Log | 4-6 hours |
| Testing | 3-4 hours |
| **Total** | **21-28 hours** |
