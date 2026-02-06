"""
Django Management Command: seed_data

Loads mock seed data into the Django database.
Based on C# MAUI project models (Product, User, Order, OrderItem).

Usage:
    python manage.py seed_data
    python manage.py seed_data --flush
    python manage.py seed_data --categories-only
    python manage.py seed_data --users-only
    python manage.py seed_data --products-only
    python manage.py seed_data --sales-only
"""

import sys
import os
from django.core.management.base import BaseCommand, CommandError
from django.contrib.auth import get_user_model
from django.db import transaction
from django.utils import timezone
from decimal import Decimal

# Add parent directory to path to import generate_seed_data
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(__file__))))
from generate_seed_data import (
    get_categories_data,
    get_products_data,
    get_users_data,
    get_sales_data,
)

User = get_user_model()


class Command(BaseCommand):
    help = 'Load mock seed data into the database (based on C# project structure)'

    def add_arguments(self, parser):
        parser.add_argument(
            '--flush',
            action='store_true',
            dest='flush',
            help='Flush existing data before seeding (WARNING: deletes all data)',
        )
        parser.add_argument(
            '--categories-only',
            action='store_true',
            dest='categories_only',
            help='Only seed categories',
        )
        parser.add_argument(
            '--users-only',
            action='store_true',
            dest='users_only',
            help='Only seed users',
        )
        parser.add_argument(
            '--products-only',
            action='store_true',
            dest='products_only',
            help='Only seed products (requires categories)',
        )
        parser.add_argument(
            '--sales-only',
            action='store_true',
            dest='sales_only',
            help='Only seed sales (requires users and products)',
        )

    def handle(self, *args, **options):
        from api.models import Category, Product, Sale, SaleItem

        flush = options.get('flush', False)
        categories_only = options.get('categories_only', False)
        users_only = options.get('users_only', False)
        products_only = options.get('products_only', False)
        sales_only = options.get('sales_only', False)

        self.stdout.write(self.style.SUCCESS('Starting seed data loading...'))

        if flush:
            self.flush_data()

        try:
            with transaction.atomic():
                # Seed in order: Categories -> Users -> Products -> Sales
                if not users_only and not products_only and not sales_only:
                    self.seed_categories(Category)

                if not categories_only and not products_only and not sales_only:
                    self.seed_users(User)

                if not categories_only and not users_only and not sales_only:
                    self.seed_products(Product, Category)

                if not categories_only and not users_only and not products_only:
                    self.seed_sales(Sale, SaleItem, User, Product)

            self.stdout.write(self.style.SUCCESS('[OK] Seed data loaded successfully!'))
            self.print_summary()

        except Exception as e:
            self.stdout.write(self.style.ERROR(f'[ERROR] Error loading seed data: {e}'))
            raise CommandError(f'Seed data loading failed: {e}')

    def flush_data(self):
        """Flush existing data from all tables."""
        from api.models import Category, Product, Sale, SaleItem, Customer, Expense

        self.stdout.write(self.style.WARNING('Flushing existing data...'))
        
        SaleItem.objects.all().delete()
        Sale.objects.all().delete()
        Product.objects.all().delete()
        Category.objects.all().delete()
        Customer.objects.all().delete()
        Expense.objects.all().delete()
        
        # Keep the admin user if it exists, otherwise flush all users
        User.objects.filter(is_superuser=False).delete()
        
        self.stdout.write(self.style.SUCCESS('[OK] Data flushed'))

    def seed_categories(self, Category):
        """Seed categories from C# Product.Category values."""
        self.stdout.write('Seeding categories...')

        categories_data = get_categories_data()
        created_count = 0

        for cat_data in categories_data:
            category, created = Category.objects.get_or_create(
                name=cat_data['name'],
                defaults={
                    'description': cat_data['description'],
                }
            )
            if created:
                created_count += 1

        self.stdout.write(self.style.SUCCESS(f'  [OK] Created {created_count} categories'))

    def seed_users(self, User):
        """Seed users from C# User model."""
        self.stdout.write('Seeding users...')

        users_data = get_users_data()
        created_count = 0

        for user_data in users_data:
            password = user_data.pop('password')
            role = user_data.pop('role', '')
            monthly_salary = user_data.pop('monthly_salary', None)
            overtime_multiplier = user_data.pop('overtime_multiplier', None)

            user, created = User.objects.get_or_create(
                username=user_data['username'],
                defaults={
                    **user_data,
                    'password': password,  # Will be hashed by set_password
                }
            )

            if created:
                user.set_password(password)
                user.save()
                created_count += 1
                self.stdout.write(f'    Created user: {user.username} ({role})')
            else:
                self.stdout.write(f'    User exists: {user.username}')

        self.stdout.write(self.style.SUCCESS(f'  [OK] Created {created_count} users'))

    def seed_products(self, Product, Category):
        """Seed products from C# Product model."""
        self.stdout.write('Seeding products...')

        products_data = get_products_data()
        created_count = 0

        for prod_data in products_data:
            category_name = prod_data.pop('category_name')
            
            try:
                category = Category.objects.get(name=category_name)
            except Category.DoesNotExist:
                self.stdout.write(self.style.WARNING(f'    Category "{category_name}" not found, skipping product'))
                continue

            product, created = Product.objects.get_or_create(
                name=prod_data['name'],
                defaults={
                    **prod_data,
                    'category': category,
                }
            )

            if created:
                created_count += 1

        self.stdout.write(self.style.SUCCESS(f'  [OK] Created {created_count} products'))

    def seed_sales(self, Sale, SaleItem, User, Product):
        """Seed sales from C# Order model."""
        self.stdout.write('Seeding sales...')

        sales_data = get_sales_data()
        created_count = 0
        items_count = 0

        for sale_data in sales_data:
            username = sale_data.pop('username')
            items_data = sale_data.pop('items')

            try:
                user = User.objects.get(username=username)
            except User.DoesNotExist:
                self.stdout.write(self.style.WARNING(f'    User "{username}" not found, skipping sale'))
                continue

            # Create sale
            sale = Sale.objects.create(
                **sale_data,
                staff=user,
                synced=False,  # Mark as not synced for testing
            )
            created_count += 1

            # Create sale items
            for item_data in items_data:
                product_name = item_data['product']
                quantity = item_data['quantity']
                unit_price = Decimal(str(item_data['price']))

                try:
                    product = Product.objects.get(name=product_name)
                except Product.DoesNotExist:
                    self.stdout.write(self.style.WARNING(f'    Product "{product_name}" not found, skipping item'))
                    continue

                # Calculate cost price (70% of unit price)
                cost_price = unit_price * Decimal('0.70')
                subtotal = unit_price * Decimal(str(quantity))

                SaleItem.objects.create(
                    sale=sale,
                    product=product,
                    quantity=int(quantity) if quantity == int(quantity) else quantity,
                    unit_price=float(unit_price),     # Convert to float for SQLite
                    cost_price=float(cost_price),     # Convert to float for SQLite
                    subtotal=float(subtotal),         # Convert to float for SQLite
                    discount=0.0,                     # Use float instead of Decimal
                )
                items_count += 1

        self.stdout.write(self.style.SUCCESS(f'  [OK] Created {created_count} sales with {items_count} items'))

    def print_summary(self):
        """Print summary of seeded data."""
        from api.models import Category, Product, Sale, SaleItem

        self.stdout.write('\n' + '=' * 50)
        self.stdout.write(self.style.SUCCESS('SEED DATA SUMMARY'))
        self.stdout.write('=' * 50)
        self.stdout.write(f'  Categories: {Category.objects.count()}')
        self.stdout.write(f'  Products:   {Product.objects.count()}')
        self.stdout.write(f'  Users:      {User.objects.count()}')
        self.stdout.write(f'  Sales:      {Sale.objects.count()}')
        self.stdout.write(f'  Sale Items: {SaleItem.objects.count()}')
        self.stdout.write('=' * 50)
