"""
API Bridge for SweetShopMa Desktop Application.

Bridges the PyWebView frontend with the Django backend.
Handles all API communication and error handling.
"""

import requests
import sys
import os
import json
import base64
import hashlib
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from config.settings import BRANCH_ID, BRANCH_NAME
from sync.sync_service import get_sync_service

# Credentials file path (in frontend directory)
CREDENTIALS_FILE = Path(__file__).parent / '.credentials.json'
CREDENTIALS_SALT = b'SweetShopMa_Credential_Salt_2024'  # Salt for password hashing


class ApiBridge:
    """
    Bridge between PyWebView frontend and Django backend.
    Provides methods for all API operations.
    """
    
    def __init__(self):
        """Initialize API bridge"""
        self.base_url = 'http://127.0.0.1:8000/api/'
        self.timeout = 30
        self.branch_id = BRANCH_ID
        self.branch_name = BRANCH_NAME
        self.auth_token = None
        self.current_user = None
    
    def _get_headers(self):
        """Get request headers with authentication"""
        headers = {
            'Content-Type': 'application/json',
        }
        if self.auth_token:
            headers['Authorization'] = f'Token {self.auth_token}'
        return headers
    
    def _get(self, endpoint, params=None):
        """
        Make GET request to API.
        
        Args:
            endpoint (str): API endpoint
            params (dict): Query parameters
            
        Returns:
            dict: Response data or error
        """
        try:
            response = requests.get(
                f"{self.base_url}{endpoint}",
                params=params,
                headers=self._get_headers(),
                timeout=self.timeout
            )
            
            if response.status_code == 200:
                return response.json()
            else:
                return {
                    'error': f"API Error: {response.status_code}",
                    'message': response.text
                }
        except requests.exceptions.ConnectionError:
            return {'error': 'Cannot connect to backend. Is Django running?'}
        except requests.exceptions.Timeout:
            return {'error': 'Request timeout'}
        except Exception as e:
            return {'error': str(e)}
    
    def _post(self, endpoint, data=None):
        """
        Make POST request to API.
        
        Args:
            endpoint (str): API endpoint
            data (dict): Request data
            
        Returns:
            dict: Response data or error
        """
        try:
            response = requests.post(
                f"{self.base_url}{endpoint}",
                json=data,
                headers=self._get_headers(),
                timeout=self.timeout
            )
            
            if response.status_code in [200, 201]:
                return response.json()
            else:
                return {
                    'error': f"API Error: {response.status_code}",
                    'message': response.text
                }
        except requests.exceptions.ConnectionError:
            return {'error': 'Cannot connect to backend. Is Django running?'}
        except requests.exceptions.Timeout:
            return {'error': 'Request timeout'}
        except Exception as e:
            return {'error': str(e)}
    
    def _put(self, endpoint, data=None):
        """
        Make PUT request to API.
        
        Args:
            endpoint (str): API endpoint
            data (dict): Request data
            
        Returns:
            dict: Response data or error
        """
        try:
            response = requests.put(
                f"{self.base_url}{endpoint}",
                json=data,
                headers=self._get_headers(),
                timeout=self.timeout
            )
            
            if response.status_code == 200:
                return response.json()
            else:
                return {
                    'error': f"API Error: {response.status_code}",
                    'message': response.text
                }
        except requests.exceptions.ConnectionError:
            return {'error': 'Cannot connect to backend. Is Django running?'}
        except requests.exceptions.Timeout:
            return {'error': 'Request timeout'}
        except Exception as e:
            return {'error': str(e)}
    
    def _delete(self, endpoint):
        """
        Make DELETE request to API.
        
        Args:
            endpoint (str): API endpoint
            
        Returns:
            dict: Response data or error
        """
        try:
            response = requests.delete(
                f"{self.base_url}{endpoint}",
                headers=self._get_headers(),
                timeout=self.timeout
            )
            
            if response.status_code == 204:
                return {'success': True}
            else:
                return {
                    'error': f"API Error: {response.status_code}",
                    'message': response.text
                }
        except requests.exceptions.ConnectionError:
            return {'error': 'Cannot connect to backend. Is Django running?'}
        except requests.exceptions.Timeout:
            return {'error': 'Request timeout'}
        except Exception as e:
            return {'error': str(e)}
    
    # Products API
    
    def get_products(self, category=None, search=None, is_active=None, low_stock=None):
        """Get products with optional filters"""
        params = {}
        if category:
            params['category'] = category
        if search:
            params['search'] = search
        if is_active is not None:
            params['is_active'] = str(is_active).lower()
        if low_stock is not None:
            params['low_stock'] = str(low_stock).lower()
        
        return self._get('products/', params)
    
    def get_product(self, product_id):
        """Get single product"""
        return self._get(f'products/{product_id}/')
    
    def create_product(self, data):
        """Create new product"""
        return self._post('products/', data)
    
    def update_product(self, product_id, data):
        """Update product"""
        return self._put(f'products/{product_id}/', data)
    
    def delete_product(self, product_id):
        """Delete product"""
        return self._delete(f'products/{product_id}/')
    
    def get_low_stock_products(self):
        """Get low stock products"""
        return self._get('products/low_stock/')
    
    def get_out_of_stock_products(self):
        """Get out of stock products"""
        return self._get('products/out_of_stock/')
    
    def bulk_update_quantity(self, updates):
        """Bulk update product quantities"""
        return self._post('products/bulk_update_quantity/', {'updates': updates})
    
    # Categories API
    
    def get_categories(self, search=None):
        """Get categories"""
        params = {}
        if search:
            params['search'] = search
        
        return self._get('categories/', params)
    
    def create_category(self, data):
        """Create new category"""
        return self._post('categories/', data)
    
    def update_category(self, category_id, data):
        """Update category"""
        return self._put(f'categories/{category_id}/', data)
    
    def delete_category(self, category_id):
        """Delete category"""
        return self._delete(f'categories/{category_id}/')
    
    # Authentication API
    
    def authenticate_user(self, username, password):
        """
        Authenticate user and store token.
        
        Args:
            username (str): Username
            password (str): Password
            
        Returns:
            dict: Authentication result with token and user info
        """
        result = self._post('user/authenticate/', {
            'username': username,
            'password': password
        })
        
        # If authentication successful, store token and user info
        if 'token' in result:
            self.auth_token = result['token']
            # Get role from UserProfile, default to 'User' if not set
            user_role = result.get('role', 'User')
            # Map role ID to display name if needed
            role_mapping = {
                '1': 'Developer',
                '2': 'Admin',
                '3': 'Moderator',
                '4': 'Employee',
                '5': 'Seller'
            }
            role_display = role_mapping.get(str(user_role), user_role)
            self.current_user = {
                'id': result.get('user_id'),
                'username': result.get('username'),
                'email': result.get('username') + '@sweetshop.ma',
                'role': role_display,
                'first_name': result.get('full_name', '').split()[0] if result.get('full_name') else result.get('username'),
                'last_name': result.get('full_name', '').split()[-1] if result.get('full_name') and len(result.get('full_name', '').split()) > 1 else '',
                'is_staff': result.get('is_staff', False),
                'is_superuser': result.get('is_superuser', False),
            }
            print(f"[ApiBridge] User authenticated: {self.current_user['username']}, role: {role_display}")
            
            # Return format expected by frontend
            return {
                'success': True,
                'token': result['token'],
                'user': self.current_user
            }
        else:
            print(f"[ApiBridge] Authentication failed: {result.get('error', 'Unknown error')}")
            return result
    
    def logout(self):
        """Clear authentication token and user info"""
        self.auth_token = None
        self.current_user = None
        print("[ApiBridge] User logged out")
    
    def is_authenticated(self):
        """Check if user is authenticated"""
        return self.auth_token is not None
    
    # Credentials Storage (Remember Me)

    def _hash_password(self, password):
        """
        Hash a password for secure storage using SHA-256 with salt.

        Note: This is NOT for authentication - Django handles that securely.
        This only protects stored passwords at rest on the local machine.

        Args:
            password (str): Password to hash

        Returns:
            str: Base64 encoded hashed password
        """
        # Create a hash with salt using SHA-256
        salted_password = CREDENTIALS_SALT + password.encode('utf-8')
        hash_obj = hashlib.sha256(salted_password)
        hashed = hash_obj.digest()
        return base64.b64encode(hashed).decode('utf-8')

    def save_credentials(self, username, password):
        """
        Save login credentials to local file for Remember Me functionality.
        Password is hashed using SHA-256 with salt before storage.

        SECURITY NOTE: This only provides basic protection at rest.
        For production, consider using Windows Credential Manager or keyring.

        Args:
            username (str): Username to save
            password (str): Password to save (will be hashed)

        Returns:
            dict: Success status
        """
        try:
            # Hash password before storing
            hashed_password = self._hash_password(password)

            credentials = {
                'username': username,
                'password_hash': hashed_password,
                'version': '2.0'  # Version to identify hashed format
            }
            with open(CREDENTIALS_FILE, 'w') as f:
                json.dump(credentials, f)
            print("[ApiBridge] Credentials saved (password hashed)")
            return {'success': True}
        except Exception as e:
            print(f"[ApiBridge] Error saving credentials: {e}")
            return {'success': False, 'error': str(e)}

    def get_saved_credentials(self):
        """
        Get saved login credentials from local file.

        Returns:
            dict: Saved credentials with original password, or empty dict if none
        """
        try:
            if CREDENTIALS_FILE.exists():
                with open(CREDENTIALS_FILE, 'r') as f:
                    data = json.load(f)

                # Handle old format (plaintext) and new format (hashed)
                if 'version' in data and data['version'] == '2.0':
                    # New format: password_hash is stored
                    # We CANNOT recover the original password from hash
                    # Return empty to force user to re-enter credentials
                    print("[ApiBridge] Found hashed credentials (cannot recover password)")
                    return {}
                else:
                    # Old format: plaintext password (migration needed)
                    print("[ApiBridge] Loaded saved credentials (legacy format)")
                    return data
            return {}
        except Exception as e:
            print(f"[ApiBridge] Error loading credentials: {e}")
            return {}

    def clear_saved_credentials(self):
        """
        Clear saved login credentials.

        Returns:
            dict: Success status
        """
        try:
            if CREDENTIALS_FILE.exists():
                CREDENTIALS_FILE.unlink()
                print("[ApiBridge] Credentials cleared")
            return {'success': True}
        except Exception as e:
            print(f"[ApiBridge] Error clearing credentials: {e}")
            return {'success': False, 'error': str(e)}
    
    # Sales API
    
    def get_sales(self, start_date=None, end_date=None, status=None, customer=None):
        """Get sales with optional filters"""
        params = {}
        if start_date:
            params['start_date'] = start_date
        if end_date:
            params['end_date'] = end_date
        if status:
            params['status'] = status
        if customer:
            params['customer'] = customer
        
        return self._get('sales/', params)
    
    def get_sale(self, sale_id):
        """Get single sale"""
        return self._get(f'sales/{sale_id}/')
    
    def create_sale(self, data):
        """
        Create new sale with items.
        
        Data format:
        {
            'subtotal': 25.50,
            'tax': 0,
            'discount': 0,
            'total': 25.50,
            'payment_method': 'cash',
            'customer': None,
            'notes': '',
            'items': [
                {'product_id': 1, 'quantity': 2, 'price': 12.75},
                ...
            ]
        }
        """
        return self._post('sales/', data)
    
    def refund_sale(self, sale_id, reason=''):
        """Refund a sale"""
        return self._post(f'sales/{sale_id}/refund/', {'reason': reason})
    
    def get_today_stats(self):
        """Get today's sales statistics"""
        return self._get('sales/today_stats/')
    
    def get_week_stats(self):
        """Get this week's sales statistics"""
        return self._get('sales/week_stats/')
    
    def get_month_stats(self):
        """Get this month's sales statistics"""
        return self._get('sales/month_stats/')
    
    def get_recent_sales(self, limit=10):
        """Get recent sales"""
        return self._get('sales/recent/', {'limit': limit})
    
    # Customers API
    
    def get_customers(self, search=None):
        """Get customers"""
        params = {}
        if search:
            params['search'] = search
        
        return self._get('customers/', params)
    
    def get_customer(self, customer_id):
        """Get single customer"""
        return self._get(f'customers/{customer_id}/')
    
    def create_customer(self, data):
        """Create new customer"""
        return self._post('customers/', data)
    
    def update_customer(self, customer_id, data):
        """Update customer"""
        return self._put(f'customers/{customer_id}/', data)
    
    def delete_customer(self, customer_id):
        """Delete customer"""
        return self._delete(f'customers/{customer_id}/')
    
    # Expenses API
    
    def get_expenses(self, start_date=None, end_date=None, category=None):
        """Get expenses"""
        params = {}
        if start_date:
            params['start_date'] = start_date
        if end_date:
            params['end_date'] = end_date
        if category:
            params['category'] = category
        
        return self._get('expenses/', params)
    
    def create_expense(self, data):
        """Create new expense"""
        return self._post('expenses/', data)
    
    def update_expense(self, expense_id, data):
        """Update expense"""
        return self._put(f'expenses/{expense_id}/', data)
    
    def delete_expense(self, expense_id):
        """Delete expense"""
        return self._delete(f'expenses/{expense_id}/')
    
    def get_expense_summary(self, start_date=None, end_date=None):
        """Get expense summary by category"""
        params = {}
        if start_date:
            params['start_date'] = start_date
        if end_date:
            params['end_date'] = end_date
        
        return self._get('expenses/summary/', params)
    
    # Dashboard API
    
    def get_dashboard_stats(self):
        """Get dashboard statistics"""
        return self._get('dashboard/')
    
    # Utility Methods
    
    def get_page_html(self, page):
        """
        [DEPRECATED] Get HTML for a specific page or component.
        
        This method is kept for backward compatibility during migration.
        React Router now handles all page navigation.
        
        Args:
            page (str): Page name or component path
            
        Returns:
            dict: Response with deprecation notice
        """
        print(f"[ApiBridge] ⚠ get_page_html() called for '{page}' - Navigation handled by React Router")
        return {
            'success': True,
            'html': '<p>Navigation handled by React Router</p>',
            'deprecated': True
        }
    
    def get_sync_status(self):
        """
        Get sync status from sync service.
        
        Returns:
            dict: Sync status information
        """
        try:
            sync_service = get_sync_service()
            
            if sync_service:
                return sync_service.get_sync_status()
            else:
                # Sync service not running
                return {
                    'branch_id': self.branch_id,
                    'branch_name': self.branch_name,
                    'is_online': False,
                    'is_syncing': False,
                    'last_sync': None,
                    'last_successful_sync': None,
                    'sync_count': 0,
                    'last_error': 'Sync service not running',
                    'sync_interval': 300,
                }
        except Exception as e:
            return {
                'error': str(e)
            }
    
    def sync_now(self):
        """
        Trigger immediate sync.
        
        Returns:
            dict: Sync result
        """
        try:
            sync_service = get_sync_service()
            
            if sync_service:
                success = sync_service.sync_now()
                return {
                    'success': success,
                    'message': 'Sync completed' if success else 'Sync failed'
                }
            else:
                return {
                    'success': False,
                    'message': 'Sync service not running'
                }
        except Exception as e:
            return {
                'success': False,
                'message': str(e)
            }
    
    # Restock API
    
    def get_restock_records(self, product_id=None):
        """
        Get restock records.
        
        Args:
            product_id (int): Optional product ID to filter by
        
        Returns:
            dict: Restock records or error
        """
        params = {}
        if product_id:
            params['product_id'] = product_id
        return self._get('restocks/', params)
    
    def restock_product(self, product_id, quantity):
        """
        Restock a product.
        
        Args:
            product_id (int): Product ID
            quantity (float): Quantity to add
        
        Returns:
            dict: Restock record or error
        """
        return self._post('restocks/restock/', {
            'product_id': product_id,
            'quantity': quantity
        })
    
    # Attendance API
    
    def get_attendance_records(self, query_params=''):
        """
        Get attendance records with optional filters.
        
        Args:
            query_params (str): URL-encoded query parameters
        
        Returns:
            dict: Attendance records or error
        """
        if query_params:
            return self._get('attendance/?' + query_params)
        return self._get('attendance/')
    
    def get_attendance_record(self, record_id):
        """
        Get single attendance record.
        
        Args:
            record_id (int): Attendance record ID
        
        Returns:
            dict: Attendance record or error
        """
        return self._get(f'attendance/{record_id}/')
    
    def create_attendance_record(self, data):
        """
        Create new attendance record.
        
        Args:
            data (dict): Attendance record data
        
        Returns:
            dict: Created record or error
        """
        return self._post('attendance/', data)
        
    def bulk_create_attendance(self, data):
        """
        Create multiple attendance records at once.
        
        Args:
            data (dict): {user, start_date, end_date, status, skip_weekends, notes}
        """
        return self._post('attendance/bulk_create/', data)
    
    def update_attendance_record(self, record_id, data):
        """
        Update attendance record.
        
        Args:
            record_id (int): Attendance record ID
            data (dict): Updated data
        
        Returns:
            dict: Updated record or error
        """
        return self._put(f'attendance/{record_id}/', data)
    
    def delete_attendance_record(self, record_id):
        """
        Delete attendance record.

        Args:
            record_id (int): Attendance record ID

        Returns:
            dict: Success status or error
        """
        return self._delete(f'attendance/{record_id}/')

    def check_attendance_duplicate(self, user_id, date):
        """
        Check if attendance record already exists for a user and date.

        Args:
            user_id (int): User profile ID
            date (str): Date in YYYY-MM-DD format

        Returns:
            dict: {"exists": true/false, "record": {...}} or error
        """
        return self._get(f'attendance/check_duplicate/?user_id={user_id}&date={date}')

    def get_attendance_records_summary(self, query_params=''):
        """
        Get attendance records summary statistics.
        
        Args:
            query_params (str): URL-encoded query parameters
        
        Returns:
            dict: Summary statistics or error
        """
        if query_params:
            return self._get('attendance/summary/?' + query_params)
        return self._get('attendance/summary/')
    
    def bulk_delete_attendance_records(self, data):
        """
        Bulk delete attendance records.
        
        Args:
            data (dict): {'ids': [id1, id2, ...]}
        
        Returns:
            dict: Deletion result or error
        """
        return self._post('attendance/bulk_delete/', data)
    
    def get_attendance_summaries(self, query_params=''):
        """
        Get monthly attendance summaries.
        
        Args:
            query_params (str): URL-encoded query parameters
        
        Returns:
            dict: Monthly summaries or error
        """
        if query_params:
            return self._get('attendance-summary/?' + query_params)
        return self._get('attendance-summary/')
    
    def get_attendance_expenses(self, query_params=''):
        """
        Get attendance expenses.
        
        Args:
            query_params (str): URL-encoded query parameters
        
        Returns:
            dict: Expenses or error
        """
        if query_params:
            return self._get('attendance-expenses/?' + query_params)
        return self._get('attendance-expenses/')
    
    def create_attendance_expense(self, data):
        """
        Create new attendance expense.
        
        Args:
            data (dict): Expense data
        
        Returns:
            dict: Created expense or error
        """
        return self._post('attendance-expenses/', data)

    def export_attendance_pdf(self, user_id, month, lang='ar'):
        """
        Export attendance report as PDF.
        
        Args:
            user_id (int): User ID
            month (str): Month (YYYY-MM)
            lang (str): Language (en/ar)
            
        Returns:
            dict: Response with file path or error
        """
        return self._get('attendance-summary/export_pdf/', {'user_id': user_id, 'month': month, 'lang': lang})
    
    def delete_attendance_expense(self, expense_id):
        """
        Delete attendance expense.
        
        Args:
            expense_id (int): Expense ID
        
        Returns:
            dict: Success status or error
        """
        return self._delete(f'attendance-expenses/{expense_id}/')

    # Shop Settings
    def get_shop_settings(self):
        """Get shop settings"""
        return self._get('shop-settings/')

    def update_shop_settings(self, data):
        """
        Update shop settings.
        
        Args:
            data (dict): Settings data
        """
        return self._put('shop-settings/1/', data)

    
    # User API

    def get_users(self):
        """
        Get all users with their profiles.

        Returns:
            dict: List of users or error
        """
        return self._get('user/')

    def get_user(self, user_id):
        """
        Get a single user by ID.

        Args:
            user_id (int): User profile ID

        Returns:
            dict: User data or error
        """
        return self._get(f'user/{user_id}/')

    def create_user(self, data):
        """
        Create a new user.

        Args:
            data (dict): User data including username, password, role, salary

        Returns:
            dict: Created user or error
        """
        return self._post('user/', data)

    def update_user(self, user_id, data):
        """
        Update a user.

        Args:
            user_id (int): User profile ID
            data (dict): Updated user data

        Returns:
            dict: Updated user or error
        """
        return self._put(f'user/{user_id}/', data)

    def delete_user(self, user_id):
        """
        Delete a user.

        Args:
            user_id (int): User profile ID

        Returns:
            dict: Success status or error
        """
        return self._delete(f'user/{user_id}/')

    def get_user_salary_history(self, user_id):
        """
        Get salary history for a user.

        Args:
            user_id (int): User profile ID

        Returns:
            dict: Salary history or error
        """
        return self._get(f'user/{user_id}/salary_history/')

    def get_all_salary_history(self, user_id=None):
        """
        Get salary history for all users.

        Args:
            user_id (int, optional): Filter by specific user ID

        Returns:
            dict: Salary history or error
        """
        if user_id:
            return self._get(f'user/all_salary_history/?user_id={user_id}')
        return self._get('user/all_salary_history/')

    def get_user_activities(self, user_id):
        """
        Get activity log for a user.

        Args:
            user_id (int): User profile ID

        Returns:
            dict: Activity log or error
        """
        return self._get(f'user/{user_id}/activities/')

    def get_all_activities(self, filters=None):
        """
        Get all activity logs.

        Args:
            filters (dict, optional): Filter parameters (user_id, activity_type, start_date, end_date, limit)

        Returns:
            dict: Activity logs or error
        """
        print(f"[ApiBridge] get_all_activities called with filters: {filters}")
        if filters:
            params = '&'.join([f'{k}={v}' for k, v in filters.items() if v is not None and v != ''])
            url = f'user/all_activities/?{params}' if params else 'user/all_activities/'
            print(f"[ApiBridge] Calling endpoint: {url}")
            return self._get(url)
        print(f"[ApiBridge] Calling endpoint: user/all_activities/")
        return self._get('user/all_activities/')

    def get_users_for_attendance(self):
        """
        Get all users for attendance tracking.
        
        Returns:
            dict: List of users or error
        """
        return self._get('user/list-for-attendance/')
    
    def get_user_permissions(self):
        """
        Get current user permissions.
        
        Returns:
            dict: User permissions including can_restock flag
        """
        try:
            return self._get('user/permissions/')
        except Exception as e:
            return {
                'error': str(e),
                'is_authenticated': False,
                'is_staff': False,
                'can_restock': False
            }
    
    # Receipt Printing
    
    def get_sale_receipt(self, sale_id):
        """
        Get sale details for receipt printing.
        
        Args:
            sale_id (int): Sale ID
        
        Returns:
            dict: Sale data with items or error
        """
        return self._get(f'sales/{sale_id}/receipt/')
    
    def print_receipt(self, sale_id):
        """
        Print receipt for sale.
        
        Args:
            sale_id (int): Sale ID
        
        Returns:
            dict: Success status or error
        """
        from services.print_service import PrintService
        
        result = self.get_sale_receipt(sale_id)
        if 'error' in result:
            return {'success': False, 'error': result['error']}
        
        printer = PrintService()
        return printer.print_receipt(result)
    
    # Cash Drawer
    
    def open_cash_drawer(self):
        """
        Open cash drawer.
        
        Returns:
            dict: Success status or error
        """
        from services.cash_drawer_service import CashDrawerService
        
        service = CashDrawerService()
        return service.open_drawer()
    
    def health_check(self):
        """
        Check if backend is running.
        
        Returns:
            bool: True if backend is accessible
        """
        try:
            response = requests.get(
                f"{self.base_url}dashboard/",
                timeout=5
            )
            return response.status_code == 200
        except:
            return False
