from django.core.management.base import BaseCommand
from django.contrib.auth import get_user_model
from api.models import Product
from decimal import Decimal

User = get_user_model()


class Command(BaseCommand):
    help = 'Seeds initial data (Developer user and sample products)'

    def handle(self, *args, **options):
        # Create default Developer user if no users exist
        if not User.objects.exists():
            User.objects.create_user(
                username='ama',
                password='AsrAma12@#',
                name='ahmad',
                role='Developer',
                is_staff=True,
                is_superuser=True
            )
            self.stdout.write(self.style.SUCCESS('Created default Developer user (ama/AsrAma12@#)'))
        else:
            self.stdout.write(self.style.WARNING('Users already exist, skipping user creation'))

        # Create sample products if none exist
        if not Product.objects.exists():
            products = [
                {'name': 'Chocolate Cake', 'emoji': '🍰', 'barcode': '501', 'price': Decimal('4.99'), 'stock': Decimal('50'), 'is_sold_by_weight': False},
                {'name': 'Gummy Bears', 'emoji': '🫐', 'barcode': '502', 'price': Decimal('3.49'), 'stock': Decimal('100'), 'is_sold_by_weight': False},
                {'name': 'Lollipop', 'emoji': '🍭', 'barcode': '503', 'price': Decimal('1.99'), 'stock': Decimal('200'), 'is_sold_by_weight': False},
                {'name': 'Donut', 'emoji': '🍩', 'barcode': '504', 'price': Decimal('2.49'), 'stock': Decimal('75'), 'is_sold_by_weight': False},
                {'name': 'Ice Cream', 'emoji': '🍦', 'barcode': '505', 'price': Decimal('3.99'), 'stock': Decimal('60'), 'is_sold_by_weight': False},
                {'name': 'Candy Corn', 'emoji': '🌽', 'barcode': '506', 'price': Decimal('2.99'), 'stock': Decimal('150'), 'is_sold_by_weight': False},
                {'name': 'Cupcake', 'emoji': '🧁', 'barcode': '507', 'price': Decimal('3.99'), 'stock': Decimal('80'), 'is_sold_by_weight': False},
                {'name': 'Chocolate Bar', 'emoji': '🍫', 'barcode': '508', 'price': Decimal('2.49'), 'stock': Decimal('120'), 'is_sold_by_weight': True},
                {'name': 'Marshmallow', 'emoji': '☁️', 'barcode': '509', 'price': Decimal('1.99'), 'stock': Decimal('90'), 'is_sold_by_weight': False},
                {'name': 'Candy Apple', 'emoji': '🍎', 'barcode': '510', 'price': Decimal('3.49'), 'stock': Decimal('40'), 'is_sold_by_weight': False},
                {'name': 'Waffle', 'emoji': '🧇', 'barcode': '511', 'price': Decimal('4.49'), 'stock': Decimal('30'), 'is_sold_by_weight': False},
                {'name': 'Croissant', 'emoji': '🥐', 'barcode': '512', 'price': Decimal('3.49'), 'stock': Decimal('55'), 'is_sold_by_weight': False},
            ]
            
            for product_data in products:
                Product.objects.create(**product_data)
            
            self.stdout.write(self.style.SUCCESS(f'Created {len(products)} sample products'))
        else:
            self.stdout.write(self.style.WARNING('Products already exist, skipping product creation'))

