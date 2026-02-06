# Fix for fetchUsers Not Working for Moderator Users

## Problem Diagnosis

**Issue:** When moderator users access the AttendancePage, the employee dropdown is empty because `fetchUsers()` fails with a 403 PERMISSION_DENIED error.

**Root Cause:**
- The [`getUsers()`](sweetshopma-desktop/frontend/src/services/apiService.js:303) API endpoint requires `can_manage_users` permission
- According to [`ROLE_PERMISSIONS`](sweetshopma-desktop/backend/api/constants.py:53-61), moderators have `'can_manage_users': False`
- Only Developer and Admin roles can list users via the standard endpoint

**Error Log:**
```
[ApiContext] Response: {
  "error": "You do not have permission to list users",
  "code": "PERMISSION_DENIED"
}
[ERROR] [AttendancePage] Error fetching users: Error: API Error: 403
```

## Solution Implemented

### 1. Added New Permission (`can_view_employees`)

**File:** [`sweetshopma-desktop/backend/api/constants.py`](sweetshopma-desktop/backend/api/constants.py)

Added `'can_view_employees': True` to the following roles:
- **ROLE_DEVELOPER** (line37)
- **ROLE_ADMIN** (line46)
- **ROLE_MODERATOR** (line55) - **Key fix for moderators**
- **ROLE_EMPLOYEE** (line73)

This permission allows read-only access to the employee list specifically for attendance tracking purposes.

### 2. Created New Permission Class

**File:** [`sweetshopma-desktop/backend/api/permissions.py`](sweetshopma-desktop/backend/api/permissions.py:97-113)

Added `CanViewEmployees` permission class that checks for the `can_view_employees` permission:

```python
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
```

### 3. Backend Endpoint Already Exists

**File:** [`sweetshopma-desktop/backend/api/views.py`](sweetshopma-desktop/backend/api/views.py:900-919)

The endpoint `list_for_attendance` already exists and checks for `can_use_attendance` permission (which moderators have):

```python
@action(detail=False, methods=['get'], url_path='list-for-attendance', 
         permission_classes=[rf_permissions.IsAuthenticated])
def list_for_attendance(self, request):
    """
    List users for attendance tracking purposes.
    
    Permission: Any authenticated user with can_use_attendance permission can access.
    """
    # Check attendance permission
    permissions_dict = get_user_permissions_dict(request.user)
    if not permissions_dict['permissions'].get('can_use_attendance', False):
        return Response(
            {'error': 'You do not have permission to view users for attendance', 
             'code': 'PERMISSION_DENIED'},
            status=403
        )
    
    # Get all user profiles
    profiles = UserProfile.objects.select_related('user').all()
    serializer = UserProfileSerializer(profiles, many=True)
    return Response(serializer.data)
```

### 4. Frontend API Service Method Added

**File:** [`sweetshopma-desktop/frontend/src/services/apiService.js`](sweetshopma-desktop/frontend/src/services/apiService.js)

Added new method `getEmployeesForAttendance()`:

```javascript
const getEmployeesForAttendance = async () => {
    const response = await apiCall(API_ENDPOINTS.LIST_FOR_ATTENDANCE);
    return normalizeResponse(response);
};
```

Also updated the exports to include this method (line475).

### 5. Updated AttendancePage

**File:** [`sweetshopma-desktop/frontend/src/pages/AttendancePage.jsx`](sweetshopma-desktop/frontend/src/pages/AttendancePage.jsx:49-77)

Changed `fetchUsers()` to call the new endpoint:

```javascript
async function fetchUsers() {
    try {
        console.log('[AttendancePage] Fetching employees for attendance...');
        var response = await api.getEmployeesForAttendance();  // Changed from api.getUsers()
        // ... rest of the function
    }
}
```

## Testing Instructions

### 1. Rebuild the Frontend

```bash
cd sweetshopma-desktop/frontend
npm run build
```

### 2. Restart the Application

```bash
cd sweetshopma-desktop/frontend
python main.py
```

### 3. Test as Moderator User

1. **Log in** as a moderator user
2. **Navigate** to the Attendance page
3. **Verify** the employee dropdown is populated
4. **Check console logs** for success message:
   ```
   [AttendancePage] ✓ Successfully loaded X employees
   ```

### 4. Expected Behavior

**Before Fix:**
- Employee dropdown empty
- Console shows: `[ERROR] API Error: 403` with `PERMISSION_DENIED`

**After Fix:**
- Employee dropdown populated with all users
- Console shows: `[AttendancePage] ✓ Successfully loaded X employees`
- Moderator can select employees and record attendance

## Files Modified

1. **Backend:**
   - [`sweetshopma-desktop/backend/api/constants.py`](sweetshopma-desktop/backend/api/constants.py) - Added `can_view_employees` permission
   - [`sweetshopma-desktop/backend/api/permissions.py`](sweetshopma-desktop/backend/api/permissions.py) - Added `CanViewEmployees` class

2. **Frontend:**
   - [`sweetshopma-desktop/frontend/src/services/apiService.js`](sweetshopma-desktop/frontend/src/services/apiService.js) - Added `getEmployeesForAttendance()` method
   - [`sweetshopma-desktop/frontend/src/pages/AttendancePage.jsx`](sweetshopma-desktop/frontend/src/pages/AttendancePage.jsx) - Updated to use new endpoint

## Security Considerations

- The new `can_view_employees` permission is **read-only** - it doesn't allow modifying users
- Moderators still cannot create, update, or delete users (requires `can_manage_users`)
- The permission is specifically scoped for attendance tracking purposes
- Seller role does NOT have this permission (they only need `can_sell`)

## Rollback Plan

If issues arise, revert the changes to:
1. Remove `'can_view_employees': True` from [`constants.py`](sweetshopma-desktop/backend/api/constants.py)
2. Remove `CanViewEmployees` class from [`permissions.py`](sweetshopma-desktop/backend/api/permissions.py)
3. Revert [`AttendancePage.jsx`](sweetshopma-desktop/frontend/src/pages/AttendancePage.jsx) to use `api.getUsers()`
4. Remove `getEmployeesForAttendance` from [`apiService.js`](sweetshopma-desktop/frontend/src/services/apiService.js)

## Related Issues

- This fix enables moderators to use the attendance tracking feature
- Similar pattern can be used for other features that need read-only access to users
- The `list_for_attendance` endpoint was already present but not being used by the frontend
