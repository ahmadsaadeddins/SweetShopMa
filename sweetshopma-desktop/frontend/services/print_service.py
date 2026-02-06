"""
Print Service for SweetShopMa Desktop Application.

Handles receipt printing using HTML + browser approach.
"""

import os
import tempfile
import webbrowser
import threading
import time
import logging

logger = logging.getLogger(__name__)


class PrintService:
    """Handles receipt printing using HTML file and browser auto-print"""
    
    def print_receipt(self, sale_data):
        """
        Print receipt by creating HTML file and opening in browser.
        
        Args:
            sale_data (dict): Sale data with items
        
        Returns:
            dict: Success status and error message if failed
        """
        logger.info(f'Print receipt requested for sale ID={sale_data.get("id")}')
        
        try:
            # Validate sale data
            if not sale_data or 'id' not in sale_data:
                logger.error(f'Invalid sale data for printing: {sale_data}')
                return {'success': False, 'error': 'Invalid sale data'}
            
            # Generate HTML receipt
            html = self._generate_receipt_html(sale_data)
            
            # Create temp file
            temp_path = os.path.join(tempfile.gettempdir(), f'receipt_{sale_data["id"]}_{int(time.time())}.html')
            
            logger.debug(f'Creating receipt file: {temp_path}')
            
            with open(temp_path, 'w', encoding='utf-8') as f:
                f.write(html)
            
            # Open in browser (triggers print dialog)
            webbrowser.open('file://' + temp_path)
            logger.info(f'Receipt sent to browser: Sale ID={sale_data["id"]}, File={temp_path}')
            
            # Schedule cleanup
            threading.Thread(
                target=self._cleanup_file,
                args=(temp_path,),
                daemon=True
            ).start()
            
            return {'success': True}
            
        except Exception as e:
            logger.exception(f'Failed to print receipt for sale {sale_data.get("id")}: {str(e)}')
            return {'success': False, 'error': f'Unexpected error: {str(e)}'}
    
    def _generate_receipt_html(self, sale_data):
        """
        Generate HTML receipt with auto-print.
        
        Args:
            sale_data (dict): Sale data with items
        
        Returns:
            str: HTML content
        """
        items_html = ""
        for item in sale_data.get('items', []):
            items_html += f"""
            <div style="display: flex; justify-content: space-between; margin: 5px 0;">
                <span>{item.get('product_name', 'N/A')} x{item.get('quantity', 0)}</span>
                <span>${item.get('subtotal', '0.00'):.2f}</span>
            </div>
            """
        
        return f"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Receipt #{sale_data.get('id', '')}</title>
    <style>
        @media print {{
            @page {{ margin: 0.5in; size: 80mm auto; }}
            body {{ margin: 0; padding: 10px; }}
            .no-print {{ display: none; }}
        }}
        body {{
            font-family: 'Courier New', monospace;
            font-size: 12px;
            width: 80mm;
            margin: 0 auto;
        }}
        .header {{ text-align: center; border-bottom: 1px dashed #000; padding-bottom: 10px; }}
        .items {{ margin: 15px 0; }}
        .total {{ border-top: 1px dashed #000; padding-top: 10px; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 15px; font-size: 10px; }}
    </style>
</head>
<body>
    <div class="header">
        <h2>SWEET SHOP</h2>
        <p>Receipt #{sale_data.get('id', '')}</p>
        <p>{sale_data.get('created_at', '')}</p>
    </div>
    
    <div class="items">
        {items_html}
    </div>
    
    <div class="total">
        <div style="display: flex; justify-content: space-between;">
            <span>TOTAL</span>
            <span>${sale_data.get('total', '0.00'):.2f}</span>
        </div>
    </div>
    
    <div class="footer">
        <p>Thank you for your purchase!</p>
        <p class="no-print">
            <button onclick="window.print()">Print Again</button>
            <button onclick="window.close()">Close</button>
        </p>
    </div>
    
    <script>
        window.onload = function() {{
            setTimeout(function() {{ window.print(); }}, 250);
        }};
    </script>
</body>
</html>
        """
    
    def _cleanup_file(self, path, delay=30):
        """
        Clean up temp file after delay.
        
        Args:
            path (str): Path to temp file
            delay (int): Delay in seconds before cleanup
        """
        time.sleep(delay)
        try:
            if os.path.exists(path):
                os.remove(path)
                logger.debug(f'Cleaned up receipt file: {path}')
        except Exception as e:
            logger.warning(f'Failed to clean up receipt file {path}: {str(e)}')
