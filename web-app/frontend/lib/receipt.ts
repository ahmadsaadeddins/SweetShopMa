import { Order, OrderItem } from './types'

/**
 * Generates a formatted receipt text for printing
 */
export function generateReceiptText(order: Order): string {
  const lines: string[] = []
  
  // Header
  lines.push('='.repeat(40))
  lines.push('        SWEET SHOP MA')
  lines.push('        Point of Sale')
  lines.push('='.repeat(40))
  lines.push('')
  
  // Order info
  lines.push(`Order #${order.id}`)
  lines.push(`Date: ${new Date(order.order_date).toLocaleString()}`)
  lines.push(`Cashier: ${order.user_name}`)
  lines.push('')
  lines.push('-'.repeat(40))
  lines.push('')
  
  // Items
  lines.push('ITEMS:')
  lines.push('')
  order.items.forEach((item: OrderItem) => {
    const name = `${item.product_emoji} ${item.product_name}`
    const qty = `${item.quantity} ${item.unit_label}`
    const price = `$${parseFloat(item.price).toFixed(2)}`
    const total = `$${parseFloat(item.item_total).toFixed(2)}`
    
    lines.push(name)
    lines.push(`  ${qty} × ${price} = ${total}`)
  })
  
  lines.push('')
  lines.push('-'.repeat(40))
  lines.push('')
  
  // Totals
  lines.push(`Item Count: ${order.item_count}`)
  lines.push(`Subtotal: $${parseFloat(order.total).toFixed(2)}`)
  lines.push(`Total: $${parseFloat(order.total).toFixed(2)}`)
  lines.push('')
  lines.push('='.repeat(40))
  lines.push('')
  lines.push('Thank you for your purchase!')
  lines.push('')
  lines.push('='.repeat(40))
  
  return lines.join('\n')
}

/**
 * Opens browser print dialog with receipt
 */
export function printReceipt(order: Order) {
  const receiptText = generateReceiptText(order)
  
  // Create a new window with receipt content
  const printWindow = window.open('', '_blank')
  if (!printWindow) {
    alert('Please allow popups to print receipt')
    return
  }
  
  printWindow.document.write(`
    <!DOCTYPE html>
    <html>
    <head>
      <title>Receipt - Order #${order.id}</title>
      <style>
        @media print {
          @page {
            margin: 0.5in;
            size: letter;
          }
        }
        body {
          font-family: 'Courier New', Consolas, monospace;
          font-size: 12pt;
          line-height: 1.4;
          white-space: pre-wrap;
          margin: 0;
          padding: 20px;
        }
      </style>
    </head>
    <body>
      ${receiptText.replace(/\n/g, '<br>')}
      <script>
        window.onload = function() {
          setTimeout(function() {
            window.print();
          }, 250);
        };
      </script>
    </body>
    </html>
  `)
  
  printWindow.document.close()
}

